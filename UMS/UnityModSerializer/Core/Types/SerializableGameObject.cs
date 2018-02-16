using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

                Debugging.Info("Serializing component " + comp + " of " + obj);

                _components.Add(Reference.Create(comp));
            }

            _children = new List<Reference>();
            foreach (Transform child in obj.transform)
            {
                Debugging.Info("Serializing child " + child + " of " + obj);

                _children.Add(Reference.Create(child.gameObject));
            }
        }

        public override string Extension => "gameObject";

        public IList<Reference> Components { get { return _components; } }
        public IList<Reference> Children { get { return _children; } }

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
        [JsonProperty]
        private List<Reference> _children;

        public override GameObject Deserialize(SerializableGameObject serialized)
        {
            GameObject gameObject = new GameObject(serialized.Name);

            serialized.Deserialize(gameObject);

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

            foreach (Reference child in serialized.Children)
            {
                Deserializer.GetDeserializedObject<GameObject>(child.ID, instance =>
                {
                    instance.transform.SetParent(gameObject.transform);
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