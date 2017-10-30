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
	public bool Infected = false;
	#endregion

	#region serialized private variables
	[SerializeField] private AnimationCurve SpeedCurve;

	[SerializeField] private Material MaterialImmune;
	[SerializeField] private Material MaterialInfectionGhost;
	[SerializeField] private Material MaterialInfectionFull;

	[SerializeField] private float Epsilon = 0.001f;
	[SerializeField] private float Distance = 0.03f;

	[SerializeField] private float TimeToDestruction = 10.0f;
	[SerializeField] private float TimeToInfection = 2.0f;
	[SerializeField] private float TimeBetweenInfectionsMin = 0.5f;
	[SerializeField] private float TimeBetweenInfectionsMax = 5.0f;
	[SerializeField] private float TimeInfectionStability = 20.0f;

	[SerializeField] private float TimeToRemoveImmunity = 20.0f;
	[SerializeField] private float TimeToRemoveRepairImmunity = 3.0f;

	[SerializeField] private bool Immune = false;
	[SerializeField] private bool RepairImmune = false;
	[SerializeField] private bool Repairing = false;
	[SerializeField] private VirusType VirusTypeVar = VirusType.ALL_AT_ONCE;
	[SerializeField] private BlockType BlockTypeVar = BlockType.DESTRUCTABLE_INFECTABLE;
	[SerializeField] private Material[] MaterialsInfection = new Material[7];
	#endregion

	#region private variables
	private GameManager GM;
	private float _factorRemoveInfection = 4.0f;
	private MeshRenderer _meshRenderer;
	private MeshRenderer[] _meshRendererChildren; 
	private Material _originalMaterial;
	private float _timeBetweenReparations;
	private DateTime _startInfectionTime = new DateTime(0);
	private DateTime _lastCheckInfectionTime = new DateTime(0);
	private DateTime _reparationTime = new DateTime(0);
	private DateTime _infectionTime = new DateTime(0);
	private DateTime _immunityTime = new DateTime(0);
	private int _blockTurn = 0;
	private Vector3[] _directions = {
		Vector3.up, Vector3.down,
		Vector3.left, Vector3.right,
		Vector3.forward, Vector3.back
	};
	private ArrayList _neighbours = new ArrayList();
	private ArrayList _neighboursPerTurn = new ArrayList();
	private bool _isInit = false;
	private float _initTimer;
	#endregion

	#region events
	void Start ()
	{
		GM = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ();
	}

	// Update is called once per frame
	void Update ()
	{
		if (!_isInit)
		{
			Init ();
		}
		else if (Repairing || Infected || Immune || RepairImmune)
		{
			if (GameManager.Restart)
			{
				Repairing = false;
				RemoveInfection ();
				RemoveImmunity ();
			}
			if (Repairing)
			{
				float duration_repaired = (float)(DateTime.Now - _reparationTime).TotalMilliseconds;

				RepairNeighbours (duration_repaired);
			}
			else if (Infected)
			{
				float duration_infected = (float)(DateTime.Now - _infectionTime).TotalMilliseconds;

				ManageSelfInfection (duration_infected);
				InfectNeighbours (duration_infected);
			}
			else if (Immune)
			{
				float duration_immuned = (float)(DateTime.Now - _immunityTime).TotalMilliseconds;
				CheckImmunity (duration_immuned, TimeToRemoveImmunity);
			}
			else if (RepairImmune)
			{
				float duration_repair_immuned = (float)(DateTime.Now - _reparationTime).TotalMilliseconds;
				CheckImmunity (duration_repair_immuned, TimeToRemoveRepairImmunity);
			}
		}
	}
	#endregion

	#region private functions
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Init()
	{
		_isInit = true;

		TimeToDestruction *= 1000;
		TimeToInfection *= 1000;
		TimeBetweenInfectionsMin *= 1000;
		TimeBetweenInfectionsMax *= 1000;
		TimeToRemoveImmunity *= 1000;
		TimeToRemoveRepairImmunity *= 1000;
		TimeInfectionStability *= 1000;

		_timeBetweenReparations = TimeBetweenInfectionsMin * _factorRemoveInfection;

		_meshRenderer = transform.GetComponent<MeshRenderer> ();
		_meshRendererChildren = new MeshRenderer[transform.childCount];
		for (int i = 0; i < _meshRendererChildren.Length; i++)
		{
			GameObject child = transform.GetChild (i).gameObject;
			_meshRendererChildren[i] = child.GetComponent<MeshRenderer> ();
		}

		_originalMaterial = GetMaterial();

		// check all valid directions
		for (int i = 0; i < _directions.Length; i++)
		{
			Transform neighbour = GetNeighbourBlocks (_directions [i], Distance);
			if (neighbour == null)
			{
				_directions [i] = Vector3.zero;
			}
			else
			{
				_neighbours.Add (neighbour.gameObject);
			}
		}

		ResetNeighbours ();
		GameManager.TotalBlocks++;
		this.enabled = false;
	}

	/// <summary>
	/// Resets the neighbours.
	/// </summary>
	void ResetNeighbours()
	{
		_neighboursPerTurn.Clear ();
		foreach (GameObject neighbour in _neighbours)
		{
			if (neighbour != null)
			{
				_neighboursPerTurn.Add (neighbour);
			}
		}
	}

	/// <summary>
	/// Gets the neighbour.
	/// </summary>
	/// <returns>The neighbour.</returns>
	GameObject GetNeighbour()
	{
		int rand_index = UnityEngine.Random.Range (0, _neighboursPerTurn.Count);
		GameObject neighbour = (GameObject)_neighboursPerTurn [rand_index];

		_neighboursPerTurn.RemoveAt (rand_index);
		if (_neighboursPerTurn.Count == 0)
		{
			ResetNeighbours ();
		}

		return neighbour;
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
	/// Changes the material.
	/// </summary>
	/// <param name="mat">Mat.</param>
	void ChangeMaterial(Material mat)
	{
		if (_meshRenderer != null)
		{
			_meshRenderer.material = mat;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			if (_meshRendererChildren[i] != null)
			{
				_meshRendererChildren[i].material = mat;
			}
		}
	}

	/// <summary>
	/// Gets the material.
	/// </summary>
	/// <returns>The material.</returns>
	Material GetMaterial()
	{
		if (_meshRenderer != null)
		{
			return _meshRenderer.sharedMaterial;
		}

		if (_meshRendererChildren[0] != null)
		{
			return _meshRendererChildren[0].sharedMaterial;
		}
		return null;
	}

	/// <summary>
	/// Manages the infection of the current Block.
	/// </summary>
	/// <param name="duration_infected">Duration infected.</param>
	void ManageSelfInfection(float duration_infected)
	{
		if (duration_infected >= TimeToDestruction) // time's up -> destroy object
		{
			if (BlockTypeVar == BlockType.DESTRUCTABLE_INFECTABLE)
			{
				//Destroy (gameObject);
				gameObject.layer = 11;
				ChangeMaterial(MaterialInfectionGhost);
				this.enabled = false;
			}
			else if (BlockTypeVar == BlockType.UNDESTRUCTABLE_INFECTABLE)
			{
				ChangeMaterial(MaterialInfectionFull);
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
			int material_index = (int)Math.Floor (duration_infected * MaterialsInfection.Length / TimeToDestruction);
			if (material_index < MaterialsInfection.Length && material_index >= 0)
			{
				ChangeMaterial (MaterialsInfection [material_index]);
			}
		}
	}

	/// <summary>
	/// Infects the neighbours
	/// </summary>
	/// <param name="duration_infected">Duration infected.</param>
	void InfectNeighbours(float duration_infected)
	{
		if (VirusTypeVar == VirusType.ALL_AT_ONCE) // parallel infections
		{
			if (duration_infected >= TimeToInfection)
			{
				foreach (Vector3 direction in _directions)
				{
					//Transform t = GetNeighbourBlocks (direction, distance);
					Transform t = GetNeighbour ().transform;
					if (t != null)
					{
						InfectionRaycast r = t.gameObject.GetComponent<InfectionRaycast> ();
						if (r != null)
						{
							r.CreateInfection (_startInfectionTime);
						}
					}
				}
			}
		}
		else if (VirusTypeVar == VirusType.ONE_AT_TIME) // serial infections
		{
			float time_step = SpeedCurve.Evaluate((float) (DateTime.Now - _startInfectionTime).TotalMilliseconds / TimeInfectionStability);
		
			if (duration_infected > TimeToInfection && duration_infected - (_lastCheckInfectionTime - _infectionTime).TotalMilliseconds >= Mathf.Lerp(TimeBetweenInfectionsMin, TimeBetweenInfectionsMax, time_step))
			{
				/*Vector3 direction = GetDirection ();
				Transform t = GetNeighbourBlocks (direction, distance);*/
				Transform t = GetNeighbour ().transform;
				if (t != null)
				{
					InfectionRaycast r = t.gameObject.GetComponent<InfectionRaycast> ();
					if (r != null)
					{
						r.CreateInfection (_startInfectionTime);
					}
				}
				_lastCheckInfectionTime = DateTime.Now;
			}
		}
	}

	/// <summary>
	/// Repairs the neighbours
	/// </summary>
	/// <param name="duration_infected">Duration Repaired.</param>
	void RepairNeighbours(float duration_repaired)
	{
		if (VirusTypeVar == VirusType.ALL_AT_ONCE) // parallel infections
		{
			foreach (Vector3 direction in _directions)
			{
				//Transform t = GetNeighbourBlocks (direction, distance);
				Transform t = GetNeighbour ().transform;
				if (t != null)
				{
					InfectionRaycast r = t.gameObject.GetComponent<InfectionRaycast> ();
					if (r != null)
					{
						r.RepairInfection ();
					}
				}
			}
		}
		else if (VirusTypeVar == VirusType.ONE_AT_TIME) // serial infections
		{
			if (duration_repaired >= _timeBetweenReparations * _blockTurn && _blockTurn < _directions.Length)
			{
				/*Vector3 direction = GetDirection ();
				Transform t = GetNeighbourBlocks (direction, distance);*/
				Transform t = GetNeighbour ().transform;
				if (t != null)
				{
					InfectionRaycast r = t.gameObject.GetComponent<InfectionRaycast> ();
					if (r != null)
					{
						r.RepairInfection ();
					}
				}
				_blockTurn++;
			}
			else if (_blockTurn >= _directions.Length)
			{
				Repairing = false;
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
			direction.x * (transform.localScale.x * b.size.x / 2 - Epsilon),
			direction.y * (transform.localScale.y * b.size.y / 2 - Epsilon),
			direction.z * (transform.localScale.z * b.size.z / 2 - Epsilon));
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
		if (BlockTypeVar == BlockType.OBJECTIVE)
		{
			GM.GameOver ();
		}
		else if (BlockTypeVar != BlockType.UNDESTRUCTABLE_UNINFECTABLE && !Immune && !RepairImmune && !Infected)
		{
			_startInfectionTime = startInfectionTime;
			_lastCheckInfectionTime = DateTime.Now;
			_infectionTime = DateTime.Now;
			Infected = true;
			GameManager.InfectedBlocks++;
			this.enabled = true;

			gameObject.layer = 15;

			/*float distance_to_objective = Vector3.Distance (transform.position, GM.Objective.transform.position);
			if (distance_to_objective < GM.SmallestDistance)
			{
				GM.SmallestDistance = distance_to_objective;
			}*/
		}
	}

	/// <summary>
	/// Removes the self infection.
	/// </summary>
	public void RemoveInfection()
	{
		if (BlockTypeVar != BlockType.UNDESTRUCTABLE_UNINFECTABLE && Infected)
		{
			Infected = false;
			ChangeMaterial(_originalMaterial);
			gameObject.layer = 0;
			_infectionTime = new DateTime (0);
			_blockTurn = 0;
			GameManager.InfectedBlocks--;
			if (GameManager.InfectedBlocks <= 0)
			{
				GameManager.InfectedBlocks = 0;
			}
		}
	}

	/// <summary>
	/// Creates the self immunity.
	/// </summary>
	public void CreateImmunity()
	{
		if (BlockTypeVar != BlockType.UNDESTRUCTABLE_UNINFECTABLE && BlockTypeVar != BlockType.OBJECTIVE)
		{
			RemoveInfection();

			Immune = true;
			RepairImmune = false;
			ChangeMaterial(MaterialImmune);
			_immunityTime = DateTime.Now;
		}
	}

	/// <summary>
	/// Removes the self immunity.
	/// </summary>
	public void RemoveImmunity()
	{
		if (BlockTypeVar != BlockType.UNDESTRUCTABLE_UNINFECTABLE && (Immune || RepairImmune))
		{
			Immune = false;
			RepairImmune = false;
			ChangeMaterial(_originalMaterial);
			_immunityTime = new DateTime (0);
			_blockTurn = 0;
			this.enabled = false;
		}
	}

	/// <summary>
	/// Repairs the self infection.
	/// </summary>
	public void RepairInfection()
	{
		if (BlockTypeVar != BlockType.UNDESTRUCTABLE_UNINFECTABLE && Infected && BlockTypeVar != BlockType.OBJECTIVE)
		{
			RemoveInfection ();

			Immune = false;
			Infected = false;
			Repairing = true;
			RepairImmune = true;
			ChangeMaterial(MaterialImmune);
			_reparationTime = DateTime.Now;
			_blockTurn = 0;
			//ResetDirections ();
			ResetNeighbours ();
			this.enabled = true;
		}
	}
	#endregion
}
