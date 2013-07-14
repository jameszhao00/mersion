using System;
using SharpDX;

namespace app
{
    public class Transform
    {
        private Quaternion _rotation = Quaternion.Identity;
        
        public Quaternion Rotation
        {
            get { return _rotation; }
            private set { _rotation = value; }
        }

        public Vector3 Position { get; set; }

        public Vector3 Forward
        {
            get
            {
                return Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, Rotation));
            }
        }

        public Vector3 Up
        {
            get
            {
                return Vector3.Normalize(Vector3.Transform(Vector3.UnitY, Rotation));
            }
        }

        public Transform MoveLocal(Vector3 local)
        {
            var global = Vector3.Transform(local, Rotation);
            return new Transform
            {
                Rotation = Rotation,
                Position = Position + global
            };
        }
        public Transform RotateGlobal(Quaternion quat)
        {
            return new Transform
            {
                Rotation = Rotation * quat,
                Position = Position
            };
        }

        public Transform RotateLocal(Quaternion quat)
        {
            return new Transform
            {
                Rotation = quat*Rotation,
                Position = Position
            };
        }
        public Transform RotateLocal(YawPitchRoll dp)
        {
            return RotateLocal(Quaternion.RotationYawPitchRoll(dp.Yaw.Value, dp.Pitch.Value, dp.Roll.Value));
        }

        public Transform RotateGlobal(YawPitchRoll dp)
        {
            return RotateGlobal(Quaternion.RotationYawPitchRoll(dp.Yaw.Value, dp.Pitch.Value, dp.Roll.Value));
        }


        public Matrix Matrix()
        {
            return SharpDX.Matrix.Translation(Position) * SharpDX.Matrix.RotationQuaternion(Rotation);
        }

        public Vector3 Left
        {
            get
            {
                return Vector3.Normalize(Vector3.Transform(- Vector3.UnitX, Rotation));
            }
        }

        public Transform ZeroRoll()
        {
            throw new NotImplementedException();
            var expectedLeft = Vector3.Cross(Forward, Vector3.UnitY);
            if (1 - Vector3.Dot(expectedLeft, Left) < 0.01)
            {
                return this;
            }
            var sign = Vector3.Dot(Vector3.Cross(expectedLeft, Left), Forward) > 0;
            var angle = (float)Math.Acos(Vector3.Dot(expectedLeft, Left)) * (sign ? -1 :1);
            var rotation = Quaternion.RotationAxis(Forward,0.1f * angle);
            return RotateGlobal(rotation);
        }
    }
}