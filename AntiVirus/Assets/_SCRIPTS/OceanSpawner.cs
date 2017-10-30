using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanSpawner : MonoBehaviour
{
	#region serialized private variables
	[SerializeField] private float OceanSize = 100.0f;
	[SerializeField] private float WaveSize = 10.0f;
	[SerializeField] private float Altitude = -30.0f;
	[SerializeField] private float OscillationPeriod = 1.0f;
	[SerializeField] private GameObject WavePrefab = null;
	[SerializeField] private float FollowPlayerDistance = 10.0f;
	[SerializeField] private float FollowPlayerSpeed = 1.0f;
	#endregion

	#region private variables
	private GameManager GM;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		GM = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ();
		Vector3 offset = new Vector3 ();

		for (int i = 0; i < OceanSize; i++)
		{
			for (int j = 0; j < OceanSize; j++)
			{
				offset.x = (WaveSize * OceanSize / 2) - WaveSize * i;
				offset.z = (WaveSize * OceanSize / 2) - WaveSize * j;
				offset.y = Altitude;
				GameObject obj = Instantiate (WavePrefab, offset, Quaternion.identity);
				obj.transform.localScale = new Vector3 (WaveSize, WaveSize * 2, WaveSize);
				obj.transform.SetParent (gameObject.transform);
			}
		}
	}

	void Update()
	{
		FollowPlayer ();
	}
	#endregion

	#region private functions
	void FollowPlayer()
	{
		Vector3 playerPositionProjected = GM.GetPlayerPosition ();
		playerPositionProjected.y = transform.position.y;
		float distToPlayer = Vector3.Distance (transform.position, playerPositionProjected);
		float speed = FollowPlayerSpeed * distToPlayer / FollowPlayerDistance;

		if (distToPlayer > FollowPlayerDistance)
		{
			transform.position += (playerPositionProjected - transform.position).normalized * speed * Time.deltaTime;
		}
	}
	#endregion
}
