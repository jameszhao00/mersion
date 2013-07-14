using System;

namespace app
{
    public static class Option
    {
        public static Option<T> Create<T>(T value)
        {
            return new Option<T>(value);
        }
    }

    public struct Option<T>
    {
        private readonly T _value;
        private readonly bool _isSome;
        public static Option<T> None
        {
            get { return new Option<T>(); }
        }
        public bool IsSome
        {
            get { return _isSome; }
        }
        public T Value
        {
            get
            {
                if (!IsSome)
                {
                    throw new InvalidOperationException("Option does not have a value");
                }
                return _value;
            }
        }

        public static implicit operator Option<T>(T v)
        {
            return new Option<T>(v);
        }

        public Option(T value)
        {
            _isSome = true;
            _value = value;
        }
    }
}