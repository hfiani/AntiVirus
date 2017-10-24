using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
	#region public variables
	public GameObject VirusPrefab;
	public GameObject VirusSpawners;
	public float VirusSpawnDelay = 10.0f;
	public int VirusNumber = 1;
	#endregion

	#region private variables
	private int virusNumberKilled;
	private int virusNumberRemaining;
	private float virusSpawnTimer;
	private bool waveHasStarted = false;
	private List<int> freeSpawnerIndexes = new List<int>();
	private List<GameObject> virusList = new List<GameObject> ();
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		
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
		if (freeSpawnerIndexes.Count == 0)
		{
			return;
		}

		// get random index picked from the free index list
		int r = Random.Range (0, freeSpawnerIndexes.Count);
		int index = freeSpawnerIndexes [r];

		GameObject virus = Instantiate (VirusPrefab, VirusSpawners.transform.GetChild(index).position, Quaternion.identity);
		virusList.Add (virus);
		// add virus to its spawner
		virus.transform.parent = VirusSpawners.transform.GetChild (index);

		// decrement number of viruses remaining
		virusNumberRemaining --;
		Debug.Log ("spawning virus #" + (VirusNumber - virusNumberRemaining));

		UpdateFreeSpawnersIndexes ();
	}

	void UpdateFreeSpawnersIndexes()
	{
		freeSpawnerIndexes.Clear ();

		for (int i = 0; i < VirusSpawners.transform.childCount; i++)
		{
			if (VirusSpawners.transform.GetChild (i).childCount == 0)
			{
				freeSpawnerIndexes.Add (i);
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
