using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
	/* KillBox
	 * if player touches the object attaching this script, he dies
	 */
	private GameManager GM;

	// Use this for initialization
	void Start ()
	{
		GM = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.CompareTag ("Player"))
		{
			GM.PlayerDeath ();
		}
		else
		{
			Destroy (col.gameObject);
		}
	}
}
