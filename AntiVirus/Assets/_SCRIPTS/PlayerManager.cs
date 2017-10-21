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

	#endregion

	#region private variables

	private bool isShooting = false;
	private float timer;
	private float _energy = 0.0f;
	private float _maxEnergy = 100.0f;
	private float _energyBarWidth, _energyBarHeight;
	private GameObject _energyBar = null;

	#endregion



	// Use this for initialization
	void Start () {
		_energyBar = GameObject.FindGameObjectWithTag ("EnergyBar");
		_energyBarHeight = _energyBar.GetComponent<RectTransform>(). rect.height;
		_energyBarWidth = _energyBar.GetComponent<RectTransform>(). rect.width;
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

	}

	void EnergyUpdate(float value){

		_energy += value;

		if (_energy > _maxEnergy) {

			_energy = _maxEnergy;
		}

		if (_energy < 0f) {

			_energy = 0f;
		}


		_energyBar.GetComponent<RectTransform>(). sizeDelta = new Vector2 (_energyBarWidth * _energy / _maxEnergy,_energyBarHeight);

	}



}
