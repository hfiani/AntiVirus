using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	[SerializeField]
	private GameObject _LoadingPanel;

	// Use this for initialization
	void Start () {

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		_LoadingPanel.SetActive (false);
		
	}
	
	public void ExitGame(){

		Application.Quit ();
	} 

	public void LoadScene (string sceneName){

		SceneManager.LoadScene (sceneName);
	}

	public void ShowLoadingPanel(){
		_LoadingPanel.SetActive (true);
	}
		
		


}
