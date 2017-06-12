using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Callbacks;

public class OTEditor : MonoBehaviour {
	[MenuItem("OTEditor/SyncSourceCodeIOS")]
	public static void SyncSourceCodeIOS()
	{
        if (File.Exists(Application.persistentDataPath + "/proj_ios_path.txt"))
        {
            StreamReader file = new StreamReader(Application.persistentDataPath + "/proj_ios_path.txt");
            string pathSrc = file.ReadLine()+"/Libraries/_External";
            file.Close();

            string pathDes = new DirectoryInfo(Application.dataPath) + "/_External";
            int lengthPathSrc = pathSrc.Length;
            foreach (string filePath in Directory.GetFiles(pathSrc, "*.*", SearchOption.AllDirectories))
            {
                string fileDes = pathDes + filePath.Substring(lengthPathSrc);
                Debug.Log(fileDes);
                File.Copy(filePath, fileDes, true);
            }
            Debug.Log("Syns Path:"+pathSrc);
        }
	}
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            StreamWriter file=new StreamWriter(Application.persistentDataPath+"/proj_ios_path.txt",false);
            file.WriteLine(pathToBuiltProject);
            file.Close();
        }
    }
    [MenuItem("OTEditor/Clear UserDefault")]
    public static void clearUserDefault()
    {
        PlayerPrefs.DeleteAll();
    }

    static int go_count = 0, components_count = 0, missing_count = 0;

    [MenuItem("OTEditor/Find Missing Scripts")]
    private static void FindInSelected()
    {
        GameObject[] go = Selection.gameObjects;
        go_count = 0;
        components_count = 0;
        missing_count = 0;
        foreach (GameObject g in go)
        {
            FindInGO(g);
        }
        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
    }

    private static void FindInGO(GameObject g)
    {
        go_count++;
        Component[] components = g.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            components_count++;
            if (components[i] == null)
            {
                missing_count++;
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log(s + " has an empty script attached in position: " + i, g);
            }
        }
        // Now recurse through each child GO (if there are any):
        foreach (Transform childT in g.transform)
        {
            //Debug.Log("Searching " + childT.name  + " " );
            FindInGO(childT.gameObject);
        }
    }


    //FIND REFERENCE
    [MenuItem("CONTEXT/Component/Find references to this")]
    private static void FindReferences(MenuCommand data)
    {
        Object context = data.context;
        if (context)
        {
            var comp = context as Component;
            if (comp)
                FindReferencesTo(comp);
        }
    }

    [MenuItem("Assets/Find references to this")]
    private static void FindReferencesToAsset(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
            FindReferencesTo(selected);
    }

    private static void FindReferencesTo(Object to)
    {
        var referencedBy = new List<Object>();
        var allObjects = Object.FindObjectsOfType<GameObject>();
        for (int j = 0; j < allObjects.Length; j++)
        {
            var go = allObjects[j];

            if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
            {
                if (PrefabUtility.GetPrefabParent(go) == to)
                {
                    Debug.Log(string.Format("referenced by {0}, {1}", go.name, go.GetType()), go);
                    referencedBy.Add(go);
                }
            }

            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                var c = components[i];
                if (!c) continue;

                var so = new SerializedObject(c);
                var sp = so.GetIterator();

                while (sp.NextVisible(true))
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == to)
                        {
                            Debug.Log(string.Format("referenced by {0}, {1}", c.name, c.GetType()), c);
                            referencedBy.Add(c.gameObject);
                        }
                    }
            }
        }

        if (referencedBy.Any())
            Selection.objects = referencedBy.ToArray();
        else Debug.Log("no references in scene");
    }
}