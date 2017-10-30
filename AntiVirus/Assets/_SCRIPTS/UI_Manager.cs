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
	[SerializeField] private AnimationCurve InfectionBarCurve;
	[SerializeField] private GameObject Virus = null;
	[SerializeField] private GameObject CompassBase = null;
	[SerializeField] private GameObject ObjectiveCanvas;
	[SerializeField] private GameObject EnergyBar = null;
	[SerializeField] private GameObject InfectionBar = null;
	[SerializeField] private GameObject InfectionBarGroup = null;
	[SerializeField] private GameObject RespawnScreen = null;
	[SerializeField] private GameObject StartScreen = null;
	[SerializeField] private GameObject GameOverScreen = null;
	[SerializeField] private GameObject GameOverRetryScreen = null;
	[SerializeField] private GameObject Crosshair = null;
	[SerializeField] private GameObject Compass = null;
	[SerializeField] private string TextLevel = "LEVEL {1} STARTING ...";
	[SerializeField] private GameObject BuffText = null;
	[SerializeField] private GameObject DebuffText = null;
	[SerializeField] private GameObject InfectionCubes = null;
	[SerializeField] private GameObject AlertMessageText = null;
	[SerializeField] private GameObject VirusNumber = null;
	[SerializeField] private GameObject WaveCompleteText = null;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		WC = GameObject.FindGameObjectWithTag ("GameController").GetComponent<WaveController> ();
		_objective = GameObject.FindGameObjectWithTag ("Objective");

		_energyBarWidth = EnergyBar.GetComponent<RectTransform>().rect.width;
		_energyBarHeight = EnergyBar.GetComponent<RectTransform>().rect.height;

		_compassWidth = CompassBase.GetComponent<RectTransform> ().rect.width;

		_infectionBarWidth = InfectionBar.GetComponent<RectTransform>().rect.width;
		_infectionBarHeight = InfectionBar.GetComponent<RectTransform>().rect.height;
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
		StartScreen.SetActive (false);
		EnergyBar.transform.parent.gameObject.SetActive (false);
		RespawnScreen.SetActive (false);
		Crosshair.SetActive (false);
		GameOverScreen.SetActive (false);
		GameOverRetryScreen.SetActive (false);
		Compass.SetActive (false);
		BuffText.SetActive (false);
		DebuffText.SetActive (false);
		AlertMessageText.SetActive (false);
		WaveCompleteText.SetActive (false);
	}

	void MoveCompass()
	{
		if (_player == null)
		{
			return;
		}

		if (ObjectiveCanvas != null && _objective != null)
		{
			SetOnCompass (ObjectiveCanvas, _objective.transform.position - _player.transform.position);
		}

		foreach (GameObject virus in _virusCanvasList)
		{
			Destroy (virus);
		}
		_virusCanvasList.Clear ();

		foreach (GameObject virus in WC.GetViruses())
		{
			GameObject virus_canvas = Instantiate (Virus, Virus.transform.parent, true);
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
			InfectionBarGroup.SetActive (false);
		} else {
			InfectionBarGroup.SetActive (true);
		}
		percent = InfectionBarCurve.Evaluate (percent);
		InfectionBar.GetComponent<RectTransform>(). sizeDelta = new Vector2 (_infectionBarWidth * percent, _infectionBarHeight);
	}

	public void UpdateEnergyBar(float percent)
	{

		EnergyBar.GetComponent<RectTransform>(). sizeDelta = new Vector2 (_energyBarWidth * percent, _energyBarHeight);
	}

	public void SetEnergyBar(bool state)
	{

		EnergyBar.transform.parent.gameObject.SetActive (state);
	}

	public void SetRespawnScreen(bool state)
	{

		RespawnScreen.SetActive (state);
	}

	public void SetLevelText()
	{
		GameObject text_level = StartScreen.transform.GetChild (0).gameObject;
		text_level.GetComponent<Text> ().text = TextLevel.Replace ("{1}", (++GameManager.Level).ToString());

		GameObject text_level2 = StartScreen.transform.GetChild(1).gameObject;
		text_level2.GetComponent<Text>().text = text_level.GetComponent<Text>().text;
	}

	public void SetWaveCompleteScreen(bool state)
	{
		WaveCompleteText.SetActive (state);
	}

	public void SetStartScreen(bool state)
	{
		StartScreen.SetActive (state);
	}

	public void SetAlertText(bool state)
	{
		AlertMessageText.SetActive (state);
	}

	public void SetCrosshair(bool state)
	{

		Crosshair.SetActive (state);
	}

	public void SetCompass(bool state)
	{

		Compass.SetActive (state);
	}

	public void SetGameOverScreen(bool state)
	{
		GameOverScreen.SetActive (state);
	}

	public void SetGameOverRetryScreen(bool state)
	{
		GameOverRetryScreen.SetActive (state);

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void UpdateCubeText(int n){
		InfectionCubes.GetComponent<Text> ().text = "CORRUPTED CUBES : " + n;
	}

	public void UpdateVirusNumberText(int n){
		VirusNumber.GetComponent<Text> ().text = "VIRUS : " + n;
	}

	public void SetBuffText(bool state)
	{
		BuffText.SetActive (state);
	}

	public void SetDeBuffText(bool state)
	{
		DebuffText.SetActive (state);
	}
	#endregion
}
