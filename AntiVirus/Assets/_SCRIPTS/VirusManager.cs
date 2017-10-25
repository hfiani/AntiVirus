using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VirusManager : MonoBehaviour
{

	#region public variables
	[SerializeField] private bool killPlayerOnContact = true;
	[SerializeField] private bool canDieFromAge = false;
	[SerializeField] private float lifetime = 10.0f;
	[SerializeField] private GameObject redExplosionPrefab = null;
	[SerializeField] private GameObject greenExplosionPrefab = null;
	[SerializeField] private float maxHealth = 100;
	[SerializeField] private float damageTakenPerProjectile = 10;
	[SerializeField] private Color damagedColor;
	[SerializeField] private AudioClip damagedSound;
	[SerializeField] private float damagedVolume;
	#endregion

	#region private variables
	private float timerAge;
	private float timerUpdate;
	private float health;
	private bool isAlive;

	private bool hasLanded;
	private Color startBaseColor;
	private Color startFadeColor;
	private Color startEmiColor;
	private Color currentBaseColor;
	private Color currentFadeColor;
	private Color currentEmiColor;
	private Color damagedColorFade;
	private float sphereFadeAlpha;
	private GameObject firstInfectedBlock;
	private GameManager GM;
	private WaveController WC;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		GM = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager>();
		WC = GameObject.FindGameObjectWithTag ("GameController").GetComponent<WaveController>();

		isAlive = true;
		hasLanded = false;

		startBaseColor = transform.GetChild(0).GetComponent<Renderer> ().material.GetColor ("_Color");
		startFadeColor = transform.GetChild(1).GetComponent<Renderer> ().material.GetColor ("_Color");
		startEmiColor = transform.GetChild(0).GetComponent<Renderer> ().material.GetColor ("_EmissionColor");
		damagedColorFade = new Color (damagedColor.r, damagedColor.g, damagedColor.b, startFadeColor.a);
		currentBaseColor = startBaseColor;
		currentEmiColor = startEmiColor;
		health = maxHealth;
	}

	void Update()
	{
		if (Time.time - timerAge > lifetime && isAlive && hasLanded && canDieFromAge)
		{
			DeathFromAge ();
		}

		// re-infect first block
		if (hasLanded && Time.time - timerUpdate > 0.25 && isAlive && firstInfectedBlock != null)
		{
			firstInfectedBlock.GetComponent<InfectionRaycast> ().RemoveImmunity ();
			firstInfectedBlock.GetComponent<InfectionRaycast> ().CreateInfection(DateTime.Now);

			timerUpdate = Time.time;
		}
	}

	void OnTriggerEnter(Collider col)
	{
		// infect when land on ground
		if (col.gameObject.GetComponent<InfectionRaycast> () != null && !hasLanded && isAlive)
		{
			col.gameObject.GetComponent<InfectionRaycast> ().enabled = true;
			//col.gameObject.GetComponent<InfectionRaycast> ().Init ();
			col.gameObject.GetComponent<InfectionRaycast> ().RemoveImmunity();
			col.gameObject.GetComponent<InfectionRaycast> ().CreateInfection(DateTime.Now);
			firstInfectedBlock = col.gameObject;

			OnLanding ();
		}

		// take damage when hit by player's projectile
		if (col.gameObject.GetComponent<Projectile> () != null && isAlive)
		{
			updateHealth (-1*damageTakenPerProjectile);
		}

		// kill player on contact
		if (col.gameObject.CompareTag("Player") && isAlive)
		{
			GM.PlayerDeath ();
		}
	}
	#endregion

	#region private functions
	void updateHealth(float value)
	{
		health += value;

		if (value < 0) {
			GetComponent<AudioSource> ().PlayOneShot (damagedSound, damagedVolume);
		}

		if (health > maxHealth) {

			health = maxHealth;
		}

		if (health < 0f) {

			health = 0f;
			DeathFromPlayer ();
		}

		currentBaseColor = Color.Lerp(damagedColor,startBaseColor,health/maxHealth);
		currentFadeColor = Color.Lerp(damagedColorFade,startFadeColor,health/maxHealth);
		currentEmiColor = Color.Lerp(damagedColor,startEmiColor,health/maxHealth);

		//GetComponent<Renderer> ().material.SetColor ("_Color", currentBaseColor);
		//GetComponent<Renderer> ().material.SetColor ("_EmissionColor", currentEmiColor);

		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject child = transform.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> ())
			{
				if (i == 1) {
					child.GetComponent<Renderer> ().material.SetColor ("_Color", currentFadeColor);

				} else {
					child.GetComponent<Renderer> ().material.SetColor ("_Color", currentBaseColor);
				
				}
				child.GetComponent<Renderer> ().material.SetColor ("_EmissionColor", currentEmiColor);
			}
		}
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

		GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().IncrementVirusDeathNumber ();

		WC.removeVirusFromList (this.gameObject);
		Destroy (gameObject);
	}

	// when killed by player, virus will remove corruption
	void DeathFromPlayer()
	{
		isAlive = false;
		Instantiate (greenExplosionPrefab, transform.position, Quaternion.identity);
		if (firstInfectedBlock) {
			firstInfectedBlock.GetComponent<InfectionRaycast> ().infected = true;
			firstInfectedBlock.GetComponent<InfectionRaycast> ().RepairInfection ();
		}

		GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().IncrementVirusDeathNumber ();
		WC.removeVirusFromList (this.gameObject);
		Destroy (gameObject);
	}
	#endregion

	#region public region
	public Color GetCurrentBaseColor()
	{
		return currentBaseColor;
	}

	public Color GetCurrentEmiColor()
	{
		return currentEmiColor;
	}
	#endregion
}
