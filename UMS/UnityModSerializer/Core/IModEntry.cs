using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Core
{
    public interface IModEntry
    {
        string Extension { get; }
        string FileName { get; }
    }
}
