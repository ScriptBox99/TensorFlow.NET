﻿namespace Tensorflow.Keras.ArgsDefinition.Rnn
{
    public class LSTMArgs : RNNArgs
    {
        // TODO: maybe change the `RNNArgs` and implement this class.
        public bool UnitForgetBias { get; set; }
        public float Dropout { get; set; }
        public float RecurrentDropout { get; set; }
        public int Implementation { get; set; }
    }
}
