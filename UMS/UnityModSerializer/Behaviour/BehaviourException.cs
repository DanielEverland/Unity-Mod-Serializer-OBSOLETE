using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Behaviour
{
    public class BehaviourException : Exception
    {
        public BehaviourException() : base()
        {
        }
        public BehaviourException(string message) : base(message)
        {
        }
        public BehaviourException(string messsage, Exception innerException) : base(messsage, innerException)
        {
        }

        public static BehaviourException Generate(BehaviourBase behaviourBase, string message)
        {
            return new BehaviourException(string.Format("Detected an improper implementation of {0}. {1}", behaviourBase.GetType().Name, message));
        }
    }
}
