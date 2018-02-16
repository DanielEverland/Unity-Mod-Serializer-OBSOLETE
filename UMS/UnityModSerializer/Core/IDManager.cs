using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UMS.Serialization;
using UMS.Behaviour;

namespace UMS.Core
{
    public class IDManager
    {
        private static CustomIDGeneratorManager IDGeneratorManager
        {
            get
            {
                if (_customIDGeneratorManager == null)
                    _customIDGeneratorManager = new CustomIDGeneratorManager();

                return _customIDGeneratorManager;
            }
        }
        private static CustomIDGeneratorManager _customIDGeneratorManager;

        private static Dictionary<int, int> _objectLookup;
        private static HashSet<int> _blockedIDs;

        public static void Initialize()
        {
            _objectLookup = new Dictionary<int, int>();
            _blockedIDs = new HashSet<int>();
        }
        public static int GetID(object obj)
        {
            if (_blockedIDs.Contains(obj.GetHashCode()))
            {
                Debugging.Warning("Object " + obj + " has been blocked. Returning -1");
                return -1;
            }

            return GenerateID(obj, true, null);
        }
        private static int GetCustomID(object obj)
        {
            return IDGeneratorManager.GetID(obj);
        }
        private static bool CustomGeneratorExists(Type type)
        {
            return IDGeneratorManager.ContainsGenerator(type);
        }
        private static void CacheID(object obj, int id)
        {
            int hash = obj.GetHashCode();

            if (_objectLookup.ContainsKey(hash))
            {
                if (_objectLookup[hash] != id)
                {
                    FlagImproperID(obj, id, _objectLookup[hash]);
                }
            }
            else
            {
                _objectLookup.Add(hash, id);
            }
        }
        private static void FlagImproperID(object obj, int a, int b)
        {
            UnityEngine.Debug.LogError(string.Format("Found improper implementation of ID for {0} - {1}|{2}", obj, a, b));

            _blockedIDs.Add(obj.GetHashCode());

            IDGeneratorAnalyzer analyzer = new IDGeneratorAnalyzer(obj);

            analyzer.RunTest();
        }
        private static int GenerateID(object obj, bool doCache, Action<MemberInfo, int> memberValues)
        {
            if (obj == null)
                return -1;

            int id = GetID(obj, memberValues);

            if (doCache)
                CacheID(obj, id);

            return id;
        }
        private static int GetID(object obj, Action<MemberInfo, int> memberValues)
        {
            if (CustomGeneratorExists(obj.GetType()))
            {
                Debugging.Verbose("Using custom ID generator for " + obj.GetType());

                return GetCustomID(obj);
            }
            else
            {
                return GenerateNewID(obj, memberValues);
            }
        }
        private static int GenerateNewID(object obj, Action<MemberInfo, int> memberValues)
        {
            unchecked
            {
                int i = 17;
                int buffer = -1;

                foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if(GetPropertyHash(property, obj, memberValues, ref buffer))
                    {
                        i += buffer * 23;
                    }
                }
                foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (GetFieldHash(field, obj, memberValues, ref buffer))
                    {
                        i += buffer * 61;
                    }
                }

