using System.Linq;
using Assimp;
using SharpDX.Toolkit.Graphics;

namespace asset
{
    public static class Import
    {
        public static AssetMesh[] LoadAsset(string path, string assetRoot)
        {
            var importer = new AssimpImporter();

            var scene = importer.ImportFile(path,
                PostProcessSteps.PreTransformVertices
                | PostProcessSteps.Triangulate
                | PostProcessSteps.GenerateSmoothNormals
                | PostProcessSteps.FlipUVs);
            var textures =
                scene.Meshes.Select(mesh => scene.Materials[mesh.MaterialIndex].GetTexture(TextureType.Diffuse, 0));
            var vertices = scene.Meshes.Select(mesh => mesh.Vertices
                .Zip(mesh.Normals.Zip(mesh.GetTextureCoords(0), (normal, uv) => new
                {
                    normal,
                    uv
                }), (pos, normalUv) =>
                    new VertexPositionNormalTexture(VectorUtils.Convert(pos), VectorUtils.Convert(normalUv.normal),
                        VectorUtils.ToVec2(VectorUtils.Convert(normalUv.uv)))));
            var indices = scene.Meshes.Select(mesh => mesh.GetIndices());
            //Debug.Assert(vertices.All(x => x.All(y => y.TextureCoordinate.X < 1 && y.TextureCoordinate.Y < 1)));
            return vertices.Zip(indices.Zip(textures, (i, t) => new {i, t}), (v, it) => new AssetMesh
            {
                Vertices = v.ToArray(),
                Indices = it.i.ToArray(),
                Texture = !string.IsNullOrWhiteSpace(it.t.FilePath)
                    ? new AssetTexture
                    {
                        Path = it.t.FilePath
                    }
                    : Option<AssetTexture>.None
            }).ToArray();
        }
    }
}