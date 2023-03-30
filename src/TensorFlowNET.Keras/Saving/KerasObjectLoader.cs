﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Tensorflow.Keras.ArgsDefinition;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.Keras.Layers.Rnn;
using Tensorflow.Keras.Losses;
using Tensorflow.Keras.Metrics;
using Tensorflow.Keras.Saving.SavedModel;
using Tensorflow.Keras.Utils;
using Tensorflow.Train;
using Tensorflow.Training;
using ThirdParty.Tensorflow.Python.Keras.Protobuf;
using static Tensorflow.ApiDef.Types;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace Tensorflow.Keras.Saving
{
    public class KerasObjectLoader
    {
        private static readonly IDictionary<string, Trackable> PUBLIC_ATTRIBUTES = new CommonEndPoints().CheckpointableObjects;
        private SavedMetadata _metadata;
        private SavedObjectGraph _proto;
        private Dictionary<int, string> _node_paths = new Dictionary<int, string>();
        private Dictionary<int, (Model, int[])> model_layer_ids_dependencies = new Dictionary<int, (Model, int[])>();
        private Dictionary<int, (Model, Layer[])> model_layer_dependencies = new Dictionary<int, (Model, Layer[])>();
        private List<int> _traversed_nodes_from_config = new List<int>();
        private Dictionary<int, (Trackable, Action<object, object, object>)> loaded_nodes;
        private List<int> _models_to_reconstruct;
        public Dictionary<int, (Trackable, Action<object, object, object>)> LoadedNodes => loaded_nodes;

        static KerasObjectLoader()
        {
            PUBLIC_ATTRIBUTES[Keras.Saving.SavedModel.Constants.KERAS_ATTR] = null;
        }

        public KerasObjectLoader(SavedMetadata metadata, SavedObjectGraph object_graph_def)
        {
            _metadata = metadata;
            _proto = object_graph_def;
            _metadata.Nodes.ToList().ForEach(x => _node_paths[x.NodeId] = x.NodePath);
            _models_to_reconstruct = new List<int>();
            loaded_nodes = new Dictionary<int, (Trackable, Action<object, object, object>)>();
        }

        /// <summary>
        /// Load all layer nodes from the metadata.
        /// </summary>
        /// <param name="compile"></param>
        public void load_layers(bool compile = true)
        {
            var metric_list = new List<ThirdParty.Tensorflow.Python.Keras.Protobuf.SavedObject>();
            foreach (var node_metadata in _metadata.Nodes)
            {
                if (node_metadata.Identifier == "_tf_keras_metric")
                {
                    metric_list.Add(node_metadata);
                    continue;
                }

                loaded_nodes[node_metadata.NodeId] = _load_layer(node_metadata.NodeId, node_metadata.Identifier, node_metadata.Metadata);
            }
            foreach(var node_metadata in metric_list)
            {
                try
                {
                    if (node_metadata.Identifier.Equals("_tf_keras_metric"))
                    {
                        continue;
                    }
                    loaded_nodes[node_metadata.NodeId] = _load_layer(node_metadata.NodeId, node_metadata.Identifier,
                        node_metadata.Metadata);
                }
                catch(ValueError e)
                {
                    if (compile)
                    {
                        throw e;
                    }
                    // TODO: add logging.warning.
                }
            }
        }

        public string get_path(int node_id)
        {
            return _node_paths[node_id];
        }

        /// <summary>
        /// Finish setting up Keras objects.
        ///
        /// This function is executed after all objects and functions have been created.
        /// Call functions and losses are attached to each layer, and once all layers
        /// have been fully set up, graph networks are initialized.
        ///
        /// Subclassed models that are revived from the SavedModel are treated like
        /// layers, and have their call/loss functions attached here.
        /// </summary>
        public void finalize_objects()
        {
            List<Layer> layers_revived_from_config = new();
            List<Layer> layers_revived_from_saved_model = new();
            foreach(var item in loaded_nodes)
            {
                var node_id = item.Key;
                var node = item.Value.Item1;
                if(node is not Layer || model_layer_ids_dependencies.ContainsKey(node_id))
                {
                    continue;
                }

                _unblock_model_reconstruction(node_id, node as Layer);

                if(node is InputLayer or Metric)
                {
                    continue;
                }

                // TODO: deal with `RevivedLayer` and `RevivedInputLayer`.
                layers_revived_from_config.Add(node as Layer);
            }

            _finalize_saved_model_layers(layers_revived_from_saved_model);
            _finalize_config_layers(layers_revived_from_config);

            _reconstruct_all_models();
        }

        private void _reconstruct_all_models()
        {
            HashSet<int> all_initialized_models = new();
            for(int i = _models_to_reconstruct.Count - 1; i >= 0; i--)
            {
                int model_id = _models_to_reconstruct[i];
                all_initialized_models.Add(model_id);
                var (model, layers) = model_layer_dependencies[model_id];
                _reconstruct_model(model_id, model, layers.ToList());
                _finalize_config_layers(new List<Layer>() { model });
            }

            Debug.Assert(all_initialized_models.SequenceEqual(model_layer_dependencies.Keys));
        }

        private void _reconstruct_model(int model_id, Model model, List<Layer> layers)
        {
            var config = JsonConvert.DeserializeObject<JObject>(_metadata.Nodes[model_id].Metadata)["config"];

            if(model.input is not null && model.input.Length > 0)
            {

            }
            else if(model is Sequential s)
            {
                if(layers is null || layers.Count == 0 || layers[0] is not InputLayer)
                {
                    if (config["layers"][0]["class_name"].ToObject<string>() == "InputLayer")
                    {
                        layers.Insert(0, new InputLayer(config["layers"][0]["config"].ToObject<InputLayerArgs>()));
                    }
                    else if (config["layers"][0]["config"]["batch_input_shape"] is not null)
                    {
                        // TODO(Rinne): implement it
                    }
                }
                
                // `model.__init__(layers, config["name"])`
                s.InitLayers(layers);
                s.Name = config["name"].ToObject<string>();
                if(s.input is null || s.input.Length == 0)
                {
                    var first_layer = _get_child_layer_node_ids(model_id)[0];
                    var input_specs = _infer_inputs(first_layer);
                    var input_shapes = _infer_inputs(first_layer, true);
                    // `model._set_inputs(input_specs)`

                    // skip the check of input_specs is Dictionary
                    if (!s.Built)
                    {
                        s.build(input_shapes);
                    }
                }
            }
            else
            {
                // skip the parameter `created_layers`.
                var (inputs, outputs, created_layers) = Functional.reconstruct_from_config(generic_utils.deserialize_model_config(config), 
                    layers.ToDictionary(x => x.Name, x => x as ILayer));
                // skip the `model.__init__`
                (model as Functional).Initialize(inputs, outputs, config["name"].ToObject<string>());
                (model as Functional).connect_ancillary_layers(created_layers);
            }

            _set_network_attributes_from_metadata(model);
            _unblock_model_reconstruction(model_id, model);
        }

        private void _set_network_attributes_from_metadata(Model revived_object)
        {
            // TODO: implement it.
        }

        /// <summary>
        /// Runs the final steps of loading Keras Layers from config.
        /// </summary>
        /// <param name="layers"></param>
        private void _finalize_config_layers(List<Layer> layers)
        {
            foreach(var layer in layers)
            {
                if (_is_graph_network(layer))
                {
                    _restore_layer_unconditional_losses(layer);
                }
                _restore_layer_activation_loss(layer);
                _restore_layer_metrics(layer);

                // TODO(Rinne): deal with RNN.
            }
        }

        /// <summary>
        /// Runs the final steps of loading Keras Layers from SavedModel.
        /// </summary>
        /// <param name="layers"></param>
        private void _finalize_saved_model_layers(List<Layer> layers)
        {
            foreach(var layer in layers)
            {
                // TODO(Rinne): deal with `RevivedNetwork`.
                
                _restore_layer_unconditional_losses(layer);
                _restore_layer_activation_loss(layer);
                _restore_layer_metrics(layer);
            }
        }

        private void _restore_layer_unconditional_losses(Layer layer)
        {
            // TODO(Rinne): implement it.
        }

        private void _restore_layer_activation_loss(Layer layer)
        {
            // TODO(Rinne): implement it.
        }

        private void _restore_layer_metrics(Layer layer)
        {
            // TODO(Rinne): implement it.
        }

        /// <summary>
        /// Removes layer from blocking model reconstruction.
        /// </summary>
        /// <param name="layer_id"></param>
        /// <param name="layer"></param>
        private void _unblock_model_reconstruction(int layer_id, Layer layer)
        {
            foreach(var depencency in model_layer_ids_dependencies)
            {
                var layer_ids = depencency.Value.Item2;
                var layers = model_layer_dependencies.SetDefault(depencency.Key,
                    (depencency.Value.Item1, new Layer[depencency.Value.Item2.Length])).Item2;
                if (!layer_ids.Contains(layer_id))
                {
                    continue;
                }
                layers[Array.IndexOf(layer_ids, layer_id)] = layer;
                if (layers.All(x => x is not null))
                {
                    _models_to_reconstruct.Add(depencency.Key);
                }
            }
        }

        private (Trackable, Action<object, object, object>) _load_layer(int node_id, string identifier, string metadata_json)
        {
            var metadata = JsonConvert.DeserializeObject<KerasMetaData>(metadata_json);

            if (loaded_nodes.ContainsKey(node_id))
            {
                var (node, setter) = loaded_nodes[node_id];

                _maybe_add_serialized_attributes(node as Layer, metadata);
                var config = metadata.Config;
                if(_is_graph_network(node as Layer) && generic_utils.validate_config(config))
                {
                    Debug.Assert(node is Model);
                    var child_nodes = _get_child_layer_node_ids(node_id);
                    model_layer_ids_dependencies[node_id] = (node as Model, child_nodes);
                    if(child_nodes is null || child_nodes.Length == 0)
                    {
                        _models_to_reconstruct.Add(node_id);
                    }
                }
                return (node, setter);
            }
            else
            {
                var (obj, setter) = _revive_from_config(identifier, metadata, node_id);
                if (obj is null)
                {
                    (obj, setter) = _revive_custom_object(identifier, metadata);
                }
                Debug.Assert(obj is Layer);
                _maybe_add_serialized_attributes(obj as Layer, metadata);
                return (obj, setter);
            }
        }

        /// <summary>
        /// Revives a layer/model from config, or returns None.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="metadata"></param>
        /// <param name="node_id"></param>
        private (Trackable, Action<object, object, object>) _revive_from_config(string identifier, KerasMetaData metadata, int node_id)
        {
            Trackable obj;
            if(identifier == Keras.Saving.SavedModel.Constants.METRIC_IDENTIFIER)
            {
                // TODO(Rinne): implement it.
                return (null, null);
                //throw new NotImplementedException("Not implemented, please submit an issue to https://github.com/SciSharp/TensorFlow.NET/issues.");
            }
            else
            {
                obj = _revive_graph_network(identifier, metadata, node_id);
                obj = obj ?? _revive_layer_or_model_from_config(metadata, node_id);
            }

            if(obj is null)
            {
                return (null, null);
            }
            var setter = _config_node_setter(_revive_setter);
            _add_children_recreated_from_config(obj, _proto.Nodes[node_id], node_id);
            return (obj, setter);
        }

        private (Trackable, Action<object, object, object>) _revive_custom_object(string identifier, KerasMetaData metadata)
        {
            // TODO(Rinne): implement it.
            throw new NotImplementedException();
        }

        Model _revive_graph_network(string identifier, KerasMetaData metadata, int node_id)
        {
            var config = metadata.Config;
            var class_name = metadata.ClassName;
            Model model = null;

            if(!metadata.IsGraphNetwork && class_name != "Sequential" && class_name != "Functional")
            {
                return null;
            }

            if (class_name == "Sequential")
            {
                model = new Sequential(new SequentialArgs
                {
                    Name = config.GetValue("name").ToString()
                });
            }
            else if(identifier == Keras.Saving.SavedModel.Constants.SEQUENTIAL_IDENTIFIER)
            {
                model = new Sequential(new SequentialArgs
                {
                    Name = class_name
                });
            }
            else
            {
                model = new Functional(new Tensors(), new Tensors(), config["name"].ToObject<string>());
            }

            // Record this model and its layers. This will later be used to reconstruct
            // the model.
            var layers = _get_child_layer_node_ids(node_id);
            model_layer_ids_dependencies[node_id] = (model, layers);
            if(layers is null || layers.Length == 0)
            {
                _models_to_reconstruct.Add(node_id);
            }
            return model;
        }

        Layer _revive_layer_or_model_from_config(KerasMetaData metadata, int node_id)
        {
            var config = metadata.Config;
            var class_name = metadata.ClassName;
            var shared_object_id = metadata.SharedObjectId;
            var must_restore_from_config = metadata.MustRestoreFromConfig;

            var obj = generic_utils.deserialize_keras_object(class_name, config);

            obj.Name = metadata.Name;
            // TODO(Rinne): add `trainable`, `dtype`, `stateful` and `save_spec`
           

            var built = _try_build_layer(obj, node_id, metadata.BuildInputShape);
            if (!built)
            {
                return null;
            }
            return obj;
        }

        private void _revive_setter(object layer, object name, object value)
        {
            Debug.Assert(name is string);
            Debug.Assert(layer is Layer);
            if(PUBLIC_ATTRIBUTES.ContainsKey(name as string))
            {
                if(value is Trackable)
                {
                    (layer as Layer)._track_trackable(value as Trackable, name as string);
                }
                if((layer as Layer).SerializedAttributes is null)
                {
                    (layer as Layer).SerializedAttributes = new JObject();
                }
                (layer as Layer).SerializedAttributes[name as string] = JToken.FromObject(value);
            }
            else if(layer is Functional && Regex.Match(name as string, @"^layer(_with_weights)?-[\d+]").Success)
            {
                (layer as Functional)._track_trackable(value as Trackable, name as string, overwrite: true);
            }
            else
            {
                var properties = layer.GetType().GetProperties();
                foreach(var p in properties)
                {
                    if(p.Name == name as string && p.GetValue(layer) is not null)
                    {
                        return;
                    }
                }
                Loader.setattr(layer, name, value);
            }
        }

        /// <summary>
        /// Returns the node ids of each layer in a Sequential/Functional model.
        /// </summary>
        /// <param name="node_id"></param>
        int[] _get_child_layer_node_ids(int node_id)
        {
            int num_layers = 0;
            Dictionary<int, int> child_layers = new Dictionary<int, int>();
            foreach (var child in _proto.Nodes[node_id].Children)
            {
                var m = Regex.Match(child.LocalName, @"layer-(\d+)");
                if (!m.Success)
                    continue;
                var layer_n = int.Parse(m.Groups[1].Value);
                num_layers = max(layer_n + 1, num_layers);
                child_layers[layer_n] = child.NodeId;
            }

            var ordered = new List<int>();
            foreach (var n in range(num_layers))
            {
                if (child_layers.ContainsKey(n))
                    ordered.Add(child_layers[n]);
                else
                    break;
            }
            return ordered.ToArray();
        }

        /// <summary>
        /// Recursively records objects recreated from config.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="proto"></param>
        /// <param name="node_id"></param>
        void _add_children_recreated_from_config(Trackable obj, SavedObject proto, int node_id)
        {
            if (_traversed_nodes_from_config.Contains(node_id))
                return;
            var parent_path = _node_paths[node_id];
            _traversed_nodes_from_config.Add(node_id);
            obj._maybe_initialize_trackable();

            if(obj is Layer layer && !layer.Built)
            {
                var metadata = JsonConvert.DeserializeObject<KerasMetaData>(_metadata.Nodes[node_id].Metadata);
                _try_build_layer(layer, node_id, metadata.BuildInputShape);
            }


            List<(Trackable, int, string)> children = new();
            foreach(var refer in proto.Children)
            {
                var obj_child = obj._lookup_dependency(refer.LocalName);
                children.Add((obj_child, refer.NodeId, refer.LocalName));
            }

            var metric_list_node_id = _search_for_child_node(node_id, new string[] { 
                Keras.Saving.SavedModel.Constants.KERAS_ATTR, "layer_metrics" 
            });
            if(metric_list_node_id is not null && obj is Model model && model.metrics is not null)
            {
                var obj_metrics = model.metrics.ToDictionary(x => x.Name, x => x);
                foreach(var refer in _proto.Nodes[metric_list_node_id.Value].Children)
                {
                    if (obj_metrics.TryGetValue(refer.LocalName, out var metric))
                    {
                        var metric_path = $"{Keras.Saving.SavedModel.Constants.KERAS_ATTR}.layer_metrics.{refer.LocalName}";
                        children.Add((metric as Metric, refer.NodeId, metric_path));
                    }
                }
            }

            foreach(var (obj_child, child_id, child_name) in children)
            {
                if(obj_child is null)
                {
                    continue;
                }
                var child_proto = _proto.Nodes[child_id];

                // skip the check for registered identifier

                Action<object, object, object> setter;
                if (Keras.Saving.SavedModel.Constants.KERAS_OBJECT_IDENTIFIERS.Contains(obj_child.ObjectIdentifier))
                {
                    setter = _revive_setter;
                }
                else
                {
                    setter = Loader.setattr;
                }

                if (loaded_nodes.ContainsKey(child_id))
                {
                    // skip the logging.warning
                    continue;
                }

                if(child_proto.KindCase == SavedObject.KindOneofCase.Variable && !string.IsNullOrEmpty(child_proto.Variable.Name))
                {
                    (obj_child as BaseResourceVariable).handle_name = child_proto.Variable.Name + ":0";
                }

                if(obj_child is TrackableDataStructure)
                {
                    setter = (x, y, z) => { };
                }

                var child_path = $"{parent_path}.{child_name}";
                _node_paths[child_id] = child_path;
                _add_children_recreated_from_config(obj_child, child_proto, child_id);
                loaded_nodes[child_id] = (obj_child, setter);
            }
        }

        private bool _try_build_layer(Layer obj, int node_id, Shape build_input_shape)
        {
            if (obj.Built)
                return true;

            if(build_input_shape is null)
            {
                build_input_shape = _infer_inputs(node_id, convert_to_shapes: true);
            }

            if(build_input_shape is not null)
            {
                obj.build(build_input_shape);
                // In tf python here is a `base_layer.Layer.build(obj, build_input_shape)`.
                // On the one hand, C# does not support call a method from specified parent class.
                // On the other hand, currently All class derived from Layer call `Layer.Build` or
                // move the implementation of `Layer.build` to its own `build` method.
                // Therefore we do not call it here.
                // However, it's still quite risky once in the future a certain class derived from 
                // `Layer` does not call `Layer.build`.

                return true;
            }

            return false;
        }

        /// <summary>
        /// Infers input shape of layer from SavedModel functions.
        /// </summary>
        /// <param name="layer_node_id"></param>
        /// <param name="convert_to_shapes"></param>
        /// <returns></returns>
        private Shape _infer_inputs(int layer_node_id, bool convert_to_shapes = false)
        {
            var call_fn_id = _search_for_child_node(layer_node_id, new string[] { "call_and_return_all_conditional_losses" });
            if(call_fn_id is null)
            {
                return null;
            }

            var concrete_functions = _proto.Nodes[call_fn_id.Value].Function.ConcreteFunctions;
            if(concrete_functions is null)
            {
                return null;
            }
            var call_fn_name = concrete_functions[0];
            var call_fn_proto = _proto.ConcreteFunctions[call_fn_name];
            throw new NotImplementedException("Not implemented, please submit an issue to https://github.com/SciSharp/TensorFlow.NET/issues.");
        }

        private int? _search_for_child_node(int parent_id, IEnumerable<string> path_to_child)
        {
            if(path_to_child is null || path_to_child.Count() == 0)
            {
                return parent_id;
            }

            foreach(var child in _proto.Nodes[parent_id].Children)
            {
                if(child.LocalName == path_to_child.First())
                {
                    return _search_for_child_node(child.NodeId, path_to_child.Skip(1));
                }
            }
            return null;
        }

        private bool _is_graph_network(Layer layer)
        {
            // TODO: deal with `RevivedLayer`
            if(layer is Functional)
            {
                return (layer as Functional).IsGraphNetwork || layer is Sequential;
            }
            return false;
        }

        private void _maybe_add_serialized_attributes(Layer layer, KerasMetaData metadata)
        {
            // TODO: deal with `RevivedLayer`
        }

        /// <summary>
        /// Creates edges for nodes that are recreated from config.
        /// </summary>
        /// <returns></returns>
        private Action<object, object, object> _config_node_setter(Action<object, object, object> setter)
        {
            void setattr_wrapper(object obj, object name, object value)
            {
                Debug.Assert(obj is Trackable);
                Debug.Assert(name is string);
                if((obj as Trackable)._lookup_dependency(name as string) is null)
                {
                    setter(obj, name, value);
                }
            }
            return setattr_wrapper;
        }
    }
}
