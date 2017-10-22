using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerGhost : MonoBehaviour {

	#region public variables
	public AnimationCurve movementCurve;

	#endregion

	#region private variables
	private float travelTime;
	private Vector3 travelStartPosition;
	private Vector3 travelEndPosition;
	private Quaternion travelStartOrientation;
	private Quaternion travelEndOrientation;
	private bool isTravelling = false;
	private float timer;


	#endregion

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (isTravelling) {
			TravelFromStartToEndPoint ();
		}
		
	}

	void TravelFromStartToEndPoint(){
		float alpha = (Time.time - timer) / travelTime;
		if (alpha > 1) {
			isTravelling = false;
			transform.position = travelEndPosition;
			transform.rotation = travelEndOrientation;
		}
		transform.position = Vector3.Lerp (travelStartPosition, travelEndPosition, movementCurve.Evaluate(alpha));
		transform.rotation = Quaternion.Lerp (travelStartOrientation, travelEndOrientation, movementCurve.Evaluate(alpha));

	}

	public void TriggerTravelToPoint(GameObject destination, float travelduration){

		travelStartPosition = transform.position;
		travelEndPosition = destination.transform.position;
		travelStartOrientation = transform.rotation;
		travelEndOrientation = destination.transform.rotation;
		isTravelling = true;
		travelTime = travelduration;
		timer = Time.time;

	}
}
