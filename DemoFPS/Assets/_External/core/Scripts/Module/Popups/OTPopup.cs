using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OTPopup : MonoBehaviour {
    public enum ButtonType {
        NONE = 0,
        OK = 1,
        YES = 2,
        NO = 3
    }
    private const string kDefaultPopupPath= "Default/DefaultPopup";
    [SerializeField]
    private OTButton mBtnEmptyPrefab;
    [SerializeField]
    private OTButton mBtnOKPrefab=null, mBtnYesPrefab=null, mBtnNoPrefab=null;
    [SerializeField]
    private Transform mBtnGroup;
    [SerializeField]
    private Text mTitle;
    [SerializeField]
    private Text mMsg;
    private Button[] mListButton;
    public static OTPopup createPopup(string title, string msg, string button1, string button2 = "", string button3 = "") {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>(kDefaultPopupPath));
        popupGO.gameObject.SetActive(false);
        OTPopup popup = popupGO.GetComponent<OTPopup>();
        popup.mBtnOKPrefab.transform.parent.gameObject.SetActive(false);
        popup.mTitle.text = title;
        popup.mMsg.text = msg;
        popup.mListButton = new Button[3];
        popup.addButton(0,button1);
        popup.addButton(1,button2);
        popup.addButton(2,button3);
        PopupFx popupFx = popup.GetComponent<PopupFx>();
        if (popupFx != null)
        {
            popupFx.hideComplete();
        }
        return popup;
    }
    public static OTPopup createPopup(string title, string msg, ButtonType button1, ButtonType button2 = ButtonType.NONE, ButtonType button3 = ButtonType.NONE)
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>(kDefaultPopupPath));
        popupGO.gameObject.SetActive(false);
        OTPopup popup = popupGO.GetComponent<OTPopup>();
        popup.mBtnOKPrefab.transform.parent.gameObject.SetActive(false);
        popup.mTitle.text = title;
        popup.mMsg.text = msg;
        popup.mListButton = new OTButton[3];
        popup.addButton(0,button1);
        popup.addButton(1,button2);
        popup.addButton(2,button3);
        PopupFx popupFx = popup.GetComponent<PopupFx>();
        if (popupFx != null)
        {
            popupFx.hideComplete();
        }
        return popup;
    }
    void Awake()
    {
        if (EventSystem.current == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
    public void setCallBack(UnityAction callback1, UnityAction callback2 = null, UnityAction callback3=null)
    {
        setCallBackButton(0, callback1);
        setCallBackButton(1, callback2);
        setCallBackButton(2, callback3);
    }
    private void addButton(int index,ButtonType buttonType)
    {
        OTButton btnPref = null;
        switch (buttonType)
        {
            case ButtonType.NONE:
                break;
            case ButtonType.OK:
                btnPref = mBtnOKPrefab;
                break;
            case ButtonType.YES:
                btnPref = mBtnYesPrefab;
                break;
            case ButtonType.NO:
                btnPref = mBtnNoPrefab;
                break;
            default:
                break;
        }
        if (btnPref != null)
        {
            GameObject btnObj = GameObject.Instantiate(btnPref.gameObject);
            btnObj.transform.SetParent(mBtnGroup,false);
            mListButton[index]= btnObj.GetComponent<OTButton>();
        }
    }
    private void addButton(int index,string buttonName)
    {
        OTButton btnPref = mBtnEmptyPrefab;
        if (buttonName!=null&&buttonName.Length == 0)
        {
            GameObject btnObj = GameObject.Instantiate(btnPref.gameObject);
            btnObj.transform.SetParent(mBtnGroup,false);
            mListButton[index] = btnObj.GetComponent<OTButton>();
        }
    }
    private void setCallBackButton(int index, UnityAction callBack)
    {
        Button btn = null;
        if (index >= 0 && index < 3)
        {
            btn = mListButton[index];
        }
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (callBack != null)
                    callBack();
                close();
            });
        }
    }
    public void show()
    {
        
        PopupFx popupFx = GetComponent<PopupFx>();
        if (popupFx != null)
        {
            popupFx.showPopup();
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
    public void close()
    {
        PopupFx popupFx = GetComponent<PopupFx>();
        if (popupFx != null)
        {
            popupFx.closePopup();
            StartCoroutine(BaseUtils.RunAction(0.5f, closeComplete));
        }
        else
        {
            closeComplete();
        }
        
    }
    private void closeComplete()
    {
        GameObject.Destroy(this.gameObject);
    }
}
