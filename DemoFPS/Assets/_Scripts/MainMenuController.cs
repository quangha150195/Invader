using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

    [SerializeField]
    private GameObject m_btnAR;
    [SerializeField]
    private GameObject m_btnInfo;
    [SerializeField]
    private GameObject m_btnQuit;

    //[SerializeField]
    //private GameObject m_CountdownVideo;
    [SerializeField]
    private AudioClip m_btn;
    [SerializeField]
    private AudioClip m_SoundTitle;
    [SerializeField]
    private AudioClip m_SoundCountdown;
    [SerializeField]
    private GameObject m_Countdown;
    [SerializeField]
    private GameObject m_Count1;
    [SerializeField]
    private GameObject m_Count2;
    [SerializeField]
    private GameObject m_Count3;
    [SerializeField]
    private GameObject m_CountGo;

    private bool checkARMode = true;


    // Use this for initialization
    void Start () {
        //m_CountdownVideo.SetActive(false);
        AudioManager.playEffect(m_SoundTitle);
	}

    // Update is called once per frame
    void Update()
    {

    }

    public void arMode()
    {
        if(checkARMode)
        {
            checkARMode = false;
            OnButtonPress(m_btnAR);
            StartCoroutine(loadScene(0.7f));
            AudioManager.playEffect(m_btn);
        }

    }

    public void info()
    {
        OnButtonPress(m_btnInfo);
        AudioManager.playEffect(m_btn);
    }

    public void quit()
    {
        OnButtonPress(m_btnQuit);
        AudioManager.playEffect(m_btn);
        Application.Quit();
    }

    void OnButtonPress(GameObject _gameObject)
    {
        _gameObject.transform.localScale = new Vector3(1, 1, 1);
        iTween.ScaleTo(_gameObject, iTween.Hash("x",1.3f ,"y",1.3f, "time", 0.3f));
        iTween.ScaleTo(_gameObject, iTween.Hash("x", 1.0f, "y", 1.0f, "time", 0.2f, "delay", 0.2f));
    }

    IEnumerator loadScene(float _time)
    {
        yield return new WaitForSeconds(_time);
        m_Countdown.SetActive(true);
        iTween.ScaleTo(m_Countdown, iTween.Hash("x", 1, "y", 1, "time", _time));
        AudioManager.playEffect(m_SoundCountdown);
        yield return new WaitForSeconds(1.0f);
        setActive(m_Count1, m_Count2);
        yield return new WaitForSeconds(1.0f);
        setActive(m_Count2, m_Count3);
        yield return new WaitForSeconds(1.0f);
        setActive(m_Count3, m_CountGo);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("_GamePlay");
    }

    void setActive(GameObject _g1, GameObject _g2)
    {
        _g1.SetActive(false);
        _g2.SetActive(true);
    }

}
