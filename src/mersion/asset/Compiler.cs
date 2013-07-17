using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using asset.Compiled;

namespace asset
{
    public static class Compiler
    {
        public static byte[] CompileTexture(string path)
        {
            Console.WriteLine("Compressing {0}", path);
            var outputPath = Path.ChangeExtension(path, "dds");
            const string options = "-color -bc1";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "nvcompress.exe",
                Arguments = string.Format("{0} {1} {2}", options, path, outputPath),
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            });
            process.WaitForExit();
            Console.WriteLine("nvcompress output: {0}", process.StandardOutput.ReadToEnd());
            Console.WriteLine("Finished compressing {0}", path);
            return File.ReadAllBytes(outputPath);
        }
        public static CompiledPack Compile(AssetMesh[] meshes, string assetRoot)
        {
            return new CompiledPack
            {
                CompiledMeshes = meshes.Select(x =>
                {
                    var texture = x.Texture.IsSome
                        ? new Option<CompiledTexture>(new CompiledTexture
                        {
                            Data = CompileTexture(Path.Combine(assetRoot, x.Texture.Value.Path))
                        })
                        : Option<CompiledTexture>.None;
                    return new CompiledMesh
                    {
                        Vertices = x.Vertices.Select(y => new CompiledVertexPositionNormalTexture
                        {
                            Normal = y.Normal,
                            Position = y.Position,
                            TextureCoordinate = y.TextureCoordinate
                        }).ToArray(),
                        Indices = x.Indices,
                        Texture = texture
                    };
                }).ToArray()
            };
        }
    }
}