using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	#region serialized private variables
	[SerializeField] private float StartSize = 1.0f;
	[SerializeField] private float EndSize = 1.0f;
	[SerializeField] private AnimationCurve SizeCurve;
	[SerializeField] private float GrowthTime = 2.0f;
	[SerializeField] private float Speed = 10.0f;
	[SerializeField] private float Lifetime = 2.0f;
	#endregion

	#region private variables
	private float _timer;
	private bool _isAlive = true;
	private Color _startColor;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		_timer = Time.time;
		transform.localScale = new Vector3 (1, 1, 1) * StartSize;
		GetComponent<Rigidbody>().AddForce(transform.forward * Speed);
		_startColor = GetComponent<Renderer> ().material.GetColor ("_Color");
	}
	
	// Update is called once per frame
	void Update ()
	{
		updateSize ();
		updateFade ();

		if (Time.time - _timer > Lifetime)
		{
			Destroy (gameObject);
		}
	}

	void OnTriggerEnter(Collider col)
	{

		if (_isAlive)
		{
			InfectionRaycast inf = col.gameObject.GetComponent<InfectionRaycast> ();
			if (inf != null)
			{
				inf.enabled = true;
				inf.CreateImmunity ();
			}
			else if (col.gameObject.transform.parent)
			{
				inf = col.gameObject.transform.parent.GetComponent<InfectionRaycast> ();
				if (inf != null)
				{
					inf.enabled = true;
					inf.CreateImmunity ();
				}
			}

			_isAlive = false;

			Destroy (gameObject);
		}
	}
	#endregion

	#region private functions
	void updateFade()
	{
		float alpha = Mathf.Lerp (1,0,SizeCurve.Evaluate ((Time.time - _timer) / GrowthTime)); 

		Color newColor = new Color (_startColor.r, _startColor.g, _startColor.b, alpha);

		GetComponent<Renderer> ().material.SetColor ("_Color", newColor);
	}

	void updateSize()
	{
		float scale = Mathf.Lerp (StartSize,EndSize,SizeCurve.Evaluate ((Time.time - _timer) / GrowthTime)); 

		transform.localScale  = new Vector3 (scale,scale,2*scale);
	}
	#endregion
}
