using System.Collections.Generic;
using UnityEngine;
using UMS.Core;
using UnityEngine.Rendering;
using UnityEditor;
using UMS.Behaviour;
using Newtonsoft.Json;

namespace UMS.Serialization
{
    public static class CustomSerializers
    {
        //------------------Template------------------//
        //[TypeSerializer(typeof(TYPE))]
        //public static object SerializeTYPE(TYPE input)
        //{
        //    return new SerializableTYPE(input);
        //}
        //[System.Serializable]
        //public class SerializableTYPE
        //{
        //    public SerializableTYPE(TYPE input)
        //    {
        //        //Set fields
        //    }

        //    //field1
        //    //field2

        //    public static implicit operator TYPE(SerializableTYPE serialized)
        //    {
        //        return new TYPE();
        //    }
        //    public static implicit operator SerializableTYPE(TYPE info)
        //    {
        //        return new TYPE();
        //    }
        //}
        //------------------------------------//


        //------------------Object------------------//
        [System.Serializable]
        public abstract class SerializableObject
        {
            public SerializableObject(Object input)
            {
                if (input == null)
                    return;

                this.name = input.name;
                this.hideFlags = (uint)input.hideFlags;

                ID = Mod.Current.GetID(input);

                Mod.Current.Add(this, ID);
            }

            public string name;
            public uint hideFlags;
            
            [JsonIgnore]
            public abstract string Extension { get; }
            [JsonIgnore]
            public int ID { get; private set; }
            
            public override string ToString()
            {
                return name;
            }
        }
        //------------------------------------//

        //------------------GameObject------------------//
        [TypeSerializer(typeof(GameObject))]
        public static object SerializeGameObject(GameObject input)
        {
            return new SerializableGameObject(input);
        }
        [System.Serializable]
        public class SerializableGameObject : SerializableObject
        {
            public SerializableGameObject(GameObject input) : base(input)
            {
                if (input == null)
                    return;

                layer = input.layer;
                activeInHierarchy = input.activeInHierarchy;
                isStatic = input.isStatic;
                tag = input.tag;

                _components = new List<SerializableComponent>();

                foreach (Component comp in input.GetComponents<Component>())
                {
                    _components.Add(new SerializableComponent(comp));
                }

                foreach (Transform child in input.transform)
                {
                    Mod.Current.Serialize(child.gameObject);
                }
            }

            public override string Extension => "gameObject";

            public int layer;
            public bool activeInHierarchy;
            public bool isStatic;
            public string tag;

            public List<SerializableComponent> _components;

            public static implicit operator GameObject(SerializableGameObject serialized)
            {
                GameObject obj = new GameObject(serialized.name);

                obj.layer = serialized.layer;
                obj.SetActive(serialized.activeInHierarchy);
                obj.isStatic = serialized.isStatic;
                obj.tag = serialized.tag;

                return obj;
            }
            public static implicit operator SerializableGameObject(GameObject input)
            {
                return new SerializableGameObject(input);
            }
        }
        //------------------------------------//
        
        //------------------Component------------------//
        [System.Serializable]
        public class SerializableComponent : SerializableObject
        {
            public SerializableComponent(Component input) : base(input)
            {
                if (input == null)
                    return;

                _customName = string.Format("{0} - {1}", input.gameObject.name, input.GetType().Name);

                _type = input.GetType();
                _members = Utility.GetMembers(input);
                _gameObject = new Reference(input.gameObject);
            }

            public override string Extension => "component";

            public Reference _gameObject;
            public System.Type _type;
            public List<Member> _members;
            public List<Reference> _children;

            [JsonIgnore]
            private string _customName = null;

            public static implicit operator Component(SerializableComponent serialized)
            {
                throw new System.NotImplementedException();
            }
            public static implicit operator SerializableComponent(Component info)
            {
                return new SerializableComponent(info);
            }
            public override string ToString()
            {
                if (_customName != null)
                    return _customName;

                return base.ToString();
            }
        }
        //------------------------------------//

