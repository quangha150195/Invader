using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RatioFilter : MonoBehaviour {
    public enum ScaleType
    {
        FILL,
        FIT
    }
    [SerializeField]
    private ScaleType mScaleType;
    [SerializeField]
    RectTransform mRectTransform;
    [SerializeField]
    RectTransform mRectTransformParent;
    void OnValidate()
    {
        mRectTransform = GetComponent<RectTransform>();
        mRectTransformParent = transform.parent.GetComponent<RectTransform>();
    }
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
        if (mRectTransformParent == null)
            OnValidate();
#endif
        Vector2 sizeParent = mRectTransformParent.rect.size;
        float scaleX= sizeParent.x/mRectTransform.sizeDelta.x;
        float scaleY = sizeParent.y/mRectTransform.sizeDelta.y;

        if (mScaleType == ScaleType.FIT)
        {
            transform.localScale = Vector3.one * Mathf.Min(scaleX, scaleY);
        }
        else
        {
            transform.localScale = Vector3.one * Mathf.Max(scaleX, scaleY);
        }
    }
}
