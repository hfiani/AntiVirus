using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		
	}
	
	public void ExitGame(){

		Application.Quit ();
	} 

	public void LoadScene (string sceneName){

		SceneManager.LoadScene (sceneName);
	}
		


}