        //------------------Vector2------------------//
        [TypeSerializer(typeof(Vector2))]
        public static object SerializeVector2(Vector2 input)
        {
            return new SerializableVector2(input);
        }
        [System.Serializable]
        public class SerializableVector2
        {
            public SerializableVector2(Vector2 input)
            {
                if (input == null)
                    return;

                this.x = input.x;
                this.y = input.y;
            }

            public float x;
            public float y;

            public static implicit operator Vector2(SerializableVector2 serialized)
            {
                if (serialized == null)
                    return default(Vector2);

                return new Vector2(serialized.x, serialized.y);
            }
            public static implicit operator SerializableVector2(Vector2 input)
            {
                return new SerializableVector2(input);
            }
        }
        //------------------------------------//

        //------------------Vector3------------------//
        [TypeSerializer(typeof(Vector3))]
        public static object SerializeVector3(Vector3 input)
        {
            return new SerializableVector3(input);
        }
        [System.Serializable]
        public class SerializableVector3
        {
            public SerializableVector3(Vector3 input)
            {
                if (input == null)
                    return;

                this.x = input.x;
                this.y = input.y;
                this.z = input.z;
            }

            public float x;
            public float y;
            public float z;

            public static implicit operator Vector3(SerializableVector3 serialized)
            {
                if (serialized == null)
                    return default(Vector3);

                return new Vector3(serialized.x, serialized.y, serialized.z);
            }
            public static implicit operator SerializableVector3(Vector3 input)
            {
                return new SerializableVector3(input);
            }
        }
        //------------------------------------//

        //------------------Vector4------------------//
        [TypeSerializer(typeof(Vector4))]
        public static object SerializeVector4(Vector4 input)
        {
            return new SerializableVector4(input);
        }
        [System.Serializable]
        public class SerializableVector4
        {
            public SerializableVector4(Vector4 input)
            {
                if (input == null)
                    return;

                this.x = input.x;
                this.y = input.y;
                this.z = input.z;
                this.w = input.w;
            }

            public float x;
            public float y;
            public float z;
            public float w;

            public static implicit operator Vector4(SerializableVector4 serialized)
            {
                if (serialized == null)
                    return default(Vector4);

                return new Vector4(serialized.x, serialized.y, serialized.z, serialized.w);
            }
            public static implicit operator SerializableVector4(Vector4 input)
            {
                return new SerializableVector4(input);
            }
        }
        //------------------------------------//

        //------------------Color------------------//
        [TypeSerializer(typeof(Color32))]
        public static object SerializeColor(Color32 color)
        {
            return new SerializableColor(color);
        }
        [TypeSerializer(typeof(Color))]
        public static object SerializeColor(Color color)
        {
            return new SerializableColor(color);
        }
        [System.Serializable]
        public class SerializableColor
        {
            public SerializableColor(Color color)
            {
                if (color == null)
                    return;

                this.r = color.r;
                this.g = color.g;
                this.b = color.b;
                this.a = color.a;
            }

            public float r;
            public float g;
            public float b;
            public float a;

            public static implicit operator Color(SerializableColor serialized)
            {
                if (serialized == null)
                    return Color.white;

                return new Color(serialized.r, serialized.g, serialized.b, serialized.a);
            }
            public static implicit operator SerializableColor(Color serialized)
            {
                return new SerializableColor(serialized);
            }
        }
        //------------------------------------//

        //------------------BoneWeight------------------//
        [TypeSerializer(typeof(BoneWeight))]
        public static object SerializeBoneWeight(BoneWeight boneWeight)
        {
            return new SerializableBoneWeight(boneWeight);
        }
        [System.Serializable]
        public class SerializableBoneWeight
        {
            public SerializableBoneWeight(BoneWeight boneWeight)
            {
                if (boneWeight == null)
                    return;

                weight0 = boneWeight.weight0;
                weight1 = boneWeight.weight1;
                weight2 = boneWeight.weight2;
                weight3 = boneWeight.weight3;
                boneIndex0 = boneWeight.boneIndex0;
                boneIndex1 = boneWeight.boneIndex1;
                boneIndex2 = boneWeight.boneIndex2;
                boneIndex3 = boneWeight.boneIndex3;
            }

