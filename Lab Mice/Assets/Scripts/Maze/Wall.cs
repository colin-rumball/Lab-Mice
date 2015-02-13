using UnityEngine;
using System.Collections;

public class Wall 
{
	// State of the wall.
	private bool bIsUp = true;
	// If true, cannot be brought down in normal operations.
	private bool bIsHard;

	public Wall(){}

	public void setAsHard()
	{
		bIsHard = true;
	}

	public bool isHard()
	{
		return bIsHard;
	}

	public bool isUp()
	{
		return bIsUp;
	}
	public void bringDown()
	{
		bIsUp = false;
	}
	public void buildUp()
	{
		bIsUp = true;
	}
}
