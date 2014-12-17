using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Game))]

public class GameStateManager : MonoBehaviour 
{
	// Represents the Game.
	private Game game;
	// Enum to store avaiable game states.
	public enum GameState
	{
		NAME_INPUT = 0,
		MAIN_MENU = 1,
		INIT_MAZE = 2,
		CREATING_MAZE = 3,
		LOBBY = 4,
		ROOM = 5,
		PLAYING = 6,
		GAME_OVER = 7,
		LEVEL_EDITOR = 8,
		WAITING = 9,
		ERROR = 10,
		PAUSED = 11
	}
	// Represents the current game state.
	private GameState currentState = GameState.NAME_INPUT;

	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	void Start()
	{
		game = this.GetComponent<Game>();
	}

	//--------------------------------------------------------------
	/// Use to change the state of the game.
	//--------------------------------------------------------------
	public void goToState(GameState _newState)
	{
		currentState = _newState;
		game.getMenuManager().newStateChange(_newState);
		game.getAudioManager().newStateChange(_newState);
	}

	//--------------------------------------------------------------
	/// Returns the current game state.
	//--------------------------------------------------------------
	public GameState getGameState()
	{
		return currentState;
	}
}