            public float weight0;
            public float weight1;
            public float weight2;
            public float weight3;
            public int boneIndex0;
            public int boneIndex1;
            public int boneIndex2;
            public int boneIndex3;

            public static implicit operator BoneWeight(SerializableBoneWeight serialized)
            {
                if (serialized == null)
                    return new BoneWeight();

                return new BoneWeight()
                {
                    weight0 = serialized.weight0,
                    weight1 = serialized.weight1,
                    weight2 = serialized.weight2,
                    weight3 = serialized.weight3,
                    boneIndex0 = serialized.boneIndex0,
                    boneIndex1 = serialized.boneIndex1,
                    boneIndex2 = serialized.boneIndex2,
                    boneIndex3 = serialized.boneIndex3,
                };
            }
            public static implicit operator SerializableBoneWeight(BoneWeight input)
            {
                return new SerializableBoneWeight(input);
            }
        }
        //------------------------------------//

        //------------------Matrix4x4------------------//
        [TypeSerializer(typeof(Matrix4x4))]
        public static object SerializeMatrix4x4(Matrix4x4 input)
        {
            return new SerializableMatrix4x4(input);
        }
        [System.Serializable]
        public class SerializableMatrix4x4
        {
            public SerializableMatrix4x4(Matrix4x4 input)
            {
                if (input == null)
                    return;

                columns = new SerializableVector4[4];

                for (int i = 0; i < 4; i++)
                {
                    columns[i] = input.GetColumn(i);
                }
            }

            SerializableVector4[] columns;

            public static implicit operator Matrix4x4(SerializableMatrix4x4 serialized)
            {
                if (serialized == null)
                    return Matrix4x4.identity;

                return new Matrix4x4(serialized.columns[0], serialized.columns[1], serialized.columns[2], serialized.columns[3]);
            }
            public static implicit operator SerializableMatrix4x4(Matrix4x4 input)
            {
                return new SerializableMatrix4x4(input);
            }
        }
        //------------------------------------//

        //------------------Bounds------------------//
        [TypeSerializer(typeof(Bounds))]
        public static object SerializeBounds(Bounds input)
        {
            return new SerializableBounds(input);
        }
        [System.Serializable]
        public class SerializableBounds
        {
            public SerializableBounds(Bounds input)
            {
                if (input == null)
                    return;

                this.center = input.center;
                this.size = input.size;
            }

            public SerializableVector3 center;
            public SerializableVector3 size;

            public static implicit operator Bounds(SerializableBounds serialized)
            {
                if (serialized == null)
                    return new Bounds();

                return new Bounds(serialized.center, serialized.size);
            }
            public static implicit operator SerializableBounds(Bounds input)
            {
                return new SerializableBounds(input);
            }
        }
        //------------------------------------//

        //------------------Mesh------------------//
        [TypeSerializer(typeof(Mesh))]
        public static object SerializeMesh(Mesh mesh)
        {
            return new Reference(mesh);
        }
        [System.Serializable]
        public class SerializableMesh : SerializableObject
        {
            public SerializableMesh(Mesh mesh) : base(mesh)
            {
                if (mesh == null)
                    return;
                
                this.indexFormat = (uint)mesh.indexFormat;
                this.boneWeights = Utility.Copy<SerializableBoneWeight>(mesh.boneWeights);
                this.bindposes = Utility.Copy<SerializableMatrix4x4>(mesh.bindposes);
                this.subMeshCount = mesh.subMeshCount;
                this.vertices = Utility.Copy<SerializableVector3>(mesh.vertices);
                this.normals = Utility.Copy<SerializableVector3>(mesh.normals);
                this.tangents = Utility.Copy<SerializableVector4>(mesh.tangents);
                this.uv = Utility.Copy<SerializableVector2>(mesh.uv);
                this.uv2 = Utility.Copy<SerializableVector2>(mesh.uv2);
                this.uv3 = Utility.Copy<SerializableVector2>(mesh.uv3);
                this.uv4 = Utility.Copy<SerializableVector2>(mesh.uv4);
                this.colors = Utility.Copy<SerializableColor>(mesh.colors);
                this.colors32 = Utility.Copy<SerializableColor>(mesh.colors32);
                this.triangles = mesh.triangles;
            }

