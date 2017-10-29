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
	private Vector3 startPos;

	private GameObject target;
	private GameObject parent;

	private bool canMove = false;
	private float travelTime;
	private float startY;

	// Use this for initialization
	void Start () {
		parent = transform.parent.gameObject;

		startY = transform.position.y;

		target = Instantiate (targetPrefab, transform.position, Quaternion.identity);

		timer = Time.time + Random.Range(0f,destinationChangeDelay);

		CC = GetComponent<CharacterController> ();
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Time.time - timer > destinationChangeDelay) {

			FindTarget();

			timer = Time.time;
		}

		MoveTowardTarget ();

	}

	void FindTarget(){
		Vector3 targetPos = transform.position;
		bool success = false;
		int max = 50;
		int k = 0;
		Vector3 dir;
		Vector3 pos = transform.position;
		float dist;
		do{
			targetPos = randomPosition();
			dir = targetPos-transform.position;
			dist = Vector3.Distance(transform.position,targetPos);
			Ray ray = new Ray(pos,dir );
			RaycastHit hit;
			success = !Physics.Raycast(ray,out hit,dist);
			travelTime = dist/moveSpeed;
			if(!success && hit.distance>2.0f){
				float alpha = Mathf.Clamp(hit.distance/dist,0.33f,0.66f);
				targetPos = Vector3.Lerp(pos,targetPos,alpha);
				dist = Vector3.Distance(transform.position,targetPos);
				success = true;
				travelTime = dist/moveSpeed;
			}

			k++;
		}while(!success && k<max);
		canMove = success;
		//Debug.Log (k);
		target.transform.position = targetPos;
		startPos = transform.position;
	}

	Vector3 randomPosition(){
		
		Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * walkRadius;

		Vector3 position = parent.transform.position + new Vector3 (randomDirection.x, 0, randomDirection.y);

		return position;

	}
	void MoveTowardTarget(){
		if (!canMove) {
			return;
		}
		float alpha = (Time.time - timer) / travelTime;

		transform.position = Vector3.Lerp (startPos, target.transform.position, alpha);



	}

}
