using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

	public static Door s_Instance;

	private Animator m_Door;

	void Awake(){
		makeInstance ();
		m_Door = GetComponent<Animator> ();
	}

	void makeInstance(){
		if (s_Instance == null) {
			s_Instance = this;
		}
	}

	public void closeAndOpenDoor(){
		StartCoroutine (closeTheDoor ());
	}

	IEnumerator closeTheDoor(){
		//yield return new WaitForSeconds (1);
		//m_Door.Play ("Close");
		yield return new WaitForSeconds (0.2f);
		//m_Door.Play ("Open");
		if (Vuforia.DefaultTrackableEventHandler.s_Instance!=null) {
			{
				if (Vuforia.DefaultTrackableEventHandler.s_Instance.m_IsTracking) {
					if (SpawnerInvader.s_Instance != null) {
						SpawnerInvader.s_Instance.isTrackingFound ();
					}
				}
			}	
		}
	}
}
