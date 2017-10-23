using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerManager : MonoBehaviour
{
	#region public variables
	public Transform _ProjectileSpawnPoint = null;
	public GameObject _ProjectilePrefab = null;

	public float _FireDelay = 0.2f;
	public float _ShootingCost = 1.0f;
	public float _EnergyRegen = 1.0f;

	public AudioClip _ShootingSound;
	public float _ShootingVolume=1.0f;

	public AnimationCurve speedCurve;
	#endregion


	#region private variables
	private bool isShooting = false;
	private float timer;
	private float _energy = 0.0f;
	private float _maxEnergy = 100.0f;
	private UI_Manager UI;
	private AudioSource Audio;

	private float speedOriginValue;

	private float _buffFactor;
	private float _buffDuration;
	private DateTime _buffTime;
	private bool _buffEnabled;
	private FirstPersonController FPS;
	#endregion

	#region events
	// do not use, use init() instead
	void Start ()
	{
		FPS = GetComponent<FirstPersonController> ();
		speedOriginValue = FPS.WalkingSpeed;
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButton (0) && Time.time - timer > _FireDelay && _energy > 10.0f)
		{
			Shoot ();
		}

		EnergyUpdate (_EnergyRegen*Time.deltaTime);

		changeWalkingSpeed ();
	}

	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "NPC")
		{
			BuffSpeed (5, 1);
		}
	}
	#endregion

	#region public functions
	// called by GameManager
	public void Init()
	{
		this.gameObject.SetActive (true);
		UI = GameObject.FindGameObjectWithTag ("UI_Controller").GetComponent<UI_Manager> ();
		Audio = GetComponents<AudioSource>()[1];

		EnergyUpdate (_maxEnergy);
	}

	public void BuffSpeed(float factorMax, float duration)
	{
		if (!_buffEnabled)
		{
			_buffFactor = factorMax;
			_buffTime = DateTime.Now;
			_buffDuration = duration * 1000;
			_buffEnabled = true;
		}
	}
	#endregion

	#region private functions
	void Shoot()
	{
		isShooting = true;

		timer = Time.time;

		Instantiate (_ProjectilePrefab, _ProjectileSpawnPoint.position, _ProjectileSpawnPoint.rotation);

		EnergyUpdate (-_ShootingCost);

		Audio.PlayOneShot (_ShootingSound, _ShootingVolume);
	}

	void changeWalkingSpeed()
	{
		if (_buffEnabled)
		{
			if ((DateTime.Now - _buffTime).TotalMilliseconds < _buffDuration)
			{
				float time_step = speedCurve.Evaluate ((float)(DateTime.Now - _buffTime).TotalMilliseconds / _buffDuration);

				float factorNow = Mathf.Lerp (1, _buffFactor, time_step);
				FPS.WalkingSpeed = speedOriginValue * factorNow;
			}
			else
			{
				_buffEnabled = false;
			}
		}
	}

	void EnergyUpdate(float value)
	{
		_energy += value;

		if (_energy > _maxEnergy)
		{
			_energy = _maxEnergy;
		}

		if (_energy < 0f)
		{
			_energy = 0f;
		}

		UI.UpdateEnergyBar (_energy/_maxEnergy);
	}
	#endregion
}
