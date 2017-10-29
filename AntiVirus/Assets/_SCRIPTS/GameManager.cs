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
	[SerializeField] private int VirusStartNumber = 1;
	[SerializeField] private int VirusIncrementPerlevel = 1;
	[SerializeField] private int VirusSpawnDelay = 10;
	[SerializeField] private string victorySceneName = "victory";


	[SerializeField] private AudioClip DeathSound;
	[SerializeField] private AudioClip SpawnSound;
	[SerializeField] private float RespawnDelay = 4.0f;
	[SerializeField] private float GameOverDelay = 4.0f;
	[SerializeField] private float StartLevelDelay = 2.0f;

	[SerializeField] private WaveController WC;
	#endregion

	#region public variables
	public GameObject Objective;
	public float SmallestDistance;
	#endregion

	#region private variables
	private GameObject PlayerSpawn;
	private GameObject LevelMusic;
	private GameObject StartCamera;
	private UI_Manager UI;
	private GameObject PlayerGhost;
	private GameObject Player;
	private float respawnTimer;
	private float gameOverTimer;
	private float startLevelTimer;
	private float initTimer;
	private bool playerIsActive;
	private bool playerRespawning = false;
	private bool gameIsOver = false;
	private bool gameIsWon = false;
	private bool firstTime = false;
	private bool levelHasStarted = false;
	//check infection level
	int layerMask = 1 << 15;
	float coef = 15;
	float[] radiuses = {1f, 2f, 3f, 4f, 5f, 6f,7f,8f,9f,10f};
	int radiusIndex;
	int noCollisionCount = 0;
	float infectionCheckTimer;
	float infectionCheckDelay = 0.25f;
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



		firstTime = true;
		initTimer = Time.time;

		RestartLevel ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (playerRespawning && Time.time - respawnTimer > RespawnDelay)
		{
			SpawnPlayer ();
		}

		if (gameIsOver && Time.time - gameOverTimer > GameOverDelay)
		{
			//SceneManager.LoadScene ("gameover");
			UI.SetGameOverRetryScreen(true);
		}

		if (!levelHasStarted && Time.time - startLevelTimer > StartLevelDelay)
		{
			StartLevel();
		}

		if (levelHasStarted)
		{
			CheckForVictory ();

			if (Time.time - infectionCheckTimer > infectionCheckDelay) {
				CheckInfectionLevel ();
				UI.UpdateCubeText (InfectedBlocks);
				UI.UpdateVirusNumberText (WC.GetViruses ().Count);
			}
		}
	}

	/*
	void OnGUI()
	{
		GUI.TextArea(new Rect(50, 50, 100, 30), (100 * InfectedBlocks / TotalBlocks).ToString() + "% - " + InfectedBlocks.ToString() + "/" + TotalBlocks.ToString());
	}
	*/
	#endregion

	#region private functions
	void StartLevel()
	{
		UI.SetStartScreen (false);
		StartCamera.GetComponent<AudioListener> ().enabled = false;

		SpawnPlayer ();

		levelHasStarted = true;


		int virusNumber = VirusStartNumber + VirusIncrementPerlevel * Level;
		WC.SetVirusParameters (VirusSpawnDelay, virusNumber);
		WC.StartWave ();

		Restart = false;

		infectionCheckTimer = Time.time;
		radiusIndex = 0;
		UI.UpdateInfectionBar (0);
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

		if (Player != null)
		{
			Destroy (Player.gameObject);
		}
		if (PlayerGhost != null)
		{
			Destroy (PlayerGhost.gameObject);
		}

		Player = Instantiate (PlayerPrefab, PlayerSpawn.transform.position, PlayerSpawn.transform.rotation);
		Player.GetComponent<PlayerManager> ().Init ();

		// add some text & sound ...

		GetComponent<AudioSource> ().PlayOneShot (SpawnSound,1.0f);
	}
	#endregion

	#region public functions
	public void RestartLevel()
	{
		Debug.Log ("RESTART");

		LevelMusic.GetComponent<AudioSource> ().Play ();

		SmallestDistance = 999999;
		// except at the start of the game, clean all ghostcubes
		if (firstTime) {
			firstTime = false;
		} else {
			int layerMask = 1 << 11;
			Vector3 pos = Objective.transform.position;
			Collider[] cols = Physics.OverlapBox(pos, new Vector3(500,500,500), Quaternion.identity, layerMask);
			for (int i = 0; i < cols.Length; i++) {
				cols[i].gameObject.GetComponent<InfectionRaycast>().RemoveInfection ();
			}
			Debug.Log ("CLEANING GHOSTS; hits : "+cols.Length);
		}
	
		Restart = true;
		gameIsOver = false;
		if (PlayerGhost != null)
		{
			Destroy (PlayerGhost.gameObject);
		}
		if (Player!= null)
		{
			Destroy (Player.gameObject);
		}

		UI.HideAll ();
		UI.SetGameOverScreen (false);
		UI.SetStartScreen (true);
		UI.SetLevelText ();

		levelHasStarted = false;
		startLevelTimer = Time.time;

		StartCamera.GetComponent<AudioListener> ().enabled = true;
		InfectedBlocks = 0;
		WC.VirusNumberKilled = 0;
		WC.VirusNumber = 0;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void GameWon()
	{
		if (gameIsWon || gameIsOver)
		{
			return;
		}
		Debug.Log ("Game Won");
		SceneManager.LoadScene (victorySceneName);
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

	public void CheckInfectionLevel()
	{
		if (radiusIndex == radiuses.Length - 1) {
			UI.UpdateInfectionBar ((float)(radiuses.Length - SmallestDistance) / radiuses.Length);
			radiusIndex = 0;
			noCollisionCount = 0;
			SmallestDistance = 99999;


			
		}

		Vector3 pos = Objective.transform.position;


		Collider[] cols = Physics.OverlapSphere (pos, radiuses[radiusIndex] * coef, layerMask);
		if (cols.Length > 0)
		{
			if (SmallestDistance > radiuses[radiusIndex] )
			{
				SmallestDistance = radiuses[radiusIndex] ;

			}
					
		}
		else
		{
			noCollisionCount++;
		}

		radiusIndex++;
		infectionCheckTimer = Time.time;

		/*
		if (noCollisionCount == radiuses.Length)
		{
			UI.UpdateInfectionBar (0);
		}
		*/
	}

	public void CheckForVictory ()
	{
		if (WC.VirusNumberKilled == WC.VirusNumber && InfectedBlocks <= 10)
		{
			if (Level == LevelMax)
			{
				GameWon ();
			}
			else
			{
				//SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
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

		if (Player)
		{
			Destroy (Player.gameObject);
			PlayerGhost = Instantiate (GhostPlayerPrefab, Player.transform.position, Player.transform.GetChild(0). rotation);
		}

		PlayerGhost.GetComponent<PlayerGhost> ().TriggerTravelToPoint (PlayerSpawn, GameOverDelay);

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
