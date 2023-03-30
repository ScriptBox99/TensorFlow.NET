﻿/*****************************************************************************
   Copyright 2018 The TensorFlow.NET Authors. All Rights Reserved.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Tensorflow.Keras.ArgsDefinition;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.Keras.Saving;
using Tensorflow.Train;

namespace Tensorflow.Keras.Utils
{
    public class generic_utils
    {
        private static readonly string _LAYER_UNDEFINED_CONFIG_KEY = "layer was saved without config";
        /// <summary>
        /// This method does not have corresponding method in python. It's close to `serialize_keras_object`.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static LayerConfig serialize_layer_to_config(ILayer instance)
        {
            var config = instance.get_config();
            Debug.Assert(config is LayerArgs);
            return new LayerConfig
            {
                Config = config as LayerArgs,
                ClassName = instance.GetType().Name
            };
        }

        public static JObject serialize_keras_object(IKerasConfigable instance)
        {
            var config = JToken.FromObject(instance.get_config());
            // TODO: change the class_name to registered name, instead of system class name.
            return serialize_utils.serialize_keras_class_and_config(instance.GetType().Name, config, instance);
        }

        public static Layer deserialize_keras_object(string class_name, JToken config)
        {
            var argType = Assembly.Load("Tensorflow.Binding").GetType($"Tensorflow.Keras.ArgsDefinition.{class_name}Args");
            var deserializationMethod = typeof(JToken).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(x => x.Name == "ToObject" && x.IsGenericMethodDefinition && x.GetParameters().Count() == 0);
            var deserializationGenericMethod = deserializationMethod.MakeGenericMethod(argType);
            var args = deserializationGenericMethod.Invoke(config, null);
            var layer = Assembly.Load("Tensorflow.Keras").CreateInstance($"Tensorflow.Keras.Layers.{class_name}", true, BindingFlags.Default, null, new object[] { args }, null, null);
            Debug.Assert(layer is Layer);
            return layer as Layer;
        }

        public static Layer deserialize_keras_object(string class_name, LayerArgs args)
        {
            var layer = Assembly.Load("Tensorflow.Keras").CreateInstance($"Tensorflow.Keras.Layers.{class_name}", true, BindingFlags.Default, null, new object[] { args }, null, null);
            Debug.Assert(layer is Layer);
            return layer as Layer;
        }

        public static LayerArgs deserialize_layer_args(string class_name, JToken config)
        {
            var argType = Assembly.Load("Tensorflow.Binding").GetType($"Tensorflow.Keras.ArgsDefinition.{class_name}Args");
            var deserializationMethod = typeof(JToken).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(x => x.Name == "ToObject" && x.IsGenericMethodDefinition && x.GetParameters().Count() == 0);
            var deserializationGenericMethod = deserializationMethod.MakeGenericMethod(argType);
            var args = deserializationGenericMethod.Invoke(config, null);
            Debug.Assert(args is LayerArgs);
            return args as LayerArgs;
        }

        public static ModelConfig deserialize_model_config(JToken json)
        {
            ModelConfig config = new ModelConfig();
            config.Name = json["name"].ToObject<string>();
            config.Layers = new List<LayerConfig>();
            var layersToken = json["layers"];
            foreach (var token in layersToken)
            {
                var args = deserialize_layer_args(token["class_name"].ToObject<string>(), token["config"]);
                config.Layers.Add(new LayerConfig()
                {
                    Config = args, 
                    Name = token["name"].ToObject<string>(), 
                    ClassName = token["class_name"].ToObject<string>(), 
                    InboundNodes = token["inbound_nodes"].ToObject<List<NodeConfig>>()
                });
            }
            config.InputLayers = json["input_layers"].ToObject<List<NodeConfig>>();
            config.OutputLayers = json["output_layers"].ToObject<List<NodeConfig>>();
            return config;
        }

        public static string to_snake_case(string name)
        {
            return string.Concat(name.Select((x, i) =>
            {
                return i > 0 && char.IsUpper(x) && !Char.IsDigit(name[i - 1]) ?
                    "_" + x.ToString() :
                    x.ToString();
            })).ToLower();
        }

        /// <summary>
        /// Determines whether config appears to be a valid layer config.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static bool validate_config(JObject config)
        {
            return !config.ContainsKey(_LAYER_UNDEFINED_CONFIG_KEY);
        }
    }
}
