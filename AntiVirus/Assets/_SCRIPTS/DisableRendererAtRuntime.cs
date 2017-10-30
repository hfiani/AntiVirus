using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRendererAtRuntime : MonoBehaviour
{
	/* DisableRendererAtRuntime
	 * Put on objects that help LD need to see in editor, but we disable their renderer in game.
	 */

	// Use this for initialization
	void Start ()
	{
		GetComponent<Renderer> ().enabled = false;
	}
}
