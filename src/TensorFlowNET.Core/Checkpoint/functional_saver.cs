﻿using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using Tensorflow.Train;
using static Tensorflow.ApiDef.Types;
using static Tensorflow.CostGraphDef.Types;
using static Tensorflow.OptimizerOptions.Types;
using static Tensorflow.Binding;
using System.Text.RegularExpressions;
using System.Linq;
using Tensorflow.Operations;
using Tensorflow.Training;
using Tensorflow.Graphs;
using System.Xml.Linq;
using System.Diagnostics;
using RestoreFunc = System.Func<object, object>;

namespace Tensorflow.Checkpoint
{
    public class Maybe<TA, TB>
    {
        private TA? _valueA = default(TA);
        private TB? _valueB = default(TB);
        private Type _type;
        private bool _assignedTA;
        public Maybe(TA value)
        {
            _valueA = value;
            _type= typeof(TA);
            _assignedTA = true;
        }
        public Maybe(TB value)
        {
            _valueB = value;
            _type = typeof(TB);
            _assignedTA = false;
        }

        public Type DataType => _type;

        /// <summary>
        /// Try to get the type T member of this instance. It returns true when TA or TB derive from T and is correspondingly assigned.
        /// It returns 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="res"></param>
        /// <returns></returns>
        public bool TryGet<T>(out T? res)
        {
            if(_valueA is T && _valueB is not T)
            {
                res = (T)(object)_valueA;
                return _assignedTA;
            }
            else if(_valueA is not T && _valueB is T)
            {
                res = (T)(object)_valueB;
                return !_assignedTA;
            }
            res = default(T);
            return false;
        }

        public bool IsTypeOrDeriveFrom<T>()
        {
            if (_valueA is T && _valueB is not T)
            {
                return _assignedTA;
            }
            else if (_valueA is not T && _valueB is T)
            {
                return !_assignedTA;
            }
            else if (_valueA is T && _valueB is T)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public T GetValue<T>()
        {
            if (_valueA is T && _valueB is not T)
            {
                return (T)(object)_valueA;
            }
            else if (_valueA is not T && _valueB is T)
            {
                return (T)(object)_valueB;
            }
            else if (_valueA is T && _valueB is T)
            {
                throw new TypeError("The type is vague, this is always because TA and TB both derive from T.");
            }
            else
            {
                throw new TypeError($"Expected {typeof(TA)} or {typeof(TB)}, but got typeof{typeof(T)}.");
            }
        }

        public static implicit operator Maybe<TA, TB>(TA a)
        {
            return new Maybe<TA, TB>(a);
        }
        public static implicit operator Maybe<TA, TB>(TB b)
        {
            return new Maybe<TA, TB>(b);
        }
    }
    internal class SingleDeviceSaver
    {
        private IDictionary<string, IDictionary<string, Maybe<Tensor, SaveSpec>>> _tensor_slice_dict;
        public SingleDeviceSaver(IDictionary<string, IDictionary<string, Maybe<Tensor, SaveSpec>>> tensor_slice_dict)
        {
            _tensor_slice_dict = tensor_slice_dict;
        }
        public SingleDeviceSaver(IDictionary<string, IDictionary<string, Tensor>> tensor_slice_dict)
        {
            _tensor_slice_dict = tensor_slice_dict.ToDictionary(
                x => x.Key, x => x.Value.ToDictionary(
                    y => y.Key, y => new Maybe<Tensor, SaveSpec>(y.Value)) 
                as IDictionary<string, Maybe<Tensor, SaveSpec>>);
        }
        public SingleDeviceSaver(IDictionary<string, IDictionary<string, SaveSpec>> tensor_slice_dict)
        {
            _tensor_slice_dict = tensor_slice_dict.ToDictionary(
                x => x.Key, x => x.Value.ToDictionary(
                    y => y.Key, y => new Maybe<Tensor, SaveSpec>(y.Value))
                as IDictionary<string, Maybe<Tensor, SaveSpec>>);
        }
        public Operation? save(Tensor file_prefix, CheckpointOptions? options = null)
        {
            if(options is null)
            {
                options = new CheckpointOptions();
            }
            List<string> tensor_names = new();
            List<Tensor> tensors = new();
            List<string> slice_specs = new();
            foreach(var pair in _tensor_slice_dict)
            {
                var checkpoint_key = pair.Key;
                var tensor_slices = pair.Value;
                foreach(var slice in tensor_slices)
                {
                    var slice_spec = slice.Key;
                    var maybe_tensor = slice.Value;
                    if(maybe_tensor.TryGet<SaveSpec>(out var spec))
                    {
                        var tensor_value = spec.tensor;
                        if (tensor_value is not null)
                        {
                            tensor_names.Add(spec.name);
                            tensors.Add(tensor_value);
                            slice_specs.Add(spec.slice_spec);
                        }
                    }
                    else
                    {
                        var tensor = maybe_tensor.GetValue<Tensor>();
                        tensor_names.Add(checkpoint_key);
                        tensors.Add(tensor);
                        slice_specs.Add(slice_spec);
                    }
                }
            }
            // TODO: specify the device.
            return tf.io.save_v2(file_prefix, tensor_names.ToArray(), slice_specs.ToArray(), tensors.ToArray());
        }

