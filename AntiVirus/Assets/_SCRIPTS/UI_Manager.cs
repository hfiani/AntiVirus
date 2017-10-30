using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
	#region private variables
	private float _energyBarWidth, _energyBarHeight;
	private float _infectionBarWidth, _infectionBarHeight;
	private List<GameObject> _virusCanvasList = new List<GameObject>();
	private float _compassWidth = -1;
	private WaveController WC;
	private GameObject _objective;
	private GameObject _player;
	private float _fov = -1;
	#endregion

	#region serialized private variables
	[SerializeField] private AnimationCurve infectionBarCurve;
	[SerializeField] private GameObject _virus = null;
	[SerializeField] private GameObject _compassBase = null;
	[SerializeField] private GameObject _objectiveCanvas;
	[SerializeField] private GameObject _energyBar = null;
	[SerializeField] private GameObject _infectionBar = null;
	[SerializeField] private GameObject _infectionBarGroup = null;
	[SerializeField] private GameObject _respawnScreen = null;
	[SerializeField] private GameObject _startScreen = null;
	[SerializeField] private GameObject _gameOverScreen = null;
	[SerializeField] private GameObject _gameOverRetryScreen = null;
	[SerializeField] private GameObject _crosshair = null;
	[SerializeField] private GameObject _compass = null;
	[SerializeField] private string _textLevel = "LEVEL {1} STARTING ...";
	[SerializeField] private GameObject _bufftext = null;
	[SerializeField] private GameObject _debufftext = null;
	[SerializeField] private GameObject _infectionCubes = null;
	[SerializeField] private GameObject _alertMessageText = null;
	[SerializeField] private GameObject _virusNumber = null;
	[SerializeField] private GameObject _waveCompleteText = null;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		WC = GameObject.FindGameObjectWithTag ("GameController").GetComponent<WaveController> ();
		_objective = GameObject.FindGameObjectWithTag ("Objective");

		_energyBarWidth = _energyBar.GetComponent<RectTransform>().rect.width;
		_energyBarHeight = _energyBar.GetComponent<RectTransform>().rect.height;

		_compassWidth = _compassBase.GetComponent<RectTransform> ().rect.width;

		_infectionBarWidth = _infectionBar.GetComponent<RectTransform>().rect.width;
		_infectionBarHeight = _infectionBar.GetComponent<RectTransform>().rect.height;
	}
	
	// Update is called once per frame
	void Update ()
	{
		MoveCompass ();
	}
	#endregion

	#region public fields
	public GameObject Player
	{
		set
		{
			_player = value;
		}
	}

	public float FOV
	{
		set
		{
			_fov = value;
		}
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
		_gameOverRetryScreen.SetActive (false);
		_compass.SetActive (false);
		_bufftext.SetActive (false);
		_debufftext.SetActive (false);
		_alertMessageText.SetActive (false);
		_waveCompleteText.SetActive (false);
	}

	void MoveCompass()
	{
		if (_player == null)
		{
			return;
		}

		if (_objectiveCanvas != null && _objective != null)
		{
			SetOnCompass (_objectiveCanvas, _objective.transform.position - _player.transform.position);
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
						alpha
					);
				}
				else
				{
					obj.GetComponent<Image> ().enabled = false;
				}
			}
		}
	}

	public void UpdateInfectionBar(float percent)
	{
		if (percent < 0.10) {
			_infectionBarGroup.SetActive (false);
		} else {
			_infectionBarGroup.SetActive (true);
		}
		percent = infectionBarCurve.Evaluate (percent);
		_infectionBar.GetComponent<RectTransform>(). sizeDelta = new Vector2 (_infectionBarWidth * percent, _infectionBarHeight);
	}

	public void UpdateEnergyBar(float percent)
	{

		_energyBar.GetComponent<RectTransform>(). sizeDelta = new Vector2 (_energyBarWidth * percent, _energyBarHeight);
	}

	public void SetEnergyBar(bool state)
	{

		_energyBar.transform.parent.gameObject.SetActive (state);
	}

	public void SetRespawnScreen(bool state)
	{

		_respawnScreen.SetActive (state);
	}

	public void SetLevelText()
	{
		GameObject text_level = _startScreen.transform.GetChild (0).gameObject;
		text_level.GetComponent<Text> ().text = _textLevel.Replace ("{1}", (++GameManager.Level).ToString());

		GameObject text_level2 = _startScreen.transform.GetChild(1).gameObject;
		text_level2.GetComponent<Text>().text = text_level.GetComponent<Text>().text;
	}

	public void SetWaveCompleteScreen(bool state)
	{
		_waveCompleteText.SetActive (state);
	}

	public void SetStartScreen(bool state)
	{
		_startScreen.SetActive (state);
	}

	public void SetAlertText(bool state)
	{
		_alertMessageText.SetActive (state);
	}

	public void SetCrosshair(bool state)
	{

		_crosshair.SetActive (state);
	}

	public void SetCompass(bool state)
	{

		_compass.SetActive (state);
	}

	public void SetGameOverScreen(bool state)
	{
		_gameOverScreen.SetActive (state);
	}

	public void SetGameOverRetryScreen(bool state)
	{
		_gameOverRetryScreen.SetActive (state);

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void UpdateCubeText(int n){
		_infectionCubes.GetComponent<Text> ().text = "CORRUPTED CUBES : " + n;
	}

	public void UpdateVirusNumberText(int n){
		_virusNumber.GetComponent<Text> ().text = "VIRUS : " + n;
	}

	public void SetBuffText(bool state)
	{
		_bufftext.SetActive (state);
	}

	public void SetDeBuffText(bool state)
	{
		_debufftext.SetActive (state);
	}
	#endregion
}
