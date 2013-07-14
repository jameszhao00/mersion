namespace app
{
    public struct Radian
    {
        public Radian(float value)
            : this()
        {
            Value = value;
        }
        public float Value { get; private set; }
    }
}