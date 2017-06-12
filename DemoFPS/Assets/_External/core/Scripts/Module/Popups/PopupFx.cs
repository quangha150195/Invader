using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PopupFx : MonoBehaviour {
    [SerializeField]
    protected GameObject mObjContent;
    [SerializeField]
    protected CanvasGroup mCanvasGroup;
    private UnityAction mPopupCallBack;
    protected bool mIsClose = false;
    private bool m_IsHidding = false;
    // Use this for initialization
    void Start() {
        
    }
    void OnValidate()
    {
        if (mObjContent == null && transform.childCount > 0)
            mObjContent = transform.GetChild(0).gameObject;
        mCanvasGroup = GetComponent<CanvasGroup>();
    }
    void Reset()
    {
        OnValidate();
    }
    public void closePopup()
    {
        if(!gameObject.activeSelf)
        {
            return;
        }
        if (mPopupCallBack != null)
        {
            mPopupCallBack.Invoke();
            mPopupCallBack = null;
        }
        mIsClose = true;
        iTween.Stop(mObjContent);
        iTween.Stop(gameObject);
        hideAnimation();
    }
    public void hidePopup()
    {
        if (!gameObject.activeSelf || m_IsHidding)
        {
            return;
        }
        m_IsHidding = true;
        if (mPopupCallBack != null)
        {
            mPopupCallBack.Invoke();
            mPopupCallBack = null;
        }     

        iTween.Stop(mObjContent);
        iTween.Stop(gameObject);
        hideAnimation();
    }
    public void showPopup(UnityAction callBack)
    {
        mPopupCallBack = callBack;
        showPopup();
    }


    public void showPopup()
    {
        if (gameObject.activeSelf && !m_IsHidding)
        {
            return;
        }
        m_IsHidding = false;
        gameObject.SetActive(true);
        iTween.Stop(mObjContent);
        iTween.Stop(gameObject);
        showAnimation();
    }
    public void hideComplete()
    {
        m_IsHidding = false;
        gameObject.SetActive(false);
        Debug.Log(gameObject.name + " hideComplete");
    }
    public void closeComplete()
    {
        Destroy(gameObject);
    }
    void updateAlpha(float alpha)
    {
        mCanvasGroup.alpha = alpha;
    }

    protected virtual void showAnimation()
    {
        mObjContent.transform.localScale = Vector3.zero;
        mCanvasGroup.alpha = 0;
        iTween.ValueTo(gameObject, iTween.Hash("from", mCanvasGroup.alpha, "to", 1, "time", 0.5f, "onupdate", "updateAlpha"));
        iTween.ScaleTo(mObjContent, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easeType", iTween.EaseType.easeOutCubic));
    }
    protected virtual void hideAnimation()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", mCanvasGroup.alpha, "to", 0, "time", 0.01f, "onupdate", "updateAlpha", "oncomplete", mIsClose? "closeComplete" : "hideComplete"));
    }
}
