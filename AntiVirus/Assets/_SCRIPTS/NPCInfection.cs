using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NPCInfection : MonoBehaviour
{
	#region private variables
	private DateTime _infectionTime;
	private Material _originalMaterial;
	#endregion

	#region public variables
	public bool infected;
	public Material infectionMaterial;
	public float epsilon = 0.001f;
	public float distance = 0.03f;

	public float timeToDeinfection = 10.0f;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		timeToDeinfection *= 1000;
		_originalMaterial = transform.GetChild (0).GetComponent<MeshRenderer> ().sharedMaterial;
	}
	
	// Update is called once per frame
	void Update ()
	{
		checkInfection ();
		checkInfectionUnder ();
	}
	#endregion

	#region private functions
	void checkInfection()
	{
		if (infected && (float)(DateTime.Now - _infectionTime).TotalMilliseconds > timeToDeinfection)
		{
			infected = false;
			ChangeMaterial (_originalMaterial);
		}
	}

	void checkInfectionUnder()
	{
		if (!infected)
		{
			Vector3 direction = Vector3.down;
			RaycastHit hit;
			BoxCollider b = GetComponent<BoxCollider> ();

			Vector3 ray_localPosition = new Vector3 (
				direction.x * (transform.localScale.x * b.size.x / 2 - epsilon),
				direction.y * (transform.localScale.y * b.size.y / 2 - epsilon),
				direction.z * (transform.localScale.z * b.size.z / 2 - epsilon));
			Vector3 ray_pos = (transform.position + b.center) + ray_localPosition;
			Debug.DrawRay (ray_pos, direction.normalized*distance, Color.red);
			if (Physics.Raycast (ray_pos, direction, out hit, distance))
			{
				GameObject obj = hit.transform.gameObject;
				//Debug.Log ("touched=" + obj.name);
				if (obj != null && obj.GetComponent<InfectionRaycast> () != null && obj.GetComponent<InfectionRaycast> ().infected)
				{
					//Debug.Log ("infected");
					_infectionTime = DateTime.Now;
					infected = true;
					ChangeMaterial (infectionMaterial);
				}
			}
		}
	}

	void ChangeMaterial(Material mat)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject child = transform.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> () != null)
			{
				child.GetComponent<MeshRenderer> ().material = mat;
				child.GetComponent<GlowAnimation> ().UpdateEmiColor ();
			}
		}
	}
	#endregion

	#region public functions

	#endregion
}
