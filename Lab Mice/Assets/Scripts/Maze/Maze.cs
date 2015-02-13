using UnityEngine;
using System.Collections;

public class Maze : MonoBehaviour 
{
	// Represents if the current maze will be randomly generated.
	public static bool spawnRandomMap = true;
	// The string representing the map to load.
	public static string stringToLoad;
	// Maze width.
	public static int MAZE_COLUMNS = 17;
	// Maze height.
	public static int MAZE_ROWS = 9;
	// Cell size.
	public const int CELL_SIZE = 1;
	// Tile size.
	public const int TILE_SIZE = 30;
	public GameObject wallPrefab, InvalidSaveObj;
	// Represents the Game.
	private Game game;
	private Cell[] cells;
	private int cellSize;
	private int nbCols, nbRows;
	private bool initialized = false;

	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	void Start () 
	{
		game = GameObject.Find("_SCRIPTS").GetComponent<Game>(); //TODO: better way to assign.
		if (game == null)
			Debug.LogError("Maze could not find game script.");
	}

	//--------------------------------------------------------------
	/// Initialize the maze for creation.
	//--------------------------------------------------------------
	public void initMaze()
	{
		Debug.Log("initMaze"+"|"+MAZE_COLUMNS+"|"+MAZE_ROWS);
		nbCols = MAZE_COLUMNS;
		nbRows = MAZE_ROWS;
		cellSize = CELL_SIZE;
		cells = new Cell[(nbCols+2) * (nbRows+2)];
		for (int c = 0; c <= nbCols+1; c++)
		{
			for (int r = 0; r <= nbRows+1; r++)
			{
				Cell cell = new Cell(this, c, r);
				cells[r * (nbCols + 2) + c] = cell;
			}
		}
		initialized = true;
		game.getObjectManager().resetSpawnLocations();
	}

