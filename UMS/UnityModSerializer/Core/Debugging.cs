using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;

namespace UMS.Core
{
    public static class Debugging
    {
        public const TraceLevel DEFAULT_TRACE_LEVEL = TraceLevel.Off;

        public class EG_TraceLogger : ITraceWriter
        {
            public TraceLevel LevelFilter => DEFAULT_TRACE_LEVEL;

            public void Trace(TraceLevel level, string message, Exception ex)
            {
                Action<string> logger = GetLogger(level);

                logger(message);

                if (ex != null)
                    throw ex;
            }

            private Action<string> GetLogger(TraceLevel level)
            {
                switch (level)
                {
                    case TraceLevel.Error:
                        return UnityEngine.Debug.LogError;

                    case TraceLevel.Warning:
                        return UnityEngine.Debug.LogWarning;

                    default:
                        return UnityEngine.Debug.Log;
                }
            }
        }
    }
}