            public override string Extension => "mesh";

            public uint indexFormat;
            public SerializableBoneWeight[] boneWeights;
            public SerializableMatrix4x4[] bindposes;
            public int subMeshCount;
            public SerializableVector3[] vertices;
            public SerializableVector3[] normals;
            public SerializableVector4[] tangents;
            public SerializableVector2[] uv;
            public SerializableVector2[] uv2;
            public SerializableVector2[] uv3;
            public SerializableVector2[] uv4;
            public SerializableColor[] colors;
            public SerializableColor[] colors32;
            public int[] triangles;

            public static implicit operator Mesh(SerializableMesh serialized)
            {
                if (serialized == null)
                    return null;

                Mesh mesh = new Mesh
                {
                    name = serialized.name,
                    vertices = Utility.Copy<Vector3>(serialized.vertices),
                    uv = Utility.Copy<Vector2>(serialized.uv),
                    uv2 = Utility.Copy<Vector2>(serialized.uv2),
                    uv3 = Utility.Copy<Vector2>(serialized.uv3),
                    uv4 = Utility.Copy<Vector2>(serialized.uv4)
                };

                mesh.subMeshCount = mesh.subMeshCount;
                mesh.triangles = serialized.triangles;
                mesh.normals = Utility.Copy<Vector3>(serialized.normals);

                mesh.tangents = Utility.Copy<Vector4>(serialized.tangents);

                mesh.indexFormat = (IndexFormat)serialized.indexFormat;
                mesh.boneWeights = Utility.Copy<BoneWeight>(serialized.boneWeights);
                mesh.bindposes = Utility.Copy<Matrix4x4>(serialized.bindposes);
                
                mesh.RecalculateBounds();

                mesh.colors = Utility.Copy<Color>(mesh.colors);
                mesh.colors32 = Utility.Copy<Color32>(mesh.colors32);

                return mesh;
            }
            public static implicit operator SerializableMesh(Mesh input)
            {
                return new SerializableMesh(input);
            }
        }
        //------------------------------------//

        //------------------Texture------------------//
        [TypeSerializer(typeof(Texture))]
        public static object SerializeTexture(Texture input)
        {
            return new Reference(input);
        }
        [System.Serializable]
        public class SerializableTexture : SerializableObject
        {
            public SerializableTexture(Texture input) : base(input)
            {
                if (input == null)
                    return;
                
                this.mipMapBias = input.mipMapBias;
                this.wrapMode = (uint)input.wrapMode;
                this.wrapModeU = (uint)input.wrapModeU;
                this.wrapModeV = (uint)input.wrapModeV;
                this.wrapModeW = (uint)input.wrapModeW;
                this.anisoLevel = input.anisoLevel;
                this.height = input.height;
                this.width = input.width;
                this.filterMode = (uint)input.filterMode;
            }

            public override string Extension => "texture";

            public float mipMapBias;
            public uint wrapModeW;
            public uint wrapModeV;
            public uint wrapModeU;
            public uint wrapMode;
            public int anisoLevel;
            public int height;
            public int width;
            public uint filterMode;

            public static implicit operator Texture(SerializableTexture serialized)
            {
                if (serialized == null)
                    return null;

                if (serialized.name == null)
                    return null;

                return new Texture()
                {
                    mipMapBias = serialized.mipMapBias,
                    wrapMode = (TextureWrapMode)serialized.wrapMode,
                    wrapModeW = (TextureWrapMode)serialized.wrapModeW,
                    wrapModeV = (TextureWrapMode)serialized.wrapModeV,
                    wrapModeU = (TextureWrapMode)serialized.wrapModeU,
                    anisoLevel = serialized.anisoLevel,
                    height = serialized.height,
                    width = serialized.width,
                    filterMode = (FilterMode)serialized.filterMode,
                    name = serialized.name,
                };
            }
            public static implicit operator SerializableTexture(Texture input)
            {
                return new SerializableTexture(input);
            }
        }
        //------------------------------------//

