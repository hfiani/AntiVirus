using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NPCManager : MonoBehaviour {

	private UnityStandardAssets.Characters.ThirdPerson.AICharacterControl AIC;

	public float destinationChangeDelay = 5.0f;
	public float walkRadius = 10.0f;

	public GameObject targetPrefab;

	private float timer;

	private GameObject target;

	// Use this for initialization
	void Start () {

		target = Instantiate (targetPrefab, transform.position, Quaternion.identity);

		timer = Time.time;

		AIC = GetComponent<UnityStandardAssets.Characters.ThirdPerson.AICharacterControl> ();
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Time.time - timer > destinationChangeDelay) {

			SetTargetPosition(randomPositionOnNavMesh());

			timer = Time.time;
		}
		
	}

	Vector3 randomPositionOnNavMesh(){
		
		Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * walkRadius ;
	
		randomDirection += transform.position;
		NavMeshHit hit;
		NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
		Vector3 finalPosition = hit.position;

		return finalPosition;



	}

	void SetTargetPosition(Vector3 position){
		target.transform.position = position;
		AIC.SetTarget (target.transform);

	}
}
