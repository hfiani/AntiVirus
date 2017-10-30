using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VirusManager : MonoBehaviour
{
	#region public variables
	[SerializeField] private bool CanDieFromAge = false;
	[SerializeField] private float Lifetime = 10.0f;
	[SerializeField] private GameObject RedExplosionPrefab = null;
	[SerializeField] private GameObject GreenExplosionPrefab = null;
	[SerializeField] private float MaxHealth = 100;
	[SerializeField] private float DamageTakenPerProjectile = 10;
	[SerializeField] private Color DamagedColor;
	[SerializeField] private AudioClip DamagedSound;
	[SerializeField] private float DamagedVolume;
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
		_damagedColorFade = new Color (DamagedColor.r, DamagedColor.g, DamagedColor.b, _startFadeColor.a);
		_currentBaseColor = _startBaseColor;
		_currentEmiColor = _startEmiColor;
		_health = MaxHealth;
	}

	void Update()
	{
		if (Time.time - _timerAge > Lifetime && _isAlive && _hasLanded && CanDieFromAge)
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
			updateHealth (-1*DamageTakenPerProjectile);
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
			GetComponent<AudioSource> ().PlayOneShot (DamagedSound, DamagedVolume);
		}

		if (_health > MaxHealth) {

			_health = MaxHealth;
		}

		if (_health < 0f) {

			_health = 0f;
			DeathFromPlayer ();
		}

		_currentBaseColor = Color.Lerp(DamagedColor,_startBaseColor,_health/MaxHealth);
		_currentFadeColor = Color.Lerp(_damagedColorFade,_startFadeColor,_health/MaxHealth);
		_currentEmiColor = Color.Lerp(DamagedColor,_startEmiColor,_health/MaxHealth);

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
		Instantiate (RedExplosionPrefab, transform.position, Quaternion.identity);

		GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().IncrementVirusDeathNumber ();

		WC.removeVirusFromList (this.gameObject);
		Destroy (gameObject);
	}

	// when killed by player, virus will remove corruption
	void DeathFromPlayer()
	{
		_isAlive = false;
		Instantiate (GreenExplosionPrefab, transform.position, Quaternion.identity);
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
