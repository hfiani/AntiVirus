using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
	/* WaveController
	 * Each level has a wave of viruses, this script makes sure
	 * that the viruses are well spread and well removed
	 */

	#region static variables
	static int ZoneIndex = 0;
	#endregion

	#region public variables
	public int VirusNumber = 1;
	#endregion

	#region serialized private variables
	[SerializeField] private GameObject VirusPrefab;
	[SerializeField] private float VirusSpawnDelay = 10.0f;
	#endregion

	#region private variables
	private GameObject _virusSpawners;
	private int _virusNumberKilled;
	private int _virusNumberRemaining;
	private float _virusSpawnTimer;
	private bool _waveHasStarted = false;
	private List<int>[] _freeSpawnerIndexes = new List<int>[4];
	private List<GameObject> _virusList = new List<GameObject> ();
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		_virusSpawners = GameObject.FindGameObjectWithTag ("VirusSpawner");

		_freeSpawnerIndexes = new List<int>[_virusSpawners.transform.childCount];

		for (int i = 0; i < _freeSpawnerIndexes.Length; i++)
		{
			_freeSpawnerIndexes[i] = new List<int> ();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		// try to spawn new virus
		if (_waveHasStarted && Time.time - _virusSpawnTimer > VirusSpawnDelay && _virusNumberRemaining > 0) {
			UpdateFreeSpawnersIndexes ();

			SpawnVirusAtRandomSpawner();

			_virusSpawnTimer = Time.time;
		}

		if (GameManager.Restart)
		{
			RemoveViruses ();
		}
	}
	#endregion

	#region private functions
	public void StartWave()
	{
		_waveHasStarted = true;
		_virusSpawnTimer = Time.time;
		UpdateFreeSpawnersIndexes ();
	}

	void SpawnVirusAtRandomSpawner()
	{
		// if all spawners already have a virus, return
		int count = 0;
		for (int i = 0; i < _freeSpawnerIndexes.Length; i++)
		{
			if (_freeSpawnerIndexes[i].Count == 0)
			{
				count++;
			}
		}
		if (count == _freeSpawnerIndexes.Length)
		{
			return;
		}

		// get random index picked from the free index list
		int r = Random.Range (0, _freeSpawnerIndexes[ZoneIndex].Count);
		int index = _freeSpawnerIndexes[ZoneIndex] [r];

		Transform spawner = _virusSpawners.transform.GetChild (ZoneIndex).GetChild (index);
		GameObject virus = Instantiate (VirusPrefab, spawner.position, Quaternion.identity);
		_virusList.Add (virus);
		// add virus to its spawner
		virus.transform.parent = spawner;

		// decrement number of viruses remaining
		_virusNumberRemaining --;
		Debug.Log ("spawning virus #" + (VirusNumber - _virusNumberRemaining));

		UpdateFreeSpawnersIndexes ();

		ZoneIndex++;
		if (ZoneIndex == _virusSpawners.transform.childCount)
		{
			ZoneIndex = 0;
		}
	}

	void UpdateFreeSpawnersIndexes()
	{
		_freeSpawnerIndexes[ZoneIndex].Clear ();

		for (int i = 0; i < _virusSpawners.transform.GetChild (ZoneIndex).childCount; i++)
		{
			Transform child = _virusSpawners.transform.GetChild (ZoneIndex).GetChild (i);
			if (child.childCount == 0 && child.gameObject.activeSelf)
			{
				_freeSpawnerIndexes[ZoneIndex].Add (i);
			}
		}
	}
	#endregion

	#region public functions
	public void SetVirusParameters(float virusSpawnDelay, int virusNumber)
	{
		VirusSpawnDelay = virusSpawnDelay;
		VirusNumber = virusNumber;
		_virusNumberRemaining = virusNumber;
	}

	public void RemoveViruses()
	{
		foreach (GameObject virus in _virusList)
		{
			Destroy (virus);
		}
		_virusList.Clear ();
	}

	public int VirusNumberKilled
	{
		get
		{
			return _virusNumberKilled;
		}
		set
		{
			_virusNumberKilled = value;
		}
	}

	public void removeVirusFromList(GameObject virus)
	{
		_virusList.Remove (virus);
	}

	public float getDistanceFromClosestVirus(Vector3 reference)
	{
		float shortestDistance = 99999.0f;
		for (int i = 0; i < _virusList.Count; i++)
		{

			float dist = Vector3.Distance (reference, _virusList [i].transform.position);
			if (dist < shortestDistance)
			{
				shortestDistance = dist;
			}
		}
		return shortestDistance;
	}

	public List<GameObject> GetViruses()
	{
		return _virusList;
	}
	#endregion
}
