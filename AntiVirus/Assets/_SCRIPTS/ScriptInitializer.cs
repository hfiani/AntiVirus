using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptInitializer : MonoBehaviour
{
	public GameObject toCopyFrom;

	// Use this for initialization
	void Start ()
	{
		CopyComponent<InfectionRaycast> (toCopyFrom.GetComponent<InfectionRaycast>(), gameObject);
		Destroy(this);
	}
	

	T CopyComponent<T>(T original, GameObject destination) where T : Component
	{
		System.Type type = original.GetType();
		Component copy = destination.AddComponent(type);
		System.Reflection.FieldInfo[] fields = type.GetFields();
		foreach (System.Reflection.FieldInfo field in fields)
		{
			field.SetValue(copy, field.GetValue(original));
		}
		return copy as T;
	}
}
