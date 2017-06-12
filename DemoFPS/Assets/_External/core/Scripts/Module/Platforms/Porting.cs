using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
namespace external
{
    public class Porting
    {
		//=== Device
        public static string GetDeviceId()
        {
#if UNITY_EDITOR
            return "m010001";
#else
            return  BaseUtils.getDeviceId();
#endif
        }
        public static string GetDefaultUserAgent()
        {
            return "";
        }
        //=== App
        public static string GetAppVersion()
        {
#if UNITY_EDITOR
            return Application.version;
#else
#if UNITY_IOS
            return BaseUtils.BaseUtils_getAppVersion();;
#else
            return Application.version;
#endif
#endif

        }
        public static string GetWritablePath()
        {
            return Application.persistentDataPath;
        }

    }
}