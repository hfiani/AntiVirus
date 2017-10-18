using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VirusManager : MonoBehaviour
{

	#region public variables
	public float lifetime = 10.0f;
	public GameObject redExplosionPrefab = null;
	public GameObject greenExplosionPrefab = null;
	#endregion

	#region private variables
	private float timerAge;
	private float timerUpdate;
	private bool isAlive;
	private bool hasLanded;
	private GameObject firstInfectedBlock;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		isAlive = true;
		hasLanded = false;
		timerAge = Time.time;
		timerUpdate = Time.time;
	}

	void Update()
	{
		if (Time.time - timerAge > lifetime && isAlive)
		{
			DeathFromAge ();
		}

		// re-infect first block
		if (hasLanded && Time.time - timerUpdate > 0.25 && isAlive && firstInfectedBlock != null)
		{
			firstInfectedBlock.GetComponent<InfectionRaycast> ().CreateInfection(DateTime.Now);

			timerUpdate = Time.time;
		}
	}

	void OnLanding()
	{
		hasLanded = true;

		GetComponent<Rigidbody> ().isKinematic = true;
	}

	void DeathFromAge()
	{
		isAlive = false;
		Instantiate (redExplosionPrefab, transform.position, Quaternion.identity);

		Destroy (gameObject);
	}

	void DeathFromPlayer()
	{
		isAlive = false;
		Instantiate (greenExplosionPrefab, transform.position, Quaternion.identity);
		firstInfectedBlock.GetComponent<InfectionRaycast> ().RepairInfection ();

		Destroy (gameObject);
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.GetComponent<InfectionRaycast> () != null && !hasLanded && isAlive)
		{
			col.gameObject.GetComponent<InfectionRaycast> ().CreateInfection(DateTime.Now);
			firstInfectedBlock = col.gameObject;

			OnLanding ();

			//Destroy (gameObject);
		}

		if (col.gameObject.GetComponent<Projectile> () != null && isAlive)
		{
			DeathFromPlayer ();

			Destroy (col.gameObject);

			//Destroy (gameObject);
		}
	}
	#endregion
}
