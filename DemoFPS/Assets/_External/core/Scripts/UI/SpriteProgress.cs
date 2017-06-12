using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SpriteProgress : MonoBehaviour {
    public GameObject mContentProgress;
    [Range(0,1)]
    public float mPercent;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
        if (mContentProgress)
        {
            SetValue(mPercent);
        }
#endif
    }
    //value (0,1)
    public void SetValue(float percent)
    {
        mPercent = Mathf.Clamp(percent,0.0f,1.0f);
        mContentProgress.transform.localScale = new Vector3(mPercent, 1,1);
    }
}
