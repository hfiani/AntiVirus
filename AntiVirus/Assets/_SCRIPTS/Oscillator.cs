using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
	/* Oscillator
	 * Animation of each OceanSpawner block (each block of water surface, or clouds etc...)
	 */

	#region serialized private variables
	[SerializeField] private AnimationCurve _waveMovement = null;
	[SerializeField] private float _waveSpeed = 25f;
	[SerializeField] private float _waveHeight = 5f;
	#endregion

	#region private variables
	private float _timer = 0f;
	private Vector3 _startPos;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		_timer = Random.Range (0f, 1f);
		_startPos = transform.localPosition;
	}

	void Update ()
	{
		_timer += Time.deltaTime * _waveSpeed / 100f;

		if (_timer > 1f)
		{
			_timer = 0f;
		}

		transform.localPosition = _startPos + new Vector3 (0f, _waveHeight * _waveMovement.Evaluate (_timer), 0f);
	}
	#endregion
}
