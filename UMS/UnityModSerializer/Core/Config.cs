﻿using System.Collections.Generic;

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

        [System.Serializable]
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