using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

	#region public variables
	public float startSize = 1.0f;
	public float endSize = 1.0f;
	public AnimationCurve sizeCurve;
	public float growthTime = 2.0f;
	public float lifetime = 2.0f;
	public AudioClip explosionSound;
	public float explosionVolume = 1.0f;
	#endregion

	#region private variables
	private float timer;
	#endregion

	// Use this for initialization
	void Start () {

		GetComponent<AudioSource> ().PlayOneShot (explosionSound, explosionVolume);

		timer = Time.time;

		transform.localScale = new Vector3 (1, 1, 1) * startSize;
		
	}
	
	// Update is called once per frame
	void Update () {


		updateSize ();

		//updateFade ();

		if (Time.time - timer > lifetime) {

			Destroy (gameObject);
		}
		
	}

	void updateSize(){

		float scale = Mathf.Lerp (startSize,endSize,sizeCurve.Evaluate ((Time.time - timer) / growthTime)); 
		//Debug.Log (scale);

		transform.localScale  = new Vector3 (scale,scale,scale);


	}
}
