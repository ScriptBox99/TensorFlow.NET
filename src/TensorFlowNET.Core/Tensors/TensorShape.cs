﻿using Tensorflow.NumPy;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using static Tensorflow.Binding;

namespace Tensorflow
{
    /// <summary>
    ///     Represents the shape of a `Tensor`.
    /// </summary>
    /// <remarks>https://www.tensorflow.org/api_docs/python/tf/TensorShape</remarks>
    public partial class TensorShape
    {
        private readonly Shape shape;

        /// <summary>
        ///     Returns a list of Dimensions, or None if the shape is unspecified.
        /// </summary>
        public long[] dims => shape.dims;

        /// <summary>
        ///     Returns the rank of this shape.
        /// </summary>
        public int ndim => rank;

        private int _rank;
        /// <summary>
        ///     Returns the rank of this shape.
        /// </summary>
        public int rank => _rank > -1 ? shape.ndim : -1;

        /// <summary>
        ///     Returns the size this shape represents.
        /// </summary>
        public long size
        {
            get
            {
                var dims = shape.dims;
                var computed = 1L;
                for (int i = 0; i < dims.Length; i++)
                {
                    var val = dims[i];
                    if (val <= 0)
                        continue;
                    computed *= val;
                }

                return computed;
            }
        }

        public TensorShape()
        {
            _rank = -1;
            shape = new Shape();
        }

        public static TensorShape Scalar
            => new TensorShape(new long[0]);

        public TensorShape(TensorShapeProto proto)
        {
            if (proto.UnknownRank) return;
            switch (proto.Dim.Count)
            {
                case 0: shape = new Shape(new long[0]); 
                    break;
                default:
                    var protodims = proto.Dim;
                    var len = protodims.Count;
                    var dims = new long[len];
                    for (int i = 0; i < len; i++)
                        dims[i] = protodims[i].Size;
                    shape = new Shape(dims); 
                    break;
            }
        }

        public TensorShape(params int[] dims)
        {
            switch (dims.Length)
            {
                case 0:
                    shape = new Shape(new long[0]);
                    break;
                default:
                    shape = new Shape(dims.Select(x => Convert.ToInt64(x)).ToArray());
                    break;
            }
        }

        public TensorShape(params long[] dims)
        {
            switch (dims.Length)
            {
                case 0: shape = new Shape(new long[0]); 
                    break;
                default: shape = new Shape(dims); 
                    break;
            }
        }

        public TensorShape(long[][] dims)
        {
            if (dims.Length == 1)
            {
                switch (dims[0].Length)
                {
                    case 0: shape = new Shape(new long[0]); 
                        break;
                    default: shape = new Shape(dims[0]); 
                        break;
                }
            }
            else
            {
                throw new NotImplementedException("TensorShape int[][] dims");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slice"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">When <see cref="Slice"/> is not an Index.</exception>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public TensorShape this[Slice slice]
        {
            get
            {
                if (!slice.Stop.HasValue)
                    slice.Stop = dims.Length - slice.Start + 1;

                if (slice.Start.HasValue == false || slice.Length.HasValue == false)
                    throw new ArgumentException("Slice must has Start and Length.");

                return new TensorShape(dims.Skip(slice.Start.Value)
                    .Take(slice.Length.Value)
                    .ToArray());
            }
        }

        public long this[int index] => index < 0 ? dims[ndim + index] : dims[index];

        /// <summary>
        ///     Returns True iff `self` is fully defined in every dimension.
        /// </summary>
        /// <returns></returns>
        public bool is_fully_defined()
        {
            return rank > -1 && dims != null && dims.Count(x => x < 1) == 0;
        }

        public bool is_compatible_with(TensorShape shape2)
        {
            if (dims != null && shape2.dims != null)
            {
                if (dims.Contains(-1) || shape2.dims.Contains(-1))
                    return true;

                if (shape.size != (ulong)shape2.size)
                    return false;
            }

            return true;
        }

        public void assert_has_rank(int rank)
        {
            if (rank != ndim)
                throw new ValueError(String.Format("Shape {0} must have rank {1}", ndim, rank));
        }

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public TensorShape with_rank_at_least(int rank)
        {
            if (ndim < rank)
                throw new ValueError($"Shape {this} must have rank at least {rank}");
            else
                return this;
        }

        public TensorShape with_rank(int rank)
        {
            return merge_with(unknown_shape(rank: rank));
        }

        /// <summary>
        /// Returns an unknown TensorShape, optionally with a known rank.
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        public TensorShape unknown_shape(int rank = -1)
        {
            if (rank == -1)
                return new TensorShape(-1);
            else
                return new TensorShape(Enumerable.Repeat(-1L, rank).ToArray());
        }

        /// <summary>
        ///     Returns the concatenation of the dimension in `self` and `other`.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TensorShape concatenate(long[] other)
        {
            return concatenate(new TensorShape(other));
        }

        /// <summary>
        ///     Returns the concatenation of the dimension in `self` and `other`.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public TensorShape concatenate(TensorShape other)
        {
            var otherShape = other;

            if (ndim < 0 || otherShape.ndim < 0)
                return new TensorShape();
            else
            {
                var concatenate_dims = new long[ndim + otherShape.ndim];
                for (int i = 0; i < ndim; i++)
                    concatenate_dims[i] = dims[i];

                for (int i = 0; i < otherShape.ndim; i++)
                    concatenate_dims[ndim + i] = otherShape.dims[i];

                return new TensorShape(concatenate_dims);
            }
        }

        /// <summary>
        /// Returns a `TensorShape` combining the information in `self` and `other`.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public TensorShape merge_with(TensorShape other)
        {
            if (dims == null)
                return other;

            var new_dims = new List<long>();

            foreach (var i in range(ndim))
            {
                var dim = new Dimension(dims[i]);
                var merged = dim.merge_with(new Dimension(other.dims[i]));
                new_dims.Add(merged.value);
            }

            return new TensorShape(new_dims.ToArray());
        }

        /// <summary>
        ///     Returns a cloned array from <see cref="dims"/>.
        /// </summary>
        public long[] as_list()
        {
            if (shape.IsEmpty)
                throw new ValueError("as_list() is not defined on an unknown TensorShape.");
            return (long[])dims.Clone();
        }

        public long[] as_list_long()
        {
            if (shape.IsEmpty)
                throw new ValueError("as_list() is not defined on an unknown TensorShape.");
            return dims.Select(x => Convert.ToInt64(x)).ToArray();
        }

        public long num_elements()
        {
            if (is_fully_defined())
            {
                var size = 1L;
                foreach (var dim in dims)
                    size *= dim;
                return size;
            }

            return -1;
        }

        public override string ToString()
        {
            switch (rank)
            {
                case -1:
                    return $"<unknown>";
                case 0:
                    return $"()";
                default:
                    return $"{string.Join(",", shape).Replace("-1", "None")}";
            }
        }
    }
}
