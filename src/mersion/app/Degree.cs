using SharpDX;

namespace app
{
    public struct Degree
    {
        public Degree(float value) : this()
        {
            Value = value;
        }

        public float Value { get; private set; }

        public Radian Radian()
        {
            return new Radian(MathUtil.DegreesToRadians(Value));
        }
    }
}