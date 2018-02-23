using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UMS.Core.Types;

namespace UMS.Deserialization
{
    /// <summary>
    /// Used for deserializing components on a gameobject by ordering them based on dependencies, which are defined on components using the RequireComponent attribute
    /// </summary>
    public class ComponentCacheDeserializer
    {
        private ComponentCacheDeserializer() { }
        public ComponentCacheDeserializer(SerializableGameObject serialized, GameObject targetObject)
        {
            _components = new List<SerializableComponent>();
            _targetObject = targetObject;
            _serializedGameObject = serialized;
            
            foreach (Reference reference in serialized.Components)
            {
                if (!Deserializer.ContainsObject(reference.ID))
                    continue;

                _targetComponentCount++;

                Deserializer.GetSerializedObject<SerializableComponent>(reference.ID, serializableComponent =>
                {
                    ReceiveComponent(serializableComponent);
                });
            }

            _finishedInitializing = true;
        }

        private static Func<SerializableComponent, int> orderByDependencies = comp =>
        {
            return comp.ComponentType.GetCustomAttributes(true).Where(x => x is RequireComponent).Count();
        };

        private readonly GameObject _targetObject;
        private readonly SerializableGameObject _serializedGameObject;
        
        private List<SerializableComponent> _components;
        private bool _finishedInitializing;
        private int _targetComponentCount;

        private void ReceiveComponent(SerializableComponent component)
        {
            _components.Add(component);

            if (_components.Count == _targetComponentCount && _finishedInitializing)
                ExecuteDeserialization();
        }
        private void ExecuteDeserialization()
        {
            if (_components.Count != _targetComponentCount || !_finishedInitializing)
                throw new ArgumentException();

            foreach (SerializableComponent serializableComponent in _components.OrderBy(x => orderByDependencies(x)))
            {
                Component component = _serializedGameObject.GetComponent(serializableComponent.ComponentType, _targetObject);
                serializableComponent.Deserialize(component);
            }
        }
    }
}
