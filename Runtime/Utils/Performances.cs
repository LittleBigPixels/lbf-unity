using System;
using UnityEngine;

namespace LBF.Utils
{
    public class Performances
    {
        public static void RunAndLog(String key, Action action)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            Debug.LogFormat("{0}: {1} ms", key, stopwatch.ElapsedMilliseconds.ToString());
        }
        
        public static T RunAndLog<T>(String key, Func<T> func)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var result = func();
            stopwatch.Stop();
            Debug.LogFormat("{0}: {1} ms", key, stopwatch.ElapsedMilliseconds.ToString());

            return result;
        }
    }
}