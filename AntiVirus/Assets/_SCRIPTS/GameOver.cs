using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
	/* GameOver
	 * loads the game over scene
	 */
	#region serialized private variables
	[SerializeField] private float delay = 3.0f;
	[SerializeField] private string sceneToLoad;
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
		if (Time.time > _timer + delay)
		{
			SceneManager.LoadScene (sceneToLoad);
		}
	}
	#endregion
}
