using System;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace ToothlessDebugger.RunTime
{
    public struct CustomDebugData
    {
        public string time;
        public string message;
        public StackTrace stackTrace;
        public DebugType debugType;
        
        public CustomDebugData(string time, string message, DebugType type)
        {
            this.time = time;
            this.message = message;
            stackTrace = new StackTrace(1,true);
            debugType = type;
        }
    }
    public static class CustomDebug
    {
        static CustomDebug()
        {
            //Debug.Log("dd");
        }
        
        public static List<CustomDebugData> debugData = new List<CustomDebugData>();
        public static Action<CustomDebugData> OnAddDebug;
        
        public static void Log(string message,DebugType debugType = DebugType.Default)
        { 
#if UNITY_EDITOR
            //콘솔 찍기
            Debug.Log(message);
            var log = new CustomDebugData(DateTime.Now.ToString("HH:mm:ss"), message, debugType);
            debugData.Add(log);
            OnAddDebug?.Invoke(log);
#else
            return;
#endif
        }
    }
}