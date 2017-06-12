using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSRigidBodyWalker : MonoBehaviour {
	
	public float m_Speed = 10.0f;
	public float m_Gravity = 10.0f;
	public float m_MaxVelocityChange = 10.0f;
	public bool m_CanJump = true;
	public float m_JumpHeight = 2.0f;
	private bool m_Grounded = false;
	 
	private Rigidbody m_Body;
	 
	void Awake () {

		m_Body = GetComponent<Rigidbody> ();

//		rigidbody.freezeRotation = true;
//		rigidbody.useGravity = false;
	}
	 
	void FixedUpdate () {
		if (m_Grounded) {
			// Calculate how fast we should be moving
			Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			targetVelocity = transform.TransformDirection(targetVelocity);
			targetVelocity *= m_Speed;
			 
			// Apply a force that attempts to reach our target velocity
			Vector3 velocity = m_Body.velocity;
			Vector3 velocityChange = (targetVelocity - velocity);
			velocityChange.x = Mathf.Clamp(velocityChange.x, - m_MaxVelocityChange, m_MaxVelocityChange);
			velocityChange.z = Mathf.Clamp(velocityChange.z, - m_MaxVelocityChange, m_MaxVelocityChange);
			velocityChange.y = 0;
			m_Body.AddForce(velocityChange, ForceMode.VelocityChange);
			 
			// Jump
			if (m_CanJump && Input.GetButton("Jump")) {
				m_Body.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
			}
		}
		 
		// We apply gravity manually for more tuning control
		m_Body.AddForce(new Vector3 (0, -m_Gravity * m_Body.mass, 0));
		 
		m_Grounded = false;
	}
	 
	void OnCollisionStay () {
		m_Grounded = true;    
	}
	 
	float CalculateJumpVerticalSpeed () {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * m_JumpHeight * m_Gravity);
	}

}
