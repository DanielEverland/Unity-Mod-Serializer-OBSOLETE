﻿using Newtonsoft.Json;
using UMS.Behaviour;
using UMS.Types;
using UMS.Deserialization;

namespace UMS.Core
{
    public static class CoreManager
    {
        public static bool HasInitialized { get; private set; }

        /// <summary>
        /// This is called when serialization or deserialization is initiated
        /// </summary>
        public static event System.Action OnSerializationStarted;

        /// <summary>
        /// This is called when serialization or deserialization is complete
        /// </summary>
        public static event System.Action OnSerializationCompleted;

        public static void Initialize()
        {
            if (HasInitialized)
                return;

            HasInitialized = true;

            HierarchyManager.Initialize();

            HookUpJSONReferenceHandler();
            OnSerializationStarted?.Invoke();

            AssemblyManager.Initialize();

            IDManager.Initialize();
            Converter.Initialize();
            BlockedTypes.Initialize();

            BehaviourManager.Initialize();
            AssemblyManager.ExecuteReflection();
        }
        public static void FinishedSerialization()
        {
            OnSerializationCompleted?.Invoke();
        }
        private static void HookUpJSONReferenceHandler()
        {
            ReferenceHandler.OnCreateReference = x =>
            {
                return Reference.Create(x);
            };

            ReferenceHandler.IsTypeReference = x =>
            {
                return x == typeof(Reference);
            };

            ReferenceHandler.AssignObjectValue = (value, type, callback) =>
            {
                Reference reference = (Reference)value;

                Deserializer.GetDeserializedObject(reference.ID, type, x =>
                {
                    callback(x);
                });
            };
        }
    }
}