        public Operation? save(string file_prefix, CheckpointOptions? options = null) => save(tf.constant(file_prefix, TF_DataType.TF_STRING), options);

        public IDictionary<string, IDictionary<string, Tensor>> restore(Tensor file_prefix, CheckpointOptions? options = null)
        {
            if(options is null)
            {
                options = new CheckpointOptions();
            }
            List<string> tensor_names = new();
            List<TF_DataType> tensor_dtypes = new();
            List<string> slice_specs = new();

            foreach(var pair in _tensor_slice_dict)
            {
                var checkpoint_key = pair.Key;
                var tensor_slices = pair.Value;
                foreach(var slice in tensor_slices)
                {
                    var slice_spec = slice.Key;
                    var maybe_tensor = slice.Value;
                    // TODO: deal with other types. Currently only `SaveSpec` is allowed.
                    if(maybe_tensor.TryGet<SaveSpec>(out var spec))
                    {
                        tensor_dtypes.Add(spec.dtype);
                        slice_specs.Add(spec.slice_spec);
                        tensor_names.Add(spec.name);
                    }
                    else
                    {
                        var tensor = maybe_tensor.GetValue<Tensor>();
                        tensor_dtypes.Add(tensor.dtype);
                        slice_specs.Add(slice_spec);
                        tensor_names.Add(checkpoint_key);
                    }
                }
            }

            string restore_device = string.IsNullOrEmpty(options.experimental_io_device) ? "cpu:0": options.experimental_io_device!;

            // tf python has code `with ops.device(restore_device):` here.
            tf.device(restore_device); // may be risky.
            var restored_tensors = gen_ops.restore_v2(file_prefix, tensor_names.ToArray(), slice_specs.ToArray(), tensor_dtypes.ToArray());

            Dictionary<string, IDictionary<string, Tensor>> restored_tensor_dict = new();
            int idx = 0;
            foreach(var pair in _tensor_slice_dict)
            {
                var checkpoint_key = pair.Key;
                var tensor_slices = pair.Value;
                foreach(var slice_spec in tensor_slices.Keys)
                {
                    var restored_tensor = restored_tensors[idx++];
                    if (!restored_tensor_dict.ContainsKey(checkpoint_key))
                    {
                        restored_tensor_dict[checkpoint_key] = new Dictionary<string, Tensor>();
                    }
                    restored_tensor_dict[checkpoint_key][slice_spec] = restored_tensor;
                }
            }
            return restored_tensor_dict;
        }

