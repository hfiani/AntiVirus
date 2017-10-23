using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour {

	#region private variables
	private float _energyBarWidth, _energyBarHeight;
	#endregion

	#region public variables
	public string _textLevel = "LEVEL {1} STARTING ...";
	public GameObject _energyBar = null;
	public GameObject _respawnScreen = null;
	public GameObject _startScreen = null;
	public GameObject _gameOverScreen = null;
	public GameObject _crosshair = null;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		_energyBarHeight = _energyBar.GetComponent<RectTransform>(). rect.height;
		_energyBarWidth = _energyBar.GetComponent<RectTransform>(). rect.width;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	#endregion

	#region public functions
	public void HideAll()
	{
		_startScreen.SetActive (false);
		_energyBar.transform.parent.gameObject.SetActive (false);
		_respawnScreen.SetActive (false);
		_crosshair.SetActive (false);
		_gameOverScreen.SetActive (false);
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

	public void SetLevelText() {
		GameObject text_level = _startScreen.transform.GetChild (0).gameObject;
		text_level.GetComponent<Text> ().text = _textLevel.Replace ("{1}", (++GameManager.Level).ToString());
	}

	public void SetStartScreen(bool state) {
		_startScreen.SetActive (state);
	}

	public void SetCrosshair(bool state) {

		_crosshair.SetActive (state);
	}
	public void SetGameOverScreen(bool state) {

		_gameOverScreen.SetActive (state);
	}
	#endregion
}
