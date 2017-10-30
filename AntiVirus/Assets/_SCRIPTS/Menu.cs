using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	[SerializeField]
	private GameObject _LoadingPanel;
	[SerializeField]
	private GameObject _CreditsPanel;
	[SerializeField]
	private GameObject _InfoPanel;

	// Use this for initialization
	void Start () {

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		_LoadingPanel.SetActive (false);
		_CreditsPanel.SetActive (false);
		_InfoPanel.SetActive (false);
		
	}
	
	public void ExitGame(){

		Application.Quit ();
	} 

	public void LoadScene (string sceneName){

		SceneManager.LoadScene (sceneName);
	}

	public void SetLoadingPanel(bool state){
		_LoadingPanel.SetActive (state);
	}
	public void SetCreditPanel(bool state){
		_CreditsPanel.SetActive (state);
	}
	public void SetInfoPanel(bool state){
		_InfoPanel.SetActive (state);
	}

		
		


}
