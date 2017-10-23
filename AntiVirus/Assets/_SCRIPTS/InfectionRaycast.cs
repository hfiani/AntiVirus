using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InfectionRaycast : MonoBehaviour
{
	#region enum
	public enum VirusType{ALL_AT_ONCE, ONE_AT_TIME};
	public enum BlockType{DESTRUCTABLE_INFECTABLE, UNDESTRUCTABLE_INFECTABLE, UNDESTRUCTABLE_UNINFECTABLE, OBJECTIVE};
	#endregion

	#region public variables
	public AnimationCurve speedCurve;

	public float epsilon = 0.001f;
	public float distance = 0.03f;

	public float timeToDestruction = 10.0f;
	public float timeToInfection = 2.0f;
	public float timeBetweenInfectionsMin = 0.5f;
	public float timeBetweenInfectionsMax = 5.0f;
	public float timeInfectionStability = 20.0f;

	public float timeToRemoveImmunity = 20.0f;
	public float timeToRemoveRepairImmunity = 3.0f;

	public float factorRemoveInfection = 0.2f;

	public bool infected = false;
	public bool immune = false;
	public bool repair_immune = false;
	public bool repairing = false;
	public VirusType virusType = VirusType.ALL_AT_ONCE;
	public BlockType blockType = BlockType.DESTRUCTABLE_INFECTABLE;
	public Material[] materialsInfection = new Material[7];
	#endregion

	#region private variables
	private Material original_material;
	private float timeBetweenReparations;
	private DateTime start_infection_time = new DateTime(0);
	private DateTime last_check_infection_time = new DateTime(0);
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
	private ArrayList directions_per_turn = new ArrayList();
	private MaterialPropertyBlock matBlock;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		matBlock = new MaterialPropertyBlock();
		timeToDestruction *= 1000;
		timeToInfection *= 1000;
		timeBetweenInfectionsMin *= 1000;
		timeBetweenInfectionsMax *= 1000;
		timeToRemoveImmunity *= 1000;
		timeToRemoveRepairImmunity *= 1000;
		timeInfectionStability *= 1000;

		timeBetweenReparations = timeBetweenInfectionsMin * factorRemoveInfection;

		/*object[] materials_infection_object = Resources.LoadAll("Materials/Infections", typeof(Material));
		materialsInfection = new Material[materials_infection_object.Length];
		for (int i = 0; i < materials_infection_object.Length; i++)
		{
			materialsInfection [i] = (Material)materials_infection_object [i];
		}*/

		original_material = GetMaterial();

		if (blockType == BlockType.UNDESTRUCTABLE_UNINFECTABLE)
		{
			ChangeMaterial(Resources.Load<Material>("Materials/mat_cube - uninfectable"));
		}

		ResetDirections ();
		GameManager.TotalBlocks++;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//to be removed
		if (repairing && reparation_time == new DateTime (0))
		{
			RepairInfection ();
		}

		if (repairing)
		{
			float duration_repaired = (float)(DateTime.Now - reparation_time).TotalMilliseconds;

			RepairNeighbours (duration_repaired);
		}
		else if (infected)
		{
			float duration_infected = (float)(DateTime.Now - infection_time).TotalMilliseconds;

			ManageSelfInfection (duration_infected);
			InfectNeighbours (duration_infected);
		}

		if (immune)
		{
			float duration_immuned = (float)(DateTime.Now - immunity_time).TotalMilliseconds;
			CheckImmunity (duration_immuned, timeToRemoveImmunity);
		}

		if (repair_immune)
		{
			float duration_repair_immuned = (float)(DateTime.Now - reparation_time).TotalMilliseconds;
			CheckImmunity (duration_repair_immuned, timeToRemoveRepairImmunity);
		}
	}
	#endregion

	#region private functions
	/// <summary>
	/// Resets the directions.
	/// </summary>
	void ResetDirections()
	{
		directions_per_turn.Clear ();
		foreach (Vector3 direction in directions)
		{
			directions_per_turn.Add (direction);
		}
	}

	/// <summary>
	/// Gets the direction.
	/// </summary>
	/// <returns>The direction.</returns>
	Vector3 GetDirection()
	{
		int rand_index = UnityEngine.Random.Range (0, directions_per_turn.Count);
		Vector3 direction = (Vector3)directions_per_turn [rand_index];

		directions_per_turn.RemoveAt (rand_index);
		if (directions_per_turn.Count == 0)
		{
			ResetDirections ();
		}

		return direction;
	}

	/// <summary>
	/// Checks the immunity.
	/// </summary>
	/// <param name="duration_immuned">Duration immuned.</param>
	void CheckImmunity(float duration_immuned, float time_to_remove_immunity)
	{
		if (duration_immuned >= time_to_remove_immunity)
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
			if (child.GetComponent<MeshRenderer> () != null)
			{
				child.GetComponent<MeshRenderer> ().material = mat;
			}
		}
	}

	/// <summary>
	/// Gets the material.
	/// </summary>
	/// <returns>The material.</returns>
	Material GetMaterial()
	{
		if (transform.GetComponent<MeshRenderer> () != null)
		{
			return transform.gameObject.GetComponent<MeshRenderer> ().material;
		}

		GameObject child = transform.GetChild (0).gameObject;
		if (child.GetComponent<MeshRenderer> () != null)
		{
			return child.GetComponent<MeshRenderer> ().material;
		}
		return null;
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
				//Destroy (gameObject);
				gameObject.layer = 11;
				ChangeMaterial(Resources.Load<Material>("Materials/mat_cube - infection ghost"));
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
			int material_index = (int)Math.Floor (duration_infected * materialsInfection.Length / timeToDestruction);
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
						t.gameObject.GetComponent<InfectionRaycast> ().CreateInfection (start_infection_time);
					}
				}
			}
		}
		else if (virusType == VirusType.ONE_AT_TIME) // serial infections
		{
			float time_step = speedCurve.Evaluate((float) (DateTime.Now - start_infection_time).TotalMilliseconds / timeInfectionStability);
		
			if (duration_infected > timeToInfection && duration_infected - (last_check_infection_time - infection_time).TotalMilliseconds >= Mathf.Lerp(timeBetweenInfectionsMin, timeBetweenInfectionsMax, time_step))
			{
				Vector3 direction = GetDirection ();
				Transform t = GetNeighbourBlocks (direction, distance);
				if (t != null && t.gameObject.GetComponent<InfectionRaycast> () != null)
				{
					t.gameObject.GetComponent<InfectionRaycast> ().CreateInfection (start_infection_time);
				}
				last_check_infection_time = DateTime.Now;
			}
		}
	}

	/// <summary>
	/// Repairs the neighbours
	/// </summary>
	/// <param name="duration_infected">Duration Repaired.</param>
	void RepairNeighbours(float duration_repaired)
	{
		if (virusType == VirusType.ALL_AT_ONCE) // parallel infections
		{
			foreach (Vector3 direction in directions)
			{
				Transform t = GetNeighbourBlocks (direction, distance);
				if (t != null && t.gameObject.GetComponent<InfectionRaycast> () != null)
				{
					t.gameObject.GetComponent<InfectionRaycast> ().RepairInfection ();
				}
			}
		}
		else if (virusType == VirusType.ONE_AT_TIME) // serial infections
		{
			if (duration_repaired >= timeBetweenReparations * blockTurn && blockTurn < directions.Length)
			{
				Vector3 direction = GetDirection ();
				Transform t = GetNeighbourBlocks (direction, distance);
				if (t != null && t.gameObject.GetComponent<InfectionRaycast> () != null)
				{
					t.gameObject.GetComponent<InfectionRaycast> ().RepairInfection ();
				}
				blockTurn++;
			}
			else if (blockTurn >= directions.Length)
			{
				repairing = false;
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
	public void CreateInfection(DateTime startInfectionTime)
	{
		if (blockType == BlockType.OBJECTIVE)
		{
			GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ().GameOver ();
		}
		else if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && !immune && !repair_immune && !infected)
		{
			start_infection_time = startInfectionTime;
			last_check_infection_time = DateTime.Now;
			infection_time = DateTime.Now;
			infected = true;
			GameManager.InfectedBlocks++;
		}
	}

	/// <summary>
	/// Removes the self infection.
	/// </summary>
	public void RemoveInfection()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && infected)
		{
			infected = false;
			ChangeMaterial(original_material);
			gameObject.layer = 0;
			infection_time = new DateTime (0);
			blockTurn = 0;
			GameManager.InfectedBlocks--;
		}
	}

	/// <summary>
	/// Creates the self immunity.
	/// </summary>
	public void CreateImmunity()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && blockType != BlockType.OBJECTIVE)
		{
			RemoveInfection();

			immune = true;
			repair_immune = false;
			ChangeMaterial(Resources.Load<Material>("Materials/mat_cube - immune"));
			immunity_time = DateTime.Now;
		}
	}

	/// <summary>
	/// Removes the self immunity.
	/// </summary>
	public void RemoveImmunity()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && (immune || repair_immune))
		{
			immune = false;
			repair_immune = false;
			ChangeMaterial(original_material);
			immunity_time = new DateTime (0);
			blockTurn = 0;
		}
	}

	/// <summary>
	/// Repairs the self infection.
	/// </summary>
	public void RepairInfection()
	{
		if (blockType != BlockType.UNDESTRUCTABLE_UNINFECTABLE && infected && blockType != BlockType.OBJECTIVE)
		{
			RemoveInfection ();

			immune = false;
			infected = false;
			repairing = true;
			repair_immune = true;
			ChangeMaterial(Resources.Load<Material>("Materials/mat_cube - immune"));
			reparation_time = DateTime.Now;
			blockTurn = 0;
			ResetDirections ();
		}
	}
	#endregion
}
