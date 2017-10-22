using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//animation d'une colonne de lave (montée/descente aléatoire)
//=========================================================================================================
public class GlowAnimation : MonoBehaviour {


	[SerializeField] private float _AnimSpeed = 25f;


	[SerializeField] private AnimationCurve _EmiColorCurve = null;

	[SerializeField] private float _MinEmiIntensity = 0.5f;
	[SerializeField] private float _MaxEmiIntensity = 1.0f;

	private Color startEmiColor;

	private float _timer = 0f;


	// Use this for initialization
	void Start () {

		startEmiColor = GetComponent<Renderer> ().material.GetColor ("_EmissionColor");

		_timer = Random.Range (0f, 1f);

	}

	//=========================================================================================================
	void Update () {


		_timer += Time.deltaTime * _AnimSpeed / 100f;

		if (_timer > 1f) {
			_timer = 0f;
		}


		AnimateColor (_timer);

	
	}
		

	void AnimateColor(float t){

		float intensity = Mathf.Lerp (_MinEmiIntensity, _MaxEmiIntensity, _EmiColorCurve.Evaluate (_timer));

		Color col = startEmiColor * intensity;

		GetComponent<Renderer> ().material.SetColor ("_EmissionColor", col);

		for (int i = 0; i < transform.childCount; i++) {
			GameObject child = transform.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> ()) {
				child.GetComponent<Renderer> ().material.SetColor ("_EmissionColor", col);
			}


		}
	}



}
