using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;

namespace UMS.Types
{
    public class SerializableMesh : SerializableObject<Mesh, SerializableMesh>
    {
        public override string Extension => "mesh";

        public SerializableMesh() { }
        public SerializableMesh(Mesh mesh) : base(mesh)
        {
            _indexFormat = mesh.indexFormat;
            _boneWeights = mesh.boneWeights;
            _bindposes = mesh.bindposes;
            _subMeshCount = mesh.subMeshCount;
            _bounds = mesh.bounds;
            _triangles = mesh.triangles;
            _vertices = mesh.vertices;
            _normals = mesh.normals;
            _tangents = mesh.tangents;
            _uv = mesh.uv;
            _uv2 = mesh.uv2;
            _uv3 = mesh.uv3;
            _uv4 = mesh.uv4;
            _colors = mesh.colors;
            _colors32 = mesh.colors32;
        }

        [JsonProperty]
        private IndexFormat _indexFormat;
        [JsonProperty]
        private BoneWeight[] _boneWeights;
        [JsonProperty]
        private Matrix4x4[] _bindposes;
        [JsonProperty]
        private int _subMeshCount;
        [JsonProperty]
        private Bounds _bounds;
        [JsonProperty]
        private int[] _triangles;
        [JsonProperty]
        private Vector3[] _vertices;
        [JsonProperty]
        private Vector3[] _normals;
        [JsonProperty]
        private Vector4[] _tangents;
        [JsonProperty]
        private Vector2[] _uv;
        [JsonProperty]
        private Vector2[] _uv2;
        [JsonProperty]
        private Vector2[] _uv3;
        [JsonProperty]
        private Vector2[] _uv4;
        [JsonProperty]
        private Color[] _colors;
        [JsonProperty]
        private Color32[] _colors32;

        public static SerializableMesh Serialize(Mesh obj)
        {
            return new SerializableMesh(obj);
        }
        public override Mesh Deserialize()
        {
            Mesh newMesh = new Mesh();

            Deserialize(newMesh);

            newMesh.indexFormat = _indexFormat;
            newMesh.boneWeights = _boneWeights;
            newMesh.bindposes = _bindposes;
            newMesh.subMeshCount = _subMeshCount;
            newMesh.bounds = _bounds;
            newMesh.vertices = _vertices;
            newMesh.normals = _normals;
            newMesh.tangents = _tangents;
            newMesh.uv = _uv;
            newMesh.uv2 = _uv2;
            newMesh.uv3 = _uv3;
            newMesh.uv4 = _uv4;
            newMesh.triangles = _triangles;
            newMesh.colors = _colors;
            newMesh.colors32 = _colors32;

            return newMesh;
        }
    }
}