        //------------------Texture2D------------------//
        [TypeSerializer(typeof(Texture2D))]
        public static object SerializeTexture2D(Texture2D input)
        {
            return new SerializableTexture2D(input);
        }
        [System.Serializable]
        public sealed class SerializableTexture2D : SerializableTexture
        {
            public SerializableTexture2D(Texture2D input) : base(input)
            {
                if (input == null)
                    return;

                this.alphaIsTransparency = input.alphaIsTransparency;
                this.format = (int)input.format;
                this.mipmapCount = input.mipmapCount;
                this.textureData = input.GetRawTextureData();
            }

            public bool alphaIsTransparency;
            public int format;
            public int mipmapCount;
            public byte[] textureData;

            public static implicit operator Texture2D(SerializableTexture2D serialized)
            {
                if (serialized == null)
                    return null;

                Texture2D texture = new Texture2D(serialized.width, serialized.height)
                {
                    mipMapBias = serialized.mipMapBias,
                    wrapMode = (TextureWrapMode)serialized.wrapMode,
                    wrapModeU = (TextureWrapMode)serialized.wrapModeU,
                    wrapModeV = (TextureWrapMode)serialized.wrapModeV,
                    wrapModeW = (TextureWrapMode)serialized.wrapModeW,
                    anisoLevel = serialized.anisoLevel,
                    filterMode = (FilterMode)serialized.filterMode,
                    name = serialized.name
                };

                texture.LoadRawTextureData(serialized.textureData);
                texture.Apply(true);

                return texture;
            }
            public static implicit operator SerializableTexture2D(Texture2D input)
            {
                return new SerializableTexture2D(input);
            }
        }
        //------------------------------------//

        //------------------RenderTexture------------------//
        [TypeSerializer(typeof(RenderTexture))]
        public static object SerializeRenderTexture(RenderTexture input)
        {
            return new SerializableRenderTexture(input);
        }
        [System.Serializable]
        public class SerializableRenderTexture : SerializableTexture
        {
            public SerializableRenderTexture(RenderTexture input) : base(input)
            {
                if (input == null)
                    return;

                this.descriptor = input.descriptor;
            }

            public SerializableRenderTextureDescriptor descriptor;
            
            public static implicit operator RenderTexture(SerializableRenderTexture serialized)
            {
                if (serialized == null)
                    return null;

                return new RenderTexture(serialized.descriptor)
                {
                    name = serialized.name,
                };
            }
            public static implicit operator SerializableRenderTexture(RenderTexture input)
            {
                return new SerializableRenderTexture(input);
            }
        }
        [System.Serializable]
        public class SerializableRenderTextureDescriptor
        {
            public SerializableRenderTextureDescriptor(RenderTextureDescriptor input)
            {
                this.useMipMap = input.useMipMap;
                this.sRGB = input.sRGB;
                this.memoryless = (uint)input.memoryless;
                this.vrUsage = (uint)input.vrUsage;
                this.shadowSamplingMode = (uint)input.shadowSamplingMode;
                this.dimension = (int)input.dimension;
                this.depthBufferBits = input.depthBufferBits;
                this.colorFormat = (uint)input.colorFormat;
                this.bindMS = input.bindMS;
                this.volumeDepth = input.volumeDepth;
                this.msaaSamples = input.msaaSamples;
                this.height = input.height;
                this.width = input.width;
                this.autoGenerateMips = input.autoGenerateMips;
                this.enableRandomWrite = input.enableRandomWrite;
        }

            public bool useMipMap;
            public bool sRGB;
            public uint memoryless;
            public uint vrUsage;
            public uint shadowSamplingMode;
            public int dimension;
            public int depthBufferBits;
            public uint colorFormat;
            public bool bindMS;
            public int volumeDepth;
            public int msaaSamples;
            public int height;
            public int width;
            public bool autoGenerateMips;
            public bool enableRandomWrite;

