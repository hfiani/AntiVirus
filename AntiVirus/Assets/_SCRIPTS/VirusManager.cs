using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VirusManager : MonoBehaviour
{

	#region public variables
	public float lifetime = 10.0f;
	public GameObject explosionPrefab = null;
	#endregion

	#region private variables
	private float timer;
	private bool isAlive;
	private bool hasLanded;
	private GameObject blockInfected;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		isAlive = true;
		hasLanded = false;
		timer = Time.time;
	}

	void Update()
	{
		if (Time.time - timer > lifetime)
		{

			Debug.Log ("virus lifetime end");
			Death ();
		}
	}

	void OnLanding()
	{
		hasLanded = true;

		GetComponent<Rigidbody> ().isKinematic = true;
	}

	void Death()
	{
		Instantiate (explosionPrefab, transform.position, Quaternion.identity);
		blockInfected.GetComponent<InfectionRaycast> ().RepairInfection ();

		Destroy (gameObject);
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.GetComponent<InfectionRaycast> () != null && !hasLanded)
		{
			col.gameObject.GetComponent<InfectionRaycast> ().CreateInfection(DateTime.Now);
			blockInfected = col.gameObject;

			OnLanding ();

			//Destroy (gameObject);
		}
	}
	#endregion
}
