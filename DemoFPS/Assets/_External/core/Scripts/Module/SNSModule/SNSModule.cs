using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine.Events;

class SNSModule:MonoBehaviour
{
    private static SNSModule s_instance=null;
#if UNITY_IOS
    [DllImport("__Internal")]
	private static extern void SNSModule_Share(int snsType,string message, string url,string pathImage);
#endif
#if !UNITY_EDITOR
#if UNITY_IOS
	//Banner
	public static void Share(SNSType snsType,string message, string url,string pathImage, ShareSNSDelegate fCallBackShare = null)
    {
        sCallBackShare = fCallBackShare;
		s_instance=BaseUtils.RegisterListener<SNSModule>("SNSListener");
		SNSModule_Share((int)snsType,message,url,pathImage);
    }
#elif UNITY_ANDROID
    private const string CLASS_NAME = "jp.onetech.core.SNSModule";
    private static AndroidJavaObject jo = null;
    private static AndroidJavaObject GetJO()
    {
        if (jo == null)
            jo = new AndroidJavaObject(CLASS_NAME);
        return jo;
    }
    public static void Share(SNSType snsType, string message, string url, string pathImage, ShareSNSDelegate fCallBackShare = null)
    {
         s_instance = BaseUtils.RegisterListener<SNSModule>("SNSListener");
        if(s_instance!=null)
        {
             s_instance.StartCoroutine(coroutineShare(snsType, message, url, pathImage, fCallBackShare));
        }
    }
     public static IEnumerator coroutineShare(SNSType snsType, string message, string url, string pathImage, ShareSNSDelegate fCallBackShare = null)
    {
        string pathImageAbsolute = Application.persistentDataPath + "/image_share.jpg";
        WWW www = new WWW(pathImage);
        yield return www;
        if (www.error == null)
        {
            File.WriteAllBytes(pathImageAbsolute, www.bytes);
        }

        sCallBackShare = fCallBackShare;
       

        string functionName;
        switch (snsType)
        {
            case SNSType.SNS_Facebook:
                {
                    functionName = "shareImageToFacebook";
                }
                break;
            case SNSType.SNS_Twitter:
                {
                    functionName = "shareImageToTwiter";
                }
                break;
            case SNSType.SNS_Line:
                {
                    functionName = "shareImageToLine";
                }
                break;
            case SNSType.SNS_Instagram:
                {
                    functionName = "shareImageToInstagram";
                }
                break;
            default:
                functionName = "";
                break;
        }
        GetJO().CallStatic(functionName, message + "\n" + url, pathImageAbsolute);
    }
#endif
#else
    public static void Share(SNSType snsType, string message, string url, string pathImage, ShareSNSDelegate fCallBackShare = null)
    {
        s_instance = BaseUtils.RegisterListener<SNSModule>("SNSListener");
        if (s_instance != null)
        {
            s_instance.StartCoroutine(coroutineShare(snsType, message, url, pathImage, fCallBackShare));
        }
    }
    public static IEnumerator coroutineShare(SNSType snsType, string message, string url, string pathImage, ShareSNSDelegate fCallBackShare = null)
    {
        string pathImageAbsolute = Application.persistentDataPath + "/image_share.jpg";
        WWW www = new WWW(pathImage);
        yield return www;
        if (www.error == null)
        {
            File.WriteAllBytes(pathImageAbsolute, www.bytes);
        }
        Debug.Log("Image shared:"+pathImageAbsolute);
    }
#endif
    public enum SNSType{
		SNS_Facebook = 0,
		SNS_Twitter=1,
        SNS_Line=2,
        SNS_Instagram=3
	};
    public delegate void ShareSNSDelegate(SNSType snsType, bool isSuccess);
    static ShareSNSDelegate sCallBackShare=null;
    public void OnShared(string result)
    {
#if UNITY_IOS
        string[]results=BaseUtils.split(result, ',');
        OnShared((SNSType)BaseUtils.atoi(results[0]), bool.Parse(results[1]));
#elif UNITY_ANDROID

#endif
    }
	private void OnShared(SNSType snsType, bool isSuccess)
	{
		if (sCallBackShare != null)
			sCallBackShare(snsType, isSuccess);
	}
};
