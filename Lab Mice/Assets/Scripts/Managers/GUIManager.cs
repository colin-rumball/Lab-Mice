using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Game))]

public class GUIManager : MonoBehaviour 
{
	// Self explanatory images.
	public Texture2D hostSymbolIMG, cheeseIMG, controllerIMG, directionalPadIMG, labMiceTitleIMG, tabletIMG;
	public Texture2D[] loadingImages, scoreImages, numbers; //TODO: fix
	// Custom GUI skin for all UI elements.
	public GUISkin defaultGUISkin;

	// Represents the Game.
	private Game game;
	// Save GUI positions for multiple uses.
	private float[] guiScoreXPositions = {35, Screen.width-105, Screen.width-105, 35};
	private float[] guiScoreYPositions = {35, Screen.height-153, 35, Screen.height-153};
	private float loadingFrame = 0.0f; //TODO: fix
	// Strings for map save slots buttons.
	private string[] SAVE_SLOT_STRINGS = {"SAVE SLOT 1", "SAVE SLOT 2", "SAVE SLOT 3", "SAVE SLOT 4", "SAVE SLOT 5"};
	private string[] TOUCH_OPTIONS = {"EDITING", "SCROLLING"};
	private int touchOptionChosen = 0;
	// Names for randomly generated names.
	private string[] generatedNames = {"Tod", "Britt", "Andre", "Kylar", "Juno", "Penny", "Mason", "Kali", "Tony", "Scot", "Carla", "Jadon"};
	// Preset lengths of games.
	private string[] gameLengths = {"0:30", "0:45", "1:00", "1:15", "1:30", "1:45", "2:00"};
	// Preset number of bots.
	private string[] botNumberStrings = {"0", "1", "2", "3"};
	// Preset bot difficulties
	private string[] botDifficultyStrings = {"EASY", "MEDIUM", "HARD"};
	// Strings to display number of rows and columns.
	private string newMazeRows="9", newMazeColumns="17";
	// Int used to record countdown time each frame for beep sound.
	private int countDownTimeLastFrame = 0;
	// Represents the current save slot selected for saved maps.
	private int saveSlotSelected = 6;
	// Represents whether or not the level editor options menu UI is open.
	private bool optionsMenuOpen = false;

	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	public void Start()
	{
		game = this.GetComponent<Game>();

		for (int i = 0; i < SAVE_SLOT_STRINGS.Length; i++)
		{
			string savedString = PlayerPrefs.GetString("SAVE SLOT "+i.ToString());
			if (savedString != null);
			{
				string[] splitString = savedString.Split('|');
				if (splitString.Length == 3)
				{
					SAVE_SLOT_STRINGS[i] += " (FILLED)";
				}
			}
		}
	}

	//--------------------------------------------------------------
	/// Called automatically to display all GUI elements.
	//--------------------------------------------------------------
	void OnGUI()
	{
		//GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
		GUI.skin = defaultGUISkin;
		switch (game.getGameStateManager().getGameState()) 
		{
		case GameStateManager.GameState.NAME_INPUT:
			GUI.DrawTexture(new Rect(Screen.width/2-208, -20, 417, 250), labMiceTitleIMG);
			showNameInputWindow();
			break;
		case GameStateManager.GameState.MAIN_MENU:
			showMainMenuWindow();
			showHowToPlayWindow();
			break;
		case GameStateManager.GameState.LOBBY:
			if (!PhotonNetwork.offlineMode)
			{
				showLocatingGameWindow();
			}
			break;
		case GameStateManager.GameState.ROOM:
			showRoomUI();
			break;
		case GameStateManager.GameState.WAITING:
			Timer waitingTimer = game.getTimerManager().getTimer("WAITING_COUNTDOWN");
			if (waitingTimer != null)
			{
				if (!waitingTimer.hasExpired())
				{
					if (countDownTimeLastFrame != (int)waitingTimer.getTimeLeft())
						game.getAudioManager().playClip("BEEP_SOUND");
					countDownTimeLastFrame = (int)waitingTimer.getTimeLeft();
					GUI.Box(new Rect(Screen.width/2-30, Screen.height/2-40, 60, 80),"");
					GUI.Label(new Rect(Screen.width/2-22, Screen.height/2-40, 60, 80), ((int)waitingTimer.getTimeLeft()+1).ToString(), "CountDownText");
				}
			}
			break;
		case GameStateManager.GameState.LEVEL_EDITOR:
			showLevelEditorUI();
			break;
		case GameStateManager.GameState.PLAYING:
			showGamePlayUI();
			break;
		case GameStateManager.GameState.GAME_OVER:
			showGameOverWindow();
			break;
		case GameStateManager.GameState.ERROR:
			showErrorWindow();
			break;
		case GameStateManager.GameState.PAUSED:
			showPausedWindow();
			break;
		}

		// If the user is using a controller then highlight their selected menu item.
		if (game.getControllerInputManager().usingController())
			GUI.FocusControl(game.getMenuManager().getActiveMenuItemName());
	}

