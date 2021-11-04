// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: tensorflow/python/framework/cpp_shape_inference.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Tensorflow {

  /// <summary>Holder for reflection information generated from tensorflow/python/framework/cpp_shape_inference.proto</summary>
  public static partial class CppShapeInferenceReflection {

    #region Descriptor
    /// <summary>File descriptor for tensorflow/python/framework/cpp_shape_inference.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static CppShapeInferenceReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjV0ZW5zb3JmbG93L3B5dGhvbi9mcmFtZXdvcmsvY3BwX3NoYXBlX2luZmVy",
            "ZW5jZS5wcm90bxIKdGVuc29yZmxvdxopdGVuc29yZmxvdy9jb3JlL2ZyYW1l",
            "d29yay9mdWxsX3R5cGUucHJvdG8aLHRlbnNvcmZsb3cvY29yZS9mcmFtZXdv",
            "cmsvdGVuc29yX3NoYXBlLnByb3RvGiV0ZW5zb3JmbG93L2NvcmUvZnJhbWV3",
            "b3JrL3R5cGVzLnByb3RvIpsDChdDcHBTaGFwZUluZmVyZW5jZVJlc3VsdBIr",
            "CgVzaGFwZRgBIAEoCzIcLnRlbnNvcmZsb3cuVGVuc29yU2hhcGVQcm90bxJD",
            "CgtoYW5kbGVfZGF0YRgEIAEoCzIuLnRlbnNvcmZsb3cuQ3BwU2hhcGVJbmZl",
            "cmVuY2VSZXN1bHQuSGFuZGxlRGF0YRqTAQoSSGFuZGxlU2hhcGVBbmRUeXBl",
            "EisKBXNoYXBlGAEgASgLMhwudGVuc29yZmxvdy5UZW5zb3JTaGFwZVByb3Rv",
            "EiMKBWR0eXBlGAIgASgOMhQudGVuc29yZmxvdy5EYXRhVHlwZRIlCgR0eXBl",
            "GAQgASgLMhcudGVuc29yZmxvdy5GdWxsVHlwZURlZkoECAMQBBpsCgpIYW5k",
            "bGVEYXRhEg4KBmlzX3NldBgBIAEoCBJOCg5zaGFwZV9hbmRfdHlwZRgCIAMo",
            "CzI2LnRlbnNvcmZsb3cuQ3BwU2hhcGVJbmZlcmVuY2VSZXN1bHQuSGFuZGxl",
            "U2hhcGVBbmRUeXBlSgQIAhADSgQIAxAEImUKHUNwcFNoYXBlSW5mZXJlbmNl",
            "SW5wdXRzTmVlZGVkEhwKFGlucHV0X3RlbnNvcnNfbmVlZGVkGAEgAygFEiYK",
            "HmlucHV0X3RlbnNvcnNfYXNfc2hhcGVzX25lZWRlZBgCIAMoBUJhWlxnaXRo",
            "dWIuY29tL3RlbnNvcmZsb3cvdGVuc29yZmxvdy90ZW5zb3JmbG93L2dvL3B5",
            "dGhvbi9mcmFtZXdvcmsvY3BwX3NoYXBlX2luZmVyZW5jZV9nb19wcm90b/gB",
            "AWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Tensorflow.FullTypeReflection.Descriptor, global::Tensorflow.TensorShapeReflection.Descriptor, global::Tensorflow.TypesReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.CppShapeInferenceResult), global::Tensorflow.CppShapeInferenceResult.Parser, new[]{ "Shape", "HandleData" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.CppShapeInferenceResult.Types.HandleShapeAndType), global::Tensorflow.CppShapeInferenceResult.Types.HandleShapeAndType.Parser, new[]{ "Shape", "Dtype", "Type" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.CppShapeInferenceResult.Types.HandleData), global::Tensorflow.CppShapeInferenceResult.Types.HandleData.Parser, new[]{ "IsSet", "ShapeAndType" }, null, null, null, null)}),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.CppShapeInferenceInputsNeeded), global::Tensorflow.CppShapeInferenceInputsNeeded.Parser, new[]{ "InputTensorsNeeded", "InputTensorsAsShapesNeeded" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class CppShapeInferenceResult : pb::IMessage<CppShapeInferenceResult> {
    private static readonly pb::MessageParser<CppShapeInferenceResult> _parser = new pb::MessageParser<CppShapeInferenceResult>(() => new CppShapeInferenceResult());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<CppShapeInferenceResult> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.CppShapeInferenceReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CppShapeInferenceResult() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CppShapeInferenceResult(CppShapeInferenceResult other) : this() {
      shape_ = other.shape_ != null ? other.shape_.Clone() : null;
      handleData_ = other.handleData_ != null ? other.handleData_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CppShapeInferenceResult Clone() {
      return new CppShapeInferenceResult(this);
    }

    /// <summary>Field number for the "shape" field.</summary>
    public const int ShapeFieldNumber = 1;
    private global::Tensorflow.TensorShapeProto shape_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.TensorShapeProto Shape {
      get { return shape_; }
      set {
        shape_ = value;
      }
    }

    /// <summary>Field number for the "handle_data" field.</summary>
    public const int HandleDataFieldNumber = 4;
    private global::Tensorflow.CppShapeInferenceResult.Types.HandleData handleData_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.CppShapeInferenceResult.Types.HandleData HandleData {
      get { return handleData_; }
      set {
        handleData_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as CppShapeInferenceResult);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(CppShapeInferenceResult other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Shape, other.Shape)) return false;
      if (!object.Equals(HandleData, other.HandleData)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (shape_ != null) hash ^= Shape.GetHashCode();
      if (handleData_ != null) hash ^= HandleData.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (shape_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Shape);
      }
      if (handleData_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(HandleData);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (shape_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Shape);
      }
      if (handleData_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(HandleData);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(CppShapeInferenceResult other) {
      if (other == null) {
        return;
      }
      if (other.shape_ != null) {
        if (shape_ == null) {
          Shape = new global::Tensorflow.TensorShapeProto();
        }
        Shape.MergeFrom(other.Shape);
      }
      if (other.handleData_ != null) {
        if (handleData_ == null) {
          HandleData = new global::Tensorflow.CppShapeInferenceResult.Types.HandleData();
        }
        HandleData.MergeFrom(other.HandleData);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (shape_ == null) {
              Shape = new global::Tensorflow.TensorShapeProto();
            }
            input.ReadMessage(Shape);
            break;
          }
          case 34: {
            if (handleData_ == null) {
              HandleData = new global::Tensorflow.CppShapeInferenceResult.Types.HandleData();
            }
            input.ReadMessage(HandleData);
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the CppShapeInferenceResult message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public sealed partial class HandleShapeAndType : pb::IMessage<HandleShapeAndType> {
        private static readonly pb::MessageParser<HandleShapeAndType> _parser = new pb::MessageParser<HandleShapeAndType>(() => new HandleShapeAndType());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<HandleShapeAndType> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Tensorflow.CppShapeInferenceResult.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public HandleShapeAndType() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public HandleShapeAndType(HandleShapeAndType other) : this() {
          shape_ = other.shape_ != null ? other.shape_.Clone() : null;
          dtype_ = other.dtype_;
          type_ = other.type_ != null ? other.type_.Clone() : null;
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public HandleShapeAndType Clone() {
          return new HandleShapeAndType(this);
        }

        /// <summary>Field number for the "shape" field.</summary>
        public const int ShapeFieldNumber = 1;
        private global::Tensorflow.TensorShapeProto shape_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public global::Tensorflow.TensorShapeProto Shape {
          get { return shape_; }
          set {
            shape_ = value;
          }
        }

        /// <summary>Field number for the "dtype" field.</summary>
        public const int DtypeFieldNumber = 2;
        private global::Tensorflow.DataType dtype_ = global::Tensorflow.DataType.DtInvalid;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public global::Tensorflow.DataType Dtype {
          get { return dtype_; }
          set {
            dtype_ = value;
          }
        }

        /// <summary>Field number for the "type" field.</summary>
        public const int TypeFieldNumber = 4;
        private global::Tensorflow.FullTypeDef type_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public global::Tensorflow.FullTypeDef Type {
          get { return type_; }
          set {
            type_ = value;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as HandleShapeAndType);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(HandleShapeAndType other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (!object.Equals(Shape, other.Shape)) return false;
          if (Dtype != other.Dtype) return false;
          if (!object.Equals(Type, other.Type)) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (shape_ != null) hash ^= Shape.GetHashCode();
          if (Dtype != global::Tensorflow.DataType.DtInvalid) hash ^= Dtype.GetHashCode();
          if (type_ != null) hash ^= Type.GetHashCode();
          if (_unknownFields != null) {
            hash ^= _unknownFields.GetHashCode();
          }
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (shape_ != null) {
            output.WriteRawTag(10);
            output.WriteMessage(Shape);
          }
          if (Dtype != global::Tensorflow.DataType.DtInvalid) {
            output.WriteRawTag(16);
            output.WriteEnum((int) Dtype);
          }
          if (type_ != null) {
            output.WriteRawTag(34);
            output.WriteMessage(Type);
          }
          if (_unknownFields != null) {
            _unknownFields.WriteTo(output);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (shape_ != null) {
            size += 1 + pb::CodedOutputStream.ComputeMessageSize(Shape);
          }
          if (Dtype != global::Tensorflow.DataType.DtInvalid) {
            size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Dtype);
          }
          if (type_ != null) {
            size += 1 + pb::CodedOutputStream.ComputeMessageSize(Type);
          }
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(HandleShapeAndType other) {
          if (other == null) {
            return;
          }
          if (other.shape_ != null) {
            if (shape_ == null) {
              Shape = new global::Tensorflow.TensorShapeProto();
            }
            Shape.MergeFrom(other.Shape);
          }
          if (other.Dtype != global::Tensorflow.DataType.DtInvalid) {
            Dtype = other.Dtype;
          }
          if (other.type_ != null) {
            if (type_ == null) {
              Type = new global::Tensorflow.FullTypeDef();
            }
            Type.MergeFrom(other.Type);
          }
          _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                break;
              case 10: {
                if (shape_ == null) {
                  Shape = new global::Tensorflow.TensorShapeProto();
                }
                input.ReadMessage(Shape);
                break;
              }
              case 16: {
                Dtype = (global::Tensorflow.DataType) input.ReadEnum();
                break;
              }
              case 34: {
                if (type_ == null) {
                  Type = new global::Tensorflow.FullTypeDef();
                }
                input.ReadMessage(Type);
                break;
              }
            }
          }
        }

      }

      public sealed partial class HandleData : pb::IMessage<HandleData> {
        private static readonly pb::MessageParser<HandleData> _parser = new pb::MessageParser<HandleData>(() => new HandleData());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<HandleData> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Tensorflow.CppShapeInferenceResult.Descriptor.NestedTypes[1]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public HandleData() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public HandleData(HandleData other) : this() {
          isSet_ = other.isSet_;
          shapeAndType_ = other.shapeAndType_.Clone();
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public HandleData Clone() {
          return new HandleData(this);
        }

        /// <summary>Field number for the "is_set" field.</summary>
        public const int IsSetFieldNumber = 1;
        private bool isSet_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool IsSet {
          get { return isSet_; }
          set {
            isSet_ = value;
          }
        }

        /// <summary>Field number for the "shape_and_type" field.</summary>
        public const int ShapeAndTypeFieldNumber = 2;
        private static readonly pb::FieldCodec<global::Tensorflow.CppShapeInferenceResult.Types.HandleShapeAndType> _repeated_shapeAndType_codec
            = pb::FieldCodec.ForMessage(18, global::Tensorflow.CppShapeInferenceResult.Types.HandleShapeAndType.Parser);
        private readonly pbc::RepeatedField<global::Tensorflow.CppShapeInferenceResult.Types.HandleShapeAndType> shapeAndType_ = new pbc::RepeatedField<global::Tensorflow.CppShapeInferenceResult.Types.HandleShapeAndType>();
        /// <summary>
        /// Only valid if &lt;is_set>.
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public pbc::RepeatedField<global::Tensorflow.CppShapeInferenceResult.Types.HandleShapeAndType> ShapeAndType {
          get { return shapeAndType_; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as HandleData);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(HandleData other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (IsSet != other.IsSet) return false;
          if(!shapeAndType_.Equals(other.shapeAndType_)) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (IsSet != false) hash ^= IsSet.GetHashCode();
          hash ^= shapeAndType_.GetHashCode();
          if (_unknownFields != null) {
            hash ^= _unknownFields.GetHashCode();
          }
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (IsSet != false) {
            output.WriteRawTag(8);
            output.WriteBool(IsSet);
          }
          shapeAndType_.WriteTo(output, _repeated_shapeAndType_codec);
          if (_unknownFields != null) {
            _unknownFields.WriteTo(output);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (IsSet != false) {
            size += 1 + 1;
          }
          size += shapeAndType_.CalculateSize(_repeated_shapeAndType_codec);
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(HandleData other) {
          if (other == null) {
            return;
          }
          if (other.IsSet != false) {
            IsSet = other.IsSet;
          }
          shapeAndType_.Add(other.shapeAndType_);
          _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                break;
              case 8: {
                IsSet = input.ReadBool();
                break;
              }
              case 18: {
                shapeAndType_.AddEntriesFrom(input, _repeated_shapeAndType_codec);
                break;
              }
            }
          }
        }

      }

    }
    #endregion

  }

  public sealed partial class CppShapeInferenceInputsNeeded : pb::IMessage<CppShapeInferenceInputsNeeded> {
    private static readonly pb::MessageParser<CppShapeInferenceInputsNeeded> _parser = new pb::MessageParser<CppShapeInferenceInputsNeeded>(() => new CppShapeInferenceInputsNeeded());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<CppShapeInferenceInputsNeeded> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.CppShapeInferenceReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CppShapeInferenceInputsNeeded() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CppShapeInferenceInputsNeeded(CppShapeInferenceInputsNeeded other) : this() {
      inputTensorsNeeded_ = other.inputTensorsNeeded_.Clone();
      inputTensorsAsShapesNeeded_ = other.inputTensorsAsShapesNeeded_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CppShapeInferenceInputsNeeded Clone() {
      return new CppShapeInferenceInputsNeeded(this);
    }

    /// <summary>Field number for the "input_tensors_needed" field.</summary>
    public const int InputTensorsNeededFieldNumber = 1;
    private static readonly pb::FieldCodec<int> _repeated_inputTensorsNeeded_codec
        = pb::FieldCodec.ForInt32(10);
    private readonly pbc::RepeatedField<int> inputTensorsNeeded_ = new pbc::RepeatedField<int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<int> InputTensorsNeeded {
      get { return inputTensorsNeeded_; }
    }

    /// <summary>Field number for the "input_tensors_as_shapes_needed" field.</summary>
    public const int InputTensorsAsShapesNeededFieldNumber = 2;
    private static readonly pb::FieldCodec<int> _repeated_inputTensorsAsShapesNeeded_codec
        = pb::FieldCodec.ForInt32(18);
    private readonly pbc::RepeatedField<int> inputTensorsAsShapesNeeded_ = new pbc::RepeatedField<int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<int> InputTensorsAsShapesNeeded {
      get { return inputTensorsAsShapesNeeded_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as CppShapeInferenceInputsNeeded);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(CppShapeInferenceInputsNeeded other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!inputTensorsNeeded_.Equals(other.inputTensorsNeeded_)) return false;
      if(!inputTensorsAsShapesNeeded_.Equals(other.inputTensorsAsShapesNeeded_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= inputTensorsNeeded_.GetHashCode();
      hash ^= inputTensorsAsShapesNeeded_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      inputTensorsNeeded_.WriteTo(output, _repeated_inputTensorsNeeded_codec);
      inputTensorsAsShapesNeeded_.WriteTo(output, _repeated_inputTensorsAsShapesNeeded_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += inputTensorsNeeded_.CalculateSize(_repeated_inputTensorsNeeded_codec);
      size += inputTensorsAsShapesNeeded_.CalculateSize(_repeated_inputTensorsAsShapesNeeded_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(CppShapeInferenceInputsNeeded other) {
      if (other == null) {
        return;
      }
      inputTensorsNeeded_.Add(other.inputTensorsNeeded_);
      inputTensorsAsShapesNeeded_.Add(other.inputTensorsAsShapesNeeded_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10:
          case 8: {
            inputTensorsNeeded_.AddEntriesFrom(input, _repeated_inputTensorsNeeded_codec);
            break;
          }
          case 18:
          case 16: {
            inputTensorsAsShapesNeeded_.AddEntriesFrom(input, _repeated_inputTensorsAsShapesNeeded_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
