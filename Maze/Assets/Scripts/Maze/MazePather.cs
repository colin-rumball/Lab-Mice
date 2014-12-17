using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazePather 
{
	// Stack used for pathing
	public List<Cell> stack = new List<Cell>();
	// The maze this pather uses
	private Maze maze;
	// Current cell in the stack
	private Cell current;
	// States of the cells
	private static int VISITED = 1;

	//--------------------------------------------------------------
	/// MazePather Constructor
	//--------------------------------------------------------------
	public MazePather(Maze m, Cell c)
	{
		maze = m;
		current = c;
		stack = new List<Cell>();
	}

	//--------------------------------------------------------------
	/// Carves the maze to create a traversable path (used in maze creation)
	//--------------------------------------------------------------
	public void carveMaze()
	{
		if (current == null) {
			return;
		}

		current.setState(VISITED);
		Cell neigh = getRandomVirginNeighbor(current);

		if (neigh != null)
		{
			stack.Add(current);
			Wall w = current.getWallWith(neigh);
			w.bringDown();
			current = neigh;
		}
		else
		{
			if (stack.Count > 0)
			{
				current = (Cell) stack[(stack.Count-1)];
				stack.RemoveAt(stack.Count-1);
			} else
			{
				current = null;
			}
		}
	}

	//--------------------------------------------------------------
	/// Paths the maze using a breadth-first search algorithm.
	//--------------------------------------------------------------
	public void pathMazeShortestPath(int cheeseX, int cheeseY)
	{
		bool endFound = false;
		current.setPathingValue(0);
		current.setState(VISITED);
		int currentValue = 1;
		List<Cell> neighbours = new List<Cell>();
		List<Cell> tempNeighbours = new List<Cell>();
		tempNeighbours = getAllAccessibleNeighbors(current);
		do
		{
			neighbours.Clear();
			neighbours.AddRange(tempNeighbours);
			tempNeighbours.Clear();

			for (int i = 0; i < neighbours.Count; i++)
			{

				if (neighbours[i].getPathingValue() == -1)
				{
					neighbours[i].setPathingValue(currentValue);
					neighbours[i].setState(VISITED);
				}
				if (neighbours[i].getColumn() == cheeseX && neighbours[i].getRow() == cheeseY)
				{
					current = neighbours[i];
					endFound = true;
					break;
				}
				tempNeighbours.AddRange(getAllAccessibleNeighbors(neighbours[i]));
			}
			if (!endFound)
				currentValue++;
		} while (!endFound);

		stack.Clear();
		do
		{
			if (current != null)
			{
				stack.Add(current);
				current = getNeighbourWithValue(current, currentValue-1);
			}
			currentValue--;
		} while (currentValue > 0);
		stack.Reverse();
	}

	//--------------------------------------------------------------
	/// Paths the maze using a simple recursive algorithm
	//--------------------------------------------------------------
	public void pathMazeRecursiveBacktracker(int cheeseX, int cheeseY)
	{
		do
		{
			if (current == null)
				return;
			
			current.setState(VISITED);
			Cell neigh = getRandomAccessibleNeighbor(current);
			
			if (neigh != null)
			{
				stack.Add(current);
				current = neigh;
				if (current.getColumn() == cheeseX && current.getRow() == cheeseY)
				{
					stack.Add(current);
					current = null;
				}
			}
			else
			{
				if (stack.Count > 0)
				{
					current = (Cell) stack[(stack.Count-1)];
					stack.RemoveAt(stack.Count-1);
				} else
				{
					current = null;
				}
			}
		} while (!this.isFinished());
	}

	//--------------------------------------------------------------
	/// Paths the maze using a very rough wall following algorithm.
	//--------------------------------------------------------------
	public void pathMazeRoughWallFollow(int cheeseX, int cheeseY)
	{
		int stepCounter = 0;
		do
		{
			if (current == null)
				return;
			
			current.setState(VISITED);
			Cell neigh = getAccessibleNeighbor(current);
			
			if (neigh != null)
			{
				stack.Add(current);
				current = neigh;
				if (current.getColumn() == cheeseX && current.getRow() == cheeseY)
				{
					stack.Add(current);
					current = null;
				}
			}
			else
			{
				if (stack.Count > 0)
				{
					current = (Cell) stack[(stack.Count-1)];
					stack.RemoveAt(stack.Count-1);
				} else
				{
					current = null;
				}
			}
			stepCounter++;
		} while (!this.isFinished() && stepCounter < 20);
	}

	//--------------------------------------------------------------
	/// Returns the next cell from the pathing stack.
	//--------------------------------------------------------------
	public Cell getNextPathedMovement()
	{
		if (stack.Count > 0)
		{
			Cell rc = (Cell) stack[0];
			stack.RemoveAt(0);
			return rc;
		} else
			return null;
	}

	//--------------------------------------------------------------
	/// Returns true if there are no more cells to path to.
	//--------------------------------------------------------------
	public bool isFinished()
	{
		return (current == null);
	}

	//--------------------------------------------------------------
	/// Returns the first found neighbor that is accesible from the provided cell.
	//--------------------------------------------------------------
	private Cell getAccessibleNeighbor(Cell _cell)
	{
		Wall w = _cell.getTopWall();
		if (w != null && !w.isUp())
		{
			Cell c = _cell.getTopCell();
			if (c != null && c.getState() == Cell.STATE_VIRGIN)
			{
				return c;
			}
		}

		w = _cell.getLeftWall();
		if (w != null && !w.isUp())
		{
			Cell c = _cell.getLeftCell();
			if (c != null && c.getState() == Cell.STATE_VIRGIN)
			{
				return c;
			}
		}

		w = _cell.getBottomWall();
		if (w != null && !w.isUp())
		{
			Cell c = _cell.getBottomCell();
			if (c != null && c.getState() == Cell.STATE_VIRGIN)
			{
				return c;
			}
		}
		
		w = _cell.getRightWall();
		if (w != null && !w.isUp())
		{
			Cell c = _cell.getRightCell();
			if (c != null && c.getState() == Cell.STATE_VIRGIN)
			{
				return c;
			}
		}
		
		return null;
	}

	//--------------------------------------------------------------
	/// Returns a neighbor with the provided cell that has a the provided value assigned to it. (breadth-first search algorithm)
	//--------------------------------------------------------------
	private Cell getNeighbourWithValue(Cell _cell, int _value)
	{
		Wall w = _cell.getTopWall();
		if (w != null && !w.isUp())
		{
			Cell c = _cell.getTopCell();
			if (c != null && c.getPathingValue() == _value)
			{
				return c;
			}
		}

		w = _cell.getRightWall();
		if (w != null && !w.isUp())
		{
			Cell c = _cell.getRightCell();
			if (c != null && c.getPathingValue() == _value)
			{
				return c;
			}
		}

		w = _cell.getBottomWall();
		if (w != null && !w.isUp())
		{
			Cell c = _cell.getBottomCell();
			if (c != null && c.getPathingValue() == _value)
			{
				return c;
			}
		}

		w = _cell.getLeftWall();
		if (w != null && !w.isUp())
		{
			Cell c = _cell.getLeftCell();
			if (c != null && c.getPathingValue() == _value)
			{
				return c;
			}
		}

		return null;
	}

	//--------------------------------------------------------------
	/// Returns a list of all the acceddible neighbors of the provided cell.
	//--------------------------------------------------------------
	private List<Cell> getAllAccessibleNeighbors(Cell cell)
	{
		List<Cell> unvisitedCells = new List<Cell>();
		
		Wall w = cell.getTopWall();
		if (w != null && !w.isUp())
		{
			Cell c = cell.getTopCell();
			if (c.getState() == Cell.STATE_VIRGIN)
			{
				unvisitedCells.Add(c);
			}
		}
		
		w = cell.getRightWall();
		if (w != null && !w.isUp())
		{
			Cell c = cell.getRightCell();
			if (c.getState() == Cell.STATE_VIRGIN)
			{
				unvisitedCells.Add(c);
			}
		}
		
		w = cell.getBottomWall();
		if (w != null && !w.isUp())
		{
			Cell c = cell.getBottomCell();
			if (c.getState() == Cell.STATE_VIRGIN)
			{
				unvisitedCells.Add(c);
			}
		}
		
		w = cell.getLeftWall();
		if (w != null && !w.isUp())
		{
			Cell c = cell.getLeftCell();
			if (c.getState() == Cell.STATE_VIRGIN)
			{
				unvisitedCells.Add(c);
			}
		}

		return unvisitedCells; // Can be empty
	}

	//--------------------------------------------------------------
	/// Returns a random neighbor that is accesible from the provided cell.
	//--------------------------------------------------------------
	private Cell getRandomAccessibleNeighbor(Cell cell)
	{
		Cell[] unvisitedCell = new Cell[4];
		int count = 0;
		
		Wall w = cell.getTopWall();
		if (w != null && !w.isUp())
		{
			Cell c = cell.getTopCell();
			if (c.getState() == Cell.STATE_VIRGIN)
			{
				unvisitedCell[count++] = c;
			}
		}
		
		w = cell.getRightWall();
		if (w != null && !w.isUp())
		{
			Cell c = cell.getRightCell();
			if (c.getState() == Cell.STATE_VIRGIN)
			{
				unvisitedCell[count++] = c;
			}
		}
		
		w = cell.getBottomWall();
		if (w != null && !w.isUp())
		{
			Cell c = cell.getBottomCell();
			if (c.getState() == Cell.STATE_VIRGIN)
			{
				unvisitedCell[count++] = c;
			}
		}
		
		w = cell.getLeftWall();
		if (w != null && !w.isUp())
		{
			Cell c = cell.getLeftCell();
			if (c.getState() == Cell.STATE_VIRGIN)
			{
				unvisitedCell[count++] = c;
			}
		}
		
		int n = maze.getRandomInt(0, count-1);
		return unvisitedCell[n]; // Can be null
	}

	//--------------------------------------------------------------
	/// Returns a random neightbor that the pather has not processed yet from the provided cell.
	//--------------------------------------------------------------
	private Cell getRandomVirginNeighbor(Cell cell)
	{
		Cell[] unvisitedCell = new Cell[4];
		int count = 0;
		Cell c = cell.getTopCell();
		
		if (c.getState() == Cell.STATE_VIRGIN)
		{
			unvisitedCell[count] = c;
			count++;
		}
		c = cell.getRightCell();
		if (c.getState() == Cell.STATE_VIRGIN)
		{
			unvisitedCell[count] = c;
			count++;
		}
		c = cell.getBottomCell();
		if (c.getState() == Cell.STATE_VIRGIN)
		{
			unvisitedCell[count] = c;
			count++;
		}
		c = cell.getLeftCell();
		if (c.getState() == Cell.STATE_VIRGIN)
		{
			unvisitedCell[count] = c;
			count++;
		}
		int n = maze.getRandomInt(0, count);
		return unvisitedCell[n]; // Can be null
	}

	//--------------------------------------------------------------
	/// Returns true if there is a valid path from the current cell to a given x and y.
	//--------------------------------------------------------------
	public bool checkForVaildPath(int endX, int endY)
	{
		if (current.getColumn() == endX && current.getRow() == endY)
			return true;
		do
		{
			if (current == null)
				return false;
			
			current.setState(VISITED);
			Cell neigh = getRandomAccessibleNeighbor(current);
			
			if (neigh != null)
			{
				stack.Add(current);
				current = neigh;
				if (current.getColumn() == endX && current.getRow() == endY)
				{
					return true;
				}
			}
			else
			{
				if (stack.Count > 0)
				{
					current = (Cell) stack[(stack.Count-1)];
					stack.RemoveAt(stack.Count-1);
				} else
				{
					return false;
				}
			}
		} while (!this.isFinished());
		return false;
	}

	//--------------------------------------------------------------
	/// Returns the pathing stack.
	//--------------------------------------------------------------
	public List<Cell> getStack()
	{
		return stack;
	}
}
