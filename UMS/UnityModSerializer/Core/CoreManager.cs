using UMS.Behaviour;
using UMS.Serialization;

namespace UMS.Core
{
    public static class CoreManager
    {
        public static void Initialize()
        {
            AssemblyManager.Initialize();

            IDManager.Initialize();
            ObjectManager.Initialize();
            CustomSerializers.Initialize();
            BlockedTypes.Initialize();

            BehaviourManager.Initialize();
            AssemblyManager.ExecuteReflection();
        }
    }
}