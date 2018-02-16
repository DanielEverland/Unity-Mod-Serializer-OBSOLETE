using UMS.Behaviour;
using UMS.Serialization;

namespace UMS.Core
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
    }
}