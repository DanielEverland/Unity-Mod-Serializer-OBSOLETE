using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UMS.Serialization;

namespace UMS.Core
{
    public class IDManager
    {
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
                UnityEngine.Debug.LogWarning("Object " + obj + " has been blocked. Returning -1");
                return -1;
            }                

            return GenerateID(obj, true, null);
        }
        private static void CacheID(object obj, int id)
        {
            int hash = obj.GetHashCode();

            if (_objectLookup.ContainsKey(hash))
            {
                if(_objectLookup[hash] != id)
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

            unchecked
            {
                int i = 17;

                foreach (PropertyInfo info in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (BlockedTypes.IsBlocked(Utility.GetObjectMemberName(info.DeclaringType.Name, info.Name)))
                        continue;

                    if (BlockedTypes.IsBlocked(info.PropertyType))
                        continue;

                    MethodInfo method = info.GetGetMethod();

                    if (method == null)
                        continue;

                    if (!Utility.CanAccessMember(info))
                        continue;

                    try
                    {
                        int hash = 0;
                        
                        if (info.PropertyType.IsArray)
                        {
                            hash = GetArrayID(info.GetValue(obj, null));
                        }
                        else
                        {
                            hash = GetObjectID(info.GetValue(obj, null));
                        }
                        
                        memberValues?.Invoke(info, hash);

                        i = i + hash * 13;
                    }
                    catch (System.Exception)
                    {
                    }
                }

                if(doCache)
                    CacheID(obj, i);

                return i;
            }
        }
        private static int GetArrayID(object obj)
        {
            int i = 0;

            unchecked
            {
                foreach (object item in obj as System.Collections.IEnumerable)
                {
                    i += item.GetHashCode() * 23;
                }
            }

            return i;
        }
        private static int GetObjectID(object obj)
        {
            return obj.GetHashCode();
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
                if(_descrepencies.Count == 0)
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
                    if(_values[info] != hash)
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
