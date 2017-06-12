using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LinkEnableProperty : MonoBehaviour {
    [SerializeField]
    private GameObject mObjLinked;
	// Use this for initialization
	void Start () {
	
	}

    void OnEnable()
    {
        if (mObjLinked != null)
        {
            mObjLinked.SetActive(true);
        }
    }
    void OnDisable()
    {
        if (mObjLinked != null)
        {
            mObjLinked.SetActive(false);
        }
    }
}
