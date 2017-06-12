using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageProgress : MonoBehaviour {
    [SerializeField]
    private Image mContentProgress;
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
            setValue(mPercent);
        }
#endif
    }
    //value (0,1)
    public void setValue(float percent)
    {
        mPercent = Mathf.Clamp(percent,0.0f,1.0f);
        mContentProgress.fillAmount = percent;
    }
    public float getValue()
    {
        return mPercent;
    }
}
