using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerInvader : MonoBehaviour {

	[SerializeField]
	private GameObject[] m_Invaders;//An array of invaders

	[SerializeField]
	private AudioSource m_AudioSource;//audio of spawner position to create audio effect

	[SerializeField]
	private AudioClip m_InvaderDeathClip;//audio effect of invader when hit raycast and death

	public static SpawnerInvader s_Instance;

	private int m_CurrentIndex;

	void Awake(){
		makeInstance ();
	}


	void makeInstance(){
		if (s_Instance == null) {
			s_Instance = this;
		}
	}

	IEnumerator Spawner(){
		yield return new WaitForSeconds (0.5f);
		activeRandomInvader();
	}

	//use this to spawner enemy when enemy hit the bullet. Call function in Shooting Class
	public void spawner(){
		StartCoroutine (Spawner ());
	}

	//use this to create audio effect of invaders when death. Call function in Shooting Class
	public void playInvaderDeathSound(){
		m_AudioSource.PlayOneShot (m_InvaderDeathClip);
	}

 	void activeRandomInvader(){
		int index = Random.Range (0, m_Invaders.Length);
        m_Invaders[index].transform.position += new Vector3(Random.Range(-5, 5), Random.RandomRange(0,3), Random.Range(-5, 5));
        m_Invaders[index].SetActive (true);
		m_CurrentIndex = index;
	}

 	public void deActiveRandomInvader(){
		m_Invaders[m_CurrentIndex].SetActive (false);
	}

	public void isTrackingFound(){
		if (Vuforia.DefaultTrackableEventHandler.s_Instance!=null) {
			{
				if (Vuforia.DefaultTrackableEventHandler.s_Instance.m_IsTracking) {
					activeRandomInvader ();
				}
			}	
		}
	}

	public void isTrackingLost(){
		if (Vuforia.DefaultTrackableEventHandler.s_Instance!=null) {
			{
				if (!Vuforia.DefaultTrackableEventHandler.s_Instance.m_IsTracking) {
					deActiveRandomInvader ();
				}
			}	
		}
	}
}
