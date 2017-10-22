using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour {

	private float _energyBarWidth, _energyBarHeight;
	public GameObject _energyBar = null;
	public GameObject _respawnScreen = null;
	public GameObject _startScreen = null;
	public GameObject _crosshair = null;

	// Use this for initialization
	void Start () {


		_energyBarHeight = _energyBar.GetComponent<RectTransform>(). rect.height;
		_energyBarWidth = _energyBar.GetComponent<RectTransform>(). rect.width;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void HideAll(){

		_startScreen.SetActive (false);
		_energyBar.transform.parent.gameObject.SetActive (false);
		_respawnScreen.SetActive (false);
		_crosshair.SetActive (false);
	}


	public void UpdateEnergyBar(float percent){

		_energyBar.GetComponent<RectTransform>(). sizeDelta = new Vector2 (_energyBarWidth * percent,_energyBarHeight);
	}

	public void SetEnergyBar(bool state){

		_energyBar.transform.parent.gameObject.SetActive (state);
	}

	public void SetRespawnScreen(bool state){

		_respawnScreen.SetActive (state);
	}

	public void SetStartScreen(bool state){

		_startScreen.SetActive (state);
	}

	public void SetCrosshair(bool state){

		_crosshair.SetActive (state);
	}
}
