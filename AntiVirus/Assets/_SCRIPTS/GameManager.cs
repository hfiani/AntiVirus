using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	/* GameManager
	 * Placed on a game manager. All the game events are treated here (spawn player, start level, etc...)
	 */

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
	[SerializeField] private float alertDelay = 4.0f;
	[SerializeField] private float waveCompleteDelay = 4.0f;
	[SerializeField] private float RespawnDelay = 4.0f;
	[SerializeField] private float GameOverDelay = 4.0f;
	[SerializeField] private float StartLevelDelay = 2.0f;

	[SerializeField] private WaveController WC;
	[SerializeField] private GameObject Objective;
	[SerializeField] private float SmallestDistance;
	#endregion

	#region private variables
	private GameObject _playerSpawn;
	private GameObject _levelMusic;
	private GameObject _startCamera;
	private UI_Manager UI;
	private GameObject _playerGhost;
	private GameObject _player;
	private float _respawnTimer;
	private float _gameOverTimer;
	private float _startLevelTimer;
	private bool _playerIsActive;
	private bool _playerRespawning = false;
	private bool _gameIsOver = false;
	private bool _gameIsWon = false;
	private bool _firstTime = false;
	private bool _levelHasStarted = false;
	private bool _alertMessage = false;
	private bool _waveComplete = false;
	//check infection level
	private int _layerMask = 1 << 15;
	private float _coef = 9;
	private float[] _radiuses = {1f, 2f, 3f, 4f, 5f, 6f,7f,8f,9f,10f};
	private int _radiusIndex;
	private int _noCollisionCount = 0;
	private float _infectionCheckTimer;
	private float _infectionCheckDelay = 0.25f;
	private float _waveCompleteTimer;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		UI = GameObject.FindGameObjectWithTag ("UI_Controller").GetComponent<UI_Manager> ();
		//WC = GetComponent<WaveController> ();

		Objective = GameObject.FindGameObjectWithTag ("Objective");
		_levelMusic = GameObject.FindGameObjectWithTag ("Music");
		_startCamera = GameObject.FindGameObjectWithTag ("StartCamera");
		_playerSpawn = GameObject.FindGameObjectWithTag ("PlayerSpawner");

		_firstTime = true;

		RestartLevel ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_playerRespawning && Time.time - _respawnTimer > RespawnDelay)
		{
			SpawnPlayer ();
		}

		if (_gameIsOver && Time.time - _gameOverTimer > GameOverDelay)
		{
			//SceneManager.LoadScene ("gameover");
			UI.SetGameOverRetryScreen(true);
		}

		if (!_levelHasStarted && Time.time - _startLevelTimer > StartLevelDelay)
		{
			StartLevel();
		}


		if (_levelHasStarted)
		{
			CheckForVictory ();

			if (_waveComplete && Time.time - _waveCompleteTimer > waveCompleteDelay) {
				Endlevel ();
				_waveComplete = false;
			}

			if (!_alertMessage && Time.time - _startLevelTimer > StartLevelDelay + alertDelay) {
				UI.SetAlertText (true);
				_alertMessage = true;
			}
			if (_alertMessage && Time.time - _startLevelTimer > StartLevelDelay + 3.0f*alertDelay) {
				UI.SetAlertText (false);
				_alertMessage = false;
			}

			if (Time.time - _infectionCheckTimer > _infectionCheckDelay) {
				CheckInfectionLevel ();
				UI.UpdateCubeText (InfectedBlocks);
				UI.UpdateVirusNumberText (WC.GetViruses ().Count);
			}
		}
	}
	#endregion

	#region private functions
	void StartLevel()
	{
		UI.SetStartScreen (false);
		_startCamera.GetComponent<AudioListener> ().enabled = false;

		SpawnPlayer ();

		_levelHasStarted = true;


		int virusNumber = VirusStartNumber + VirusIncrementPerlevel * Level;
		WC.SetVirusParameters (VirusSpawnDelay, virusNumber);
		WC.StartWave ();

		Restart = false;

		_infectionCheckTimer = Time.time;
		_radiusIndex = 0;
		UI.UpdateInfectionBar (0);
	}

	void TriggerRespawn()
	{

		UI.SetRespawnScreen (true);

		_respawnTimer = Time.time;

		_playerRespawning = true;
	}

	void SpawnPlayer()
	{
		Debug.Log ("Spawning Player");

		_playerRespawning = false;
		_playerIsActive = true;

		UI.SetRespawnScreen (false);
		UI.SetCrosshair (true);
		UI.SetEnergyBar (true);
		UI.SetCompass (true);

		if (_player != null)
		{
			Destroy (_player.gameObject);
		}
		if (_playerGhost != null)
		{
			Destroy (_playerGhost.gameObject);
		}

		_player = Instantiate (PlayerPrefab, _playerSpawn.transform.position, _playerSpawn.transform.rotation);
		_player.GetComponent<PlayerManager> ().Init ();

		// add some text & sound ...

		GetComponent<AudioSource> ().PlayOneShot (SpawnSound,1.0f);
	}
	#endregion

	#region public functions
	public void RestartLevel()
	{
		Debug.Log ("RESTART");

		_levelMusic.GetComponent<AudioSource> ().Play ();

		SmallestDistance = 999999;
		// except at the start of the game, clean all ghostcubes
		if (_firstTime) {
			_firstTime = false;
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
		_gameIsOver = false;
		if (_playerGhost != null)
		{
			Destroy (_playerGhost.gameObject);
		}
		if (_player!= null)
		{
			Destroy (_player.gameObject);
		}

		UI.HideAll ();
		UI.SetGameOverScreen (false);
		UI.SetStartScreen (true);
		UI.SetLevelText ();

		_levelHasStarted = false;
		_startLevelTimer = Time.time;

		_startCamera.GetComponent<AudioListener> ().enabled = true;
		InfectedBlocks = 0;
		WC.VirusNumberKilled = 0;
		WC.VirusNumber = 0;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void GameWon()
	{
		if (_gameIsWon || _gameIsOver)
		{
			return;
		}
		Debug.Log ("Game Won");
		_gameIsWon = true;
		GameManager.Level = 0;
		SceneManager.LoadScene (victorySceneName);

	}

	public void GameOver()
	{
		if (_gameIsOver)
		{
			return;
		}
		Debug.Log ("Game Over");
		UI.HideAll ();
		UI.SetGameOverScreen (true);
		_gameIsOver = true;
		_gameOverTimer = Time.time;
		ShowGameOver ();
		Level--;
		//SceneManager.LoadScene ("gameover");
	}

	public void CheckInfectionLevel()
	{
		if (_radiusIndex == _radiuses.Length - 1) {
			UI.UpdateInfectionBar ((float)(_radiuses.Length - SmallestDistance) / _radiuses.Length);
			_radiusIndex = 0;
			_noCollisionCount = 0;
			SmallestDistance = 99999;


			
		}

		Vector3 pos = Objective.transform.position;


		Collider[] cols = Physics.OverlapSphere (pos, _radiuses[_radiusIndex] * _coef, _layerMask);
		if (cols.Length > 0)
		{
			if (SmallestDistance > _radiuses[_radiusIndex] )
			{
				SmallestDistance = _radiuses[_radiusIndex] ;

			}
					
		}
		else
		{
			_noCollisionCount++;
		}

		_radiusIndex++;
		_infectionCheckTimer = Time.time;
	}

	public void CheckForVictory ()
	{
		if (!_waveComplete && WC.VirusNumberKilled == WC.VirusNumber && InfectedBlocks <= 1)
		{
			Debug.Log ("wave complete");
			_waveComplete = true;
			UI.SetWaveCompleteScreen (true);
			GetComponent<AudioSource> ().PlayOneShot (SpawnSound,1.0f);
			_waveCompleteTimer = Time.time;
	
		}
	}

	void Endlevel(){
		if (Level == LevelMax)
		{
			GameWon ();
		}
		else
		{
			RestartLevel();
		}
	}

	public void PlayerDeath()
	{
		if (!_playerIsActive) {
			return;
		}
		Debug.Log ("Player Death");

		_playerIsActive = false;
		UI.SetCrosshair (false);
		UI.SetEnergyBar (false);

		Destroy (_player.gameObject);

		_playerGhost = Instantiate (GhostPlayerPrefab, _player.transform.position, _player.transform.GetChild(0). rotation);
		_playerGhost.GetComponent<PlayerGhost> ().TriggerTravelToPoint (_playerSpawn, RespawnDelay);

		GetComponent<AudioSource> ().PlayOneShot (DeathSound,1.0f);

		TriggerRespawn ();
	}

	public void ShowGameOver()
	{
		_playerIsActive = false;
		_playerRespawning = false;

		if (_player)
		{
			Destroy (_player.gameObject);
			_playerGhost = Instantiate (GhostPlayerPrefab, _player.transform.position, _player.transform.GetChild(0). rotation);
		}

		_playerGhost.GetComponent<PlayerGhost> ().TriggerTravelToPoint (_playerSpawn, GameOverDelay);

		GetComponent<AudioSource> ().PlayOneShot (DeathSound,1.0f);

		Destroy (_player.gameObject);
	}

	public Vector3 GetPlayerPosition()
	{
		Vector3 position = Vector3.zero;
		if (!_levelHasStarted)
		{
			position = _playerSpawn.transform.position;
		}
		else if (_playerIsActive)
		{
			position = _player.transform.position;
		}
		else
		{
			position = _playerGhost.transform.position;
		}

		return position;
	}

	public void IncrementVirusDeathNumber()
	{
		WC.VirusNumberKilled++;
	}
	#endregion
}
