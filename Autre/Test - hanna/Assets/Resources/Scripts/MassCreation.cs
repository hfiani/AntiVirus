using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassCreation : MonoBehaviour
{
	public int numberBlocksPerCube= 100;

	// Use this for initialization
	void Start ()
	{
		ArrayList toDuplicateBlock = new ArrayList ();
		for (int i = 0; i < transform.childCount; i++)
		{
			toDuplicateBlock.Add(transform.GetChild(i));
		}

		ArrayList toDuplicateLine = (ArrayList)toDuplicateBlock.Clone();
		for (int i = 0; i < numberBlocksPerCube; i++)
		{
			foreach (Transform t in toDuplicateBlock)
			{
				GameObject created = GameObject.Instantiate (t.gameObject, transform);
				created.transform.localPosition = new Vector3 (
					created.transform.localPosition.x + created.transform.localScale.x * (i + 1),
					created.transform.localPosition.y,
					created.transform.localPosition.z
				);
				toDuplicateLine.Add (created.transform);
			}
		}
		toDuplicateBlock.Clear ();

		ArrayList toDuplicateSquare = (ArrayList)toDuplicateLine.Clone();
		for (int i = 0; i < numberBlocksPerCube; i++)
		{
			foreach (Transform t in toDuplicateLine)
			{
				GameObject created = GameObject.Instantiate (t.gameObject, transform);
				created.transform.localPosition = new Vector3 (
					created.transform.localPosition.x,
					created.transform.localPosition.y + created.transform.localScale.y * (i + 1),
					created.transform.localPosition.z
				);
				toDuplicateSquare.Add (created.transform);
			}
		}
		toDuplicateLine.Clear ();

		ArrayList toDuplicateCube = (ArrayList)toDuplicateSquare.Clone();
		for (int i = 0; i < numberBlocksPerCube; i++)
		{
			foreach (Transform t in toDuplicateSquare)
			{
				GameObject created = GameObject.Instantiate (t.gameObject, transform);
				created.transform.localPosition = new Vector3 (
					created.transform.localPosition.x,
					created.transform.localPosition.y,
					created.transform.localPosition.z + created.transform.localScale.z * (i + 1)
				);
				toDuplicateCube.Add (created.transform);
			}
		}
		toDuplicateSquare.Clear ();

		/*/
		((Transform)toDuplicateCube [Random.Range (0, toDuplicateCube.Count)]).gameObject.GetComponent<InfectionRaycast> ().infected = true;
		/*/
		((Transform)toDuplicateCube [0]).gameObject.GetComponent<InfectionRaycast> ().infected = true;
		//*/
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
