﻿using System.Collections.Generic;
using System.Linq;
using Tensorflow.Gradients;
using Tensorflow.Keras.Engine.DataAdapters;
using Tensorflow.Keras.Optimizers;
using static Tensorflow.Binding;

namespace Tensorflow.Keras.Engine
{
    public partial class Model
    {
        Dictionary<string, float> train_step_function(DataHandler data_handler, OwnedIterator iterator)
        {
            var data = iterator.next();
            var outputs = train_step(data_handler, data[0], data[1]);
            tf_with(ops.control_dependencies(new object[0]), ctl => _train_counter.assign_add(1));
            return outputs;
        }

        Dictionary<string, float> train_step_multi_inputs_function(DataHandler data_handler, OwnedIterator iterator)
        {
            var data = iterator.next();
            var x_size = data_handler.DataAdapter.GetDataset().FirstInputTensorCount;
            var outputs = train_step(data_handler, new Tensors(data.Take(x_size)), new Tensors(data.Skip(x_size)));
            tf_with(ops.control_dependencies(new object[0]), ctl => _train_counter.assign_add(1));
            return outputs;
        }

        /// <summary>
        /// The logic for one training step.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Dictionary<string, float> train_step(DataHandler data_handler, Tensors x, Tensors y)
        {
            (x, y) = data_handler.DataAdapter.Expand1d(x, y);
            using var tape = tf.GradientTape();
            var y_pred = Apply(x, training: true);
            var loss = compiled_loss.Call(y, y_pred);

            // For custom training steps, users can just write:
            // trainable_variables = self.trainable_variables
            // gradients = tape.gradient(loss, trainable_variables)
            // self.optimizer.apply_gradients(zip(gradients, trainable_variables))
            // The _minimize call does a few extra steps unnecessary in most cases,
            // such as loss scaling and gradient clipping.
            _minimize(tape, optimizer, loss, TrainableVariables);
            compiled_metrics.update_state(y, y_pred);

            var dict = new Dictionary<string, float>();
            metrics.ToList().ForEach(x =>
            {
                var r = x.result();
                if (r.ndim > 0)
                {
                    r = tf.reduce_mean(r);
                }
                dict[x.Name] = (float)r;
            });
            return dict;
        }

        void _minimize(GradientTape tape, IOptimizer optimizer, Tensor loss, List<IVariableV1> trainable_variables)
        {
            var gradients = tape.gradient(loss, trainable_variables);
            gradients = optimizer.aggregate_gradients(zip(gradients, trainable_variables));
            gradients = optimizer.clip_gradients(gradients);

            optimizer.apply_gradients(zip(gradients, trainable_variables.Select(x => x as ResourceVariable)),
                experimental_aggregate_gradients: false);
        }
    }
}
