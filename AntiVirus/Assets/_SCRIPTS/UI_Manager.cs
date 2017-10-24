using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour {

	#region private variables
	private float _energyBarWidth, _energyBarHeight;
	private List<GameObject> _virusCanvasList = new List<GameObject>();
	private WaveController WC;
	private GameObject _player;
	private float _fov = -1;
	private float _compassWidth = -1;
	private GameObject _virus = null;
	#endregion

	#region public variables
	public string _textLevel = "LEVEL {1} STARTING ...";
	public GameObject _energyBar = null;
	public GameObject _respawnScreen = null;
	public GameObject _startScreen = null;
	public GameObject _gameOverScreen = null;
	public GameObject _crosshair = null;
	public GameObject _compass = null;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		WC = GameObject.FindGameObjectWithTag ("GameController").GetComponent<WaveController> ();
		_energyBarHeight = _energyBar.GetComponent<RectTransform>(). rect.height;
		_energyBarWidth = _energyBar.GetComponent<RectTransform>(). rect.width;
	}
	
	// Update is called once per frame
	void Update ()
	{
		MoveCompass ();
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
		_compass.SetActive (false);
	}

	void MoveCompass()
	{
		if (_player == null ||
		   _fov == -1 ||
		   _compassWidth == -1 ||
		   _virus == null)
		{
			if (_player == null)
			{
				_player = GameObject.FindGameObjectWithTag ("Player");
			}

			if (Camera.main != null && _fov == -1)
			{
				_fov = Camera.main.fieldOfView * 2; // here it is half the visible angle
			}

			if (_compassWidth == -1 && GameObject.Find ("CompassBase") != null && GameObject.Find ("CompassBase").GetComponent<RectTransform> () != null)
			{
				_compassWidth = GameObject.Find ("CompassBase").GetComponent<RectTransform> ().rect.width;
			}

			if (GameObject.Find ("Virus") != null)
			{
				_virus = GameObject.Find ("Virus");
			}
			return;
		}

		/*
		SetOnCompass (GameObject.Find("North"), Vector3.forward);
		SetOnCompass (GameObject.Find("South"), Vector3.back);
		SetOnCompass (GameObject.Find("West"), Vector3.left);
		SetOnCompass (GameObject.Find("East"), Vector3.right);
		*/

		GameObject objective = GameObject.FindGameObjectWithTag ("Objective");
		GameObject objective_canvas = GameObject.Find ("Objective");
		if (objective_canvas != null && objective != null)
		{
			SetOnCompass (objective_canvas, objective.transform.position - _player.transform.position);
		}

		foreach (GameObject virus in _virusCanvasList)
		{
			Destroy (virus);
		}
		_virusCanvasList.Clear ();

		foreach (GameObject virus in WC.GetViruses())
		{
			GameObject virus_canvas = Instantiate (_virus, _virus.transform.parent, true);
			_virusCanvasList.Add (virus_canvas);
			virus_canvas.GetComponent<Image> ().enabled = true;
			if (virus_canvas != null && virus != null)
			{
				SetOnCompass (virus_canvas, virus.transform.position - _player.transform.position);
			}
		}
	}

	void SetOnCompass(GameObject obj, Vector3 direction)
	{
		if (Camera.main != null)
		{
			if (_player != null)
			{
				Vector3 xz_plan = Vector3.forward + Vector3.right;
				Vector3 player_forward_xz = Vector3.Scale (_player.transform.forward, xz_plan);
				Vector3 direction_xz = Vector3.Scale (direction, xz_plan);
				RectTransform rect = obj.GetComponent<RectTransform> ();
				Image img = obj.GetComponent<Image> ();

				float north_angle = Vector3.Angle (player_forward_xz, direction_xz);
				Vector3 cross = Vector3.Cross(player_forward_xz, direction_xz);
				if (cross.y < 0) north_angle = -north_angle;
				if (north_angle < _fov / 2 && north_angle > -_fov / 2)
				{
					img.enabled = true;
					rect.localPosition = 
						new Vector3(
							north_angle * _compassWidth / _fov,
							rect.localPosition.y,
							rect.localPosition.z);
					float alpha = Mathf.Clamp (20.0f / Vector3.Magnitude (direction_xz), 0.33f, 1.0f);
					img.color = new Color(
						img.color.r,
						img.color.g,
						img.color.b,
						alpha);
				}
				else
				{
					obj.GetComponent<Image> ().enabled = false;
				}
			}
		}
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

		GameObject text_level2 = _startScreen.transform.GetChild(1).gameObject;
		text_level2.GetComponent<Text>().text = text_level.GetComponent<Text>().text;
	}

	public void SetStartScreen(bool state) {
		_startScreen.SetActive (state);
	}

	public void SetCrosshair(bool state) {

		_crosshair.SetActive (state);
	}
	public void SetCompass(bool state) {

		_compass.SetActive (state);
	}
	public void SetGameOverScreen(bool state) {

		_gameOverScreen.SetActive (state);
	}
	#endregion
}