        public IDictionary<string, IDictionary<string, Tensor>> restore(string file_prefix, CheckpointOptions? options = null) => restore(tf.constant(file_prefix));
    }
    /// <summary>
    /// Saves checkpoints directly from multiple devices.
    /// Note that this is a low-level utility which stores Tensors in the keys
    /// specified by `SaveableObject`s.Higher-level utilities for object-based
    /// checkpointing are built on top of it.
    /// </summary>
    public class MultiDeviceSaver
    {
        private Dictionary<string, SingleDeviceSaver> _single_device_savers;
        private IDictionary<string, (RestoreFunc, RestoreFunc)> _registered_savers;
        private Dictionary<(string, string), RestoreFunc> _keys_to_restore_fn;
        private Dictionary<RestoreFunc, IList<(string, string)>> _restore_fn_to_keys;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialized_tensors"> A dictionary mapping `Trackable` to a tensor dict, which maps checkpoint_key -> (slice_spec ->) -> Tensor/SaveSpec. </param>
        /// <param name="registered_savers"></param>
        /// <param name="call_with_mapped_capture"></param>
        public MultiDeviceSaver(IDictionary<Trackable, IDictionary<string, Maybe<Tensor, IDictionary<string, Tensor>>>> serialized_tensors,
            IDictionary<string, IDictionary<string, Trackable>>? registered_savers = null, bool call_with_mapped_capture = false)
        {
            _keys_to_restore_fn = new Dictionary<(string, string), RestoreFunc>();
            _restore_fn_to_keys = new Dictionary<RestoreFunc, IList<(string, string)>>();
            Dictionary<string, IDictionary<string, IDictionary<string, Tensor>>>  tensors_by_device= new();
            
            foreach(var pair in serialized_tensors)
            {
                var obj = pair.Key;
                var tensor_dict = pair.Value;
                RestoreFunc restore_fn;
                if(obj == Trackable.None)
                {
                    restore_fn = new RestoreFunc(x => null);
                }
                else
                {
                    restore_fn = new RestoreFunc(x =>
                    {
                        if(x is IDictionary<string, Maybe<Tensor, IDictionary<string, Tensor>>>)
                        {
                            return obj._restore_from_tensors(x as IDictionary<string, Maybe<Tensor, IDictionary<string, Tensor>>>);
                        }
                        throw new TypeError($"Expected `IDictionary<string, Maybe<Tensor, IDictionary<string, Tensor>>>` as input, got{x.GetType()}.");
                    });
                }

                foreach(var item in tensor_dict)
                {
                    var checkpoint_key = item.Key;
                    IDictionary<string, Tensor> spec_to_tensor;
                    if(item.Value.TryGet<Tensor>(out var t))
                    {
                        spec_to_tensor = new Dictionary<string, Tensor>();
                        spec_to_tensor[""] = t;
                    }
                    else
                    {
                        spec_to_tensor = item.Value.GetValue<IDictionary<string, Tensor>>();
                    }

                    foreach(var spec in spec_to_tensor)
                    {
                        var slice_spec = spec.Key;
                        var tensor = spec.Value;
                        if(_keys_to_restore_fn.ContainsKey((checkpoint_key, slice_spec)))
                        {
                            throw new ValueError("Recieved multiple tensors with the same checkpoint key and " +
                                $"slice spec. This is invalid because one will overwrite the " +
                                $"other in the checkpoint. This indicates a bug in the Checkpoint key-generation.");
                        }
                        _keys_to_restore_fn[(checkpoint_key, slice_spec)] = restore_fn;
                        _restore_fn_to_keys.SetDefault(restore_fn, new List<(string, string)>()).Add((checkpoint_key, slice_spec));

                        // skip the process of device name because lack of API.
                        var host_device = tensor.Device;
                        var internal_dict = tensors_by_device.SetDefault(host_device, new Dictionary<string, IDictionary<string, Tensor>>());
                        if (!internal_dict.ContainsKey(checkpoint_key))
                        {
                            internal_dict[checkpoint_key] = new Dictionary<string, Tensor>();
                        }
                        internal_dict[checkpoint_key][slice_spec] = tensor;
                    }
                }
            }

            _single_device_savers = tensors_by_device.ToDictionary(x => x.Key, x => new SingleDeviceSaver(x.Value));

            _registered_savers = new Dictionary<string, (RestoreFunc, RestoreFunc)>();
            if(registered_savers is not null && registered_savers.Count > 0)
            {
                // TODO: complete the implementation.
                throw new NotImplementedException();
            }
        }

