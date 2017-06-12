using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using external;

[RequireComponent(typeof(LayoutElement))]
public class UIElementView : MonoBehaviour {
    [SerializeField]
    private LayoutElement mLayoutElement;
    void OnValidate()
    {
        mLayoutElement = GetComponent<LayoutElement>();
    }
    void Reset()
    {
        OnValidate();
    }
    void Awake()
    {
        if (mLayoutElement.preferredHeight > 0)
			mLayoutElement.preferredHeight *= BaseUtils.getDeviceDensity();
        if (mLayoutElement.preferredWidth > 0)
			mLayoutElement.preferredWidth *= BaseUtils.getDeviceDensity();
    }
	// Update is called once per frame
	void Update () {
	
	}
}
