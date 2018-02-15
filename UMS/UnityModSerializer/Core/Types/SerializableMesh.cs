using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;

namespace UMS.Core.Types
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

        public override SerializableMesh Serialize(Mesh obj)
        {
            return new SerializableMesh(obj);
        }
        public override Mesh Deserialize(SerializableMesh serializable)
        {
            Mesh newMesh = new Mesh();

            serializable.Deserialize(newMesh);

            newMesh.indexFormat = serializable._indexFormat;
            newMesh.boneWeights = serializable._boneWeights;
            newMesh.bindposes = serializable._bindposes;
            newMesh.subMeshCount = serializable._subMeshCount;
            newMesh.bounds = serializable._bounds;
            newMesh.vertices = serializable._vertices;
            newMesh.normals = serializable._normals;
            newMesh.tangents = serializable._tangents;
            newMesh.uv = serializable._uv;
            newMesh.uv2 = serializable._uv2;
            newMesh.uv3 = serializable._uv3;
            newMesh.uv4 = serializable._uv4;
            newMesh.triangles = serializable._triangles;
            newMesh.colors = serializable._colors;
            newMesh.colors32 = serializable._colors32;

            return newMesh;
        }
    }
}