        public Operation save(Tensor file_prefix, CheckpointOptions? options= null)
        {
            if(options is null)
            {
                options = new CheckpointOptions();
            }

            tf.device("CPU"); // may be risky.
            var sharded_suffix = array_ops.where(gen_ops.regex_full_match(file_prefix, tf.constant(@"^s3://.*")),
                constant_op.constant(".part"), constant_op.constant("_temp/part"));
            var tmp_checkpoint_prefix = gen_ops.string_join(new Tensor[] { file_prefix, sharded_suffix });
            IDictionary<string, Tensor> registered_paths = _registered_savers.Keys.ToDictionary(x => x, x => registered_saver_filename(file_prefix, x));

            Operation save_fn()
            {
                List<Tensor> saved_prefixes= new();
                foreach(var saver in _registered_savers)
                {
                    // TODO: implementi it later.
                    throw new NotImplementedException();
                }

                int num_shards = _single_device_savers.Count;
                List<Operation> sharded_saves = new();
                var num_shards_tensor = constant_op.constant(num_shards, name: "num_shards");
                string? last_device = null;
                int shard = 0;
                foreach(var pair in _single_device_savers.OrderBy(x => x.Key))
                {
                    var device = pair.Key;
                    var saver = pair.Value;
                    last_device = device;
                    // skip the extra process of device name because of lack of API.
                    tf.device(device);
                    var shard_prefix = sharded_filename(tmp_checkpoint_prefix, shard, num_shards_tensor);
                    saved_prefixes.Add(shard_prefix);
                    sharded_saves.Add(saver.save(shard_prefix, options));
                }
                using (var controller = ops.control_dependencies(sharded_saves.ToArray()))
                {
                    string merge_device = string.IsNullOrEmpty(options.experimental_io_device) ? last_device : options.experimental_io_device;
                    tf.device(merge_device);
                    return gen_ops.merge_v2_checkpoints(saved_prefixes.ToArray(), tf.constant(file_prefix), delete_old_dirs: true);
                }
            }

            if(tf.Context.executing_eagerly() && _single_device_savers.Count > 1)
            {
                // TODO: implement it. Currently `autograph` does not support the function with non parameter.
                throw new NotImplementedException();
            }
            else
            {
                return save_fn();
            }
        }

        public Operation save(string file_prefix, CheckpointOptions? options = null) => save(tf.constant(file_prefix), options);

        public IDictionary<string, Operation> restore(Tensor file_prefix, CheckpointOptions? options = null)
        {
            if(options is null)
            {
                options = new CheckpointOptions();
            }

            IDictionary<string, Operation> restore_func()
            {
                Dictionary<RestoreFunc, IDictionary<string, Maybe<Tensor, IDictionary<string, Tensor>>>> restore_fn_inputs = new();
                Dictionary<RestoreFunc, int> restore_fn_input_count = _restore_fn_to_keys.ToDictionary(x => x.Key, x => x.Value.Count);
                Dictionary<string, Operation> restore_ops = new();

                foreach(var single_saver in _single_device_savers.OrderBy(x => x.Key))
                {
                    var device = single_saver.Key;
                    var saver = single_saver.Value;
                    tf.device(device);
                    var restored_tensor_dict = saver.restore(file_prefix, options);

                    foreach(var pair in restored_tensor_dict)
                    {
                        var checkpoint_key = pair.Key;
                        var slice_and_tensor = pair.Value;
                        foreach(var item in slice_and_tensor)
                        {
                            var slice_spec = item.Key;
                            var tensor = item.Value;
                            var restore_fn = _keys_to_restore_fn[(checkpoint_key, slice_spec)];
                            var internal_dict = restore_fn_inputs.SetDefault(restore_fn, new Dictionary<string, Maybe<Tensor, IDictionary<string, Tensor>>>());
                            if (!string.IsNullOrEmpty(slice_spec))
                            {
                                if (!internal_dict.ContainsKey(checkpoint_key))
                                {
                                    Dictionary<string, Tensor> dict = new();
                                    dict[slice_spec] = tensor;
                                    internal_dict[checkpoint_key] = new Maybe<Tensor, IDictionary<string, Tensor>>(dict);
                                }
                                else
                                {
                                    internal_dict[checkpoint_key].GetValue<IDictionary<string, Tensor>>()[slice_spec] = tensor;
                                }
                            }
                            else
                            {
                                internal_dict[checkpoint_key] = new Maybe<Tensor, IDictionary<string, Tensor>>(tensor);
                            }
                            restore_fn_input_count[restore_fn]--;

                            if (restore_fn_input_count[restore_fn] == 0)
                            {
                                Dictionary<string, Maybe<Tensor, IDictionary<string, Tensor>>> restored_tensors = new();
                                foreach(var input in restore_fn_inputs[restore_fn])
                                {
                                    restored_tensors[TrackableUtils.extract_local_name(input.Key)] = input.Value;
                                }
                                var ret = restore_fn.DynamicInvoke(restored_tensors);
                                if(ret is IDictionary<string, Operation>)
                                {
                                    var dict = (IDictionary<string, Operation>)ret;
                                    restore_ops = restore_ops.Concat(dict).ToDictionary(x => x.Key, x => x.Value);
                                }
                            }
                        }
                    }
                }

                foreach(var item in _registered_savers)
                {
                    throw new NotImplementedException();
                }
                return restore_ops;
            }

            // TODO: complete the implementation. Currently skip it because of lack of API.
            bool has_custom_device_saver = false;

            if (tf.Context.executing_eagerly() && (_single_device_savers.Count > 1 || has_custom_device_saver))
            {
                // TODO: implement it. Currently `autograph` does not support the function with non parameter.
                throw new NotImplementedException();
            }
            else
            {
                return restore_func();
            }
        }

