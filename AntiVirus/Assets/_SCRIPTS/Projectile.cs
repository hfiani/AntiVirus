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
	private Color startColor;
	#endregion

	// Use this for initialization
	void Start () {

		timer = Time.time;

		transform.localScale = new Vector3 (1, 1, 1) * startSize;

		GetComponent<Rigidbody>().AddForce(transform.forward * speed);

		startColor = GetComponent<Renderer> ().material.GetColor ("_Color");
		
	}
	
	// Update is called once per frame
	void Update () {

		updateSize ();

		updateFade ();

		if (Time.time - timer > lifetime) {

			Destroy (gameObject);
		}
		
	}

	void updateFade(){

		float alpha = Mathf.Lerp (1,0,sizeCurve.Evaluate ((Time.time - timer) / growthTime)); 

		Color newColor = new Color (startColor.r, startColor.g, startColor.b, alpha);

		GetComponent<Renderer> ().material.SetColor ("_Color", newColor);
	

	}

	void updateSize(){

		float scale = Mathf.Lerp (startSize,endSize,sizeCurve.Evaluate ((Time.time - timer) / growthTime)); 

		transform.localScale  = new Vector3 (scale,scale,2*scale);


	}

	void OnTriggerEnter(Collider col){

		if (isAlive) {

			if (col.gameObject.GetComponent<InfectionRaycast> () != null)
			{
				col.gameObject.GetComponent<InfectionRaycast> ().enabled = true;
				col.gameObject.GetComponent<InfectionRaycast> ().CreateImmunity();
			}
			else if(col.gameObject.transform.parent)
			{
				if (col.gameObject.transform.parent.GetComponent<InfectionRaycast> () != null) {
					col.gameObject.transform.parent.GetComponent<InfectionRaycast> ().enabled = true;
					col.gameObject.transform.parent.GetComponent<InfectionRaycast> ().CreateImmunity ();
				}
			}

			isAlive = false;

			Destroy (gameObject);
		}

	
	}


}
