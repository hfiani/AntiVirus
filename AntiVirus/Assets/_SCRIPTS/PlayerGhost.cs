using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
	#region serialized private variables
	[SerializeField] private AnimationCurve MovementCurve;
	#endregion

	#region private variables
	private float _travelTime;
	private Vector3 _travelStartPosition;
	private Vector3 _travelEndPosition;
	private Quaternion _travelStartOrientation;
	private Quaternion _travelEndOrientation;
	private bool _isTravelling = false;
	private float _timer;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_isTravelling)
		{
			TravelFromStartToEndPoint ();
		}
	}
	#endregion

	#region private functions
	void TravelFromStartToEndPoint()
	{
		float alpha = (Time.time - _timer) / _travelTime;
		if (alpha > 1)
		{
			_isTravelling = false;
			transform.position = _travelEndPosition;
			transform.rotation = _travelEndOrientation;
		}
		transform.position = Vector3.Lerp (_travelStartPosition, _travelEndPosition, MovementCurve.Evaluate (alpha));
		transform.rotation = Quaternion.Lerp (_travelStartOrientation, _travelEndOrientation, MovementCurve.Evaluate (alpha));
	}
 	#endregion

	#region public functionsn
	public void TriggerTravelToPoint(GameObject destination, float travelduration)
	{
		_travelStartPosition = transform.position;
		_travelEndPosition = destination.transform.position;
		_travelStartOrientation = transform.rotation;
		_travelEndOrientation = destination.transform.rotation;
		_isTravelling = true;
		_travelTime = travelduration;
		_timer = Time.time;
	}
	#endregion
}
