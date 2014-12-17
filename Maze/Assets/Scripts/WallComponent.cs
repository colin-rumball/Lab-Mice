using UnityEngine;
using System.Collections;

public class WallComponent : MonoBehaviour 
{
	public Material aliveMat, deadMat;
	private bool bIsHard, bIsUp;
	private bool isAlive = true;
	private int col, row;

	public enum WallType
	{
		notSet = 0,
		left = 1,
		top = 2
	}
	private WallType wallType = WallType.notSet;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnRenderObject()
	{
	}

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

	public int getCol()
	{
		return col;
	}

	public int getRow()
	{
		return row;
	}

	public void setMazePosition(int c, int r)
	{
		col = c;
		row = r;
	}
	public void setWallType(WallType _type)
	{
		this.wallType = _type;
	}

	public WallType getWallType()
	{
		return this.wallType;
	}

	public void revive()
	{
		isAlive = true;
		//gameObject.GetComponent<MeshRenderer>().enabled = true;
		gameObject.GetComponent<MeshRenderer>().material = aliveMat;
	}

	public void kill()
	{
		isAlive = false;
		//gameObject.GetComponent<MeshRenderer>().enabled = false;
		gameObject.GetComponent<MeshRenderer>().material = deadMat;
	}

	public bool getIsAlive()
	{
		return isAlive;
	}
}
