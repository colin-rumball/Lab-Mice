using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GameStateManager))]
[RequireComponent(typeof(ObjectManager))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(ControllerInputManager))]
[RequireComponent(typeof(TouchInputManager))]
[RequireComponent(typeof(MenuManager))]
[RequireComponent(typeof(TimerManager))]
[RequireComponent(typeof(GUIManager))]

public class Game : MonoBehaviour 
{
	// Stores the cheese for future spawning.
	public GameObject CheeseObject;
	// Number of players in the room.
	public int PLAYERS = 1;
	// ID of the current user.
	public int PLAYER_ID = 0;
	// Text to display to the user in the event of an error.
	public string errorText = "";

	// Stores the user's character for future.
	private GameObject MyChracter;
	// Stores the Camera object.
	private GameObject mainCamera;

	// Stores all of the handlers and managers.
	private GameStateManager gameStateManager;
	private ObjectManager objectManager;
	private AudioManager audioManager;
	private NetworkManager networkManager;
	private ControllerInputManager inputHandler;
	private TouchInputManager touchInputHandler;
	private MenuManager menuManager;
	private TimerManager timerManager;
	private GUIManager guiManager;
	private Maze maze;

	private int lengthOfRound = 60;
	// Stores the name of the player who won the round.
	private string winningPlayerName;

	//--------------------------------------------------------------
	/// Called automatically at Object Initialization.
	//--------------------------------------------------------------
	void Start () 
	{
		// Store and initialize all of the managers and handlers.
		gameStateManager = this.GetComponent<GameStateManager>();
		objectManager = this.GetComponent<ObjectManager>();
		audioManager = this.GetComponent<AudioManager>();
		networkManager = this.GetComponent<NetworkManager>();
		inputHandler = this.GetComponent<ControllerInputManager>();
		menuManager = this.GetComponent<MenuManager>();
		timerManager = this.GetComponent<TimerManager>();
		guiManager = this.GetComponent<GUIManager>();
		touchInputHandler = this.GetComponent<TouchInputManager>();
		MyChracter = GameObject.Find("My Character");
		mainCamera = GameObject.Find("Main Camera");
		maze = GameObject.Find("Maze").GetComponent<Maze>();
	}

