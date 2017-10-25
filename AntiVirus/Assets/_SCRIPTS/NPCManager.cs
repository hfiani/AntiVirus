using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour {

	private CharacterController CC;

	public float destinationChangeDelay = 5.0f;
	public float walkRadius = 10.0f;
	public float moveSpeed = 10.0f;
	public GameObject targetPrefab;

	private float timer;

	private GameObject target;

	// Use this for initialization
	void Start () {

		target = Instantiate (targetPrefab, transform.position, Quaternion.identity);

		timer = Time.time;

		CC = GetComponent<CharacterController> ();
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Time.time - timer > destinationChangeDelay) {

			SetTargetPosition(randomPosition());

			timer = Time.time;
		}

		MoveTowardTarget ();

	}

	Vector3 randomPosition(){
		
		Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * walkRadius;

		Vector3 position = new Vector3 (randomDirection.x, transform.position.y, randomDirection.y);

		return position;

	}
	void MoveTowardTarget(){
		Vector3 dir = target.transform.position - transform.position;
		CC.Move (dir.normalized * moveSpeed * Time.deltaTime);
	}
	void SetTargetPosition(Vector3 position){
		target.transform.position = position;


	}
}
