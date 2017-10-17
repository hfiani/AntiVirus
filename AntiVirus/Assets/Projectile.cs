using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {


	#region public variables
	public float speed = 10.0f;
	#endregion

	#region private variables
	#endregion

	// Use this for initialization
	void Start () {

		GetComponent<Rigidbody>().AddForce(transform.forward * speed);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
