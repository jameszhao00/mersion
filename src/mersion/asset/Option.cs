using System;
using ProtoBuf;

namespace asset
{
    public static class Option
    {
        public static Option<T> Create<T>(T value)
        {
            return new Option<T>(value);
        }
    }
    [ProtoContract]
    public struct Option<T>
    {
        public static Option<T> None
        {
            get { return new Option<T>(); }
        }

        [ProtoMember(1)]
        public bool IsSome
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public T Value
        {
            get;
            set;
        }

        public static implicit operator Option<T>(T v)
        {
            return new Option<T>(v);
        }

        public Option(T value) : this()
        {
            IsSome = true;
            Value = value;
        }
    }
}