	//--------------------------------------------------------------
	/// Creates and carves the maze.
	//--------------------------------------------------------------
	public void createMaze(bool carveMaze = true)
	{
		if (carveMaze)
		{
			this.setAllCellsToVirgin();
			for (int i = 0; i < 3; i++)
			{
				Cell cell = this.getRandomCell();
				if (cell.removeRandomWall())
					i--;
			}
			MazePather carver = new MazePather(this, getRandomCell());
			do {
				carver.carveMaze();
			} while (!carver.isFinished());
		}


		string m = "";
		for (int i = 1; i <= nbCols+1; i++)
		{
			for (int j = 1; j <= nbRows+1; j++)
			{
				Cell c = getCell(i, j);
				Wall wu = c.getTopWall();
				Wall wl = c.getLeftWall();
				if ((wl != null && (wl.isUp() || wl.isHard())) &&
				    (wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "3";
				} else if ((wl != null && (wl.isUp() || wl.isHard())) &&
				           !(wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "1";
				} else if (!(wl != null && (wl.isUp() || wl.isHard())) &&
				           (wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "2";
				} else
				{
					m += "0";
				}
			}
		}

		this.GetComponent<PhotonView>().RPC("buildMaze", PhotonTargets.All, m);
	}

	//--------------------------------------------------------------
	/// Calls an RPC event to build the maze.
	//--------------------------------------------------------------
	public void RPCBuildMaze(string m)
	{
		this.GetComponent<PhotonView>().RPC("buildMaze", PhotonTargets.All, m);
	}

	//--------------------------------------------------------------
	/// RPC event that builds the maze in the world.
	//--------------------------------------------------------------
	[RPC]
	public void buildMaze(string m)
	{
		Debug.Log("buildMaze");
		for (int i = 1; i <= nbCols+1; i++)
		{
			for (int j = 1; j <= nbRows+1; j++)
			{
				GameObject go = null;
				switch(m.Substring(0, 1))
				{
				case "0":
					if (i <= nbCols && j <= nbRows)
					{
						getCell(i, j).getLeftWall().bringDown();
						//getCell(j, j).getTopWall().bringDown();
						if (game.getGameStateManager().getGameState() == GameStateManager.GameState.LEVEL_EDITOR && !((i == 0 || i == nbCols+1)||(j == 0 || j == nbRows+1)))
						{
							go = (GameObject) Instantiate(wallPrefab, new Vector3((i*cellSize)-(((float)cellSize)/2), 0.0f, -j*cellSize), Quaternion.Euler(-90, 0, 0)); //left
							go.transform.parent = gameObject.transform;
							if (go != null && (i == 1 || i == nbCols+1))
							{
								go.GetComponent<WallComponent>().setAsHard();
							}
							go.GetComponent<WallComponent>().setMazePosition(i, j);
							go.GetComponent<WallComponent>().setWallType(WallComponent.WallType.left);
							go.GetComponent<WallComponent>().kill();
							
							go = (GameObject) Instantiate(wallPrefab, new Vector3(i*cellSize, 0.0f, -(j*cellSize)+(((float)cellSize)/2)), Quaternion.Euler(-90, 0, 90));
							go.transform.parent = gameObject.transform;
							if (go != null && (j == 1 || j == nbRows+1))
							{
								go.GetComponent<WallComponent>().setAsHard();
							}
							go.GetComponent<WallComponent>().setMazePosition(i, j);
							go.GetComponent<WallComponent>().setWallType(WallComponent.WallType.top);
							go.GetComponent<WallComponent>().kill();
						}
					}
					break;
				case "1":
					if (i <= nbCols && j <= nbRows)
					{
						getCell(i, j).getTopWall().bringDown();
					}
					go = (GameObject) Instantiate(wallPrefab, new Vector3((i*cellSize)-(((float)cellSize)/2), 0.0f, -j*cellSize), Quaternion.Euler(-90, 0, 0));
					go.transform.parent = gameObject.transform;
					if (go != null && (i == 1 || i == nbCols+1))
					{
						go.GetComponent<WallComponent>().setAsHard();
					}
					go.GetComponent<WallComponent>().setMazePosition(i, j);
					go.GetComponent<WallComponent>().setWallType(WallComponent.WallType.left);

					if (game.getGameStateManager().getGameState() == GameStateManager.GameState.LEVEL_EDITOR && !((i == 0 || i == nbCols+1)||(j == 0 || j == nbRows+1)))
					{
						go = (GameObject) Instantiate(wallPrefab, new Vector3(i*cellSize, 0.0f, -(j*cellSize)+(((float)cellSize)/2)), Quaternion.Euler(-90, 0, 90));
						go.transform.parent = gameObject.transform;
						if (go != null && (j == 1 || j == nbRows+1))
						{
							go.GetComponent<WallComponent>().setAsHard();
						}
						go.GetComponent<WallComponent>().setMazePosition(i, j);
						go.GetComponent<WallComponent>().setWallType(WallComponent.WallType.top);
						go.GetComponent<WallComponent>().kill();
					}

					break;
				case "2":
					if (i <= nbCols && j <= nbRows)
					{
						getCell(i, j).getLeftWall().bringDown();
					}
					go = (GameObject) Instantiate(wallPrefab, new Vector3(i*cellSize, 0.0f, -(j*cellSize)+(((float)cellSize)/2)), Quaternion.Euler(-90, 0, 90)); //top
					go.transform.parent = gameObject.transform;
					if (go != null && (j == 1 || j == nbRows+1))
					{
						go.GetComponent<WallComponent>().setAsHard();
					}
					go.GetComponent<WallComponent>().setMazePosition(i, j);
					go.GetComponent<WallComponent>().setWallType(WallComponent.WallType.top);

					if (game.getGameStateManager().getGameState() == GameStateManager.GameState.LEVEL_EDITOR && !((i == 0 || i == nbCols+1)||(j == 0 || j == nbRows+1)))
					{
						go = (GameObject) Instantiate(wallPrefab, new Vector3((i*cellSize)-(((float)cellSize)/2), 0.0f, -j*cellSize), Quaternion.Euler(-90, 0, 0)); //left
						go.transform.parent = gameObject.transform;
						if (go != null && (i == 1 || i == nbCols+1))
						{
							go.GetComponent<WallComponent>().setAsHard();
						}
						go.GetComponent<WallComponent>().setMazePosition(i, j);
						go.GetComponent<WallComponent>().setWallType(WallComponent.WallType.left);
						go.GetComponent<WallComponent>().kill();
					}

					break;
				case "3":
					go = (GameObject) Instantiate(wallPrefab, new Vector3((i*cellSize)-(((float)cellSize)/2), 0.0f, -j*cellSize), Quaternion.Euler(-90, 0, 0)); //left
					go.transform.parent = gameObject.transform;
					if (go != null && (i == 1 || i == nbCols+1))
					{
						go.GetComponent<WallComponent>().setAsHard();
					}
					go.GetComponent<WallComponent>().setMazePosition(i, j);
					go.GetComponent<WallComponent>().setWallType(WallComponent.WallType.left);

					go = (GameObject) Instantiate(wallPrefab, new Vector3(i*cellSize, 0.0f, -(j*cellSize)+(((float)cellSize)/2)), Quaternion.Euler(-90, 0, 90));
					go.transform.parent = gameObject.transform;
					if (go != null && (j == 1 || j == nbRows+1))
					{
						go.GetComponent<WallComponent>().setAsHard();
					}
					go.GetComponent<WallComponent>().setMazePosition(i, j);
					go.GetComponent<WallComponent>().setWallType(WallComponent.WallType.top);
					break;
				}

				m = m.Substring(1);
			}
		}
	}

	//--------------------------------------------------------------
	/// Resizes the maze.
	//--------------------------------------------------------------
	public void resize()
	{
		nbCols = MAZE_COLUMNS;
		nbRows = MAZE_ROWS;
		cellSize = CELL_SIZE;
		cells = new Cell[(nbCols+2) * (nbRows+2)];
		for (int c = 0; c <= nbCols+1; c++)
		{
			for (int r = 0; r <= nbRows+1; r++)
			{
				Cell cell = new Cell(this, c, r);
				cells[r * (nbCols + 2) + c] = cell; //here
			}
		}
	}

	//--------------------------------------------------------------
	/// Returns true if the maze is a completed maze. (Every cell accessible)
	//--------------------------------------------------------------
	public bool validateMaze()
	{
		for (int i = 1; i <= getNbCols(); i++)
		{
			for (int j = 1; j <= getNbRows(); j++)
			{
				this.setAllCellsToVirgin();
				MazePather mazePathChecker = new MazePather(this, this.getCell(1, 1));
				if (!mazePathChecker.checkForVaildPath(i, j))
				{
					this.spawnInvalidMarker(new Vector3(i, 0.0f, -j));
					game.getGUIManager().closeOptionsMenu();
					return false;
				}
			}
		}
		return true;
	}

	//--------------------------------------------------------------
	/// Spawns an marker object at specified location to mark the cell that is inaccessible.
	//--------------------------------------------------------------
	public void spawnInvalidMarker(Vector3 _pos)
	{
		GameObject obj = (GameObject)Instantiate(InvalidSaveObj, _pos, Quaternion.Euler (0, 0, 0));
		obj.name = _pos.ToString();
		foreach(Transform child in obj.transform)
		{
			child.transform.parent = null;
			child.transform.position = new Vector3(0.5f, 0.5f, 0.0f);
		}
	}

	//--------------------------------------------------------------
	/// Resets all cells to their default state.
	//--------------------------------------------------------------
	public void setAllCellsToVirgin()
	{
		for (int i = 1; i < Maze.MAZE_COLUMNS+1; i++)
		{
			for (int j = 1; j < Maze.MAZE_ROWS+1; j++)
			{
				Cell c = this.getCell(i, j);
				c.setState(0);
				c.setPathingValue(-1);
			}
		}
	}

	//--------------------------------------------------------------
	/// Removes all the walls from the world.
	//--------------------------------------------------------------
	public void removeAllWalls()
	{
		foreach (Transform childTransform in this.transform) 
		{
			Destroy(childTransform.gameObject);
		}
	}

	//--------------------------------------------------------------
	/// Saves the game to a specified save slot if the maze is valid.
	//--------------------------------------------------------------
	public bool saveMaze(string _slot)
	{
		// Make sure maze has been initialized.
		if (game.getMaze().hasBeenInitialized())
		{
			// Make sure the maze is a complete maze.
			if (game.getMaze().validateMaze())
			{
				Debug.Log("SAVING GAME: "+_slot+" "+Maze.MAZE_COLUMNS.ToString()+"|"+Maze.MAZE_ROWS.ToString()+"|"+this.ToStringBasedOnWalls());
				PlayerPrefs.SetString(_slot,MAZE_COLUMNS.ToString()+"|"+MAZE_ROWS.ToString()+"|"+this.ToStringBasedOnWalls());
				return true;
			}
		}
		return false;
	}

	//--------------------------------------------------------------
	/// Loads a maze from a supplied save slot.
	//--------------------------------------------------------------
	public void loadMaze(string _slot)
	{
		removeAllWalls();
		string savedString = PlayerPrefs.GetString(_slot);
		if (savedString != null);
		{
			string[] splitString = savedString.Split('|');
			if (splitString.Length == 3)
			{
				Debug.Log("LOADING GAME: "+splitString[0]+splitString[1]+splitString[2]);
				Maze.MAZE_COLUMNS = int.Parse(splitString[0]);
				Maze.MAZE_ROWS = int.Parse(splitString[1]);
				game.getMaze().initMaze();
				game.getMaze().buildMaze(splitString[2]);
			}
		}
	}

	//--------------------------------------------------------------
	/// Returns a random cell foudn within the maze.
	//--------------------------------------------------------------
	public Cell getRandomCell()
	{
		int col = getRandomInt(1, nbCols+1);
		int row = getRandomInt(1, nbRows+1);
		return getCell(col, row);
	}

	//--------------------------------------------------------------
	/// Gets the cell foudn in the specified row and column.
	//--------------------------------------------------------------
	public Cell getCell(int col, int row)
	{
		return cells[row * (nbCols + 2) + col];
	}

	//--------------------------------------------------------------
	/// Returns the cell foudn in the specified index of the cells array.	
	//--------------------------------------------------------------
	public Cell getCellByIndex(int index)
	{
		return cells[index];
	}

	public int getNbRows() { return nbRows; }
	public int getNbCols() { return nbCols; }

	public int getRandomInt(int low, int high)
	{
		return (int)Random.Range (low, high);
	}

	public bool hasBeenInitialized()
	{
		return initialized;
	}

	//--------------------------------------------------------------
	/// Returns a string representation of the current maze layout.
	//--------------------------------------------------------------
	public override string ToString()
	{
		string m = "";
		for (int i = 1; i <= nbCols+1; i++)
		{
			for (int j = 1; j <= nbRows+1; j++)
			{
				Cell c = getCell(i, j);
				Wall wu = c.getTopWall();
				Wall wl = c.getLeftWall();
				if ((wl != null && (wl.isUp() || wl.isHard())) &&
				    (wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "3";
				} else if ((wl != null && (wl.isUp() || wl.isHard())) &&
				           !(wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "1";
				} else if (!(wl != null && (wl.isUp() || wl.isHard())) &&
				           (wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "2";
				} else
				{
					m += "0";
				}
			}
		}
		return m;
	}

	//--------------------------------------------------------------
	/// Returns a string representation of the current maze layout based off the walls in the world.
	//--------------------------------------------------------------
	public string ToStringBasedOnWalls ()
	{
		WallComponent[] walls = this.GetComponentsInChildren<WallComponent>();
		Cell[] tempCells = new Cell[(nbCols+2) * (nbRows+2)];
		for (int c = 0; c <= nbCols+1; c++)
		{
			for (int r = 0; r <= nbRows+1; r++)
			{
				Cell cell = new Cell(this, c, r);
				tempCells[r * (nbCols + 2) + c] = cell;
			}
		}

		for (int i = 1; i <= nbCols; i++)
		{
			for (int j = 1; j <= nbRows; j++)
			{
				Cell c = tempCells[j * (nbCols + 2) + i];
				c.getTopWall().bringDown();
				c.getLeftWall().bringDown();

				foreach (WallComponent wall in walls)
				{
					if (wall.getCol() == i && wall.getRow() == j)
					{
						if (wall.getWallType() == WallComponent.WallType.left && wall.getIsAlive())
							c.getLeftWall().buildUp();
						else if (wall.getWallType() == WallComponent.WallType.top && wall.getIsAlive())
							c.getTopWall().buildUp();
					}
				}
			}
		}

		string m = "";
		for (int i = 1; i <= nbCols+1; i++)
		{
			for (int j = 1; j <= nbRows+1; j++)
			{
				Cell c = tempCells[j * (nbCols + 2) + i];
				Wall wu = c.getTopWall();
				Wall wl = c.getLeftWall();
				if ((wl != null && (wl.isUp() || wl.isHard())) &&
				    (wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "3";
				} else if ((wl != null && (wl.isUp() || wl.isHard())) &&
				           !(wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "1";
				} else if (!(wl != null && (wl.isUp() || wl.isHard())) &&
				           (wu != null && (wu.isUp() || wu.isHard())))
				{
					m += "2";
				} else
				{
					m += "0";
				}
			}
		}
		return m;
	}
}
