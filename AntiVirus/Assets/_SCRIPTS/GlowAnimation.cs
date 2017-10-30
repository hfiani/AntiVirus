using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//animation d'une colonne de lave (montée/descente aléatoire)
//=========================================================================================================
public class GlowAnimation : MonoBehaviour
{
	#region serialized private variables
	[SerializeField] private float AnimSpeed = 25f;
	[SerializeField] private AnimationCurve EmiColorCurve = null;
	[SerializeField] private float MinEmiIntensity = 0.5f;
	[SerializeField] private float MaxEmiIntensity = 1.0f;
	#endregion

	#region private variables
	private Color startEmiColor;
	private float _timer = 0f;
	#endregion

	#region events
	// Use this for initialization
	void Start ()
	{
		UpdateEmiColor ();
		_timer = Random.Range (0f, 1f);
	}

	void Update ()
	{
		_timer += Time.deltaTime * AnimSpeed / 100f;

		if (_timer > 1f)
		{
			_timer = 0f;
		}

		AnimateColor (_timer);
	}
	#endregion

	#region private functions
	void AnimateColor(float t)
	{
		float intensity = Mathf.Lerp (MinEmiIntensity, MaxEmiIntensity, EmiColorCurve.Evaluate (_timer));
		Color col = startEmiColor * intensity;

		GetComponent<Renderer> ().material.SetColor ("_EmissionColor", col);
		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject child = transform.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> ())
			{
				child.GetComponent<Renderer> ().material.SetColor ("_EmissionColor", col);
			}
		}
	}

	public void UpdateEmiColor()
	{
		startEmiColor = GetComponent<Renderer> ().material.GetColor ("_EmissionColor");
	}
	#endregion
}