            public static implicit operator RenderTextureDescriptor(SerializableRenderTextureDescriptor serialized)
            {
                if (serialized == null)
                    return new RenderTextureDescriptor();

                return new RenderTextureDescriptor()
                {
                    useMipMap = serialized.useMipMap,
                    sRGB = serialized.sRGB,
                    memoryless = (RenderTextureMemoryless)serialized.memoryless,
                    vrUsage = (VRTextureUsage)serialized.vrUsage,
                    shadowSamplingMode = (ShadowSamplingMode)serialized.shadowSamplingMode,
                    dimension = (TextureDimension)serialized.dimension,
                    depthBufferBits = serialized.depthBufferBits,
                    colorFormat = (RenderTextureFormat)serialized.colorFormat,
                    bindMS = serialized.bindMS,
                    volumeDepth = serialized.volumeDepth,
                    msaaSamples = serialized.msaaSamples,
                    height = serialized.height,
                    width = serialized.width,
                    autoGenerateMips = serialized.autoGenerateMips,
                    enableRandomWrite = serialized.enableRandomWrite,
                };
            }
            public static implicit operator SerializableRenderTextureDescriptor(RenderTextureDescriptor input)
            {
                return new SerializableRenderTextureDescriptor(input);
            }
        }
        //------------------------------------//

        //------------------Shader------------------//
        [TypeSerializer(typeof(Shader))]
        public static object SerializeShader(Shader input)
        {
            return new Reference(input);
        }
        [System.Serializable]
        public class SerializableShader : SerializableObject
        {
            public SerializableShader(Shader input) : base(input) { }

            public override string Extension => "shader";

            public static implicit operator Shader(SerializableShader serialized)
            {
                if (serialized == null)
                    return null;

                return Shader.Find(serialized.name);
            }
            public static implicit operator SerializableShader(Shader input)
            {
                return new SerializableShader(input);
            }
        }
        //------------------------------------//

        //------------------Material------------------//
        [TypeSerializer(typeof(Material))]
        public static object SerializeMaterial(Material input)
        {
            return new Reference(input);
        }
        [System.Serializable]
        public class SerializableMaterial : SerializableObject
        {
            public SerializableMaterial(Material input) : base(input)
            {
                if (input == null)
                    return;

                if (input.name == null)
                    return;

                this.shader = new Reference(input.shader);
                this.globalIlluminationFlags = (uint)input.globalIlluminationFlags;
                this.shaderKeywords = input.shaderKeywords;
                this.renderQueue = input.renderQueue;
                this.mainTexture = input.mainTexture;
                this.mainTextureScale = input.mainTextureScale;
                this.mainTextureOffset = input.mainTextureOffset;
                this.enableInstancing = input.enableInstancing;
                this.doubleSidedGI = input.doubleSidedGI;
                this.color = input.color;
            }

            public override string Extension => "material";

            public Reference shader;
            public uint globalIlluminationFlags;
            public string[] shaderKeywords;
            public int renderQueue;
            public SerializableVector2 mainTextureScale;
            public SerializableVector2 mainTextureOffset;
            public SerializableTexture mainTexture;
            public SerializableColor color;
            public bool enableInstancing;
            public bool doubleSidedGI;
            
            public static implicit operator Material(SerializableMaterial serialized)
            {
                if (serialized == null)
                    return null;

                throw new System.NotImplementedException();

//#pragma warning disable
//                Material toReturn =  new Material(serialized.shader);
//#pragma warning restore
                
//                toReturn.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)serialized.globalIlluminationFlags;
//                toReturn.shaderKeywords = serialized.shaderKeywords;
//                toReturn.renderQueue = serialized.renderQueue;
//                toReturn.mainTexture = serialized.mainTexture;
//                toReturn.mainTextureOffset = serialized.mainTextureOffset;
//                toReturn.mainTextureScale = serialized.mainTextureScale;
//                toReturn.enableInstancing = serialized.enableInstancing;
//                toReturn.doubleSidedGI = serialized.doubleSidedGI;
//                toReturn.color = serialized.color;

//                return toReturn;
            }
            public static implicit operator SerializableMaterial(Material input)
            {
                return new SerializableMaterial(input);
            }
        }
        //------------------------------------//

