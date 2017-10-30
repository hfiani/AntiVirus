using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerManager : MonoBehaviour
{
	/* PlayerManager
	 * Everything related to the player: shooting, NPc buffs/debuffs, etc...
	 */

	#region serialized private variables
	[SerializeField] private Transform _ProjectileSpawnPoint = null;
	[SerializeField] private GameObject _ProjectilePrefab = null;

	[SerializeField] private float _FireDelay = 0.2f;
	[SerializeField] private float _ShootingCost = 1.0f;
	[SerializeField] private float _BaseEnergyRegen = 1.0f;
	[SerializeField] private float _BuffedEnergyRegen = 50.0f;
	[SerializeField] private float _DebuffedEnergyRegen = 0.0f;

	[SerializeField] private AudioClip _ShootingSound;
	[SerializeField] private float _ShootingVolume=1.0f;

	[SerializeField] private AnimationCurve buffSpeedCurve;
	[SerializeField] private float buffDuration = 2.0f;
	[SerializeField] private float buffMaxSpeed = 3.0f;
	[SerializeField] private float debuffMaxSpeed = 0.33f;
	[SerializeField] private float DistanceRaycast = 30.0f;
	#endregion

	#region private variables
	private float _timer;
	private float _energy = 0.0f;
	private float _maxEnergy = 100.0f;
	private float _energyRegen;
	private float _walkSpeedOriginValue;
	private float _runSpeedOriginValue;
	private float _buffFactor;
	private float _buffDuration;
	private DateTime _buffTime;
	private bool _buffEnabled;

	private AudioSource Audio;
	private FirstPersonController FPS;
	private UI_Manager UI;
	private Camera _cam = null;

	private GameObject _crosshairVirus;
	private GameObject _crosshairNoVirus;
	#endregion

	#region events
	void Start ()
	{
		FPS = GetComponent<FirstPersonController> ();

		_walkSpeedOriginValue = FPS.WalkingSpeed;
		_runSpeedOriginValue = FPS.RunningSpeed;

		_crosshairVirus = GameObject.Find ("Image_virus");
		_crosshairNoVirus = GameObject.Find ("Image_novirus");
		_crosshairNoVirus.GetComponent<Image> ().enabled = false;
	}

	// Update is called once per frame
	void Update ()
	{
		if (GameManager.Restart)
		{
			EnergyUpdate (_maxEnergy);
		}
		if (Input.GetMouseButton (0) && Time.time - _timer > _FireDelay && _energy > 10.0f)
		{
			Shoot ();
		}
			
		if (Physics.Raycast (transform.position, transform.forward, DistanceRaycast, 1 << 10))
		{
			
			_crosshairVirus.GetComponent<Image> ().enabled = true;
			_crosshairNoVirus.GetComponent<Image> ().enabled = false;
		}
		else
		{

			_crosshairVirus.GetComponent<Image> ().enabled = false;
			_crosshairNoVirus.GetComponent<Image> ().enabled = true;
		}

		EnergyUpdate (_energyRegen*Time.deltaTime);

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
				_energyRegen = _DebuffedEnergyRegen;
				UI.SetBuffText (false);
				UI.SetDeBuffText (true);
			}
			else
			{
				BuffSpeed (buffMaxSpeed, buffDuration);
				EnergyUpdate (_maxEnergy);
				_energyRegen = _BuffedEnergyRegen;
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

		_energyRegen = _BaseEnergyRegen;

		UI.Player = gameObject;
		UI.FOV = _cam.fieldOfView * 2; // here it is half the visible angle

		EnergyUpdate (_maxEnergy);
		UI.SetBuffText (false);
		UI.SetDeBuffText (false);

	}
	#endregion

	#region private functions
	void Shoot()
	{
		_timer = Time.time;
		Instantiate (_ProjectilePrefab, _ProjectileSpawnPoint.position, _ProjectileSpawnPoint.rotation);
		EnergyUpdate (-_ShootingCost);
		Audio.PlayOneShot (_ShootingSound, _ShootingVolume);
	}

	void BuffSpeed(float factorMax, float duration)
	{
		_buffFactor = factorMax;
		_buffTime = DateTime.Now;
		_buffDuration = duration * 1000;
		_buffEnabled = true;
	}

	void ManageBuff()
	{
		if (_buffEnabled)
		{
			if ((DateTime.Now - _buffTime).TotalMilliseconds < _buffDuration)
			{
				float time_step = buffSpeedCurve.Evaluate ((float)(DateTime.Now - _buffTime).TotalMilliseconds / _buffDuration);

				float factorNow = Mathf.Lerp (1, _buffFactor, time_step);
				FPS.WalkingSpeed = _walkSpeedOriginValue * factorNow;
				FPS.RunningSpeed =  _runSpeedOriginValue * factorNow;
			}
			else
			{
				_buffEnabled = false;
				_energyRegen = _BaseEnergyRegen;
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
