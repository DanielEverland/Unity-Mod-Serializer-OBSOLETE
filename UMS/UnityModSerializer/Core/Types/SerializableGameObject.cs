using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMS.Deserialization;
using UnityEngine;

namespace UMS.Core.Types
{
    public class SerializableGameObject : SerializableObject<GameObject, SerializableGameObject>
    {
        public SerializableGameObject() { }
        public SerializableGameObject(GameObject obj) : base(obj)
        {
            _activeSelf = obj.activeSelf;
            _layer = obj.layer;
            _isStatic = obj.isStatic;
            _tag = obj.tag;

            _components = new List<Reference>();
            foreach (Component comp in obj.GetComponents<Component>())
            {
                if (comp == null)
                    continue;

                _components.Add(Reference.Create(comp));
            }
        }

        public override string Extension => "gameObject";

        public IList<Reference> Components { get { return _components; } }
        
        [JsonProperty]
        public bool _activeSelf;
        [JsonProperty]
        public int _layer;
        [JsonProperty]
        public bool _isStatic;
        [JsonProperty]
        public string _tag;
        [JsonProperty]
        private List<Reference> _components;

        public override GameObject Deserialize(SerializableGameObject serialized)
        {
            GameObject gameObject = new GameObject(serialized.Name);

            gameObject.SetActive(serialized._activeSelf);
            gameObject.layer = serialized._layer;
            gameObject.isStatic = serialized._isStatic;
            gameObject.tag = serialized._tag;

            foreach (Reference reference in serialized.Components)
            {
                if (!Deserializer.ContainsObject(reference.ID))
                    continue;

                Deserializer.GetSerializedObject<SerializableComponent>(reference.ID, serializableComponent =>
                {
                    Component component = GetComponent(serializableComponent.ComponentType, gameObject);
                    serializableComponent.Deserialize(component);
                });
            }

            return gameObject;
        }
        private Component GetComponent(Type type, GameObject obj)
        {
            if (type == typeof(Transform))
            {
                return obj.GetComponent<Transform>();
            }
            else
            {
                return obj.AddComponent(type);
            }
        }
        public override SerializableGameObject Serialize(GameObject obj)
        {
            return new SerializableGameObject(obj);
        }
    }
}
