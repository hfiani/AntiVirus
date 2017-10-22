using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {


	#region public variables
	public Transform _ProjectileSpawnPoint = null;
	public GameObject _ProjectilePrefab = null;

	public float _FireDelay = 0.2f;
	public float _ShootingCost = 1.0f;
	public float _EnergyRegen = 1.0f;

	public AudioClip _ShootingSound;
	public float _ShootingVolume=1.0f;
	#endregion


	#region private variables
	private bool isShooting = false;
	private float timer;
	private float _energy = 0.0f;
	private float _maxEnergy = 100.0f;
	private UI_Manager UI;
	private AudioSource Audio;
	#endregion

	// do not use, use init() instead
	void Start () {


	}

	// called by GameManager
	public void Init(){

		this.gameObject.SetActive (true);
		UI = GameObject.FindGameObjectWithTag ("UI_Controller").GetComponent<UI_Manager> ();
		Audio = GetComponents<AudioSource>()[1];

		EnergyUpdate (_maxEnergy);
	}

	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButton (0) && Time.time - timer > _FireDelay && _energy>10.0f) {

			Shoot ();
		}

		EnergyUpdate (_EnergyRegen*Time.deltaTime);

	}

	void Shoot(){

		isShooting = true;

		timer = Time.time;

		Instantiate (_ProjectilePrefab, _ProjectileSpawnPoint.position, _ProjectileSpawnPoint.rotation);

		EnergyUpdate (-_ShootingCost);

		Audio.PlayOneShot (_ShootingSound, _ShootingVolume);

	}

	void EnergyUpdate(float value){

		_energy += value;

		if (_energy > _maxEnergy) {

			_energy = _maxEnergy;
		}

		if (_energy < 0f) {

			_energy = 0f;
		}

		UI.UpdateEnergyBar (_energy/_maxEnergy);




	}



}
