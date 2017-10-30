using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VirusManager : MonoBehaviour
{
	#region public variables
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
	private float _timerAge;
	private float _timerUpdate;
	private float _health;
	private bool _isAlive;

	private bool _hasLanded;
	private Color _startBaseColor;
	private Color _startFadeColor;
	private Color _startEmiColor;
	private Color _currentBaseColor;
	private Color _currentFadeColor;
	private Color _currentEmiColor;
	private Color _damagedColorFade;
	private float _sphereFadeAlpha;
	private GameObject _firstInfectedBlock;
	private GameManager GM;
	private WaveController WC;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		GM = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager>();
		WC = GameObject.FindGameObjectWithTag ("GameController").GetComponent<WaveController>();

		_isAlive = true;
		_hasLanded = false;

		_startBaseColor = transform.GetChild(0).GetComponent<Renderer> ().material.GetColor ("_Color");
		_startFadeColor = transform.GetChild(1).GetComponent<Renderer> ().material.GetColor ("_Color");
		_startEmiColor = transform.GetChild(0).GetComponent<Renderer> ().material.GetColor ("_EmissionColor");
		_damagedColorFade = new Color (damagedColor.r, damagedColor.g, damagedColor.b, _startFadeColor.a);
		_currentBaseColor = _startBaseColor;
		_currentEmiColor = _startEmiColor;
		_health = maxHealth;
	}

	void Update()
	{
		if (Time.time - _timerAge > lifetime && _isAlive && _hasLanded && canDieFromAge)
		{
			DeathFromAge ();
		}

		// re-infect first block
		if (_hasLanded && Time.time - _timerUpdate > 0.25 && _isAlive && _firstInfectedBlock != null)
		{
			_firstInfectedBlock.GetComponent<InfectionRaycast> ().RemoveImmunity ();
			_firstInfectedBlock.GetComponent<InfectionRaycast> ().CreateInfection(DateTime.Now);

			_timerUpdate = Time.time;
		}
	}

	void OnTriggerEnter(Collider col)
	{
		// infect when land on ground
		if (col.gameObject.GetComponent<InfectionRaycast> () != null && !_hasLanded && _isAlive)
		{
			col.gameObject.GetComponent<InfectionRaycast> ().enabled = true;
			//col.gameObject.GetComponent<InfectionRaycast> ().Init ();
			col.gameObject.GetComponent<InfectionRaycast> ().RemoveImmunity();
			col.gameObject.GetComponent<InfectionRaycast> ().CreateInfection(DateTime.Now);
			_firstInfectedBlock = col.gameObject;

			OnLanding ();
		}

		// take damage when hit by player's projectile
		if (col.gameObject.GetComponent<Projectile> () != null && _isAlive)
		{
			updateHealth (-1*damageTakenPerProjectile);
		}

		// kill player on contact
		if (col.gameObject.CompareTag("Player") && _isAlive)
		{
			GM.PlayerDeath ();
		}
	}
	#endregion

	#region private functions
	void updateHealth(float value)
	{
		_health += value;

		if (value < 0) {
			GetComponent<AudioSource> ().PlayOneShot (damagedSound, damagedVolume);
		}

		if (_health > maxHealth) {

			_health = maxHealth;
		}

		if (_health < 0f) {

			_health = 0f;
			DeathFromPlayer ();
		}

		_currentBaseColor = Color.Lerp(damagedColor,_startBaseColor,_health/maxHealth);
		_currentFadeColor = Color.Lerp(_damagedColorFade,_startFadeColor,_health/maxHealth);
		_currentEmiColor = Color.Lerp(damagedColor,_startEmiColor,_health/maxHealth);

		//GetComponent<Renderer> ().material.SetColor ("_Color", currentBaseColor);
		//GetComponent<Renderer> ().material.SetColor ("_EmissionColor", currentEmiColor);

		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject child = transform.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> ())
			{
				if (i == 1) {
					child.GetComponent<Renderer> ().material.SetColor ("_Color", _currentFadeColor);

				} else {
					child.GetComponent<Renderer> ().material.SetColor ("_Color", _currentBaseColor);
				
				}
				child.GetComponent<Renderer> ().material.SetColor ("_EmissionColor", _currentEmiColor);
			}
		}
	}

	void OnLanding()
	{
		_hasLanded = true;
		_timerAge = Time.time;
		_timerUpdate = Time.time;

		GetComponent<Rigidbody> ().isKinematic = true;
	}


	void DeathFromAge()
	{
		_isAlive = false;
		Instantiate (redExplosionPrefab, transform.position, Quaternion.identity);

		GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().IncrementVirusDeathNumber ();

		WC.removeVirusFromList (this.gameObject);
		Destroy (gameObject);
	}

	// when killed by player, virus will remove corruption
	void DeathFromPlayer()
	{
		_isAlive = false;
		Instantiate (greenExplosionPrefab, transform.position, Quaternion.identity);
		if (_firstInfectedBlock) {
			_firstInfectedBlock.GetComponent<InfectionRaycast> ().Infected = true;
			_firstInfectedBlock.GetComponent<InfectionRaycast> ().RepairInfection ();
		}

		GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().IncrementVirusDeathNumber ();
		WC.removeVirusFromList (this.gameObject);
		Destroy (gameObject);
	}
	#endregion

	#region public region
	public Color GetCurrentBaseColor()
	{
		return _currentBaseColor;
	}

	public Color GetCurrentEmiColor()
	{
		return _currentEmiColor;
	}
	#endregion
}
