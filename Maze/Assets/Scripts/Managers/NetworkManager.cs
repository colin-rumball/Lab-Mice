using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Game))]

public class NetworkManager : MonoBehaviour 
{
	// Represents the Game.
	private Game game;
	//--------------------------------------------------------------
	/// Called at the start of application.
	//--------------------------------------------------------------
	void Start()
	{
		game = this.GetComponent<Game>();
		// Assigns network name from saved username data.
		PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "");
	}

	//--------------------------------------------------------------
	/// Connect to Photon network for network play.
	//--------------------------------------------------------------
	public void Connect() {
		Debug.Log("Connect");
		PhotonNetwork.ConnectUsingSettings ("LabMice v001");
	}

	//--------------------------------------------------------------
	/// Disconnect from Photon network.
	//--------------------------------------------------------------
	public void Disconnect()
	{
		Debug.Log("Disconnect");
		game.getObjectManager().resetCameraPosition();
		game.getObjectManager().resetFloorToMenu();
		PhotonNetwork.Disconnect();
	}

	//--------------------------------------------------------------
	/// Leave current Photon network room.
	//--------------------------------------------------------------
	public void leaveRoom()
	{
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.Disconnect();
		game.getGameStateManager().goToState(GameStateManager.GameState.MAIN_MENU);
	}

	//--------------------------------------------------------------
	/// Called automatically when this script is destroyed.
	//--------------------------------------------------------------
	void OnDestroy()
	{
		// Saves current username for future use.
		PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
	}

	//--------------------------------------------------------------
	/// Automatically called when an open lobby is found.
	//--------------------------------------------------------------
	public void OnJoinedLobby()
	{
		Debug.Log("OnJoinedLobby");
		// Attempt to join a room.
		PhotonNetwork.JoinRandomRoom();
	}

	//--------------------------------------------------------------
	/// Automatically called when an open room can't be found.
	//--------------------------------------------------------------
	void OnPhotonRandomJoinFailed()
	{
		Debug.Log("OnPhotonRandomJoinFailed");
		// If an open room could not be found then create one.
		PhotonNetwork.CreateRoom (null);
	}

	//--------------------------------------------------------------
	/// Automatically called when an open room is found.
	//--------------------------------------------------------------
	public void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom");
		// If we are the "host" then set maximum amount of players for the room.
		if (PhotonNetwork.isMasterClient)
		{
			PhotonNetwork.room.maxPlayers = 4;
		} 
		game.getGameStateManager().goToState(GameStateManager.GameState.ROOM);
	}

	void onPhotonPlayerConnected(PhotonPlayer newPlayer)
	{}

	//--------------------------------------------------------------
	/// Automatically called when a player disconnects from the room.
	//--------------------------------------------------------------
	void onPhotonPlayerDisconnected(PhotonPlayer newPlayer)
	{
		GameStateManager.GameState currentState = game.getGameStateManager().getGameState();
		if (currentState == GameStateManager.GameState.PLAYING ||
		    currentState == GameStateManager.GameState.WAITING)
		{
			PhotonNetwork.room.maxPlayers--;
		}
	}

	//--------------------------------------------------------------
	/// Automatically called when we disconnect from the Photon network.
	//--------------------------------------------------------------
	void OnDisconnectedFromPhoton()
	{
		Debug.Log("OnDisconnectedFromPhoton");
		game.getMaze().removeAllWalls();
		game.getObjectManager().removeCheese();
		game.getObjectManager().removeAllBots();
		game.getObjectManager().resetFloorToMenu();
		game.getGameStateManager().goToState(GameStateManager.GameState.MAIN_MENU);
	}

	//--------------------------------------------------------------
	/// Automatically called when we fail to disconnect to the Photon network.
	//--------------------------------------------------------------
	void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		Debug.Log("OnFailedToConnectToPhoton");
		game.promptError("FAILED TO CONNECT TO SERVER");
	}

	//--------------------------------------------------------------
	/// Automatically called when we fail to connect to the Photon network.
	//--------------------------------------------------------------
	void OnConnectionFail(DisconnectCause cause)
	{
		Debug.Log("OnConnectionFail");
		game.promptError("FAILED TO CONNECT TO SERVER");
	}

	//--------------------------------------------------------------
	/// Used to sync number of maze columns between all connected users.
	//--------------------------------------------------------------
	[RPC]
	public void updateMazeColumns(string _columns)
	{
		Debug.Log("updateMazeColumns: "+_columns);
		Maze.MAZE_COLUMNS = int.Parse(_columns);
	}

	//--------------------------------------------------------------
	/// Used to sync number of maze rows between all connected users.
	//--------------------------------------------------------------
	[RPC]
	public void updateMazeRows(string _rows)
	{
		//Debug.Log("updateMazeRows: "+_rows);
		Maze.MAZE_ROWS = int.Parse(_rows);
	}

	//--------------------------------------------------------------
	/// Used to sync the length of the round between all connected users.
	//--------------------------------------------------------------
	[RPC]
	public void updateRoundLength(string _length)
	{
		//Debug.Log("updateRoundLength: "+_length);
		game.setLengthOfRound(int.Parse(_length)); //TODO: try parse first
	}

	//--------------------------------------------------------------
	/// Used to begin the round with the selected settings from the room.
	//--------------------------------------------------------------
	[RPC]
	public void begin(string _roundLength)
	{
		Debug.Log("begin");
		if (PhotonNetwork.offlineMode)
		{
			game.startRound();
		} else
		{
			game.getTimerManager().startTimer("ROOM_COUNTDOWN", 5.0f); //TODO: put this in the start function
			if (!PhotonNetwork.isMasterClient)
				game.getMaze().initMaze();
			game.setLengthOfRound(int.Parse(_roundLength));
		}
	}
}
