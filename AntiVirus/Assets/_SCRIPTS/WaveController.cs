using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
	#region static variables
	static int zoneIndex = 0;
	#endregion

	#region public variables
	public GameObject VirusPrefab;
	public float VirusSpawnDelay = 10.0f;
	public int VirusNumber = 1;
	#endregion

	#region private variables
	private GameObject VirusSpawners;
	private int virusNumberKilled;
	private int virusNumberRemaining;
	private float virusSpawnTimer;
	private bool waveHasStarted = false;
	private List<int>[] freeSpawnerIndexes = new List<int>[4];
	private List<GameObject> virusList = new List<GameObject> ();
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		VirusSpawners = GameObject.FindGameObjectWithTag ("VirusSpawner");

		freeSpawnerIndexes = new List<int>[VirusSpawners.transform.childCount];

		for (int i = 0; i < freeSpawnerIndexes.Length; i++)
		{
			freeSpawnerIndexes[i] = new List<int> ();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		// try to spawn new virus
		if (waveHasStarted && Time.time - virusSpawnTimer > VirusSpawnDelay && virusNumberRemaining > 0) {
			UpdateFreeSpawnersIndexes ();

			SpawnVirusAtRandomSpawner();

			virusSpawnTimer = Time.time;
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
		waveHasStarted = true;
		virusSpawnTimer = Time.time;
		UpdateFreeSpawnersIndexes ();
	}

	void SpawnVirusAtRandomSpawner()
	{
		// if all spawners already have a virus, return
		int count = 0;
		for (int i = 0; i < freeSpawnerIndexes.Length; i++)
		{
			if (freeSpawnerIndexes[i].Count == 0)
			{
				count++;
			}
		}
		if (count == freeSpawnerIndexes.Length)
		{
			return;
		}

		// get random index picked from the free index list
		int r = Random.Range (0, freeSpawnerIndexes[zoneIndex].Count);
		int index = freeSpawnerIndexes[zoneIndex] [r];

		Transform spawner = VirusSpawners.transform.GetChild (zoneIndex).GetChild (index);
		GameObject virus = Instantiate (VirusPrefab, spawner.position, Quaternion.identity);
		virusList.Add (virus);
		// add virus to its spawner
		virus.transform.parent = spawner;

		// decrement number of viruses remaining
		virusNumberRemaining --;
		Debug.Log ("spawning virus #" + (VirusNumber - virusNumberRemaining));

		UpdateFreeSpawnersIndexes ();

		zoneIndex++;
		if (zoneIndex == VirusSpawners.transform.childCount)
		{
			zoneIndex = 0;
		}
	}

	void UpdateFreeSpawnersIndexes()
	{
		freeSpawnerIndexes[zoneIndex].Clear ();

		for (int i = 0; i < VirusSpawners.transform.GetChild (zoneIndex).childCount; i++)
		{
			Transform child = VirusSpawners.transform.GetChild (zoneIndex).GetChild (i);
			if (child.childCount == 0 && child.gameObject.activeSelf)
			{
				freeSpawnerIndexes[zoneIndex].Add (i);
			}
		}
	}
	#endregion

	#region public functions
	public void SetVirusParameters(float virusSpawnDelay, int virusNumber)
	{
		VirusSpawnDelay = virusSpawnDelay;
		VirusNumber = virusNumber;
		virusNumberRemaining = virusNumber;
	}

	public void RemoveViruses()
	{
		foreach (GameObject virus in virusList)
		{
			Destroy (virus);
		}
		virusList.Clear ();
	}

	public int VirusNumberKilled
	{
		get
		{
			return virusNumberKilled;
		}
		set
		{
			virusNumberKilled = value;
		}
	}

	public void removeVirusFromList(GameObject virus){
		virusList.Remove (virus);
	}

	public float getDistanceFromClosestVirus(Vector3 reference){
		float shortestDistance=99999.0f;
		for (int i = 0; i < virusList.Count; i++) {

			float dist = Vector3.Distance (reference, virusList [i].transform.position);
			if (dist < shortestDistance) {
				shortestDistance = dist;
			}
		}
		return shortestDistance;
	}

	public List<GameObject> GetViruses()
	{
		return virusList;
	}
	#endregion
}
