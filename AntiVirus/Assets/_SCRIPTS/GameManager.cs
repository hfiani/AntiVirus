using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	#region public static variables
	public static int Level = 0;
	public static int InfectedBlocks = 0;
	public static int TotalBlocks = 0;
	#endregion

	#region public variables
	public GameObject PlayerPrefab;
	public GameObject GhostPlayerPrefab;
	public int LevelMax = 5;

	public GameObject PlayerSpawn;
	public AudioClip DeathSound;
	public AudioClip SpawnSound;
	public float RespawnDelay = 4.0f;
	public float GameOverDelay = 4.0f;
	public float StartLevelDelay = 2.0f;
	#endregion

	#region private variables
	private GameObject Player;
	private GameObject PlayerGhost;
	private float respawnTimer;
	private float gameOverTimer;
	private float startLevelTimer;
	private bool playerIsActive;
	private bool playerRespawning = false;
	private bool gameIsOver = false;
	private bool gameIsWon = false;
	private bool levelHasStarted = false;
	private UI_Manager UI;
	private GameObject LevelMusic;
	private GameObject StartCamera;
	private GameObject Objective;
	private WaveController WC;
	private List<GameObject> VirusCanvasList = new List<GameObject>();
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		UI = GameObject.FindGameObjectWithTag ("UI_Controller").GetComponent<UI_Manager> ();
		WC = GetComponent<WaveController> ();

		Objective = GameObject.FindGameObjectWithTag ("Objective");
		LevelMusic = GameObject.FindGameObjectWithTag ("Music");
		StartCamera = GameObject.FindGameObjectWithTag ("StartCamera");

		LevelMusic.GetComponent<AudioSource> ().Play ();

		startLevelTimer = Time.time;

		UI.HideAll ();

		UI.SetStartScreen (true);
		UI.SetLevelText ();
	}
	
	// Update is called once per frame
	void Update () {

		if (playerRespawning && Time.time - respawnTimer > RespawnDelay) {
			SpawnPlayer ();
		}

		if (gameIsOver && Time.time - gameOverTimer > GameOverDelay) {
			SceneManager.LoadScene ("gameover");
		}

		if (!levelHasStarted && Time.time - startLevelTimer > StartLevelDelay) {
			StartLevel();
		}

		if (levelHasStarted)
		{
			CheckForVictory ();
		}

		MoveCompass ();
	}

	void OnGUI()
	{
		GUI.TextArea(new Rect(50, 50, 100, 30), (100 * InfectedBlocks / TotalBlocks).ToString() + "%");
	}
	#endregion

	#region private functions
	void MoveCompass()
	{
		GameObject player = GameObject.FindGameObjectWithTag ("Player");

		if (player == null)
		{
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
			SetOnCompass (objective_canvas, objective.transform.position - player.transform.position);
		}

		foreach (GameObject virus in VirusCanvasList)
		{
			Destroy (virus);
		}
		VirusCanvasList.Clear ();

		foreach (GameObject virus in WC.GetViruses())
		{
			GameObject virus_canvas = Instantiate (GameObject.Find ("Virus"), GameObject.Find ("Virus").transform.parent, true);
			VirusCanvasList.Add (virus_canvas);
			virus_canvas.GetComponent<Image> ().enabled = true;
			if (virus_canvas != null && virus != null)
			{
				SetOnCompass (virus_canvas, virus.transform.position - player.transform.position);
			}
		}
	}

	void SetOnCompass(GameObject obj, Vector3 direction)
	{
		if (Camera.main != null)
		{
			float FOV = Camera.main.fieldOfView * 2; // here it is half the visible angle
			float CompassWidth = GameObject.Find("CompassBase").GetComponent<RectTransform> ().rect.width;

			GameObject player = GameObject.FindGameObjectWithTag ("Player");
			if (player != null)
			{
				Vector3 xz_plan = Vector3.forward + Vector3.right;
				Vector3 player_forward_xz = Vector3.Scale (player.transform.forward, xz_plan);
				Vector3 direction_xz = Vector3.Scale (direction, xz_plan);

				float north_angle = Vector3.Angle (player_forward_xz, direction_xz);
				Vector3 cross = Vector3.Cross(player_forward_xz, direction_xz);
				if (cross.y < 0) north_angle = -north_angle;
				if (north_angle < FOV / 2 && north_angle > -FOV / 2)
				{
					obj.GetComponent<Image> ().enabled = true;
					obj.GetComponent<RectTransform> ().localPosition = 
						new Vector3(
							north_angle * CompassWidth / FOV,
							obj.GetComponent<RectTransform> ().localPosition.y,
							obj.GetComponent<RectTransform> ().localPosition.z);
					float alpha = Mathf.Clamp (20.0f / Vector3.Magnitude (direction_xz), 0.33f, 1.0f);
					obj.GetComponent<Image> ().color = new Color(
						obj.GetComponent<Image> ().color.r,
						obj.GetComponent<Image> ().color.g,
						obj.GetComponent<Image> ().color.b,
						alpha);
				}
				else
				{
					obj.GetComponent<Image> ().enabled = false;
				}
			}
		}
	}

	void StartLevel()
	{
		UI.SetStartScreen (false);
		StartCamera.GetComponent<AudioListener> ().enabled = false;

		SpawnPlayer ();

		levelHasStarted = true;

		float virusSpawnDelay = 5.0f / Level;
		int virusNumber = Level;
		WC.SetVirusParameters (virusSpawnDelay, virusNumber);
		WC.StartWave ();

		//virusSpawnTimer = Time.time;

		//SpawnVirusAtRandomSpawner ();
	}

	void TriggerRespawn()
	{

		UI.SetRespawnScreen (true);

		respawnTimer = Time.time;

		playerRespawning = true;
	}

	void SpawnPlayer()
	{
		Debug.Log ("Spawning Player");

		playerRespawning = false;
		playerIsActive = true;

		UI.SetRespawnScreen (false);
		UI.SetCrosshair (true);
		UI.SetEnergyBar (true);
		UI.SetCompass (true);

		if (Player != null) {
			Destroy (Player.gameObject);
		}
		if (PlayerGhost != null) {
			Destroy (PlayerGhost.gameObject);
		}

		Player = Instantiate (PlayerPrefab, PlayerSpawn.transform.position, PlayerSpawn.transform.rotation);
		Player.GetComponent<PlayerManager> ().Init ();

		// add some text & sound ...


		GetComponent<AudioSource> ().PlayOneShot (SpawnSound,1.0f);
	}
	#endregion

	#region public functions
	public void GameWon()
	{
		if (gameIsWon)
		{
			return;
		}
		Debug.Log ("Game Won");
		gameIsWon = true;
	}

	public void GameOver()
	{
		if (gameIsOver)
		{
			return;
		}
		Debug.Log ("Game Over");
		UI.HideAll ();
		UI.SetGameOverScreen (true);
		gameIsOver = true;
		gameOverTimer = Time.time;
		ShowGameOver ();
		GameManager.Level--;
		//SceneManager.LoadScene ("gameover");
	}

	public void CheckForVictory ()
	{
		if (WC.VirusNumberKilled == WC.VirusNumber && InfectedBlocks == 0)
		{
			if (Level == LevelMax)
			{
				GameWon ();
			}
			else
			{
				SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
			}
		}
	}

	public void PlayerDeath()
	{
		if (!playerIsActive) {
			return;
		}
		Debug.Log ("Player Death");

		playerIsActive = false;
		UI.SetCrosshair (false);
		UI.SetEnergyBar (false);

		Destroy (Player.gameObject);

		PlayerGhost = Instantiate (GhostPlayerPrefab, Player.transform.position, Player.transform.GetChild(0). rotation);
		PlayerGhost.GetComponent<PlayerGhost> ().TriggerTravelToPoint (PlayerSpawn, RespawnDelay);

		//LevelMusic.GetComponent<AudioSource> ().Stop ();
		GetComponent<AudioSource> ().PlayOneShot (DeathSound,1.0f);

		TriggerRespawn ();
	}

	public void ShowGameOver()
	{
		playerIsActive = false;
		playerRespawning = false;

		if (Player) {
			Destroy (Player.gameObject);
			PlayerGhost = Instantiate (GhostPlayerPrefab, Player.transform.position, Player.transform.GetChild(0). rotation);
		}

		PlayerGhost.GetComponent<PlayerGhost> ().TriggerTravelToPoint (Objective, GameOverDelay);

		//LevelMusic.GetComponent<AudioSource> ().Stop ();
		GetComponent<AudioSource> ().PlayOneShot (DeathSound,1.0f);

		Destroy (Player.gameObject);

	}

	public Vector3 GetPlayerPosition()
	{
		Vector3 position = Vector3.zero;
		if (!levelHasStarted) {
			position =  PlayerSpawn.transform.position;
		} else if (playerIsActive) {
			position = Player.transform.position;
		} else {
			position = PlayerGhost.transform.position;
		}

		return position;
	}

	public void IncrementVirusDeathNumber()
	{
		WC.VirusNumberKilled++;
	}
	#endregion
}
