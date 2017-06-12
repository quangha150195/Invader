using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class OnlyOneObjActive : MonoBehaviour {
    [SerializeField]
    private bool m_IsEditor=false;
    [SerializeField]
    private Text m_LbLeftTime;
    void Awake()
    {
        if (m_IsEditor)
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }
        }
    }
	// Use this for initialization
	void Start () {
	
	}
    void Update()
    {

    }
    void OnEnable()
    {
        Transform parent = transform.parent;
        Transform child;
        if (parent != null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                child = parent.GetChild(i);
                if (child != transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}
