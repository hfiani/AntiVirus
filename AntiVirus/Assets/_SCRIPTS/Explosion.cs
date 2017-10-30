using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	#region serialized private variables
	[SerializeField] private float StartSize = 1.0f;
	[SerializeField] private float EndSize = 1.0f;
	[SerializeField] private AnimationCurve SizeCurve;
	[SerializeField] private float GrowthTime = 2.0f;
	[SerializeField] private float Lifetime = 2.0f;
	[SerializeField] private AudioClip ExplosionSound;
	[SerializeField] private float ExplosionVolume = 1.0f;
	#endregion

	#region private variables
	private float timer;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		GetComponent<AudioSource> ().PlayOneShot (ExplosionSound, ExplosionVolume);
		timer = Time.time;
		transform.localScale = new Vector3 (1, 1, 1) * StartSize;
	}
	
	// Update is called once per frame
	void Update ()
	{
		updateSize ();
		//updateFade ();
		if (Time.time - timer > Lifetime)
		{
			Destroy (gameObject);
		}
	}
	#endregion

	#region private functions
	void updateSize()
	{
		float scale = Mathf.Lerp (StartSize,EndSize,SizeCurve.Evaluate ((Time.time - timer) / GrowthTime)); 
		//Debug.Log (scale);

		transform.localScale  = new Vector3 (scale,scale,scale);
	}
	#endregion
}
