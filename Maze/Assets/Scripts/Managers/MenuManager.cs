using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Game))]

public class MenuManager : MonoBehaviour 
{
	// Represents the Game.
	private Game game;
	// Stores the index of the currently selected menu item.
	private int activeMenuItemIndex = 0;
	// Stores the name of the currently selected menu item.
	private string activeMenuItemName = "CONFIRM";
	// Represents whether or not the currently selected menu item has been pushed.
	private bool activeMenuItemPushed = false;
	// Represents whether or not the right button on a controller has been pushed.
	private bool rightPushed = false;
	// Represents whether or not the left button on a controller has been pushed.
	private bool leftPushed = false;
	// Stores the strings for the menu buttons and the game state they are active in.
	string[,] buttons = {
		{"CONFIRM", GameStateManager.GameState.NAME_INPUT.ToString()},
		{"RANDOMIZE", GameStateManager.GameState.NAME_INPUT.ToString()},
		{"SINGLE-PLAYER", GameStateManager.GameState.MAIN_MENU.ToString()},
		{"MULTIPLAYER", GameStateManager.GameState.MAIN_MENU.ToString()},
		{"LEVEL EDITOR", GameStateManager.GameState.MAIN_MENU.ToString()},
		{"LENGTH", GameStateManager.GameState.ROOM.ToString()},
		{"NUMBER", GameStateManager.GameState.ROOM.ToString()},
		{"DIFFICULTY", GameStateManager.GameState.ROOM.ToString()},
		{"START", GameStateManager.GameState.ROOM.ToString()},
		{"BACK", GameStateManager.GameState.ROOM.ToString()},
		{"BACK", GameStateManager.GameState.LEVEL_EDITOR.ToString()},
		{"QUIT", GameStateManager.GameState.GAME_OVER.ToString()},
		{"OK", GameStateManager.GameState.ERROR.ToString()},
		{"RESUME", GameStateManager.GameState.PAUSED.ToString()},
		{"QUIT", GameStateManager.GameState.PAUSED.ToString()}
	};

	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	public void Start()
	{
		game = this.GetComponent<Game>();
	}

	//--------------------------------------------------------------
	/// Use when a menu button was pushed.
	//--------------------------------------------------------------
	public void buttonPushed()
	{
		string currentStateString = game.getGameStateManager().getGameState().ToString();
		for (int i = 0; i < buttons.Length/2; i++)
		{
			if (activeMenuItemName == buttons[i,0]
			    && currentStateString == buttons[i,1])
			{
				activeMenuItemPushed = true;
			}
		}
	}

	//--------------------------------------------------------------
	/// Use to select the next menu item in the list.
	//--------------------------------------------------------------
	public void nextMenuItem()
	{
		this.reset();
		if (activeMenuItemIndex+1 < buttons.Length/2 && buttons[activeMenuItemIndex,1] == buttons[activeMenuItemIndex+1,1])
		{
			activeMenuItemIndex++;
			activeMenuItemName = buttons[activeMenuItemIndex,0];
		}
	}

	//--------------------------------------------------------------
	/// Use to select the previous menu item in the list.
	//--------------------------------------------------------------
	public void prevMenuItem()
	{
		this.reset();
		if (activeMenuItemIndex > 0 && buttons[activeMenuItemIndex,1] == buttons[activeMenuItemIndex-1,1])
		{
			activeMenuItemIndex--;
			activeMenuItemName = buttons[activeMenuItemIndex,0];
		}
		print(activeMenuItemIndex+"|"+activeMenuItemName);
	}

	//--------------------------------------------------------------
	/// Use when a controller has pushed right.
	//--------------------------------------------------------------
	public void menuRight()
	{
		rightPushed = true;
	}

	//--------------------------------------------------------------
	/// Use when a controller has pushed left.
	//--------------------------------------------------------------
	public void menuLeft()
	{
		leftPushed = true;
	}

	//--------------------------------------------------------------
	/// Use to reset the left and right buttons within a menu.
	//--------------------------------------------------------------
	public void resetLeftRight()
	{
		rightPushed = false;
		leftPushed = false;
	}

	//--------------------------------------------------------------
	/// Called when the game state is changed
	//--------------------------------------------------------------
	public void newStateChange(GameStateManager.GameState _newState)
	{
		string stateString = _newState.ToString();
		for (int i = 0; i < buttons.Length/2; i++)
		{
			if (buttons[i,1] == stateString)
			{
				activeMenuItemPushed = false;
				activeMenuItemIndex = i;
				activeMenuItemName = buttons[i,0];
				return;
			}
		}
	}

	public string getActiveMenuItemName()	{return activeMenuItemName;}
	public bool getLeftPushed()	{return leftPushed;}
	public bool getRightPushed()	{return rightPushed;}

	//--------------------------------------------------------------
	/// Use to reset the menu's inputs.
	//--------------------------------------------------------------
	public void reset()
	{
		activeMenuItemPushed = false;
		this.resetLeftRight();
	}

	//--------------------------------------------------------------
	/// Returns whether a specific menu button has been pushed.
	//--------------------------------------------------------------
	public bool menuButtonPushed(string _buttonName)
	{
		if (activeMenuItemPushed && _buttonName == activeMenuItemName)
			return true;
		return false;
	}

	//--------------------------------------------------------------
	/// Use to go to the single player menu.
	//--------------------------------------------------------------
	public void goToSinglePlayerMenu()
	{
		BotAI.NUMBER_OF_BOTS = 1; //TODO: setBotCount()
		if (!PhotonNetwork.offlineMode)
		{
			PhotonNetwork.offlineMode = true;
			if (PhotonNetwork.room == null)
			{
				game.getNetworkManager().OnJoinedLobby(); //TODO: fix this shit
			} else
			{
				game.getNetworkManager().OnJoinedRoom();
			}
		} else
		{
			game.getNetworkManager().OnJoinedRoom();
		}
	}

	//--------------------------------------------------------------
	/// Use to go to the multiplayer menu.
	//--------------------------------------------------------------
	public void goToMultiplayerMenu()
	{
		BotAI.NUMBER_OF_BOTS = 0; //TODO: setBotCount()
		game.getNetworkManager().Connect();
		game.getTimerManager().startTimer("LOBBY_COUNTDOWN", 15.0f);
		game.getGameStateManager().goToState(GameStateManager.GameState.LOBBY);
	}
}
