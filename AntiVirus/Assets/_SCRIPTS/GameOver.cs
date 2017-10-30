using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
	#region serialized private variables
	[SerializeField] private float Delay = 3.0f;
	[SerializeField] private string SceneToLoad;
	#endregion

	#region private variables
	private float _timer;
	#endregion

	#region events
	void Start ()
    {

		_timer = Time.time;
	
	}

	void Update ()
    {
		if (Time.time > _timer + Delay)
        {
			SceneManager.LoadScene (SceneToLoad);
		}
	}
	#endregion
}
