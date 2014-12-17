using UnityEngine;
using System.Collections;

public class InvalidSaveObj : MonoBehaviour 
{
	//--------------------------------------------------------------
	/// Called automatically at Object Initialization.
	//--------------------------------------------------------------
	void Start () 
	{
		Destroy(this.gameObject, 3.0f);
	}
}
