﻿using Tensorflow.Keras.ArgsDefinition;
using Tensorflow.Keras.Losses;
using Tensorflow.Keras.Saving.SavedModel;
using Tensorflow.Train;

namespace Tensorflow.Keras.Engine
{
    /// <summary>
    /// `Model` groups layers into an object with training and inference features.
    /// </summary>
    public partial class Model : Layer, IModel
    {
#pragma warning disable CS0169 // The field 'Model._cloning' is never used
        bool _cloning;
#pragma warning restore CS0169 // The field 'Model._cloning' is never used
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
#pragma warning disable CS0414 // The field 'Model._is_compiled' is assigned but its value is never used
        bool _is_compiled;
#pragma warning restore CS0414 // The field 'Model._is_compiled' is assigned but its value is never used
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        ILossFunc loss;
        IOptimizer optimizer;
        IVariableV1 _steps_per_execution;
        protected bool _is_graph_network;
        protected Tensors inputs;
        protected Tensors outputs;
        public string[] output_names;
        IVariableV1 _train_counter;
        IVariableV1 _test_counter;
        IVariableV1 _predict_counter;
        bool _base_model_initialized;
        bool stop_training;

        public bool IsGraphNetwork => _is_graph_network;
        
        public IOptimizer Optimizer
        {
            get => optimizer;
            set => optimizer = value;
        }

        public bool Stop_training
        {
            get => stop_training;
            set => stop_training = value;
        }

        public Model(ModelArgs args)
            : base(args)
        {
            _init_batch_counters();
        }

        internal override void Initialize(LayerArgs args)
        {
            _init_batch_counters();
            base.Initialize(args);
        }

        void _configure_steps_per_execution(int steps_per_execution)
        {
            _steps_per_execution = tf.Variable(steps_per_execution,
                dtype: TF_DataType.TF_INT64,
                aggregation: VariableAggregation.OnlyFirstReplica);
        }

        void _reset_compile_cache()
        {
            // Used to cache `trainable` attr of `Layer`s for `fit`.
            _compiled_trainable_state = _get_trainable_state();
            keras.backend._GRAPH = null;
        }

        void _init_batch_counters()
        {
            _train_counter = tf.Variable(0L,
                dtype: TF_DataType.TF_INT64,
                aggregation: VariableAggregation.OnlyFirstReplica);

            _test_counter = tf.Variable(0L,
                dtype: TF_DataType.TF_INT64,
                aggregation: VariableAggregation.OnlyFirstReplica);

            _predict_counter = tf.Variable(0L,
                dtype: TF_DataType.TF_INT64,
                aggregation: VariableAggregation.OnlyFirstReplica);
        }

        public override List<ILayer> Layers
            => _flatten_layers(recursive: false, include_self: false).ToList();

        public override List<IVariableV1> TrainableWeights
        {
            get
            {
                // skip the assertion of weights created.
                var variables = new List<IVariableV1>();

                if (!Trainable)
                {
                    return variables;
                }

                foreach (var trackable_obj in _self_tracked_trackables)
                {
                    if (trackable_obj.Trainable)
                        variables.AddRange(trackable_obj.TrainableWeights);
                }

                variables.AddRange(_trainable_weights);

                return variables.Distinct().ToList();
            }
        }

        public override List<IVariableV1> NonTrainableWeights
        {
            get
            {
                // skip the assertion of weights created.
                var variables = new List<IVariableV1>();

                foreach (var trackable_obj in _self_tracked_trackables)
                {
                    variables.AddRange(trackable_obj.NonTrainableWeights);
                }

                if (!Trainable)
                {
                    var trainable_variables = new List<IVariableV1>();
                    foreach (var trackable_obj in _self_tracked_trackables)
                    {
                        variables.AddRange(trackable_obj.TrainableWeights);
                    }
                    variables.AddRange(trainable_variables);
                    variables.AddRange(_trainable_weights);
                    variables.AddRange(_non_trainable_weights);
                }

                return variables.Distinct().ToList();
            }
        }

        public override IDictionary<string, Trackable> _trackable_children(SaveType save_type = SaveType.CHECKPOINT, IDictionary<string, IDictionary<Trackable, ISerializedAttributes>>? cache = null)
        {
            if(save_type == SaveType.SAVEDMODEL)
            {
                //TODO: deal with `train_function`, `test_function`, `predict_function`, `train_tf_function`.
            }
            var children = base._trackable_children(save_type, cache);
            return children;
        }

    }
}
