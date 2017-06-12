using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;
using UnityEngine.Events;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using UnityEngine.EventSystems;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Runtime.InteropServices;

public class BaseUtils {
#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern float BaseUtils_getDeviceDensity();
    [DllImport("__Internal")]
	private static extern float BaseUtils_savePhotoToAlbum(string imagePath);
    [DllImport("__Internal")]
	public static extern string BaseUtils_getAppVersion();
	[DllImport("__Internal")]
	public static extern string BaseUtils_setStatusBarHidden(bool isHide);
    [DllImport("__Internal")]
    private static extern void BaseUtils_showUIView(int r, int g, int b, int a);
    [DllImport("__Internal")]
    private static extern void BaseUtils_hideUIView();
#elif UNITY_ANDROID
    private const string CLASS_NAME = "jp.onetech.core.utils.BaseUtils";
    private static AndroidJavaObject sJO = null;
    private static AndroidJavaObject getJO()
    {
        if (sJO == null)
        {
            sJO = new AndroidJavaObject(CLASS_NAME);
        }
        return sJO;
    }
#endif
    private static string s_deviceId="";
    public static string getDeviceId()
    {
        if (string.IsNullOrEmpty(s_deviceId))
        {
#if UNITY_EDITOR
            s_deviceId = SystemInfo.deviceUniqueIdentifier;
#else
#if UNITY_IOS
            s_deviceId= SystemInfo.deviceUniqueIdentifier;
#else
            s_deviceId = getJO().CallStatic<string>("getDeviceId");
#endif
#endif
        }
        return s_deviceId;
    }
    public static string[] split(string text, char delim)
    {
        return text.Split(delim);
    }
    public static int atoi(string text)
    {
        int result = 0;
        Int32.TryParse(text, out result);
        return result;
    }
    public static float atof(string text)
    {
        float result = 0;
        Single.TryParse(text, out result);
        return result;
    }
    public static int Json_GetInt(JSONNode json, string name, int defaultValue = 0)
    {
        JSONNode data = json[name];
        if (data != null)
            return data.AsInt;
        return defaultValue;
    }

    public static bool Json_GetBool(JSONNode json, string name, bool defaultValue = false)
    {
        JSONNode data = json[name];
        if (data != null)
            return data.AsInt == 0 ? false : true;
        return defaultValue;
    }

    public static float Json_GetFloat(JSONNode json, string name, float defaultValue = 0)
    {
        JSONNode data = json[name];
        if (data != null)
            return data.AsFloat;
        return defaultValue;
    }
    public static string Json_GetString(JSONNode json, string name, string defaultValue = "")
    {
        JSONNode data = json[name];
        if (data != null)
            return data.Value;
        return defaultValue;
    }
    public static string EncryptToSHA1(string text)
    {
        string result;
        SHA1 sha = new SHA1CryptoServiceProvider();
        result = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
        result = result.ToLower().Replace("-", "");
        return result;
    }
    public static IEnumerator RunAction(float delay, UnityAction fn)
    {
        yield return new WaitForSeconds(delay);
        fn();
    }

    // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
    // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
    private const string initVector = "@vietslbfow12345";
    private const string passPhrase = "@itgviet";
    // This constant is used to determine the keysize of the encryption algorithm
    private const int keysize = 256;
    //Encrypt
    public static string EncryptString(string plainText)
    {
        try {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);

        }
        catch (Exception e)
        {
            Debug.Log(e);
            return "";
        }

    }

#region Event
    //Event
    public static void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
        trigger.triggers.Add(entry);
    }
