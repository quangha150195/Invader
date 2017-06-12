using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScene : MonoBehaviour {
    [SerializeField]
    private GameObject m_LoGoUnity;
    [SerializeField]
    private GameObject m_LoGoOneTech;
    [SerializeField]
    private AudioClip[] m_Sound;

    private bool m_CheckLoadScene = false;

    // Use this for initialization
    void Start () {
        StartCoroutine(playSound());
        m_LoGoUnity.transform.position = new Vector3(0, 0, 0);
        iTween.MoveFrom(m_LoGoUnity, iTween.Hash("y", 8f, "time", 1.5f, "delay", 1.0f, "easetype", "easeOutBounce"));
        iTween.MoveTo(m_LoGoUnity, iTween.Hash("x", 12f, "time", 1, "delay", 3.5f, "easetype", "linear"));
        iTween.ScaleTo(m_LoGoUnity, iTween.Hash("x", 0.2f, "y", 0.2f, "time", .5f, "delay", 3.5f));
        m_LoGoOneTech.transform.position = new Vector3(0, 0, 0);
        iTween.MoveFrom(m_LoGoOneTech, iTween.Hash("x", -10, "time", 2, "delay", 3.0f, "easetype", "easeoutcubic"));
        m_LoGoOneTech.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        iTween.ScaleTo(m_LoGoOneTech, iTween.Hash("x", 1.5f, "y", 1.5f, "time", .5f, "delay", 3.5f));
    }

    // Update is called once per frame
    void Update () {

	}

    IEnumerator playSound()
    {
        yield return new WaitForSeconds(1.5f);
        AudioManager.playEffect(m_Sound[0]);
        yield return new WaitForSeconds(0.5f);
        AudioManager.playEffect(m_Sound[0]);
        yield return new WaitForSeconds(0.4f);
        AudioManager.playEffect(m_Sound[2]);
        yield return new WaitForSeconds(1.0f);
        AudioManager.playEffect(m_Sound[1]);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("_MainMenu");
    }

}
