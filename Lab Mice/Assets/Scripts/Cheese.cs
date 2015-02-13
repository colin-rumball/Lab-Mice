using UnityEngine;
using System.Collections;

public class Cheese : MonoBehaviour 
{
	public static bool CHEESE_SPAWNED = false;

	//--------------------------------------------------------------
	/// Called automatically at Object Initialization.
	//--------------------------------------------------------------
	void Start () 
	{
		Cheese.CHEESE_SPAWNED = true;
	}
}