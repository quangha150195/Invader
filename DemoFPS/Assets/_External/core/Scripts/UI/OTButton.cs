using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class OTButton : Button {
    [SerializeField]
    private string mPressSound = "sfx_press";
    [SerializeField]
    private string mPressSoundDisable = "";
    // Use this for initialization
    protected override void Start() {

    }
    public string PressSound
    {
        get { return mPressSound; }
        set { mPressSound = value; }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        AudioManager.playEffect(interactable? mPressSound:mPressSoundDisable);
        base.OnPointerClick(eventData);
    }
	#if UNITY_EDITOR
    [MenuItem("GameObject/UI/OTButton")]
    public static void CreateOTButton()
    {
        var parentGameObject = Selection.activeObject as GameObject;
        var parentTransform = parentGameObject == null ? null : parentGameObject.transform;

        int index = 0;
        foreach (Transform buttonGo in parentTransform)
        {
            Button btn=buttonGo.GetComponent<Button>();
            if (btn != null)
                index++;
        }
        var gameObject = new GameObject("Button ("+ index+")", typeof(Image));
        gameObject.AddComponent<OTButton>();
        gameObject.transform.SetParent(parentTransform, false);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = gameObject;
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
	#endif
}