                return i;
            }
        }
        private static bool GetFieldHash(FieldInfo field, object obj, Action<MemberInfo, int> memberValues, ref int i)
        {
            if (BlockedTypes.IsBlocked(Utility.GetObjectMemberName(field)))
                return false;

            if (BlockedTypes.IsBlocked(field.FieldType))
                return false;

            try
            {
                if (field.FieldType.IsArray)
                {
                    i = GetArrayID(field.GetValue(obj));
                }
                else
                {
                    i = GetObjectID(field.GetValue(obj));
                }
            }
            catch (TargetException)
            {
                throw;
            }
            catch (Exception)
            {
                return false;
            }

            memberValues?.Invoke(field, i);

            return true;
        }
        private static bool GetPropertyHash(PropertyInfo property, object obj, Action<MemberInfo, int> memberValues, ref int i)
        {
            if (BlockedTypes.IsBlocked(Utility.GetObjectMemberName(property)))
                return false;

            if (BlockedTypes.IsBlocked(property.PropertyType))
                return false;

            MethodInfo method = property.GetGetMethod();

            if (method == null)
                return false;

            if (!Utility.CanAccessMember(property))
                return false;

            try
            {
                if (property.PropertyType.IsArray)
                {
                    i = GetArrayID(property.GetValue(obj, null));
                }
                else
                {
                    i = GetObjectID(property.GetValue(obj, null));
                }
            }
            catch (TargetException)
            {
                throw;
            }
            catch (Exception)
            {
                return false;
            }

            memberValues?.Invoke(property, i);

            return true;
        }
        private static int GetArrayID(object obj)
        {
            int i = 0;

            unchecked
            {
                foreach (object item in obj as System.Collections.IEnumerable)
                {
                    if (item == null)
                        continue;

                    i += item.GetHashCode() * 23;
                }
            }

            return i;
        }
        private static int GetObjectID(object obj)
        {
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }
        private class CustomIDGeneratorManager
        {
            public CustomIDGeneratorManager()
            {
                _generators = new Dictionary<Type, CustomIDGenerator>();

                if (BehaviourManager.HasInitialized)
                {
                    GetGenerators();
                }
                else
                {
                    BehaviourManager.OnFinishedInitializing += GetGenerators;
                }
            }

            private Dictionary<Type, CustomIDGenerator> _generators;

            public bool ContainsGenerator(Type type)
            {
                return _generators.ContainsKey(type);
            }
            public int GetID(object obj)
            {
                return _generators[obj.GetType()].GetID(obj);
            }
            private void GetGenerators()
            {
                foreach (CustomIDGenerator generator in BehaviourManager.GetBehaviours<CustomIDGenerator>())
                {
                    _generators.Add(generator.Type, generator);
                }
            }
        }
        private class IDGeneratorAnalyzer
        {
            private IDGeneratorAnalyzer() { }
            public IDGeneratorAnalyzer(object obj)
            {
                this._obj = obj;
            }

            private readonly object _obj;
            private Dictionary<MemberInfo, int> _values = new Dictionary<MemberInfo, int>();
            private HashSet<MemberInfo> _blacklistedMembers = new HashSet<MemberInfo>();
            private List<Descrepency> _descrepencies = new List<Descrepency>();

            private const int TEST_AMOUNT = 5;

            public void RunTest()
            {
                UnityEngine.Debug.Log("----------Running ID generator test on " + _obj + "----------");

                for (int i = 0; i < TEST_AMOUNT; i++)
                {
                    GenerateID(_obj, false, Receive);
                }

                OutputResults();

                UnityEngine.Debug.Log("------------------------------End of test------------------------------");
            }
            private void OutputResults()
            {
                if (_descrepencies.Count == 0)
                {
                    UnityEngine.Debug.LogWarning("Couldn't find any descrepencies");
                }
                else
                {
                    UnityEngine.Debug.Log("Found " + _descrepencies.Count + " descrepencies");
                    for (int i = 0; i < _descrepencies.Count; i++)
                    {
                        UnityEngine.Debug.Log(_descrepencies[i]);
                    }
                }
            }
            private void Receive(MemberInfo info, int hash)
            {
                if (_blacklistedMembers.Contains(info))
                    return;

                if (_values.ContainsKey(info))
                {
                    if (_values[info] != hash)
                    {
                        _blacklistedMembers.Add(info);
                        _descrepencies.Add(new Descrepency(info, hash, _values[info]));
                    }
                }
                else
                {
                    _values.Add(info, hash);
                }
            }
            private struct Descrepency
            {
                public Descrepency(MemberInfo member, int a, int b)
                {
                    _member = member;
                    _a = a;
                    _b = b;
                }

                public MemberInfo _member;
                public int _a;
                public int _b;

                public override string ToString()
                {
                    return string.Format("{0} - {1}|{2}", Utility.GetObjectMemberName(_member.DeclaringType.Name, _member.Name), _a, _b);
                }
            }
        }
    }
}