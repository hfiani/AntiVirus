using System.Collections;
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
	public float timeBetweenInfections = 0.5f;

	public float timeToRemoveImmunity = 20.0f;

	public float factorRemoveInfection = 0.6f;

	public bool infected = false;
	public bool immune = false;
	public bool repairing = false;
	public VirusType virusType = VirusType.ALL_AT_ONCE;
	public BlockType blockType = BlockType.DESTRUCTABLE_INFECTABLE;
	public Material[] materialsInfection = new Material[7];
	#endregion

	#region private variables
	private float timeToRemoveInfection;
	private float timeToReparation;
	private float timeBetweenReparations;
	private DateTime reparation_time = new DateTime(0);
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
	private int material_index_infected = 0;
	private int material_index_repaired = 0;
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

		timeToRemoveInfection = timeToDestruction * factorRemoveInfection;
		timeToReparation = timeToInfection * factorRemoveInfection;
		timeBetweenReparations = timeBetweenInfections * factorRemoveInfection;

		/*object[] materials_infection_object = Resources.LoadAll("Materials/Infections", typeof(Material));
		materialsInfection = new Material[materials_infection_object.Length];
		for (int i = 0; i < materials_infection_object.Length; i++)
		{
			materialsInfection [i] = (Material)materials_infection_object [i];
		}*/

		if (blockType == BlockType.UNDESTRUCTABLE_UNINFECTABLE)
		{
			ChangeMaterial(Resources.Load<Material>("Materials/mat_cube - uninfectable"));
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (repairing && reparation_time == new DateTime (0))
		{
			RepairInfection ();
		}
		if (infected)
		{
			float duration_infected = (float)(DateTime.Now - infection_time).TotalMilliseconds;

			ManageSelfInfection (duration_infected);
			InfectNeighbours (duration_infected);
		}
		else if (repairing)
		{
			float duration_repaired = (float)(DateTime.Now - reparation_time).TotalMilliseconds;

			ManageSelfReparation (duration_repaired);
			RepairNeighbours (duration_repaired);
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
			RemoveImmunity ();
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
	/// <param name="mat">Mat.</param>
	void ChangeMaterial(Material mat)
	{
		
		if (transform.GetComponent<MeshRenderer> () != null)
		{
			transform.gameObject.GetComponent<MeshRenderer> ().material = mat;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject child = transform.GetChild (i).gameObject;
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
				ChangeMaterial(Resources.Load<Material>("Materials/mat_cube - infection full"));
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
			material_index_infected = (int)Math.Floor (duration_infected * materialsInfection.Length / timeToDestruction);
			int material_index = material_index_infected + material_index_repaired;
			if (material_index < materialsInfection.Length && material_index >= 0)
			{
				ChangeMaterial (materialsInfection [material_index]);
			}
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
					if (t != null && t.gameObject.GetComponent<InfectionRaycast> () != null)
					{
						t.gameObject.GetComponent<InfectionRaycast> ().CreateInfection ();
					}
				}
			}
		}
		else if (virusType == VirusType.ONE_AT_TIME) // serial infections
		{
			if (duration_infected >= timeToInfection + timeBetweenInfections * blockTurn && blockTurn < directions.Length)
			{
				Transform t = GetNeighbourBlocks (directions [blockTurn], distance);
				if (t != null && t.gameObject.GetComponent<InfectionRaycast> () != null)
				{
					t.gameObject.GetComponent<InfectionRaycast> ().CreateInfection ();
				}
				blockTurn++;
			}
		}
	}

	/// <summary>
	/// Manages the infection of the current Block.
	/// </summary>
	/// <param name="duration_infected">Duration infected.</param>
	void ManageSelfReparation(float duration_repaired)
	{
		if (duration_repaired >= timeToRemoveInfection) // time's up -> destroy object
		{
			RemoveInfection ();
		}
		else
		{
			material_index_repaired = (int)Math.Floor (duration_repaired * materialsInfection.Length / timeToRemoveInfection);
			int material_index = material_index_infected + material_index_repaired;
			if (material_index < materialsInfection.Length && material_index >= 0)
			{
				ChangeMaterial (materialsInfection [materialsInfection.Length - 1 - material_index]);
			}
		}
	}

	/// <summary>
	/// Infects the neighbours
	/// </summary>
	/// <param name="duration_infected">Duration infected.</param>
	void RepairNeighbours(float duration_repaired)
	{
		if (virusType == VirusType.ALL_AT_ONCE) // parallel infections
		{
			if (duration_repaired >= timeToReparation)
			{
				foreach (Vector3 direction in directions)
				{
					Transform t = GetNeighbourBlocks (direction, distance);
					t.gameObject.GetComponent<InfectionRaycast>().RepairInfection ();
				}
			}
		}
		else if (virusType == VirusType.ONE_AT_TIME) // serial infections
		{
			if (duration_repaired >= timeToReparation + timeBetweenReparations * blockTurn && blockTurn < directions.Length)
			{
				Transform t = GetNeighbourBlocks (directions [blockTurn], distance);
				t.gameObject.GetComponent<InfectionRaycast>().RepairInfection ();
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
	/// Creates the self infection.
	/// </summary>
	public void CreateInfection()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && !immune)
		{
			infection_time = DateTime.Now;
			infected = true;
			material_index_repaired += material_index_infected;
			material_index_infected = 0;

		}
	}

	/// <summary>
	/// Removes the self infection.
	/// </summary>
	public void RemoveInfection()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE)
		{
			infected = false;
			ChangeMaterial(Resources.Load<Material>("Materials/mat_cube"));
			blockTurn = 0;
			material_index_infected = 0;
			material_index_repaired = 0;
		}
	}

	/// <summary>
	/// Creates the self immunity.
	/// </summary>
	public void CreateImmunity()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && !immune)
		{
			RemoveInfection();

			immune = true;
			ChangeMaterial(Resources.Load<Material>("Materials/mat_cube - immune"));
			immunity_time = DateTime.Now;
		}
	}

	/// <summary>
	/// Removes the self immunity.
	/// </summary>
	public void RemoveImmunity()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && immune)
		{
			immune = false;
			ChangeMaterial(Resources.Load<Material>("Materials/mat_cube"));
			immunity_time = new DateTime (0);
			blockTurn = 0;
		}
	}

	/// <summary>
	/// Repairs the self infection.
	/// </summary>
	public void RepairInfection()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && infected)
		{
			material_index_infected += material_index_repaired;
			material_index_repaired = 0;
			immune = false;
			infected = false;
			repairing = true;
			reparation_time = DateTime.Now;
			blockTurn = 0;
		}
	}
	#endregion
}
