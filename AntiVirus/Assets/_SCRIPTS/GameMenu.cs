﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void Retry()
	{
		GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ().RestartLevel ();
	}

	public void MainMenu()
	{
		SceneManager.LoadScene ("startmenu");
	}
}