	//--------------------------------------------------------------
	/// Called automatically every frame.
	//--------------------------------------------------------------
	void Update () 
	{
		switch (gameStateManager.getGameState()) 
		{
		case GameStateManager.GameState.NAME_INPUT:
			break;
		case GameStateManager.GameState.MAIN_MENU:
			if (Input.GetKeyDown(KeyCode.Escape))
			    gameStateManager.goToState(GameStateManager.GameState.NAME_INPUT);
			break;
		case GameStateManager.GameState.LOBBY:
			Debug.Log("GameState.LOBBY");
			Timer lobbyTimer = timerManager.getTimer("LOBBY_COUNTDOWN");
			// If the game takes to long ot connect then queue and error prompt.
			if (lobbyTimer != null && lobbyTimer.hasExpired())
			{
				timerManager.removeTimer("LOBBY_COUNTDOWN");
				promptError("CONNECTION TIMED OUT");
			}
			break;
		case GameStateManager.GameState.ROOM:
			Timer roomTimer = timerManager.getTimer("ROOM_COUNTDOWN");
			if (roomTimer != null && roomTimer.hasExpired())
			{
				startRound();
				timerManager.removeTimer("ROOM_COUNTDOWN");
			}
			break;
		case GameStateManager.GameState.INIT_MAZE:
			Debug.Log("GameState.INIT_MAZE");
			maze.initMaze();
			gameStateManager.goToState(GameStateManager.GameState.ROOM);
			break;
		case GameStateManager.GameState.CREATING_MAZE:
			Debug.Log("GameState.CREATING_MAZE");
			maze.initMaze();
			maze.createMaze();
			if (PhotonNetwork.offlineMode)
				gameStateManager.goToState(GameStateManager.GameState.WAITING);
			else
				gameStateManager.goToState(GameStateManager.GameState.ROOM);
			break;
		case GameStateManager.GameState.LEVEL_EDITOR:
			if (Application.platform != RuntimePlatform.Android)
			{
				// Camera movement
				if (mainCamera.transform.position.x+(Input.GetAxis("Horizontal")*Time.deltaTime) > 6.5f &&
				    mainCamera.transform.position.x+(Input.GetAxis("Horizontal")*Time.deltaTime) < (6.5f+Maze.MAZE_COLUMNS))
						mainCamera.transform.position = new Vector3(mainCamera.transform.position.x+(Input.GetAxis("Horizontal")*Time.deltaTime), 
					                                            mainCamera.transform.position.y, 
					                                            mainCamera.transform.position.z);
				if (mainCamera.transform.position.z+(Input.GetAxis("Vertical")*Time.deltaTime) < -1.0f &&
				    mainCamera.transform.position.z+(Input.GetAxis("Vertical")*Time.deltaTime) > (-1.0f-Maze.MAZE_ROWS))
						mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, 
					                                            mainCamera.transform.position.y, 
					                                            mainCamera.transform.position.z+(Input.GetAxis("Vertical")*Time.deltaTime));
				// Don't let the user remove or add walls unless the options menu is closed.
				if (!guiManager.optionsIsMenuOpen())
				{
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					if (Physics.Raycast(ray, out hit, 100.0f))
					{
						if (hit.collider.transform.tag == "Wall")
						{
							GameObject wall = hit.collider.gameObject;
							if (!wall.GetComponent<WallComponent>().isHard() && wall.GetComponent<WallComponent>().getIsAlive() && Input.GetMouseButton(0))
							{
								wall.GetComponent<WallComponent>().kill();
								Cell c = maze.getCell(wall.GetComponent<WallComponent>().getCol(), wall.GetComponent<WallComponent>().getRow());
								if (wall.GetComponent<WallComponent>().getWallType() == WallComponent.WallType.left)
								{
									Wall w = c.getLeftWall();
									w.bringDown();
								}
								else if (wall.GetComponent<WallComponent>().getWallType() == WallComponent.WallType.top)
								{
									Wall w = c.getTopWall();
									w.bringDown();
								}
							}	else if (!wall.GetComponent<WallComponent>().isHard() && !wall.GetComponent<WallComponent>().getIsAlive() && Input.GetMouseButton(1))
							{
								wall.GetComponent<WallComponent>().revive();
								Cell c = maze.getCell(wall.GetComponent<WallComponent>().getCol(), wall.GetComponent<WallComponent>().getRow());
								if (wall.GetComponent<WallComponent>().getWallType() == WallComponent.WallType.left)
								{
									Wall w = c.getLeftWall();
									w.buildUp();
								}
								else if (wall.GetComponent<WallComponent>().getWallType() == WallComponent.WallType.top)
								{
									Wall w = c.getTopWall();
									w.buildUp();
								}
							}
							
						}
					}
				}
			}
			break;
		case GameStateManager.GameState.WAITING:
			Timer waitingTimer = timerManager.getTimer("WAITING_COUNTDOWN");
			if (waitingTimer != null && waitingTimer.hasExpired())
			{
				timerManager.removeTimer("WAITING_COUNTDOWN");
				timerManager.startTimer("ROUND_COUNTDOWN", getLengthOfRound());
				gameStateManager.goToState(GameStateManager.GameState.PLAYING);
			}
			break;
		case GameStateManager.GameState.PLAYING:
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				timerManager.pauseAllTimers();
				gameStateManager.goToState(GameStateManager.GameState.PAUSED);
			}

			// Spawn a new cheese if no cheese is found
			if (PhotonNetwork.isMasterClient && !Cheese.CHEESE_SPAWNED)
			{
				Vector3 cheeseSpawnLocation = new Vector3((int)Random.Range(1,Maze.MAZE_COLUMNS), 0.0f,(int)Random.Range(-Maze.MAZE_ROWS,-1));
				// Spawn the cheese for everyone.
				this.GetComponent<PhotonView>().RPC("SpawnCheese", PhotonTargets.All, cheeseSpawnLocation);
			}

