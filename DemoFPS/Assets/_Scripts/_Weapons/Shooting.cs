using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {

	public static Shooting s_Instance;

	private Transform m_CamTransform;//use raycast from camera to object

	private Animation m_Anim;//animation of Weapon when Shooting

	[SerializeField]
	private Transform m_SpawnerEnemy;//use to get position to create a effect enemy death

	[SerializeField]
	private ParticleSystem m_MuzzleFlash;//effect of weapon when shoot

	[SerializeField]
	private GameObject m_Effect;//enemy effect when hit raycast

	//[HideInInspector]
	//public int m_ShootTime;//time of shooting, set default is 3

	[SerializeField]
	private AudioSource m_AudioSource;//audio of weapon

	[SerializeField]
	private AudioClip m_GunClip;//audio effect of weapon

	public int flag = 0;

    [SerializeField]
    private GameObject m_TimeProgress;
    public float m_LimitTime;
    private float m_currentTime;
    private bool m_boolLose = false;
    public int m_CurrentScore = 0;

    private void Start()
    {
        m_currentTime = 0.0f;
        m_CurrentScore = 0;
    }
    void Update(){ 
		if (Input.GetMouseButtonDown (0) && !m_boolLose) {//if user press left button mouse and shoottime > 0 => can shoot
			shoot ();
		}
		Debug.DrawRay (m_CamTransform.position, m_CamTransform.forward * 10);

        if (m_CurrentScore != 0)
        {
            if (m_currentTime > m_LimitTime)
            {
                if (m_boolLose == true)
                {
                    return;
                }
                if (GamePlayController.s_Instance != null)
                {
                    GamePlayController.s_Instance.gameOver();
                }

            }
            else
            {
                float _scaleProgaming = 1.0f - m_currentTime / m_LimitTime;
                m_TimeProgress.transform.localScale = new Vector3(_scaleProgaming, 1, 1);
            }
            m_currentTime += Time.deltaTime;
        }
    }

	void Awake(){
		makeInstance ();
		initializeVariables ();
	}

	void makeInstance(){
		if (s_Instance == null) {
			s_Instance = this;
		}
	}

	void initializeVariables(){
		m_CamTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
		m_Anim = GetComponent<Animation> ();
	}

	void shoot(){

		if (!m_Anim.IsPlaying ("Fire") && !m_Anim.IsPlaying("TakeIn")) {

			m_Anim.CrossFade ("Fire");

			Ray ray = new Ray (m_CamTransform.position, m_CamTransform.forward);
			RaycastHit hit;

			//m_ShootTime--;
			//if (GamePlayController.s_Instance != null) {
			//	GamePlayController.s_Instance.setShootTime (m_ShootTime);
			//}

			if (Physics.Raycast (ray, out hit, 400)) {

				if (hit.collider.CompareTag ("Invader")) {
                    m_currentTime = 0;
                    m_CurrentScore++;
                    m_LimitTime = m_LimitTime - 0.2f;
                    //flag = 1;

                    //Destroy (hit.collider.gameObject);//destroy object has detected by raycast

                    //hit.collider.gameObject.SetActive (false);

                    if (SpawnerInvader.s_Instance != null) {
						SpawnerInvader.s_Instance.deActiveRandomInvader ();
					}


//					if(SpawnerInvader.s_Instance!=null){
//						SpawnerInvader.s_Instance.spawner ();
//					}

					if(SpawnerInvader.s_Instance!=null){
						SpawnerInvader.s_Instance.playInvaderDeathSound ();
					}

					//m_ShootTime++;
					//if(GamePlayController.s_Instance!=null){
					//	GamePlayController.s_Instance.setShootTime (m_ShootTime);
					//}

					if(Door.s_Instance!=null){
						Door.s_Instance.closeAndOpenDoor ();
					}
						
					GameObject temp = Instantiate (m_Effect, m_SpawnerEnemy.position, Quaternion.identity) as GameObject;
					Destroy (temp, 2f);
				}
			}

			//if (m_ShootTime <= 0) {
				
			//	m_ShootTime = 0;

			//	if (GamePlayController.s_Instance != null) {
			//		GamePlayController.s_Instance.setShootTime (m_ShootTime);
			//	}

			//	if (GamePlayController.s_Instance != null) {
			//		GamePlayController.s_Instance.gameOver();
			//	}
			//}

			m_AudioSource.PlayOneShot (m_GunClip);

			m_MuzzleFlash.Play ();
		}
	}
		
}