        /// <summary>
        /// Serializes to a SaverDef referencing the current graph.
        /// </summary>
        public SaverDef to_proto()
        {
            var filename_tensor = array_ops.placeholder(TF_DataType.TF_STRING, new int[] { }, "saver_filename");
            var traced_save_func = tf.autograph.to_graph(_traced_save, TF_DataType.TF_STRING);
            var traced_restore_func = tf.autograph.to_graph(_traced_restore, TF_DataType.TF_STRING);
            var save_tensor = traced_save_func(filename_tensor);
            var restore_op = traced_restore_func(filename_tensor).op;
            return new SaverDef()
            {
                FilenameTensorName = filename_tensor.name,
                SaveTensorName = save_tensor.name,
                RestoreOpName = restore_op.name,
                Version = SaverDef.Types.CheckpointFormatVersion.V2
            };
        }

        private Tensor _traced_save(Tensor file_prefix)
        {
            var save_op = save(file_prefix);
            tf.device("cpu:0");
            using (ops.control_dependencies(new object[]{ save_op }))
            {
                return array_ops.identity(file_prefix);
            }
        }

        private Tensor _traced_restore(Tensor file_prefix)
        {
            var restore_op = restore(file_prefix);
            tf.device("cpu:0");
            using (ops.control_dependencies(restore_op.Values.ToArray()))
            {
                return array_ops.identity(file_prefix);
            }
        }

        public static MultiDeviceSaver from_saveables(IEnumerable<MySaveableObject> saveables, IDictionary<string, IDictionary<string, Trackable>>? registered_savers = null, bool call_with_mapped_captures = false)
        {
            Dictionary<Trackable, IDictionary<string, Maybe<Tensor, IDictionary<string, Tensor>>>> serialized_tensors = new();
            foreach (var saveable in saveables)
            {
                var trackable = new SaveableCompatibilityConverter(saveable, new List<MySaveableObject>() { saveable });
                serialized_tensors[trackable] = trackable.serialize_to_tensors();
            }
            return new MultiDeviceSaver(serialized_tensors, registered_savers, call_with_mapped_captures);
        }

        private static Tensor registered_saver_filename(Tensor filename_tensor, string saver_name)
        {
            return gen_ops.string_join(new Tensor[] { filename_tensor, constant_op.constant($"-{saver_name}") });
        }
        private static Tensor sharded_filename(Tensor filename_tensor, int shard, Tensor num_shards)
        {
            return gen_ops.sharded_filename(filename_tensor, tf.constant(shard), num_shards);
        }
    }
}
