using System;
using SharpDX;

namespace app
{
    public class Transform
    {
        public override string ToString()
        {
            return string.Format("Position: {2}, Forward: {0}, Up: {1}", Forward, Up, Position);
        }

        public Transform()
        {
            Rotation = Quaternion.Identity;
            Scale = 1;
        }
        public float Scale { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Forward
        {
            get { return Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, Rotation)); }
        }

        public Vector3 Up
        {
            get { return Vector3.Normalize(Vector3.Transform(Vector3.UnitY, Rotation)); }
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
            //WARNING: These codes seem really fishy...
            //I had to change their order randomly after recompiling sharpdx...
            return new Transform
            {
                Rotation = quat * Rotation,
                Position = Position
            };
        }

        public Transform RotateLocal(Quaternion quat)
        {
            //WARNING: These codes seem really fishy...
            //I had to change their order randomly after recompiling sharpdx...
            return new Transform
            {
                Rotation = Rotation * quat,
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
        public Transform UniformScale(float scale)
        {
            return new Transform
            {
                Position = Position,
                Rotation = Rotation,
                Scale = scale
            };
        }
        public Matrix Matrix()
        {
            return SharpDX.Matrix.Scaling(Scale)*SharpDX.Matrix.RotationQuaternion(Rotation)*
                   SharpDX.Matrix.Translation(Position);
        }

        public Vector3 Left
        {
            get { return Vector3.Normalize(Vector3.Transform(- Vector3.UnitX, Rotation)); }
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
            var angle = (float) Math.Acos(Vector3.Dot(expectedLeft, Left))*(sign ? -1 : 1);
            var rotation = Quaternion.RotationAxis(Forward, 0.1f*angle);
            return RotateGlobal(rotation);
        }

    }
}