#endregion

    public static string DecryptString(string cipherText)
    {
        try
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
        catch (Exception e) {
            Debug.Log(e);
            return "";
        }
    }
    public static T RegisterListener<T>(string name) where T : Component
    {
        GameObject adListenerGo = GameObject.Find(name);
        if (!GameObject.Find(name))
        {
            adListenerGo = new GameObject();
            adListenerGo.name = name;
            return adListenerGo.AddComponent<T>();
        }
        else
        {
            return adListenerGo.GetComponent<T>();
        }

    }
    private static float sDensity = 0;
    public static float getDeviceDensity()
    {
        if (sDensity == 0) {
#if UNITY_EDITOR
            sDensity = 1;
#else
#if UNITY_IOS
			sDensity= BaseUtils_getDeviceDensity();
#elif UNITY_ANDROID
            sDensity = getJO().CallStatic<float>("getDeviceDensity");
#endif
#endif
        }
        return sDensity;
    }

    public static void CopyAndroidLib(string moduleName)
    {
#if UNITY_EDITOR && UNITY_ANDROID
        string pathLib = Path.GetFullPath("Assets/Plugins/Android/" + moduleName);
        CopyAndroidPartLib(pathLib, "libs");
        CopyAndroidPartLib(pathLib, "assets");
#endif
    }
    public static void CopyAndroidPartLib(string pathLibSrc, string folderName)
    {
#if UNITY_EDITOR && UNITY_ANDROID
        string pathPartLib = pathLibSrc + "/" + folderName;
        if (Directory.Exists(pathPartLib))
        {
            string pathLibRoot = Path.GetFullPath("Assets/Plugins/Android/") + folderName + "/";
            if (!Directory.Exists(pathLibRoot))
            {
                Directory.CreateDirectory(pathLibRoot);
            }
            foreach (string fileLib in Directory.GetFiles(pathPartLib, "*.*", SearchOption.AllDirectories))
            {
				if (Path.GetExtension(fileLib).Contains("meta"))
                    continue;
                string fileName = Path.GetFileName(fileLib);
                string pathAbsolute = fileLib.Substring(pathPartLib.Length, fileLib.Length - pathPartLib.Length - fileName.Length);

                string fileLibDes = pathLibRoot + pathAbsolute;
                if (!Directory.Exists(fileLibDes))
                {
                    Directory.CreateDirectory(fileLibDes);
                }
                fileLibDes += fileName;
                File.Copy(fileLib, fileLibDes, true);
            }
        }
#endif
    }


    public static void savePhotoToAlbum(string imagePath)
    {
#if UNITY_EDITOR
        external.NativeListener.getInstance().receiveMessage("BaseUtils.savePhotoToAlbum," + imagePath);
#else
#if UNITY_IOS
        BaseUtils_savePhotoToAlbum(imagePath);
#else
        if (File.Exists(imagePath))
        {
            string pathDes = getJO().CallStatic<string>("getMediaFolderDevicePath")+"/Pictures/"+Application.productName;
            if (!Directory.Exists(pathDes))
            {
                Directory.CreateDirectory(pathDes);
            }
            if (Directory.Exists(pathDes))
            {
                FileInfo fileInfo = new FileInfo(imagePath);
                pathDes +="/"+ fileInfo.Name; 
                File.Copy(imagePath, pathDes,true);
                getJO().CallStatic("savePhotoToAlbum",pathDes);
            }
        }
#endif
#endif
    }
        
	public static void SetStatusBarHidden (bool isHidden)
	{
        Screen.fullScreen = !isHidden;
#if !UNITY_EDITOR
#if UNITY_IOS
		BaseUtils_setStatusBarHidden(isHidden);
#elif UNITY_ANDROID
        getJO().CallStatic("setStatusBarHidden", isHidden);  
#endif
#endif
    }

    public static IEnumerator RotateScreenTo(ScreenOrientation toOrient, Color color, UnityAction callback)
    {
        if (IsSameOrientation(Screen.orientation, toOrient)) {
            Debug.Log ("[RotateScreenTo] orientation is the same");
            yield break;
        }

#if UNITY_IOS && !UNITY_EDITOR
        BaseUtils_showUIView ((int)color.r, (int)color.g, (int)color.b, (int)color.a);
        ScreenOrientation fromOrient = Screen.orientation;
        ScreenOrientation prev = Screen.orientation;
        for (int i = 0; i < 3; i++)
        {
            Screen.orientation = (prev == fromOrient ? toOrient : fromOrient);
            yield return new WaitWhile(() => {
                return prev == Screen.orientation;
            });
            prev = Screen.orientation; 
            yield return new WaitForSeconds(0.05f); //this is an arbitrary wait value -- it may need tweaking for different iPhones!
        }
        BaseUtils_hideUIView ();
#else
        Screen.orientation = toOrient;
        yield return new WaitForEndOfFrame();
#endif

        if (callback != null) {
            callback.Invoke ();
        }
    }

    public static bool IsSameOrientation(ScreenOrientation orient1, ScreenOrientation orient2) {
        if (orient1 == ScreenOrientation.Portrait || orient1 == ScreenOrientation.PortraitUpsideDown) {
            return orient2 == ScreenOrientation.Portrait || orient2 == ScreenOrientation.PortraitUpsideDown;
        }else if (orient1 == ScreenOrientation.Landscape || orient1 == ScreenOrientation.LandscapeLeft 
            || orient1 == ScreenOrientation.LandscapeRight) {
            return orient2 == ScreenOrientation.Landscape || orient2 == ScreenOrientation.LandscapeLeft
                || orient2 == ScreenOrientation.LandscapeRight;
        }

        return false;
    }

    public static Sprite loadSpriteFromPath(string path)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(path))
        {
            try
            {
                fileData = File.ReadAllBytes(path);
                tex = new Texture2D(2,2,TextureFormat.ARGB32,false);
                tex.LoadImage(fileData);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        return null;
    }
#if UNITY_EDITOR
    public static List<T> loadAssetAtPath<T>(string path) where T : UnityEngine.Object
    {
        List<T> al = new List<T>();
        FileInfo[] fileEntries = new DirectoryInfo(Application.dataPath + "/" + path).GetFiles();
        foreach (FileInfo fileName in fileEntries)
        {
            string localPath = "Assets/" + path+"/"+fileName.Name;
            T t = AssetDatabase.LoadAssetAtPath<T>(localPath);

            if (t != null)
                al.Add(t);
        }
        return al;
    }
#endif
}
