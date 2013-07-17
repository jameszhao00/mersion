using System;
using Assimp;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace asset
{
    public static class VectorUtils
    {
        public static Vector3 Convert(Vector3D v3D)
        {
            return new Vector3(v3D.X, v3D.Y, v3D.Z);
        }

        public static Vector2 ToVec2(Vector3 v3)
        {
            return new Vector2(v3.X, v3.Y);
        }

        public static EffectData CompileEffect(string path)
        {
            var effectCompiler = new EffectCompiler();
            var compilerResult = effectCompiler.CompileFromFile(path,
                EffectCompilerFlags.Debug | EffectCompilerFlags.SkipOptimization);
            if (compilerResult.HasErrors)
            {
                throw new Exception();
            }
            return compilerResult.EffectData;
        }
    }
}