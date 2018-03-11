using System.Collections.Generic;

namespace UMS.Runtime.Core
{
    [System.Serializable]
    public class Manifest
    {
        public static Manifest Instance { get; set; }

        public void Add(int id, string localPath, string key)
        {
            data.Add(new Data(id, localPath, key));
        }

        public List<Data> data = new List<Data>();

        [System.Serializable]
        public class Data
        {
            public Data() { }
            public Data(int id, string localPath, string key)
            {
                this.id = id;
                this.localPath = localPath;
                this.key = key;
            }

            public int id;
            public string localPath;
            public string key;
        }
    }
}