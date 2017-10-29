using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerManager : MonoBehaviour
{
	#region public variables
	public Transform _ProjectileSpawnPoint = null;
	public GameObject _ProjectilePrefab = null;

	public float _FireDelay = 0.2f;
	public float _ShootingCost = 1.0f;
	public float _BaseEnergyRegen = 1.0f;
	public float _BuffedEnergyRegen = 50.0f;
	public float _DebuffedEnergyRegen = 0.0f;

	public AudioClip _ShootingSound;
	public float _ShootingVolume=1.0f;

	public AnimationCurve buffSpeedCurve;
	public float buffDuration = 2.0f;
	public float buffMaxSpeed = 3.0f;
	public float debuffMaxSpeed = 0.33f;


	#endregion

	#region serialized private variables
	#endregion

	#region private variables
	private bool isShooting = false;
	private bool isBuffed = false;
	private bool isDebuffed = false;
	private float timer;
	private float _energy = 0.0f;
	private float _maxEnergy = 100.0f;
	private float _EnergyRegen;
	private float walkSpeedOriginValue;
	private float runSpeedOriginValue;
	private float _buffFactor;
	private float _buffDuration;
	private DateTime _buffTime;
	private bool _buffEnabled;

	private AudioSource Audio;
	private FirstPersonController FPS;
	private WaveController WC;
	private UI_Manager UI;

	private Camera _cam = null;

	#endregion

	#region events
	// do not use, use init() instead
	void Start ()
	{
		WC =  GameObject.FindGameObjectWithTag ("GameController").GetComponent<WaveController>();

		FPS = GetComponent<FirstPersonController> ();

		walkSpeedOriginValue = FPS.WalkingSpeed;
		runSpeedOriginValue = FPS.RunningSpeed;
	
	
	}

	// Update is called once per frame
	void Update ()
	{
		if (GameManager.Restart)
		{
			EnergyUpdate (_maxEnergy);
		}
		if (Input.GetMouseButton (0) && Time.time - timer > _FireDelay && _energy > 10.0f)
		{
			Shoot ();
		}

	

		EnergyUpdate (_EnergyRegen*Time.deltaTime);

		ManageBuff ();
	}

	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "NPC")
		{
			if (c.GetComponent<NPCInfection> ().infected)
			{
				BuffSpeed (debuffMaxSpeed, buffDuration);
				EnergyUpdate (-_maxEnergy);
				_EnergyRegen = _DebuffedEnergyRegen;
				isDebuffed = true;
				isBuffed = false;
				UI.SetBuffText (false);
				UI.SetDeBuffText (true);
			}
			else
			{
				BuffSpeed (buffMaxSpeed, buffDuration);
				EnergyUpdate (_maxEnergy);
				_EnergyRegen = _BuffedEnergyRegen;
				isDebuffed = false;
				isBuffed = true;
				UI.SetBuffText (true);
				UI.SetDeBuffText (false);
			}
		}
	}
	#endregion

	#region public functions
	// called by GameManager
	public void Init()
	{
		this.gameObject.SetActive (true);
		UI = GameObject.FindGameObjectWithTag ("UI_Controller").GetComponent<UI_Manager> ();
		_cam = transform.GetChild (0).GetComponent<Camera> ();
		Audio = GetComponents<AudioSource>()[1];

		_EnergyRegen = _BaseEnergyRegen;

		UI.Player = gameObject;
		UI.FOV = _cam.fieldOfView * 2; // here it is half the visible angle

		EnergyUpdate (_maxEnergy);
	}

	public void BuffSpeed(float factorMax, float duration)
	{
		_buffFactor = factorMax;
		_buffTime = DateTime.Now;
		_buffDuration = duration * 1000;
		_buffEnabled = true;
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

	void ManageBuff()
	{
		if (_buffEnabled)
		{
			if ((DateTime.Now - _buffTime).TotalMilliseconds < _buffDuration)
			{
				float time_step = buffSpeedCurve.Evaluate ((float)(DateTime.Now - _buffTime).TotalMilliseconds / _buffDuration);

				float factorNow = Mathf.Lerp (1, _buffFactor, time_step);
				FPS.WalkingSpeed = walkSpeedOriginValue * factorNow;
				FPS.RunningSpeed =  runSpeedOriginValue * factorNow;
			}
			else
			{
				_buffEnabled = false;
				isDebuffed = false;
				isBuffed = false;
				_EnergyRegen = _BaseEnergyRegen;
				UI.SetBuffText (false);
				UI.SetDeBuffText (false);
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