        //------------------Quaternion------------------//
        [TypeSerializer(typeof(Quaternion))]
        public static object SerializeQuaternion(Quaternion input)
        {
            return new SerializableQuaternion(input);
        }
        [System.Serializable]
        public class SerializableQuaternion
        {
            public SerializableQuaternion(Quaternion input)
            {
                if (input == null)
                    return;

                this.x = input.x;
                this.y = input.y;
                this.z = input.z;
                this.w = input.w;
            }

            public float x;
            public float y;
            public float z;
            public float w;

            public static implicit operator Quaternion(SerializableQuaternion serialized)
            {
                if (serialized == null)
                    return Quaternion.identity;

                return new Quaternion(serialized.x, serialized.y, serialized.z, serialized.w);
            }
            public static implicit operator SerializableQuaternion(Quaternion input)
            {
                return new SerializableQuaternion(input);
            }
        }
        //------------------------------------//
        
        //------------------PhysicMaterial------------------//
        [TypeSerializer(typeof(PhysicMaterial))]
        public static object SerializeTYPE(PhysicMaterial input)
        {
            return new Reference(input);
        }
        [System.Serializable]
        public class SerializablePhysicMaterial : SerializableObject
        {
            public SerializablePhysicMaterial(PhysicMaterial input) : base(input)
            {
                if (input == null)
                    return;

                dynamicFriction = input.dynamicFriction;
                staticFriction = input.staticFriction;
                bounciness = input.bounciness;
                frictionCombine = (int)input.frictionCombine;
                bounceCombine = (int)input.bounceCombine;
            }

            public override string Extension => "physicsMaterial";

            public float dynamicFriction;
            public float staticFriction;
            public float bounciness;
            public int frictionCombine;
            public int bounceCombine;
            
            public static implicit operator PhysicMaterial(SerializablePhysicMaterial serialized)
            {
                if (serialized == null)
                    return null;

                return new PhysicMaterial()
                {
                    dynamicFriction = serialized.dynamicFriction,
                    staticFriction = serialized.staticFriction,
                    bounciness = serialized.bounciness,
                    frictionCombine = (PhysicMaterialCombine)serialized.frictionCombine,
                    bounceCombine = (PhysicMaterialCombine)serialized.bounceCombine,
                };
            }
            public static implicit operator SerializablePhysicMaterial(PhysicMaterial input)
            {
                return new SerializablePhysicMaterial(input);
            }
        }
        //------------------------------------//

        //------------------Rect------------------//
        [TypeSerializer(typeof(Rect))]
        public static object SerializeRect(Rect input)
        {
            return new SerializableRect(input);
        }
        [System.Serializable]
        public class SerializableRect
        {
            public SerializableRect(Rect input)
            {
                _position = input.position;
                _size = input.size;
            }

            private SerializableVector2 _position;
            private SerializableVector2 _size;

            public static implicit operator Rect(SerializableRect serialized)
            {
                return new Rect(serialized._position, serialized._size);
            }
            public static implicit operator SerializableRect(Rect input)
            {
                return new SerializableRect(input);
            }
        }
        //------------------------------------//

        //------------------Sprite------------------//
        [TypeSerializer(typeof(Sprite))]
        public static object SerializeSprite(Sprite input)
        {
            return new Reference(input);
        }
        [System.Serializable]
        public class SerializableSprite : SerializableObject
        {
            public SerializableSprite(Sprite input) : base(input)
            {
                if (input == null)
                    return;

                _texture = new Reference(input.texture);
                _rect = input.rect;
                _pivot = input.pivot;
                _pixelsPerUnit = input.pixelsPerUnit;
                _border = input.border;
            }

            public override string Extension => "sprite";

            public Reference _texture;
            public SerializableRect _rect;
            public SerializableVector2 _pivot;
            public float _pixelsPerUnit;
            public SerializableVector4 _border;

            public static implicit operator Sprite(SerializableSprite serialized)
            {
                throw new System.NotImplementedException();
                //return Sprite.Create(serialized._texture, serialized._rect, serialized._pivot, serialized._pixelsPerUnit, 0, SpriteMeshType.FullRect, serialized._border);
            }
            public static implicit operator SerializableSprite(Sprite input)
            {
                return new SerializableSprite(input);
            }
        }
        //------------------------------------//
    }
}
