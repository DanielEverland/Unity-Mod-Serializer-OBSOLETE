using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UMS.Core
{
    [System.Serializable]
    public class Config
    {
        public void Add(int id, string localPath)
        {
            data.Add(new Data(id, localPath));
        }
        
        public List<Data> data = new List<Data>();

        public class Data
        {
            public Data() { }
            public Data(int id, string localPath)
            {
                this.id = id;
                this.localPath = localPath;
            }

            public int id;
            public string localPath;
        }
    }
}