	//--------------------------------------------------------------
	/// Use to display the input name window.
	//--------------------------------------------------------------
	private void showNameInputWindow()
	{
		GUI.skin.settings.cursorColor = Color.grey; //TODO: needed for text boxes?
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-40,300,220),"USERNAME INPUT", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2,300,180),"");
		GUI.BeginGroup(new Rect(Screen.width/2-150,Screen.height/2,300,180));
		GUI.SetNextControlName("RANDOMIZE");
		if (GUI.Button(new Rect(45, 140, 210, 25), "RANDOMIZE") || game.getMenuManager().menuButtonPushed("RANDOMIZE"))
		{
			PhotonNetwork.player.name = generatedNames[(int)Random.Range(0, 12)];
			game.getMenuManager().reset();
			game.getAudioManager().playClip("MENU_SOUND");
		}
		PhotonNetwork.player.name = GUI.TextField(new Rect (30, 25, 240, 50), PhotonNetwork.player.name, 12);
		GUI.SetNextControlName("CONFIRM");
		if (GUI.Button(new Rect(30, 85, 240, 50), "CONFIRM") || game.getMenuManager().menuButtonPushed("CONFIRM"))
		{
			char[] charsToTrim = { '*', ' ', '\''};
			PhotonNetwork.player.name = PhotonNetwork.player.name.Trim(charsToTrim);
			if (PhotonNetwork.player.name == "")
				PhotonNetwork.player.name = generatedNames[(int)Random.Range(0, 12)];
			game.getGameStateManager().goToState(GameStateManager.GameState.MAIN_MENU);
			game.getAudioManager().playClip("MENU_SOUND");
		}
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the main menu window.
	//--------------------------------------------------------------
	private void showMainMenuWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-160,300,260),"MAIN MENU", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-120,300,240),"");
		GUI.BeginGroup(new Rect(Screen.width/2-150,Screen.height/2-120,300,240));
		if (!PhotonNetwork.connected || PhotonNetwork.offlineMode)
		{
			GUI.SetNextControlName("SINGLE-PLAYER");
			if (GUI.Button(new Rect(30, 45, 240, 50), "SINGLE-PLAYER") || game.getMenuManager().menuButtonPushed("SINGLE-PLAYER"))
			{
				game.getAudioManager().playClip("MENU_SOUND");
				game.getMenuManager().goToSinglePlayerMenu();
			}
			GUI.SetNextControlName("MULTIPLAYER");
			if (GUI.Button(new Rect(30, 105, 240, 50), "MULTIPLAYER") || game.getMenuManager().menuButtonPushed("MULTIPLAYER"))
			{
				game.getAudioManager().playClip("MENU_SOUND");
				game.getMenuManager().goToMultiplayerMenu();
			}
			GUI.SetNextControlName("LEVEL EDITOR");
			if (GUI.Button(new Rect(30, 165, 240, 50), "LEVEL EDITOR") || game.getMenuManager().menuButtonPushed("LEVEL EDITOR"))
			{
				if (!PhotonNetwork.offlineMode)
				{
					PhotonNetwork.offlineMode = true;
					if (PhotonNetwork.room == null)
					{
						game.getNetworkManager().OnJoinedLobby();
					}
				}
				game.getObjectManager().resetFloorToGameplay();
				game.getGameStateManager().goToState(GameStateManager.GameState.LEVEL_EDITOR);
				game.getAudioManager().playClip("MENU_SOUND");
			}
		}
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the how to play window.
	//--------------------------------------------------------------
	private void showHowToPlayWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2+175,Screen.height/2-160,250,280),"HOW TO PLAY", "TitleBox");
		GUI.Box(new Rect(Screen.width/2+175,Screen.height/2-120,250,240),"");
		GUI.BeginGroup(new Rect(Screen.width/2+175,Screen.height/2-120,250,240));
		GUI.DrawTexture(new Rect(120-19, 0, 34, 28), cheeseIMG);
		GUI.Label(new Rect(5, 15, 240, 80), "Collect more cheese than\nthe other mice before the\n time runs out!");
		if (Application.platform == RuntimePlatform.Android || true)
		{
			GUI.DrawTexture(new Rect(120-61, 95-12, 122, 96), tabletIMG);
			GUI.Label(new Rect(5, 165, 240, 80), "Tap the edge of the\n screen to move your\n mouse in that direction.");
		} else if (game.getControllerInputManager().usingController())
		{
			GUI.DrawTexture(new Rect(120-19, 95-10, 38, 24), loadingImages[1]);
			GUI.Label(new Rect(5, 95, 240, 80), "Use the directional\npad to move your mouse.");
			GUI.DrawTexture(new Rect(120-20, 175-20, 40, 40), directionalPadIMG);
			GUI.Label(new Rect(5, 175, 240, 80), "Or if you want you can\n use a keyboard to play!");
		} else
		{
			GUI.DrawTexture(new Rect(120-19, 95-10, 38, 24), loadingImages[1]);
			GUI.Label(new Rect(5, 95, 240, 80), "Use the arrow keys or\nWASD to move your mouse.");
			GUI.DrawTexture(new Rect(120-47, 175-47, 95, 95), controllerIMG);
			GUI.Label(new Rect(5, 175, 240, 80), "Or if you want you can\n use a controller to play!");
		}
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the locating game window while inside the lobby.
	//--------------------------------------------------------------
	private void showLocatingGameWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-100,Screen.height/2-90,200,140),"LOCATING GAME", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-100,Screen.height/2-50,200,100),"");
		GUI.BeginGroup(new Rect(Screen.width/2-100,Screen.height/2-50,200,100));
		GUI.DrawTexture(new Rect(100-19, 50-12, 38, 24), loadingImages[(int)loadingFrame]);
		GUI.EndGroup();
		loadingFrame += 0.05f;
		if (loadingFrame > loadingImages.Length-1)
		{
			loadingFrame = 0.0f;
		}
	}

	//--------------------------------------------------------------
	/// Use to display the user interface during the game.
	//--------------------------------------------------------------
	private void showGamePlayUI()
	{
		GUI.BeginGroup(new Rect(0,0,Screen.width,Screen.height)); //TODO: pointless?
		if (PhotonNetwork.offlineMode)
		{
			for (int i = 0; i < BotAI.NUMBER_OF_BOTS; i++)
			{
				switch (i)
				{
				case 0:
					GUI.DrawTexture(new Rect(Screen.width-105, Screen.height-153, 70, 118), scoreImages[1]);
					GUI.Label(new Rect(Screen.width-102, Screen.height-133, 70, 118), BotAI.BOT_CHEESE_COUNT[i].ToString(), "ScoreText");
					break;
				case 1:
					GUI.DrawTexture(new Rect(Screen.width-105, 35, 70, 118), scoreImages[3]);
					GUI.Label(new Rect(Screen.width-102, 55, 70, 118), BotAI.BOT_CHEESE_COUNT[i].ToString(), "ScoreText");
					break;
				case 2:
					GUI.DrawTexture(new Rect(35, Screen.height-153, 70, 118), scoreImages[2]);
					GUI.Label(new Rect(38, Screen.height-133, 70, 118), BotAI.BOT_CHEESE_COUNT[i].ToString(), "ScoreText");
					break;
				}
			}
		}
		foreach (PhotonPlayer player in PhotonNetwork.playerList) 
		{
			ExitGames.Client.Photon.Hashtable h = player.customProperties;
			int tempPlayerID = int.Parse(h["PlayerID"].ToString())-1;
			GUI.DrawTexture(new Rect(guiScoreXPositions[tempPlayerID], guiScoreYPositions[tempPlayerID], 70, 118), scoreImages[tempPlayerID]);
			GUI.Label(new Rect(guiScoreXPositions[tempPlayerID]+2, guiScoreYPositions[tempPlayerID]+20, 70, 118), h["CheeseCount"].ToString(), "ScoreText");
		}

		Timer roundTimer = game.getTimerManager().getTimer("ROUND_COUNTDOWN");
		if (roundTimer != null && roundTimer.getTimeLeft() >= 0)
		{
			string clockCountDown = (((int)roundTimer.getTimeLeft()/60).ToString("00") +":"+ (((int)roundTimer.getTimeLeft())%60).ToString("00")); 

			char[] clockChars = clockCountDown.ToCharArray();
			GUI.Box(new Rect(Screen.width/2-150, 25, 300, 90),"");
			for (int i = 0; i < 5; i++)
			{
				GUI.Label(new Rect(Screen.width/2-145 + (i*60), 30, 60, 80), clockCountDown.ToCharArray()[i].ToString(), "CountDownText");
			}
			//add colon for clock
		}
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the options window for the 'host'.
	//--------------------------------------------------------------
	private void showRoomOptionsWindowForHost()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-200,Screen.height/2-200,400,320),"GAME ROOM", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-200,Screen.height/2-160,400,280),"");
		GUI.BeginGroup(new Rect(Screen.width/2-200,Screen.height/2-160,400,280));
		GUI.SetNextControlName("START");
		if (GUI.Button(new Rect(30, 162, 340, 50), "START", "YellowButton") || game.getMenuManager().menuButtonPushed("START"))
		{
			if (Maze.spawnRandomMap)
			{
				//if (!PhotonNetwork.offlineMode)
				//	this.GetComponent<PhotonView>().RPC("begin", PhotonTargets.All, game.getLengthOfRound().ToString());
				if (newMazeColumns == "") {newMazeColumns = "17";}
				if (newMazeRows == "") {newMazeRows = "9";}
				this.GetComponent<PhotonView>().RPC("updateMazeColumns", PhotonTargets.All, newMazeColumns);
				this.GetComponent<PhotonView>().RPC("updateMazeRows", PhotonTargets.All, newMazeRows);
				//if (PhotonNetwork.offlineMode)
					this.GetComponent<PhotonView>().RPC("begin", PhotonTargets.All, game.getLengthOfRound().ToString());
				game.getGameStateManager().goToState(GameStateManager.GameState.CREATING_MAZE);
			}	else
			{
				string savedString = PlayerPrefs.GetString("SAVE SLOT "+saveSlotSelected.ToString());
				if (savedString != null);
				{
					string[] splitString = savedString.Split('|');
					if (splitString.Length == 3)
					{
						this.GetComponent<PhotonView>().RPC("updateMazeColumns", PhotonTargets.All, splitString[0]);
						this.GetComponent<PhotonView>().RPC("updateMazeRows", PhotonTargets.All, splitString[1]);
						game.getMaze().initMaze();
						Maze.stringToLoad = splitString[2];
					}
				}
				this.GetComponent<PhotonView>().RPC("begin", PhotonTargets.All, game.getLengthOfRound().ToString());
			}
			game.getAudioManager().playClip("MENU_SOUND");
		}
		
		string style = "Box";
		if (GUI.GetNameOfFocusedControl() == "LENGTH")
		{
			style = "SelectedBox";
			if (game.getMenuManager().getLeftPushed())
			{
				if (game.getLengthOfRound() > 30)
				{
					game.addToLengthOfRound(-15);
					this.GetComponent<PhotonView>().RPC("updateRoundLength", PhotonTargets.All, game.getLengthOfRound().ToString());
				}
				game.getMenuManager().reset();
			} else if (game.getMenuManager().getRightPushed())
			{
				if (game.getLengthOfRound() < 120)
				{
					game.addToLengthOfRound(15);
					this.GetComponent<PhotonView>().RPC("updateRoundLength", PhotonTargets.All, game.getLengthOfRound().ToString());
				}
				game.getMenuManager().reset();
			}
		}
		GUI.SetNextControlName("LENGTH");
		GUI.Box(new Rect(30, 10, 340, 35)," ", style);
		GUI.skin = defaultGUISkin;
		GUI.Label(new Rect (100, -1, 200, 25), "LENGTH OF GAME");

		game.setLengthOfRound(GUI.Toolbar (new Rect (30, 19, 340, 30), ((game.getLengthOfRound()-15)/15)-1, gameLengths, "MultipleSelectionButton"));
		this.GetComponent<PhotonView>().RPC("updateRoundLength", PhotonTargets.All, (((game.getLengthOfRound()+1)*15)+15).ToString());

		GUI.skin = defaultGUISkin;
		GUI.SetNextControlName("BACK");
		if (GUI.Button(new Rect(30, 222, 340, 50), "BACK") || game.getMenuManager().menuButtonPushed("BACK"))
		{
			if (PhotonNetwork.offlineMode)
			{
				game.getGameStateManager().goToState(GameStateManager.GameState.MAIN_MENU);
			} else
			{
				game.getNetworkManager().leaveRoom();
			}
			game.getAudioManager().playClip("MENU_SOUND");
		}
		
		if (PhotonNetwork.offlineMode)
		{
			style = "Box";
			if (GUI.GetNameOfFocusedControl() == "NUMBER")
			{
				style = "SelectedBox";
				if (game.getMenuManager().getLeftPushed())
				{
					if (BotAI.NUMBER_OF_BOTS > 0)
					{
						BotAI.NUMBER_OF_BOTS--;
					}
					game.getMenuManager().reset();
				} else if (game.getMenuManager().getRightPushed())
				{
					if (BotAI.NUMBER_OF_BOTS < 3)
					{
						BotAI.NUMBER_OF_BOTS++;
					}
					game.getMenuManager().reset();
				}
			}
			GUI.SetNextControlName("NUMBER");
			GUI.Box(new Rect(30, 62, 340, 35)," ", style);
			GUI.skin = defaultGUISkin;
			GUI.Label(new Rect (100, 51, 200, 25), "NUMBER OF BOTS");
			BotAI.NUMBER_OF_BOTS = GUI.Toolbar (new Rect (30, 71, 340, 30), BotAI.NUMBER_OF_BOTS, botNumberStrings, "MultipleSelectionButton");

			style = "Box";
			if (GUI.GetNameOfFocusedControl() == "DIFFICULTY")
			{
				style = "SelectedBox";
				if (game.getMenuManager().getLeftPushed())
				{
					if (BotAI.BOT_DIFFICULTY > 0)
					{
						BotAI.BOT_DIFFICULTY--;
					}
					game.getMenuManager().reset();
				} else if (game.getMenuManager().getRightPushed())
				{
					if (BotAI.BOT_DIFFICULTY < 2)
					{
						BotAI.BOT_DIFFICULTY++;
					}
					game.getMenuManager().reset();
				}
			}
			GUI.SetNextControlName("DIFFICULTY");
			GUI.Box(new Rect(30, 114, 340, 35)," ", style);
			GUI.skin = defaultGUISkin;
			GUI.Label(new Rect (100, 103, 200, 25), "DIFFICULTY");
			BotAI.BOT_DIFFICULTY = GUI.Toolbar (new Rect (30, 123, 340, 30), BotAI.BOT_DIFFICULTY, botDifficultyStrings, "MultipleSelectionButton");
		} else
		{
			style = "Box";
			if (GUI.GetNameOfFocusedControl() == "NUMBER")
			{
				style = "SelectedBox";
			}
			GUI.SetNextControlName("NUMBER");
			GUI.Box(new Rect(30, 62, 340, 35)," ", style);
			GUI.skin = defaultGUISkin;
			GUI.Label(new Rect (100, 51, 200, 25), "NUMBER OF BOTS");
			GUI.Toolbar (new Rect (30, 71, 340, 30), BotAI.NUMBER_OF_BOTS, botNumberStrings, "NonHostButton");
			style = "Box";
			if (GUI.GetNameOfFocusedControl() == "DIFFICULTY")
			{
				style = "SelectedBox";
			}
			GUI.SetNextControlName("DIFFICULTY");
			GUI.Box(new Rect(30, 114, 340, 35)," ", style);
			GUI.skin = defaultGUISkin;
			GUI.Label(new Rect (100, 103, 200, 25), "DIFFICULTY");
			GUI.Toolbar (new Rect (30, 123, 340, 30), BotAI.BOT_DIFFICULTY, botDifficultyStrings, "NonHostButton");
		}

		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the options window for a non 'host'.
	//--------------------------------------------------------------
	private void showRoomOptionsWindowForClient()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-200,Screen.height/2-200,400,320),"GAME ROOM", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-200,Screen.height/2-160,400,280),"");
		GUI.BeginGroup(new Rect(Screen.width/2-200,Screen.height/2-160,400,280));
		GUI.SetNextControlName("START");
		GUI.Button(new Rect(30, 162, 340, 50), "START", "NonHostButton");
		Timer roomTimer = game.getTimerManager().getTimer("ROOM_COUNTDOWN");
		if ((roomTimer == null || !roomTimer.isRunning()))
		{
			GUI.skin = defaultGUISkin;
			GUI.SetNextControlName("BACK");
			if (GUI.Button(new Rect(30, 222, 340, 50), "BACK") || game.getMenuManager().menuButtonPushed("BACK"))
			{
				game.getAudioManager().playClip("MENU_SOUND");
				if (PhotonNetwork.offlineMode)
				{
					game.getGameStateManager().goToState(GameStateManager.GameState.MAIN_MENU);
				} else
				{
					game.getNetworkManager().leaveRoom();
				}
			}
		}
		string style = "Box";
		if (GUI.GetNameOfFocusedControl() == "LENGTH")
		{
			style = "SelectedBox";
		}
		GUI.SetNextControlName("LENGTH");
		GUI.Box(new Rect(30, 10, 340, 35)," ");
		GUI.skin = defaultGUISkin;
		GUI.Label(new Rect (100, -1, 200, 25), "LENGTH OF GAME");
		GUI.Toolbar (new Rect (30, 19, 340, 30), ((game.getLengthOfRound()-15)/15)-1, gameLengths, "NonHostButton");
		style = "Box";
		if (GUI.GetNameOfFocusedControl() == "NUMBER")
		{
			style = "SelectedBox";
		}
		GUI.SetNextControlName("NUMBER");
		GUI.Box(new Rect(30, 62, 340, 35)," ");
		GUI.skin = defaultGUISkin;
		GUI.Label(new Rect (100, 51, 200, 25), "NUMBER OF BOTS");
		GUI.Toolbar (new Rect (30, 71, 340, 30), BotAI.NUMBER_OF_BOTS, botNumberStrings, "NonHostButton");
		style = "Box";
		if (GUI.GetNameOfFocusedControl() == "DIFFICULTY")
		{
			style = "SelectedBox";
		}
		GUI.SetNextControlName("DIFFICULTY");
		GUI.Box(new Rect(30, 114, 340, 35)," ");
		GUI.skin = defaultGUISkin;
		GUI.Label(new Rect (100, 103, 200, 25), "DIFFICULTY");
		GUI.Toolbar (new Rect (30, 123, 340, 30), BotAI.BOT_DIFFICULTY, botDifficultyStrings, "NonHostButton");
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the current playters connected window.
	//--------------------------------------------------------------
	private void showPlayersConnectedWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-460,Screen.height/2-160,235,280),"PLAYER LIST", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-460,Screen.height/2-120,235,240),"");
		GUI.BeginGroup(new Rect(Screen.width/2-460,Screen.height/2-120,235,240));
		int k = 1;
		foreach (PhotonPlayer player in PhotonNetwork.playerList) 
		{
			GUI.Box(new Rect(25, (45*k)-20, 185, 30), "");
			GUI.Label(new Rect (25, (45*k)-20, 185, 30), player.name);
			if (player.isMasterClient)
			{
				GUI.DrawTexture(new Rect(30, (45*k)-20, 26, 24), hostSymbolIMG);
			}
			k++;
		}

		Timer roomTimer = game.getTimerManager().getTimer("ROOM_COUNTDOWN");
		if (roomTimer != null && roomTimer.isRunning())
		{
			int roomCountDown = Mathf.CeilToInt(roomTimer.getTimeLeft());
			GUI.Label(new Rect (20, (45*5)-20, 190, 30), "BEGINNING IN "+roomCountDown.ToString());
		} else if (roomTimer == null || !roomTimer.hasExpired())
		{
			GUI.Label(new Rect (10, (45*5)-20, 210, 30), PhotonNetwork.room.playerCount.ToString()+"/4 PLAYERS CONNECTED");
		}
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the level selection window.
	//--------------------------------------------------------------
	private void showLevelSelectionWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2+225,Screen.height/2-160,235,280),"LEVEL SELECTOR", "TitleBox");
		GUI.Box(new Rect(Screen.width/2+225,Screen.height/2-120,235,240),"");
		GUI.BeginGroup(new Rect(Screen.width/2+225,Screen.height/2-120,250,240));
		GUI.Box(new Rect(10, 10, 215, 125),"");
		saveSlotSelected = GUI.SelectionGrid(new Rect(10, 10, 215, 125), saveSlotSelected, SAVE_SLOT_STRINGS, 1);//
		
		GUI.Box(new Rect(10, 150, 215, 80),"");
		if (GUI.SelectionGrid(new Rect(10, 10, 215, 125), saveSlotSelected, SAVE_SLOT_STRINGS, 1) < 6)//
		{
			Maze.spawnRandomMap = false;
		} 
		if (GUI.Toggle(new Rect(20, 150, 195, 30), Maze.spawnRandomMap, "RANDOMIZED"))
		{
			Maze.spawnRandomMap = true;
			saveSlotSelected = 6;
		}
		
		GUI.Label(new Rect(70, 210, 20, 30), "W");
		GUI.Label(new Rect(150, 210, 20, 30), "H");
		
		newMazeColumns = GUI.TextField(new Rect(60, 185, 40, 30), newMazeColumns, 2);
		int num;
		if (newMazeColumns != "" && !int.TryParse(newMazeColumns, out num))
		{
			newMazeColumns = "0";
		}
		newMazeRows = GUI.TextField(new Rect(140, 185, 40, 30), newMazeRows, 2);
		if (newMazeRows != "" && !int.TryParse(newMazeRows, out num))
		{
			newMazeRows = "0";
		}
		
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the user interface for the room based on whether the user is the 'host' or not.
	//--------------------------------------------------------------
	private void showRoomUI()
	{
		Timer roomTimer = game.getTimerManager().getTimer("ROOM_COUNTDOWN");
		if (PhotonNetwork.isMasterClient && (roomTimer == null || !roomTimer.isRunning()))
		{
			showRoomOptionsWindowForHost();
		} else
		{
			showRoomOptionsWindowForClient();
		}
		
		if (!PhotonNetwork.offlineMode && PhotonNetwork.connected)
		{
			showPlayersConnectedWindow();
		}
		
		if (PhotonNetwork.isMasterClient && (roomTimer == null || !roomTimer.isRunning()))
		{
			showLevelSelectionWindow();
		}
	}

	//--------------------------------------------------------------
	/// Use to display the level editor user interface.
	//--------------------------------------------------------------
	private void showLevelEditorUI()
	{
		GUI.SetNextControlName("BACK");
		if (GUI.Button(new Rect(5, 5, 80, 30), "BACK") || game.getMenuManager().menuButtonPushed("BACK"))
		{
			foreach (Transform childTransform in game.getMaze().transform) 
			{
				Destroy(childTransform.gameObject);
			}
			game.getObjectManager().resetFloorToMenu();
			game.getObjectManager().resetCameraPosition();
			game.getGameStateManager().goToState(GameStateManager.GameState.MAIN_MENU);
			game.getAudioManager().playClip("MENU_SOUND");
		}
		touchOptionChosen = GUI.SelectionGrid(new Rect(5, Screen.height-35, 200, 30), touchOptionChosen, TOUCH_OPTIONS, 2);
		if (optionsMenuOpen)
		{
			showLevelEditorHelpWindow();
			showLevelEditorOptionsWindow();
		} else
		{
			if (GUI.Button(new Rect(Screen.width-85, Screen.height-35, 80, 30), "OPTIONS"))
				openOptionsMenu();
		}
	}

	//--------------------------------------------------------------
	/// Use to display the Level Editor Help window.
	//--------------------------------------------------------------
	private void showLevelEditorHelpWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width-300,7,291,143),"HELP", "TitleBox");
		GUI.Box(new Rect(Screen.width-300,47,291,103),"");
		GUI.Label(new Rect(Screen.width-295,42,290,30), "TO ADD OR REMOVE WALLS:");
		GUI.Label(new Rect(Screen.width-295,67,290,30), "TAP THEM WHILE IN EDITING MODE.");
		GUI.Label(new Rect(Screen.width-295,97,290,30), "AND SWIPE TO MOVE THE CAMERA");
		GUI.Label(new Rect(Screen.width-295,122,290,30), "WHILE IN SCROLLING MODE.");
	}

	//--------------------------------------------------------------
	/// Use to display the Level Editor Options window.
	//--------------------------------------------------------------
	private void showLevelEditorOptionsWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width-500,Screen.height-200,493,193),"OPTIONS", "TitleBox");
		GUI.Box(new Rect(Screen.width-500,Screen.height-160,493,153),"");
		GUI.Label(new Rect(Screen.width-495,Screen.height-165,490,30), "TAP NEW TO GENERATE A NEW MAZE.");
		if (GUI.Button(new Rect(Screen.width-85, Screen.height-130, 80, 30), "NEW"))
		{
			game.getObjectManager().resetCameraPosition();
			if (newMazeColumns == "") {newMazeColumns = "17";}
			if (newMazeRows == "") {newMazeRows = "9";}
			Maze.MAZE_COLUMNS = int.Parse(newMazeColumns);
			Maze.MAZE_ROWS = int.Parse(newMazeRows);
			foreach (Transform childTransform in game.getMaze().transform) 
			{
				Destroy(childTransform.gameObject);
			}
			game.getMaze().initMaze();
			game.getMaze().createMaze(false);
			game.getAudioManager().playClip("MENU_SOUND");
		}
		GUI.Label(new Rect(Screen.width-85, Screen.height-97, 20, 30), "W");
		GUI.Label(new Rect(Screen.width-85, Screen.height-67, 20, 30), "H");
		newMazeRows = GUI.TextField(new Rect(Screen.width-65, Screen.height-67, 40, 30), newMazeRows, 2);
		int num;
		if (newMazeRows != "" && !int.TryParse(newMazeRows, out num))
		{
			newMazeRows = "0";
		}
		newMazeColumns = GUI.TextField(new Rect(Screen.width-65, Screen.height-97, 40, 30), newMazeColumns, 2);
		if (newMazeColumns != "" && !int.TryParse(newMazeColumns, out num))
		{
			newMazeColumns = "0";
		}
		if (GUI.Button(new Rect(Screen.width-500, Screen.height-65, 80, 30), "SAVE"))
		{
			game.getAudioManager().playClip("MENU_SOUND");
			if (game.getMaze().saveMaze("SAVE SLOT "+saveSlotSelected.ToString()))
				SAVE_SLOT_STRINGS[saveSlotSelected] = "Save Slot " + (saveSlotSelected+1).ToString() + " (FILLED)";
		}
		if (GUI.Button(new Rect(Screen.width-500, Screen.height-35, 80, 30), "LOAD"))
		{
			game.getAudioManager().playClip("MENU_SOUND");
			game.getMaze().loadMaze("SAVE SLOT "+saveSlotSelected.ToString());
		}
		saveSlotSelected = GUI.SelectionGrid(new Rect(Screen.width-415, Screen.height-130, 325, 125), saveSlotSelected, SAVE_SLOT_STRINGS, 1);
		if (GUI.Button(new Rect(Screen.width-85, Screen.height-35, 80, 30), "CLOSE"))
			closeOptionsMenu();
	}

	//--------------------------------------------------------------
	/// Use to display the gameover window.
	//--------------------------------------------------------------
	private void showGameOverWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-90,300,140),"AND THE WINNER IS...", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-50,300,100),"");
		GUI.BeginGroup(new Rect(Screen.width/2-150,Screen.height/2-50,300,100));
		GUI.Label(new Rect(10, 20, 280, 20), game.getWinnerName());
		GUI.SetNextControlName("QUIT");
		if (GUI.Button(new Rect(60, 50, 180, 40), "QUIT") || game.getMenuManager().menuButtonPushed("QUIT"))
		{
			game.getNetworkManager().Disconnect();
			game.getAudioManager().playClip("MENU_SOUND");
		}
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the error window after calling promptError on Game.
	//--------------------------------------------------------------
	private void showErrorWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-90,300,140),"THERE WAS AN ERROR", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-50,300,100),"");
		GUI.BeginGroup(new Rect(Screen.width/2-150,Screen.height/2-50,300,100));
		GUI.Label(new Rect(10, 20, 280, 20), game.getErrorText());
		GUI.SetNextControlName("OK");
		if (GUI.Button(new Rect(60, 50, 180, 40), "OK") || game.getMenuManager().menuButtonPushed("OK"))
		{
			game.getAudioManager().playClip("MENU_SOUND");
			game.getGameStateManager().goToState(GameStateManager.GameState.MAIN_MENU);
		}
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Use to display the pause window after.
	//--------------------------------------------------------------
	private void showPausedWindow()
	{
		GUI.skin = defaultGUISkin;
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-90,300,140),"GAME PAUSED", "TitleBox");
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-50,300,100),"");
		GUI.BeginGroup(new Rect(Screen.width/2-150,Screen.height/2-50,300,100));
		GUI.SetNextControlName("RESUME");
		if (GUI.Button(new Rect(60, 5, 180, 40), "RESUME") || game.getMenuManager().menuButtonPushed("RESUME"))
		{
			game.getAudioManager().playClip("MENU_SOUND");
			game.getTimerManager().unPauseAllTimers();
			game.getGameStateManager().goToState(GameStateManager.GameState.PLAYING);
		}
		GUI.SetNextControlName("QUIT");
		if (GUI.Button(new Rect(60, 55, 180, 40), "QUIT") || game.getMenuManager().menuButtonPushed("QUIT"))
		{
			game.getAudioManager().playClip("MENU_SOUND");
			game.getNetworkManager().Disconnect();
			game.getGameStateManager().goToState(GameStateManager.GameState.MAIN_MENU);
		}
		GUI.EndGroup();
	}

	//--------------------------------------------------------------
	/// Opens the level editor option menu.
	//--------------------------------------------------------------
	public void openOptionsMenu()
	{
		optionsMenuOpen = true;
	}

	//--------------------------------------------------------------
	/// Close the level editor option menu.
	//--------------------------------------------------------------
	public void closeOptionsMenu()
	{
		optionsMenuOpen = false;
	}

	public bool levelEditorIsScrolling()
	{
		if (touchOptionChosen == 0)
			return false;
		else 
			return true;
	}

	//--------------------------------------------------------------
	/// Returns true if the level editor option menu is open.
	//--------------------------------------------------------------
	public bool optionsIsMenuOpen()
	{
		return optionsMenuOpen;
	}
}
