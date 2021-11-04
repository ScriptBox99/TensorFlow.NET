﻿using System;
using System.Collections.Generic;
using Tensorflow.Util;
using static Tensorflow.tensorflow;
using static Tensorflow.Binding;
using System.Linq;
using Tensorflow.Eager;

namespace Tensorflow.Gradients
{
    public partial class Tape
    {
        long next_op_id_ = 0;
        UnorderedMap<Tensor, long> tensor_usage_;

        public void RecordOperation(string op_type,
            Tensor[] input_tensors,
            TapeTensor[] output_tensors,
            Func<BackwardFunction> backward_function_getter)
        {
            if (!ShouldRecord(input_tensors))
                return;

            var op_id = new EagerTensor(next_op_id_++);
            foreach (var i in input_tensors)
                tensor_usage_[i]++;

            foreach (var o in output_tensors)
            {
                tf.Logger.Debug($"RecordOperation: tensor_tape_[{o.GetID()}] = {op_id}");
                tensor_tape_[o.GetTensor()] = op_id;
                tensor_usage_[o.GetTensor()] = 1;
            }

            op_tape_[op_id] = new OpTapeEntry<BackwardFunction, TapeTensor>
            {
                op_type = op_type,
                output_tensor_info = output_tensors,
                input_tensor_id = input_tensors,
                backward_function = backward_function_getter()
            };
        }
    }
}
