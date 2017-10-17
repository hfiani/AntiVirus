using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusInfectOnCollision : MonoBehaviour
{

	#region private variables
	private bool isAlive;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		isAlive = true;
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.GetComponent<InfectionRaycast> () != null && isAlive)
		{
			col.gameObject.GetComponent<InfectionRaycast> ().CreateInfection(col.transform);

			isAlive = false;

			Destroy (gameObject);
		}
	}
	#endregion
}
