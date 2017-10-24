using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


//=======================================================================================================
//USED IN EXIT SCENE 
//=======================================================================================================
public class GameOver : MonoBehaviour
{

    [SerializeField]
	private float _delay = 3.0f;
	[SerializeField]
	private string _sceneToLoad;

	private float _timer;

    //=======================================================================================================
	void Start ()
    {

		_timer = Time.time;
	
	}
	
    //=======================================================================================================
	void Update ()
    {

		if (Time.time > _timer + _delay)
        {

			SceneManager.LoadScene (_sceneToLoad);

		}
	
	}
}
