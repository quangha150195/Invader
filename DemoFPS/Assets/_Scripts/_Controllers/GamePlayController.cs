using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePlayController : MonoBehaviour {

	public static GamePlayController s_Instance;

	[SerializeField]
	private Text m_ShootTimeTxt;

    [SerializeField]
    private Text m_Score;
    [SerializeField]
    private Text m_HighScore;

    [SerializeField]
	private GameObject m_GameOverPanel;

    [SerializeField]
    private AudioClip m_BGM;

    public AudioClip m_soundButton;
    private bool m_boolLose = false;

    void Awake(){
		makeInstance ();
	}

	void Start(){

        AudioManager.playBackgroundMusic(m_BGM);
		//m_ShootTimeTxt.text = "" + Shooting.s_Instance.m_ShootTime;
	}

	void makeInstance(){
		if (s_Instance == null) {
			s_Instance = this;
		}
	}
		
	public void Update(){
		m_ShootTimeTxt.text = "" + Shooting.s_Instance.m_CurrentScore;
	}

	public void gameOver(){
        AudioManager.stopBackgroundMusic();
        if (m_boolLose)
            return;
        m_boolLose = true;

        //Shooting.s_Instance.m_ShootTime = 0;
        m_GameOverPanel.SetActive (true);
        iTween.ScaleTo(m_GameOverPanel, iTween.Hash("x", 3, "y", 3, "time", 0.5));
        iTween.ScaleTo(m_GameOverPanel, iTween.Hash("x", 2.2, "y", 2.2, "time", 0.5, "delay", 0.5));

        int _highScore = PlayerPrefs.GetInt("high", 0);
        if ( Shooting.s_Instance.m_CurrentScore > _highScore)
        {
            PlayerPrefs.SetInt("high", Shooting.s_Instance.m_CurrentScore);
            PlayerPrefs.Save();
        }
        m_Score.text = "" + Shooting.s_Instance.m_CurrentScore;
        m_HighScore.text = _highScore.ToString();

    }

    public void playAgainButton(){
        AudioManager.playEffect(m_soundButton);
        SceneManager.LoadScene ("_Gameplay");
	}

    public void back()
    {
        AudioManager.stopBackgroundMusic();
        AudioManager.playEffect(m_soundButton);
        SceneManager.LoadScene("_MainMenu");
    }
}
