using Newtonsoft.Json;
using UMS.Runtime.Behaviour;
using UMS.Runtime.Types;
using UMS.Deserialization;

namespace UMS.Runtime.Core
{
    public static class CoreManager
    {
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
            HierarchyManager.Initialize();

            HookUpJSONReferenceHandler();
            OnSerializationStarted?.Invoke();

            AssemblyManager.Initialize();

            IDManager.Initialize();
            ObjectManager.Initialize();
            CustomSerializers.Initialize();
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