using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//animation d'une colonne de lave (montée/descente aléatoire)
//=========================================================================================================
public class VirusAnimation : MonoBehaviour {

	[SerializeField] private AnimationCurve _SizeCurve = null;


	[SerializeField] private float _AnimSpeed = 25f;

	[SerializeField] private float _MinSize = 3f;
	[SerializeField] private float _MaxSize = 6f;

	[SerializeField] private AnimationCurve _EmiColorCurve = null;

	[SerializeField] private float _MinEmiIntensity = 0.5f;
	[SerializeField] private float _MaxEmiIntensity = 1.0f;

	private float _timer = 0f;

	private Vector3 _startPos;

	private VirusManager VM;

	// Use this for initialization
	void Start () {

		VM = GetComponent<VirusManager> ();

		_timer = Random.Range (0f, 1f);

		//_startPos = transform.localPosition;



	}

	//=========================================================================================================
	void Update () {


		_timer += Time.deltaTime * _AnimSpeed / 100f;

		if (_timer > 1f) {
			_timer = 0f;
		}

		AnimateSize (_timer);

		AnimateColor (_timer);

	
	}

	void AnimateSize(float t){

		float scale = Mathf.Lerp (_MinSize, _MaxSize, _SizeCurve.Evaluate (t));

		transform.localScale = new Vector3 (1, 1, 1) * scale;
	}

	void AnimateColor(float t){

		float intensity = Mathf.Lerp (_MinEmiIntensity, _MaxEmiIntensity, _EmiColorCurve.Evaluate (_timer));

		Color col = VM.GetCurrentEmiColor() * intensity;

		GetComponent<Renderer> ().material.SetColor ("_EmissionColor", col);

		for (int i = 0; i < transform.childCount; i++) {
			GameObject child = transform.GetChild (i).gameObject;
			if (child.GetComponent<MeshRenderer> ()) {
				child.GetComponent<Renderer> ().material.SetColor ("_EmissionColor", col);
			}


		}
	}



}
