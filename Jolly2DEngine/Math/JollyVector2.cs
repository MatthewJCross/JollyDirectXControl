namespace Jolly2DEngine.Math
{
    public struct JollyVector2
    {
        public float X;
        public float Y;

        public JollyVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static JollyVector2 Zero => new JollyVector2(0.0f, 0.0f);

        public static JollyVector2 One => new JollyVector2(1.0f, 1.0f);

        public static JollyVector2 operator +(JollyVector2 left, JollyVector2 right)
        {
            return new JollyVector2(left.X + right.X, left.Y + right.Y);
        }

        public static JollyVector2 operator -(JollyVector2 left, JollyVector2 right)
        {
            return new JollyVector2(left.X - right.X, left.Y - right.Y);
        }

        public static JollyVector2 operator *(JollyVector2 value, float scalar)
        {
            return new JollyVector2(value.X * scalar, value.Y * scalar);
        }

        public static JollyVector2 operator /(JollyVector2 value, float scalar)
        {
            return new JollyVector2(value.X / scalar, value.Y / scalar);
        }

        public float Length()
        {
            return MathF.Sqrt(X * X + Y * Y);
        }

        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        public void Normalize()
        {
            float length = Length();

            if (length <= 0.0f)
            {
                return;
            }

            X /= length;
            Y /= length;
        }

        public static JollyVector2 Normalize(JollyVector2 value)
        {
            JollyVector2 result = value;
            result.Normalize();
            return result;
        }
    }
}
