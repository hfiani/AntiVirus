using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {


	#region public variables
	public float speed = 10.0f;
	public float startSize = 1.0f;
	public float endSize = 1.0f;
	public AnimationCurve sizeCurve;
	public float growthTime = 2.0f;
	public float lifetime = 2.0f;
	#endregion

	#region private variables
	private float timer;
	private bool isAlive = true;
	#endregion

	// Use this for initialization
	void Start () {

		timer = Time.time;

		transform.localScale = new Vector3 (1, 1, 1) * startSize;

		GetComponent<Rigidbody>().AddForce(transform.forward * speed);
		
	}
	
	// Update is called once per frame
	void Update () {

		updateSize ();

		//updateFade ();
		
	}

	void updateSize(){

		float scale = Mathf.Lerp (startSize,endSize,sizeCurve.Evaluate ((Time.time - timer) / growthTime)); 
		//Debug.Log (scale);

		transform.localScale  = new Vector3 (scale,scale,20*scale);


	}

	void OnTriggerEnter(Collider col){

		if (col.gameObject.GetComponent<InfectionRaycast> () != null && isAlive)
		{
			//col.gameObject.GetComponent<InfectionRaycast> ().CreateImmunity(col);

			isAlive = false;

			Destroy (gameObject);
		}
	}


}
