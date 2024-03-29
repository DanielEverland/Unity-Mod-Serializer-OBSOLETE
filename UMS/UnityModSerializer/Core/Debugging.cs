﻿using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;

namespace UMS.Core
{
    public static class Debugging
    {
#if DEBUG
        public const TraceLevel DEFAULT_TRACE_LEVEL = TraceLevel.Verbose;
#else
        public const TraceLevel DEFAULT_TRACE_LEVEL = TraceLevel.Warning;
#endif
        public const bool DEBUG_JSON_INTERNAL = true;

        private static EG_TraceLogger Logger
        {
            get
            {
                if (_logger == null)
                    _logger = new EG_TraceLogger();

                return _logger;
            }
        }
        private static EG_TraceLogger _logger;

        public static void Info(object msg)
        {
            Add(msg, TraceLevel.Info);
        }
        public static void Warning(object msg)
        {
            Add(msg, TraceLevel.Warning);
        }
        public static void Error(object msg)
        {
            Add(msg, TraceLevel.Error);
        }
        public static void Verbose(object msg)
        {
            Add(msg, TraceLevel.Verbose);
        }
        public static void Add(object msg, TraceLevel level)
        {
            if (Logger.IsEnabled(level))
                Logger.Trace(level, msg.ToString(), null);
        }

        public class EG_TraceLogger : ITraceWriter
        {
            public EG_TraceLogger(bool isJsonInternal = false)
            {
                _isJsonInternal = isJsonInternal;
            }

            public TraceLevel LevelFilter => DEFAULT_TRACE_LEVEL;

            private readonly bool _isJsonInternal;

            public void Trace(TraceLevel level, string message, Exception ex)
            {
                if (_isJsonInternal && !DEBUG_JSON_INTERNAL)
                    return;

                Action<string> logger = GetLogger(level);

                logger(message);

                if (ex != null)
                    throw ex;
            }
            public bool IsEnabled(TraceLevel level)
            {
                return (int)level <= (int)LevelFilter;
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