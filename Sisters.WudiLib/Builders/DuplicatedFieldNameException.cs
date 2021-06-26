using System;
using Sisters.WudiLib.Builders.Annotations;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.Builders
{
    /// <summary>
    /// Exception thrown when a subclass of <see cref="Post"/> has more than one
    /// <see cref="PostAttribute"/> with same field name.
    /// </summary>
    [Serializable]
    public class DuplicatedFieldNameException : Exception
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DuplicatedFieldNameException() { }
        public DuplicatedFieldNameException(string message) : base(message) { }
        public DuplicatedFieldNameException(string message, Exception inner) : base(message, inner) { }
        public DuplicatedFieldNameException(Type type, string fieldName, string message) : base(message)
        {
            Type = type;
            FieldName = fieldName;
        }
        protected DuplicatedFieldNameException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// The type that causes the exception.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// The duplicated field name in attributes.
        /// </summary>
        public string FieldName { get; }
    }
}
