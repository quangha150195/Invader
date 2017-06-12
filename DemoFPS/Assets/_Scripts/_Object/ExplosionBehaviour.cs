using UnityEngine;
using System.Collections;

public class ExplosionBehaviour : MonoBehaviour {

	private Rigidbody _body = null;
	private Animator _animator = null;
	private MeshRenderer[] _meshRenders;

	void Awake(){
		_body = GetComponent<Rigidbody> ();
		_animator = GetComponentInChildren<Animator>();
		_animator.speed /= 4.0f;
		_meshRenders = GetComponentsInChildren<MeshRenderer> ();

		gameObject.hideFlags = HideFlags.HideInHierarchy;
	}

//	void Update() {
//		if (transform.position.y < -3) {
//			GameController.getInstance ().deactiveExplosion (gameObject);
//			gameObject.SetActive (false);
//		}
//	}

	public void trigger(Material material){
		//set material
		foreach(var mesh in _meshRenders) {
			mesh.material = material;
		}

		//reset velocity
		_body.velocity = Vector3.zero;
		_body.angularVelocity = Vector3.zero;

		//animation
		_animator.SetTrigger ("Explode");
	}
}
