﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InfectionRaycast : MonoBehaviour
{
	#region enum
	public enum VirusType{ALL_AT_ONCE, ONE_AT_TIME};
	public enum BlockType{DESTRUCTABLE_INFECTABLE, UNDESTRUCTABLE_INFECTABLE, UNDESTRUCTABLE_UNINFECTABLE};
	#endregion

	#region public variables
	public float epsilon = 0.001f;
	public float distance = 0.03f;
	public float timeToDestruction = 10.0f;
	public float timeToInfection = 2.0f;
	public float timeToRemoveImmunity = 20.0f;
	public float timeBetweenInfections = 0.5f;
	public bool infected = false;
	public bool immune = false;
	public VirusType virusType = VirusType.ALL_AT_ONCE;
	public BlockType blockType = BlockType.DESTRUCTABLE_INFECTABLE;
	public Material[] materialsInfection = new Material[7];
	#endregion

	#region private variables
	private DateTime infection_time = new DateTime(0);
	private DateTime immunity_time = new DateTime(0);
	//private int health_max = 100;
	//private int health = 100;
	private int blockTurn = 0;
	private Vector3[] directions = {
		Vector3.up, Vector3.down,
		Vector3.left, Vector3.right,
		Vector3.forward, Vector3.back
	};
	private MaterialPropertyBlock matBlock;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		matBlock = new MaterialPropertyBlock();
		timeToDestruction *= 1000;
		timeToInfection *= 1000;
		timeBetweenInfections *= 1000;
		timeToRemoveImmunity *= 1000;

		object[] materials_infection_object = Resources.LoadAll("Materials/Infections", typeof(Material));
		materialsInfection = new Material[materials_infection_object.Length];
		for (int i = 0; i < materials_infection_object.Length; i++)
		{
			materialsInfection [i] = (Material)materials_infection_object [i];
		}

		if (blockType == BlockType.UNDESTRUCTABLE_UNINFECTABLE)
		{
			ChangeMaterial(transform, Resources.Load<Material>("Materials/mat_cube - uninfectable"));
		}

		CreateImmunity (transform);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (infected)
		{
			// initialize infection time
			if (infection_time == new DateTime (0))
			{
				infection_time = DateTime.Now;
			}

			float duration_infected = (float)(DateTime.Now - infection_time).TotalMilliseconds;

			ManageSelfInfection (duration_infected);
			InfectNeighbours (duration_infected);
		}
		else
		{
			float duration_immuned = (float)(DateTime.Now - immunity_time).TotalMilliseconds;
			CheckImmunity (duration_immuned);
		}
	}
	#endregion

	#region private functions
	/// <summary>
	/// Checks the immunity.
	/// </summary>
	/// <param name="duration_immuned">Duration immuned.</param>
	void CheckImmunity(float duration_immuned)
	{
		if (immune && duration_immuned >= timeToRemoveImmunity)
		{
			RemoveImmunity (transform);
		}
	}

	/// <summary>
	/// Changes the color of the object given
	/// </summary>
	/// <param name="trans">Trans.</param>
	/// <param name="color">Color.</param>
	void ChangeColor(Transform trans, Vector3 color)
	{
		if (trans.GetComponent<MeshRenderer> () != null)
		{
			trans.gameObject.GetComponent<MeshRenderer> ().materials [0].color = new Color (color.x, color.y, color.z);
		}
		for (int i = 0; i < trans.childCount; i++)
		{
			GameObject child = trans.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> ())
			{
				matBlock.SetColor("_Color", new Color(color.x, color.y, color.z));
				child.GetComponent<MeshRenderer> ().SetPropertyBlock(matBlock);
				//child.GetComponent<MeshRenderer> ().materials [0].color = new Color (color.x, color.y, color.z);
			}
		}
	}

	/// <summary>
	/// Changes the material.
	/// </summary>
	/// <param name="trans">Trans.</param>
	/// <param name="mat">Mat.</param>
	void ChangeMaterial(Transform trans, Material mat)
	{
		if (trans.GetComponent<MeshRenderer> () != null)
		{
			trans.gameObject.GetComponent<MeshRenderer> ().material = mat;
		}
		for (int i = 0; i < trans.childCount; i++)
		{
			GameObject child = trans.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> ())
			{
				child.GetComponent<MeshRenderer> ().material = mat;
			}
		}
	}

	/// <summary>
	/// Manages the infection of the current Block.
	/// </summary>
	/// <param name="duration_infected">Duration infected.</param>
	void ManageSelfInfection(float duration_infected)
	{
		if (duration_infected >= timeToDestruction) // time's up -> destroy object
		{
			if (blockType == BlockType.DESTRUCTABLE_INFECTABLE)
			{
				Destroy (gameObject);
			}
			else if (blockType == BlockType.UNDESTRUCTABLE_INFECTABLE)
			{
				ChangeMaterial(transform, Resources.Load<Material>("Materials/mat_cube - infection full"));
			}
		}
		/*else if (duration_infected < timeToDestruction / 2) // green to yellow
		{
			ChangeColor (transform, new Vector3 (duration_infected / (timeToDestruction / 2), 1, 0));
		}
		else if (duration_infected >= timeToDestruction / 2) // yellow to red
		{
			ChangeColor (transform, new Vector3 (1, 2 - duration_infected / (timeToDestruction / 2), 0));
		}*/
		else
		{
			int material_index = (int)Math.Floor (duration_infected * materialsInfection.Length / timeToDestruction);
			ChangeMaterial(transform, materialsInfection[material_index]);
		}
	}

	/// <summary>
	/// Infects the neighbours
	/// </summary>
	/// <param name="duration_infected">Duration infected.</param>
	void InfectNeighbours(float duration_infected)
	{
		if (virusType == VirusType.ALL_AT_ONCE) // parallel infections
		{
			if (duration_infected >= timeToInfection)
			{
				foreach (Vector3 direction in directions)
				{
					Transform t = GetNeighbourBlocks (direction, distance);
					CreateInfection (t);
				}
			}
		}
		else if (virusType == VirusType.ONE_AT_TIME) // serial infections
		{
			if (duration_infected >= timeToInfection + timeBetweenInfections * blockTurn && blockTurn < directions.Length)
			{
				Transform t = GetNeighbourBlocks (directions [blockTurn], distance);
				CreateInfection (t);
				blockTurn++;
			}
		}
	}

	/// <summary>
	/// Gets the neighbour blocks using the direction vectors
	/// </summary>
	/// <returns>The neighbour blocks.</returns>
	/// <param name="direction">Direction.</param>
	/// <param name="distance">Distance.</param>
	Transform GetNeighbourBlocks(Vector3 direction, float distance)
	{
		RaycastHit hit;
		BoxCollider b = transform.gameObject.GetComponent<BoxCollider> ();

		Vector3 ray_localPosition = new Vector3 (
			direction.x * (transform.localScale.x * b.size.x / 2 - epsilon),
			direction.y * (transform.localScale.y * b.size.y / 2 - epsilon),
			direction.z * (transform.localScale.z * b.size.z / 2 - epsilon));
		Vector3 ray_pos = (transform.position + b.center) + ray_localPosition;

		if (Physics.Raycast (ray_pos, direction, out hit, distance))
		{
			return hit.transform;
		}
		else
		{
			return null;
		}
	}
	#endregion

	#region public functions
	/// <summary>
	/// Creates the infection
	/// </summary>
	/// <param name="t">T.</param>
	public void CreateInfection(Transform t)
	{
		if (t != null &&
			t.gameObject.GetComponent<InfectionRaycast> () != null &&
			t.gameObject.GetComponent<InfectionRaycast> ().blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE &&
			!t.gameObject.GetComponent<InfectionRaycast> ().immune)
		{
			t.gameObject.GetComponent<InfectionRaycast> ().infected = true;
		}
	}

	/// <summary>
	/// Removes the infection.
	/// </summary>
	/// <param name="t">T.</param>
	public void RemoveInfection(Transform t)
	{
		if (t != null &&
			t.gameObject.GetComponent<InfectionRaycast> () != null &&
			t.gameObject.GetComponent<InfectionRaycast> ().blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE)
		{
			t.gameObject.GetComponent<InfectionRaycast> ().infected = false;
			ChangeMaterial(t, Resources.Load<Material>("Materials/mat_cube"));
		}
	}

	/// <summary>
	/// Creates the immunity.
	/// </summary>
	/// <param name="t">T.</param>
	public void CreateImmunity(Transform t)
	{
		if (t != null &&
			t.gameObject.GetComponent<InfectionRaycast> () != null &&
			t.gameObject.GetComponent<InfectionRaycast> ().blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE &&
			!t.gameObject.GetComponent<InfectionRaycast> ().immune)
		{
			t.gameObject.GetComponent<InfectionRaycast> ().immune = true;
			ChangeMaterial(t, Resources.Load<Material>("Materials/mat_cube - immune"));
			immunity_time = DateTime.Now;

			RemoveInfection(t);
		}
	}

	/// <summary>
	/// Removes the immunity.
	/// </summary>
	/// <param name="t">T.</param>
	public void RemoveImmunity(Transform t)
	{
		if (t != null &&
			t.gameObject.GetComponent<InfectionRaycast> () != null &&
			t.gameObject.GetComponent<InfectionRaycast> ().blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE &&
			t.gameObject.GetComponent<InfectionRaycast> ().immune)
		{
			t.gameObject.GetComponent<InfectionRaycast> ().immune = false;
			ChangeMaterial(t, Resources.Load<Material>("Materials/mat_cube"));
			immunity_time = new DateTime (0);
		}
	}
	#endregion
}
