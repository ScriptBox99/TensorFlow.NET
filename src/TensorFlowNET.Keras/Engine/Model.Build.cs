﻿using System;
using System.Linq;
using Tensorflow.Graphs;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace Tensorflow.Keras.Engine
{
    public partial class Model
    {
        public override void build(Shape input_shape)
        {
            if (this is Functional || this is Sequential)
            {
                base.build(input_shape);
                return;
            }

            var graph = tf.executing_eagerly() ? new FuncGraph("build_graph") : keras.backend.get_graph();

            graph.as_default();

            var x = tf.placeholder(DType, input_shape);
            Call(x, training: false);

            graph.Exit();

            base.build(input_shape);
        }
    }
}
