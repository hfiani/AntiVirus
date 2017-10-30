using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	/* Explosion
	 * used to explode virus
	 */

	#region serialized private variables
	[SerializeField] private float startSize = 1.0f;
	[SerializeField] private float endSize = 1.0f;
	[SerializeField] private AnimationCurve sizeCurve;
	[SerializeField] private float growthTime = 2.0f;
	[SerializeField] private float lifetime = 2.0f;
	[SerializeField] private AudioClip explosionSound;
	[SerializeField] private float explosionVolume = 1.0f;
	#endregion

	#region private variables
	private float timer;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		GetComponent<AudioSource> ().PlayOneShot (explosionSound, explosionVolume);
		timer = Time.time;
		transform.localScale = new Vector3 (1, 1, 1) * startSize;
	}
	
	// Update is called once per frame
	void Update ()
	{
		updateSize ();
		//updateFade ();
		if (Time.time - timer > lifetime)
		{
			Destroy (gameObject);
		}
	}
	#endregion

	#region private functions
	void updateSize()
	{
		float scale = Mathf.Lerp (startSize,endSize,sizeCurve.Evaluate ((Time.time - timer) / growthTime)); 
		//Debug.Log (scale);

		transform.localScale  = new Vector3 (scale, scale, scale);
	}
	#endregion
}
