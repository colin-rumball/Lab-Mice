using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Game))]

public class ObjectManager : MonoBehaviour 
{
	// Stores the Bot prefab.
	public GameObject botObject;
	// Stores the materials for the background of the game during different states.
	public Material gameBG, menuBG;
	// Stores the background object.
	public GameObject floor;

	public GameObject myCharacter;

	// Represents the Game.
	private Game game;
	// Represents the camera of the game.
	private GameObject mainCamera;

	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	public void Start () 
	{
		game = this.GetComponent<Game>();
		mainCamera = GameObject.Find("Main Camera");
	}

	//--------------------------------------------------------------
	/// Use to spawn the player's character into the maze.
	//--------------------------------------------------------------
	public void spawnCharacter()
	{
		Debug.Log("SpawnCharacter");
		//TODO: set spawn function
		GameObject.Find("Player 2 Spawn").transform.position = new Vector3(Maze.MAZE_COLUMNS, 0.0f, -Maze.MAZE_ROWS);
		GameObject.Find("Player 3 Spawn").transform.position = new Vector3(Maze.MAZE_COLUMNS, 0.0f, -1.0f);
		GameObject.Find("Player 4 Spawn").transform.position = new Vector3(1.0f, 0.0f, -Maze.MAZE_ROWS);
		
		GameObject spawn = GameObject.Find("Player "+game.PLAYER_ID+" Spawn");
		if (spawn != null)
		{
			GameObject characterObject = (GameObject) PhotonNetwork.Instantiate("Character", spawn.transform.position, Quaternion.Euler (0, 0, 0), 0);
			GameObject MyCharacter = GameObject.Find("My Character");
			if (MyCharacter != null)
			{
				characterObject.transform.parent = MyCharacter.transform;
			}
			characterObject.GetComponent<Character>().enabled = true;
		}
	}

	//--------------------------------------------------------------
	/// Use to spawn the bot(s) into the maze.
	//--------------------------------------------------------------
	public void spawnBots()
	{
		for (int i = 0; i < BotAI.NUMBER_OF_BOTS; i++)
		{
			GameObject bot = null;
			BotAI aiScript;
			switch (i)
			{
			case 0:
				bot = (GameObject) Instantiate(botObject, new Vector3(Maze.MAZE_COLUMNS, 0.0f, -Maze.MAZE_ROWS), Quaternion.Euler (0, 0, 0));
				break;
			case 1:
				bot = (GameObject) Instantiate(botObject, new Vector3(Maze.MAZE_COLUMNS, 0.0f, -1.0f), Quaternion.Euler (0, 0, 0));
				break;
			case 2:
				bot = (GameObject) Instantiate(botObject, new Vector3(1.0f, 0.0f, -Maze.MAZE_ROWS), Quaternion.Euler (0, 0, 0));
				break;
			}
			if (bot != null)
			{
				aiScript = bot.GetComponent<BotAI>();
				if (aiScript != null)
				{
					aiScript.setBotID(i);
					aiScript.setDifficulty();
				}
			}
		}
	}

	//--------------------------------------------------------------
	/// Resets the camera's position back to the default.
	//--------------------------------------------------------------
	public void resetCameraPosition()
	{
		mainCamera.transform.position = new Vector3(9.0f, 5.6f, -6.2f);
	}

	//--------------------------------------------------------------
	/// Use to reset the floor to it's material and position for the menu.
	//--------------------------------------------------------------
	public void resetFloorToMenu()
	{
		floor.transform.position = new Vector3(floor.transform.position.x, 1.0f, floor.transform.position.z);
		foreach(Transform child in floor.transform)
		{
			child.GetComponent<MeshRenderer>().material = menuBG;
		}
	}

	//--------------------------------------------------------------
	/// Use to reset the floor to it's material and position for for gameplay.
	//--------------------------------------------------------------
	public void resetFloorToGameplay()
	{
		floor.transform.position = new Vector3(floor.transform.position.x, -0.5f, floor.transform.position.z);
		foreach(Transform child in floor.transform)
		{
			child.GetComponent<MeshRenderer>().material = gameBG;
		}
	}

	//--------------------------------------------------------------
	/// Use to reset the spawn locations to the current maze size;
	//--------------------------------------------------------------
	public void resetSpawnLocations()
	{
		GameObject.Find("Player 2 Spawn").transform.position = new Vector3(Maze.MAZE_COLUMNS, 0.0f, -Maze.MAZE_ROWS);
		GameObject.Find("Player 3 Spawn").transform.position = new Vector3(Maze.MAZE_COLUMNS, 0.0f, -1.0f);
		GameObject.Find("Player 4 Spawn").transform.position = new Vector3(1.0f, 0.0f, -Maze.MAZE_ROWS);
	}

	//--------------------------------------------------------------
	/// Use to remove the cheese object from the maze.
	//--------------------------------------------------------------
	public void removeCheese()
	{
		GameObject cheese = GameObject.Find("Cheese(Clone)");
		if (cheese != null)
		{
			Destroy(cheese);
		}
		Cheese.CHEESE_SPAWNED = false;
	}

	//--------------------------------------------------------------
	/// Use to remove all of the bots from the maze.
	//--------------------------------------------------------------
	public void removeAllBots()
	{
		GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");
		foreach (GameObject bot in bots) 
		{
			Destroy(bot);
		}
	}

	public void removeCharacter()
	{
		foreach(Transform child in myCharacter.transform) 
		{
			Destroy(child.gameObject);
		}
	}
}
