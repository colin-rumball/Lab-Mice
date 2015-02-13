using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour 
{
	// Represents what animation should be played.
	public string currentAnimation = "Down";
	// Represents the Game.
	private Game game;
	private Animator animator;
	private GameObject mainCamera;
	// Walking speed of the character.
	private const float SPEED = 2.4f;

	//--------------------------------------------------------------
	/// Called automatically at Object Initialization.
	//--------------------------------------------------------------
	void Start()
	{
		game = GameObject.Find("_SCRIPTS").GetComponent<Game>();
		if (game == null)
			Debug.LogError("Character could not find game script.");
		animator = this.GetComponentInChildren<Animator>();
		// Don't play the animation until the mouse moves.
		animator.speed = 0;
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

		// Initialize the position of the camera based of where in the maze the mouse is.
		if (this.transform.position.x < 5.0f)
			mainCamera.transform.position = new Vector3(5.0f, mainCamera.transform.position.y, mainCamera.transform.position.z);
		else if (this.transform.position.x > Maze.MAZE_COLUMNS-5)
			mainCamera.transform.position = new Vector3(Maze.MAZE_COLUMNS-5, mainCamera.transform.position.y, mainCamera.transform.position.z);
		if (this.transform.position.z > -3.0f)
		{
			mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, -4.0f);
		}
		else if (this.transform.position.z < -(Maze.MAZE_ROWS-3))
		{
			mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, -(Maze.MAZE_ROWS-2));
		}
	}

	//--------------------------------------------------------------
	/// Called every frame automatically.
	//--------------------------------------------------------------
	void Update()
	{
		// Move the camera with the mouse if the mouse is far enough away from the edge of the maze.
		if (this.transform.position.x > 5 && this.transform.position.x < Maze.MAZE_COLUMNS-5)
			mainCamera.transform.position = new Vector3(this.gameObject.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
		if (this.transform.position.z < -3 && this.transform.position.z > -(Maze.MAZE_ROWS-3))
			mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, this.gameObject.transform.position.z-1.0f);
		// Move mouse based off the input provided from the player.
		if (Application.platform != RuntimePlatform.Android)
		{
			movementInput();
		}
	}

	//--------------------------------------------------------------
	/// Checks for input from the player and moves the mouse accordingly.
	//--------------------------------------------------------------
	private void movementInput()
	{
		float ztranslation = 0.0f;
		float xtranslation = 0.0f;
		if (game.getGameStateManager().getGameState() == GameStateManager.GameState.PLAYING)
		{
			animator.speed = 1;
			if (Input.GetAxis("Vertical") > 0)
			{
				ztranslation = SPEED;
				animator.Play("Up");
				currentAnimation = "Up";
			} else if (Input.GetAxis("Vertical") < 0)
			{
				ztranslation = -SPEED;
				animator.Play("Down");
				currentAnimation = "Down";
			} else if (Input.GetAxis("Horizontal") < 0)
			{
				xtranslation = -SPEED;
				animator.Play("Left");
				currentAnimation = "Left";
			} else if (Input.GetAxis("Horizontal") > 0)
			{
				xtranslation = SPEED;
				animator.Play("Right");
				currentAnimation = "Right";
			} else
			{
				animator.speed = 0;
			}
		}     
		rigidbody.velocity = new Vector3(xtranslation, 0.0f, ztranslation);
	}

	//--------------------------------------------------------------
	/// Checks for collision with the cheese.
	//--------------------------------------------------------------
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Cheese(Clone)" && enabled)
		{
			Debug.Log("cheese collided");
			// Call an RPC event to remove everyones cheese.
			this.GetComponent<PhotonView>().RPC("removeCheese", PhotonTargets.All, null);
			this.GetComponent<NetworkCharacter>().addOneToCheeseCount();
			game.getAudioManager().playClip("COLLECT_SOUND");
		}
	}

	public void setVelocity(Vector3 vel)
	{
		rigidbody.velocity = vel;
	}

	public void setCurrentAnimation(string _str)
	{
		currentAnimation = _str;
	}
}
