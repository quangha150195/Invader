#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using UnityEditor;
using System.Collections.Generic;
using PlistCS;
using UnityEditor.iOS.Xcode;

[InitializeOnLoad]
public static class CustomBuildApp
{
	private static void RefreshAssets()
	{
		EditorApplication.ExecuteMenuItem ("Assets/Refresh");
	}

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            // set plist path
            string plistPath = pathToBuiltProject + "/info.plist";

            // read plist
            Dictionary<string, object> dict;
            dict = (Dictionary<string, object>)Plist.readPlist(plistPath);

			dict.Add ("NSCameraUsageDescription", "Use camera for QR code");

			/*
            // update plist
			dict["CFBundleURLTypes"] = new List<object> {
				new Dictionary<string,object> {
					{ "CFBundleURLName", PlayerSettings.iPhoneBundleIdentifier },
					{ "CFBundleURLSchemes", new List<object> { "fategoar" } }
				}
			};

            //Push run in background
            dict["UIBackgroundModes"] = new List<object>{
                "remote-notification"
            };

            //Push run in background
            dict["CFBundleDisplayName"] = "Fate/GO AR";
			*/

            // write plist
            Plist.writeXml(dict, plistPath);
        }
    }
}
#endif