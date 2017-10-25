using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {


	#region serialized private variables
	[SerializeField] private float speed = 10.0f;
	[SerializeField] private float startSize = 1.0f;
	[SerializeField] private float endSize = 1.0f;
	[SerializeField] private AnimationCurve sizeCurve;
	[SerializeField] private float growthTime = 2.0f;
	[SerializeField] private float lifetime = 2.0f;
	#endregion

	#region private variables
	private float timer;
	private bool isAlive = true;
	private Color startColor;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		timer = Time.time;

		transform.localScale = new Vector3 (1, 1, 1) * startSize;

		GetComponent<Rigidbody>().AddForce(transform.forward * speed);

		startColor = GetComponent<Renderer> ().material.GetColor ("_Color");
	}
	
	// Update is called once per frame
	void Update ()
	{
		updateSize ();

		updateFade ();

		if (Time.time - timer > lifetime)
		{

			Destroy (gameObject);
		}
	}

	void OnTriggerEnter(Collider col){

		if (isAlive) {

			InfectionRaycast inf = col.gameObject.GetComponent<InfectionRaycast> ();
			if (inf != null)
			{
				inf.enabled = true;
				inf.CreateImmunity();
			}
			else if(col.gameObject.transform.parent)
			{
				inf = col.gameObject.transform.parent.GetComponent<InfectionRaycast> ();
				if (inf != null) {
					inf.enabled = true;
					inf.CreateImmunity ();
				}
			}

			isAlive = false;

			Destroy (gameObject);
		}
	}
	#endregion

	#region private functions
	void updateFade()
	{
		float alpha = Mathf.Lerp (1,0,sizeCurve.Evaluate ((Time.time - timer) / growthTime)); 

		Color newColor = new Color (startColor.r, startColor.g, startColor.b, alpha);

		GetComponent<Renderer> ().material.SetColor ("_Color", newColor);
	}

	void updateSize()
	{
		float scale = Mathf.Lerp (startSize,endSize,sizeCurve.Evaluate ((Time.time - timer) / growthTime)); 

		transform.localScale  = new Vector3 (scale,scale,2*scale);
	}
	#endregion
}
