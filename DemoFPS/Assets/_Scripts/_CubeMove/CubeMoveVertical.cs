using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMoveVertical : MonoBehaviour {

	[SerializeField]
	private float m_Speed;

	[SerializeField]
	private float m_Direct;//direction of cube move left or right

	void Update(){
		Vector3 temp = transform.position;

		temp.z += m_Direct * m_Speed * Time.deltaTime;

		transform.position = temp;
	}

	void OnTriggerEnter(Collider target){

		if (target.tag == "Detect1") {
			m_Direct = -1;
		}

		if (target.tag == "Detect2") {
			m_Direct = 1;
		}
	}
}
