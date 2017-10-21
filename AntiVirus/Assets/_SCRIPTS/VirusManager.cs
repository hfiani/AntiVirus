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
	public float maxHealth = 100;
	public float damageTakenPerProjectile = 10;
	public Color damagedColor;
	#endregion

	#region private variables
	private float timerAge;
	private float timerUpdate;
	private float health;
	private bool isAlive;

	private bool hasLanded;
	private Color startBaseColor;
	private Color startEmiColor;
	private Color currentBaseColor;
	private Color currentEmiColor;
	private GameObject firstInfectedBlock;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		isAlive = true;
		hasLanded = false;
	
		startBaseColor = GetComponent<Renderer> ().material.GetColor ("_Color");
		startEmiColor = GetComponent<Renderer> ().material.GetColor ("_EmissionColor");
		currentBaseColor = startBaseColor;
		currentEmiColor = startEmiColor;
		health = maxHealth;

	}

	void Update()
	{
		if (Time.time - timerAge > lifetime && isAlive && hasLanded)
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

	void updateHealth(float value){

		health += value;

		if (health > maxHealth) {

			health = maxHealth;
		}

		if (health < 0f) {

			health = 0f;
			DeathFromPlayer ();
		}


		currentBaseColor = Color.Lerp(damagedColor,startBaseColor,health/maxHealth);
		currentEmiColor = Color.Lerp(damagedColor,startEmiColor,health/maxHealth);
	
		GetComponent<Renderer> ().material.SetColor ("_Color", currentBaseColor);
		GetComponent<Renderer> ().material.SetColor ("_EmissionColor", currentEmiColor);

		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject child = transform.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> ())
			{
				child.GetComponent<Renderer> ().material.SetColor ("_Color", currentBaseColor);
				child.GetComponent<Renderer> ().material.SetColor ("_EmissionColor", currentEmiColor);
			}
		}


	}

	public Color GetCurrentBaseColor(){
		return currentBaseColor;
	}
	public Color GetCurrentEmiColor(){
		return currentEmiColor;
	}

	void OnLanding()
	{
		hasLanded = true;
		timerAge = Time.time;
		timerUpdate = Time.time;

		GetComponent<Rigidbody> ().isKinematic = true;
	}


	void DeathFromAge()
	{
		isAlive = false;
		Instantiate (redExplosionPrefab, transform.position, Quaternion.identity);

		Destroy (gameObject);
	}

	// when killed by player, virus will remove corruption
	void DeathFromPlayer()
	{
		isAlive = false;
		Instantiate (greenExplosionPrefab, transform.position, Quaternion.identity);
		firstInfectedBlock.GetComponent<InfectionRaycast> ().RepairInfection ();

		Destroy (gameObject);
	}

	void OnTriggerEnter(Collider col)
	{
		// infect when land on ground
		if (col.gameObject.GetComponent<InfectionRaycast> () != null && !hasLanded && isAlive)
		{
			col.gameObject.GetComponent<InfectionRaycast> ().CreateInfection(DateTime.Now);
			firstInfectedBlock = col.gameObject;

			OnLanding ();

		}

		// take damage when hit by player's projectile
		if (col.gameObject.GetComponent<Projectile> () != null && isAlive && hasLanded)
		{
			updateHealth (-1*damageTakenPerProjectile);

		

			Debug.Log ("hit");

		}
	}
	#endregion
}
