using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanSpawner : MonoBehaviour
{
	/* OceanSpawner
	 * creates surface of blocks that looks like water, lava, clouds etc...
	 */

	#region serialized private variables
	[SerializeField] private float oceanSize = 100.0f;
	[SerializeField] private float waveSize = 10.0f;
	[SerializeField] private float altitude = -30.0f;
	[SerializeField] private GameObject wavePrefab = null;
	[SerializeField] private float followPlayerDistance = 10.0f;
	[SerializeField] private float followPlayerSpeed = 1.0f;
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

		for (int i = 0; i < oceanSize; i++)
		{
			for (int j = 0; j < oceanSize; j++)
			{
				offset.x = (waveSize * oceanSize / 2) - waveSize * i;
				offset.z = (waveSize * oceanSize / 2) - waveSize * j;
				offset.y = altitude;
				GameObject obj = Instantiate (wavePrefab, offset, Quaternion.identity);
				obj.transform.localScale = new Vector3 (waveSize, waveSize * 2, waveSize);
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
		float speed = followPlayerSpeed * distToPlayer / followPlayerDistance;

		if (distToPlayer > followPlayerDistance)
		{
			transform.position += (playerPositionProjected - transform.position).normalized * speed * Time.deltaTime;
		}
	}
	#endregion
}
