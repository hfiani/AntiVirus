﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
	/* Menu
	 * manages main menu buttons
	 */
	#region serialized private variables
	[SerializeField] private GameObject _LoadingPanel;
	[SerializeField] private GameObject _CreditsPanel;
	[SerializeField] private GameObject _InfoPanel;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		if(_LoadingPanel) _LoadingPanel.SetActive (false);
		if(_CreditsPanel)_CreditsPanel.SetActive (false);
		if(_InfoPanel)_InfoPanel.SetActive (false);
	}
	#endregion

	#region public variables
	public void ExitGame()
	{
		Application.Quit ();
	} 

	public void LoadScene (string sceneName)
	{
		SceneManager.LoadScene (sceneName);
	}

	public void SetLoadingPanel(bool state)
	{
		_LoadingPanel.SetActive (state);
	}

	public void SetCreditPanel(bool state)
	{
		_CreditsPanel.SetActive (state);
	}

	public void SetInfoPanel(bool state)
	{
		_InfoPanel.SetActive (state);
	}
	#endregion
}