			Timer roundTimer = timerManager.getTimer("ROUND_COUNTDOWN");
			if (roundTimer != null && roundTimer.hasExpired())
			{
				timerManager.removeTimer("ROUND_COUNTDOWN");
				endRound();
			}
			break;
		case GameStateManager.GameState.GAME_OVER:
			break;
		case GameStateManager.GameState.PAUSED:
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				timerManager.unPauseAllTimers();
				gameStateManager.goToState(GameStateManager.GameState.PLAYING);
			}
			break;
		}
	}

	//--------------------------------------------------------------
	/// Starts the round from the singleplayer or multiplayer menu. (Starts countdown first)
	//--------------------------------------------------------------
	public void startRound()
	{
		Debug.Log("startRound");
		
		if (!Maze.spawnRandomMap)
		{
			maze.RPCBuildMaze(Maze.stringToLoad);
		}
		
		objectManager.resetFloorToGameplay();

		timerManager.startTimer("WAITING_COUNTDOWN", 5.0f);
		PLAYERS = PhotonNetwork.room.playerCount;
		
		PhotonPlayer[] players = PhotonNetwork.otherPlayers;
		int myID = 1;
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].ID < PhotonNetwork.player.ID)
			{
				myID++;
			}
		}
		PLAYER_ID = myID;
		
		ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable();
		ht.Add("CheeseCount", "0");
		ht.Add("PlayerID", myID.ToString());
		PhotonNetwork.player.SetCustomProperties(ht);
		
		objectManager.spawnCharacter();
		
		if (PhotonNetwork.offlineMode)
		{
			objectManager.spawnBots();
		}
		
		if (PhotonNetwork.isMasterClient)
		{
			PhotonNetwork.room.maxPlayers = PhotonNetwork.room.playerCount;
		}
		
		GameObject MyCharacter = GameObject.Find("My Character");
		if (MyCharacter != null)
		{
			MyCharacter.GetComponentInChildren<TextMesh>().text = "YOU";
		}
		touchInputHandler.setComponents();
		gameStateManager.goToState(GameStateManager.GameState.WAITING);
		audioManager.stopAllClips("MAIN_MENU_MUSIC");
		audioManager.fadeClipOut("MAIN_MENU_MUSIC");
	}

	//--------------------------------------------------------------
	/// Ends the current round. (Called when the countdown ends)
	//--------------------------------------------------------------
	public void endRound()
	{
		// If I'm the masters client then I will determine the winner and notify everyone else.
		if(PhotonNetwork.isMasterClient)
		{
			int winnerCheeseCount = -1;
			string winnerName = "";
			foreach (PhotonPlayer player in PhotonNetwork.playerList) 
			{
				ExitGames.Client.Photon.Hashtable h = player.customProperties;
				int playerCheeseCount = int.Parse(h["CheeseCount"].ToString());
				if (playerCheeseCount > winnerCheeseCount)
				{
					winnerName = player.name;
					winnerCheeseCount = playerCheeseCount;
				} else if (playerCheeseCount == winnerCheeseCount)
				{
					winnerName += " AND " + player.name;
				}
			}
			if (PhotonNetwork.offlineMode)
			{
				for (int i = 0; i < BotAI.NUMBER_OF_BOTS; i++)
				{
					if (BotAI.BOT_CHEESE_COUNT[i] > winnerCheeseCount)
					{
						winnerName = "BOT " + i.ToString();
						winnerCheeseCount = BotAI.BOT_CHEESE_COUNT[i];
					} else if (BotAI.BOT_CHEESE_COUNT[i] == winnerCheeseCount)
					{
						winnerName += " AND " + "BOT " + i.ToString();
					}
				}
			}
			this.GetComponent<PhotonView>().RPC("DeclareWinner", PhotonTargets.All, winnerName);
		}

		// Disable this user's character.
		if (MyChracter != null)
		{
			MyChracter.GetComponentInChildren<Character>().rigidbody.velocity = new Vector3(0.0f, 0.0f, 0.0f);
			MyChracter.GetComponentInChildren<Animator>().speed = 0;
			MyChracter.GetComponentInChildren<Character>().enabled = false;
		}
		gameStateManager.goToState(GameStateManager.GameState.GAME_OVER);
	}

	public void deleteCharacter()
	{

	}

	public GameObject getMainCamera() { return mainCamera;}
	public Maze getMaze()	{return maze;}
	public string getErrorText()	{return errorText;}
	public int getLengthOfRound()	{return lengthOfRound;}
	public string getWinnerName()	{return winningPlayerName;}
	public GUIManager getGUIManager()	{return guiManager;}
	public MenuManager getMenuManager()	{return menuManager;}
	public AudioManager getAudioManager()	{return audioManager;}
	public TimerManager getTimerManager()	{return timerManager;}
	public ObjectManager getObjectManager()	{return objectManager;}
	public NetworkManager getNetworkManager()	{return networkManager;}
	public GameStateManager getGameStateManager()	{return gameStateManager;}
	public ControllerInputManager getControllerInputManager()	{return inputHandler;}

	public void addToLengthOfRound(int _add)	{lengthOfRound += _add;}
	public void setLengthOfRound(int _length)	{lengthOfRound = _length;}

	//--------------------------------------------------------------
	/// Opens a prompt to display an error to the user.
	//--------------------------------------------------------------
	public void promptError(string _message)
	{
		networkManager.Disconnect();
		errorText = _message;
		gameStateManager.goToState(GameStateManager.GameState.ERROR);
	}

	//--------------------------------------------------------------
	/// RPC event to notify everyone in the room who the winner is.
	//--------------------------------------------------------------
	[RPC]
	public void DeclareWinner(string _winner)
	{
		winningPlayerName = _winner;
	}

	//--------------------------------------------------------------
	/// RPC event to spawn the cheese at a given location.
	//--------------------------------------------------------------
	[RPC]
	public void SpawnCheese(Vector3 _cheeseSpawn)
	{
		// Make sure the cheese is gone before spawning a new one.
		GameObject cheeseCheckObj = GameObject.Find("Cheese(Clone)");
		if (cheeseCheckObj != null)
			objectManager.removeCheese();
		GameObject obj = (GameObject) Instantiate(CheeseObject, _cheeseSpawn, Quaternion.Euler (0, 0, 0));
		// Notify all of the bots of the new position.
		GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");
		foreach(GameObject bot in bots)
		{
			bot.GetComponent<BotAI>().reassignCheese(obj);
		}
	}
}