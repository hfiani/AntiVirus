using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour {

	private GameManager GM;

	// Use this for initialization
	void Start () {

		GM = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider col){

		if (col.gameObject.CompareTag("Player"))
		{
			GM.playerIsActive = false;
			GM.SpawnPlayer ();

		}


	}
}
