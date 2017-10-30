using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRendererAtRuntime : MonoBehaviour
{
	/* DisableRendererAtRuntime
	 * 
	 */

	// Use this for initialization
	void Start ()
	{
		GetComponent<Renderer> ().enabled = false;
	}
}
