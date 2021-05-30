using System;

namespace Sisters.WudiLib.Builders.Annotations
{
#nullable enable
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class PostAttribute : Attribute
    {
        // This is a positional argument
        public PostAttribute(string field, string value)
        {
            Field = field;
            Value = value;
        }

        public string Field { get; }
        public string Value { get; }
    }
#nullable restore
}
