using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class BaseBuildChecker : MonoBehaviour
    {
        public static string ErrorStr;
        public static string InfoStr;
        public virtual bool Check()
        {
            return false;
        }
        public virtual void Dispose()
        {

        }
        public static void ClearLog()
        {
            Debug.ClearDeveloperConsole();
            ErrorStr = "";
            InfoStr = "";
        }
        public void AddErrorLog(string str)
        {
            CLog.Error(str);
            ErrorStr += str + "\n";
        }
        public void AddInfo(string str)
        {
            CLog.Info(str);
            InfoStr += str + "\n";
        }
    }

}