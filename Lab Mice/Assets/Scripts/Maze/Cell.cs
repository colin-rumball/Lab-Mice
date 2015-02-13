using UnityEngine;
using System.Collections;

public class Cell 
{
	// The base states
	public static int STATE_VIRGIN = 0;
	public static int STATE_BORDER = -1;
	// X coordinate of the cell.
	private int column;
	// Y coordinate of the cell.
	private int row;
	// Top wall of the cell.
	private Wall topWall;
	// Left wall of the cell.
	private Wall leftWall;
	// The maze the cell belongs to. Allows to access neighbor cells.
	private Maze maze;
	// An integer state, whose value will depend on the algorithm (virgin vs. visited)
	private int state;
	// An integer value, value shows distance from start.
	private int pathingValue;

	// Cell Constructor.
	public Cell(Maze m, int c, int r)
	{
		maze = m;
		column = c;
		row = r;
		state = STATE_VIRGIN;

		// If this wall is on the border of the maze flag it as a border cell to avoid deletion.
		if (r == 0 || r == maze.getNbRows()+1 || c == 0 || c == maze.getNbCols()+1)
		{
			state = STATE_BORDER;
		}
		if (r != 0 && c != 0)
		{
			if (c != maze.getNbCols()+1)
			{
				topWall = new Wall();
				if (r == 1 || r == maze.getNbRows()+1)
				{
					topWall.setAsHard();
				}
			}
			if (r != maze.getNbRows()+1)
			{
				leftWall = new Wall();
				if (c == 1 || c == maze.getNbCols()+1)
				{
					leftWall.setAsHard();
				}
			}
		}
	}
    
	//--------------------------------------------------------------
	/// Get the wall between this cell and the given neighbor cell.
	//--------------------------------------------------------------
	public Wall getWallWith(Cell neighbor)
	{
		if (row == neighbor.getRow())
		{
			// Cell is on left or on right
			if (column == neighbor.getColumn()+1)
			{
				return leftWall;
			}
			if (column == neighbor.getColumn()-1)
			{
				return neighbor.getLeftWall();
			}
		}
		else if (column == neighbor.getColumn())
		{
			// Cell is on top or on bottom
			if (row == neighbor.getRow()+1)
			{
				return topWall;
			}
			if (row == neighbor.getRow()-1)
			{
				return neighbor.getTopWall();
			}
		}
		return null;
	}

	//--------------------------------------------------------------
	/// Attempts to remove one of the walls contained within this cell.
	//--------------------------------------------------------------
	public bool removeRandomWall()
	{
		int rand = (int)Random.Range(1,3);
		if (rand == 1)
		{
			if (topWall.isUp() && !topWall.isHard())
			{
				topWall.bringDown();
				return true;
			} else if (leftWall.isUp() && !leftWall.isHard())
			{
				leftWall.bringDown();
				return true;
			}
		} else
		{
			if (leftWall.isUp() && !leftWall.isHard())
			{
				leftWall.bringDown();
				return true;
			} else if (topWall.isUp() && !topWall.isHard())
			{
				topWall.bringDown();
				return true;
			}
		}
		return false;
	}

	// Getters
	public int getRow()		{return row;}
	public int getColumn()	{return column;}

	public Cell getLeftCell()  	{return maze.getCell(column-1, row);}
	public Cell getRightCell() 	{return maze.getCell(column+1, row);}
	public Cell getTopCell()   	{return maze.getCell(column, row-1);}
	public Cell getBottomCell()	{return maze.getCell(column, row+1);}
	
	public Wall getTopWall()   	{return topWall;}
	public Wall getLeftWall()  	{return leftWall;}
	public Wall getBottomWall()	{return getBottomCell().getTopWall();}
	public Wall getRightWall() 	{return getRightCell().getLeftWall();}

	public int getState()		{return state;}
	public int getPathingValue()		{return pathingValue;}

	// Setters for value and state of this cell.
	public void setPathingValue(int _newValue) {pathingValue = _newValue;}
	public void setState(int _newState)	{state = _newState;}
}
