using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public GameObject PlayerPrefab;
	public Transform PlayerSpawn;

	private GameObject Player;

	public bool playerIsActive;

	// Use this for initialization
	void Start () {

		Player = GameObject.FindGameObjectWithTag ("Player");

		if (!Player) {
			SpawnPlayer ();
		} else {

			playerIsActive = true;
		}




		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SpawnPlayer(){

		Debug.Log ("Spawning Player...");

		if (Player != null) {
			Destroy (Player.gameObject);
		}

		Player = Instantiate (PlayerPrefab, PlayerSpawn.position, PlayerSpawn.rotation);

		// add some text & sound ...

		playerIsActive = true;

	}

	public GameObject GetPlayer(){

		return Player;
	}
}
