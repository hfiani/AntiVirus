using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
	/* NPCManager
	 * Movement of NPC ("octets")
	 */

	#region serialized private variables
	[SerializeField] private float destinationChangeDelay = 5.0f;
	[SerializeField] private float walkRadius = 10.0f;
	[SerializeField] private float moveSpeed = 10.0f;
	[SerializeField] private GameObject targetPrefab;
	[SerializeField] private AudioClip buffSound;
	[SerializeField] private float buffVolume;
	#endregion

	#region private variables
	private float _timer;
	private Vector3 _startPos;

	private GameObject _target;
	private GameObject _parent;

	private bool _canMove = false;
	private float _travelTime;
	private AudioSource _audio;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		_audio = GetComponent<AudioSource> ();
		_parent = transform.parent.gameObject;
		_target = Instantiate (targetPrefab, transform.position, Quaternion.identity);
		_timer = Time.time + Random.Range(0f,destinationChangeDelay);
		FindTarget ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Time.time - _timer > destinationChangeDelay) {

			FindTarget();

			_timer = Time.time;
		}

		MoveTowardTarget ();
	}
	#endregion

	public void PlayBuffSound(){
		if (_audio.isPlaying){
			return;
		}
		_audio.pitch = 1.0f;
		_audio.PlayOneShot (buffSound, buffVolume);
	}

	public void PlayDeBuffSound(){
		if (_audio.isPlaying){
			return;
		}
		_audio.pitch = 0.5f;
		_audio.PlayOneShot (buffSound, buffVolume);
	}

	#region private functions
	void FindTarget()
	{
		Vector3 targetPos = transform.position;
		bool success = false;
		int max = 50;
		int k = 0;
		Vector3 dir;
		Vector3 pos = transform.position;
		float dist;
		do
		{
			targetPos = randomPosition ();
			dir = targetPos - transform.position;
			dist = Vector3.Distance (transform.position, targetPos);
			Ray ray = new Ray (pos, dir);
			RaycastHit hit;
			success = !Physics.Raycast (ray, out hit, dist);
			_travelTime = dist / moveSpeed;
			if (!success && hit.distance > 2.0f)
			{
				float alpha = Mathf.Clamp (hit.distance / dist, 0.33f, 0.66f);
				targetPos = Vector3.Lerp (pos, targetPos, alpha);
				dist = Vector3.Distance (transform.position, targetPos);
				success = true;
				_travelTime = dist / moveSpeed;
			}

			k++;
		} while(!success && k < max);
		_canMove = success;
		//Debug.Log (k);
		_target.transform.position = targetPos;
		_startPos = transform.position;
	}

	Vector3 randomPosition()
	{
		Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * walkRadius;
		Vector3 position = _parent.transform.position + new Vector3 (randomDirection.x, 0, randomDirection.y);

		return position;
	}

	void MoveTowardTarget()
	{
		if (!_canMove)
		{
			return;
		}
		float alpha = (Time.time - _timer) / _travelTime;

		transform.position = Vector3.Lerp (_startPos, _target.transform.position, alpha);
	}
	#endregion
}
