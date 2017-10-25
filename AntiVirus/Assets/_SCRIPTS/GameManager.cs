using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	#region public static variables
	public static int Level = 0;
	public static bool Restart = false;
	public static int InfectedBlocks = 0;
	public static int TotalBlocks = 0;
	#endregion

	#region serialized private variables
	[SerializeField] private GameObject PlayerPrefab;
	[SerializeField] private GameObject GhostPlayerPrefab;
	[SerializeField] private int LevelMax = 5;


	[SerializeField] private AudioClip DeathSound;
	[SerializeField] private AudioClip SpawnSound;
	[SerializeField] private float RespawnDelay = 4.0f;
	[SerializeField] private float GameOverDelay = 4.0f;
	[SerializeField] private float StartLevelDelay = 2.0f;

	[SerializeField] private WaveController WC;
	#endregion

	#region private variables
	private GameObject PlayerSpawn;
	private GameObject LevelMusic;
	private GameObject StartCamera;
	private GameObject Objective;
	private UI_Manager UI;
	private GameObject PlayerGhost;
	private GameObject Player;
	private float respawnTimer;
	private float gameOverTimer;
	private float startLevelTimer;
	private bool playerIsActive;
	private bool playerRespawning = false;
	private bool gameIsOver = false;
	private bool gameIsWon = false;
	private bool levelHasStarted = false;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		UI = GameObject.FindGameObjectWithTag ("UI_Controller").GetComponent<UI_Manager> ();
		//WC = GetComponent<WaveController> ();

		Objective = GameObject.FindGameObjectWithTag ("Objective");
		LevelMusic = GameObject.FindGameObjectWithTag ("Music");
		StartCamera = GameObject.FindGameObjectWithTag ("StartCamera");
		PlayerSpawn = GameObject.FindGameObjectWithTag ("PlayerSpawner");

		LevelMusic.GetComponent<AudioSource> ().Play ();

		RestartLevel ();
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
	}

	void OnGUI()
	{
		GUI.TextArea(new Rect(50, 50, 100, 30), (100 * InfectedBlocks / TotalBlocks).ToString() + "% - " + InfectedBlocks.ToString() + "/" + TotalBlocks.ToString());
	}
	#endregion

	#region private functions
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

		Restart = false;

		//virusSpawnTimer = Time.time;

		//SpawnVirusAtRandomSpawner ();
	}

	void RestartLevel()
	{
		UI.HideAll ();

		UI.SetStartScreen (true);
		UI.SetLevelText ();

		levelHasStarted = false;
		startLevelTimer = Time.time;

		InfectedBlocks = 0;
		WC.VirusNumberKilled = 0;
		WC.VirusNumber = 0;
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
		Level--;
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
				//SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
				Restart = true;
				RestartLevel();
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
