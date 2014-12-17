using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotAI : MonoBehaviour 
{
	public static int NUMBER_OF_BOTS = 1;
	public static int BOT_DIFFICULTY = 1;
	public static int[] BOT_CHEESE_COUNT = {0,0,0};

	private Game game;
	private Maze maze;
	private GameObject cheese;
	private Animator animator;
	private int botID;
	private bool newCheeseHasSpawned;

	private enum DifficultyLevel
	{
		EASY = 0,
		MEDIUM = 1,
		HARD = 2
	}
	private DifficultyLevel difficultyLevel;
	// Stores the current state of the AI to determine what action should be taken.
	private enum AIState
	{
		LOCATING_CHEESE,
		PATHING,
		GETTING_MOVE,
		MOVING,
		BACKTRACKING
	}
	private AIState aiState;
	// The pathing algorithme
	private MazePather pathingAlgo;
	private Vector3 pathingGoal;
	private Vector3 beginningPosition;
	private Vector3 endingPosition;

	private const float speed = 2.4f;

	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	void Start () 
	{
		game = GameObject.Find("_SCRIPTS").GetComponent<Game>(); //TODO: better way to assign.
		if (game == null)
			Debug.LogError("Bot AI could not find game script.");
		maze = game.getMaze();
		// Get the animator component now for later use.
		animator = this.GetComponentInChildren<Animator>();
		animator.speed = 0;
		// Set intial state for the AI.
		aiState = AIState.LOCATING_CHEESE;

		newCheeseHasSpawned = false;
	}

	//--------------------------------------------------------------
	/// Called every frame automatically.
	//--------------------------------------------------------------
	void Update ()
	{
		if (game.getGameStateManager().getGameState() == GameStateManager.GameState.PLAYING)
		{
			Timer movementTimer;
			switch (this.aiState)
			{
			case AIState.LOCATING_CHEESE:
				if (cheese != null)
				{
					pathingGoal = cheese.transform.position;
					aiState = AIState.PATHING;
				} else
				{
					//TODO: play confused animation
				}
				break;
			case AIState.PATHING:
				maze.setAllCellsToVirgin();
				pathingAlgo = new MazePather(maze, maze.getCell(Mathf.RoundToInt(transform.position.x), -Mathf.RoundToInt(transform.position.z)));

				switch (difficultyLevel)
				{
				case DifficultyLevel.EASY:
					pathingAlgo.pathMazeRoughWallFollow((int)pathingGoal.x, (int)-pathingGoal.z);
					break;
				case DifficultyLevel.MEDIUM:
					pathingAlgo.pathMazeRecursiveBacktracker((int)pathingGoal.x, (int)-pathingGoal.z);
					break;
				case DifficultyLevel.HARD:
					pathingAlgo.pathMazeShortestPath((int)pathingGoal.x, (int)-pathingGoal.z);
					break;
				}
				aiState = AIState.GETTING_MOVE;
				break;
			case AIState.GETTING_MOVE:
				Cell nextCellInStack = pathingAlgo.getNextPathedMovement();

				beginningPosition = transform.position;
				
				if (nextCellInStack != null)
				{
					endingPosition = new Vector3(nextCellInStack.getColumn(), 0.0f, -nextCellInStack.getRow());
					game.getTimerManager().startTimer(this.botID+"_MOVEMENT_TIMER", 2.5f);
					aiState = AIState.MOVING;
				} else
				{
					Debug.Log("bot failed to find next pathed movement");
					//TODO: play confused animation
					aiState = AIState.LOCATING_CHEESE;
				}
				break;
			case AIState.MOVING:
				this.setBotVelocity();
				movementTimer = game.getTimerManager().getTimer(this.botID+"_MOVEMENT_TIMER");
				if (0.1f > Vector3.Distance(transform.position, endingPosition))
				{
					movementTimer.pause();
					destinationReached();
					if (newCheeseHasSpawned)
					{
						newCheeseHasSpawned = false;
						aiState = AIState.LOCATING_CHEESE;
					}
					else
						aiState = AIState.GETTING_MOVE;
				} else if (movementTimer != null && movementTimer.hasExpired())
				{
					Debug.Log("movement timer expired for bot "+this.botID.ToString()+", backtracking to center of cell.");

					endingPosition =  new Vector3(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.y), Mathf.RoundToInt(this.transform.position.z));
					beginningPosition = transform.position;
					movementTimer.reset();
					setBotVelocityToZero();
					aiState = AIState.BACKTRACKING;
				}
				break;
			case AIState.BACKTRACKING:
				this.setBotVelocity();
				movementTimer = game.getTimerManager().getTimer(this.botID+"_MOVEMENT_TIMER");
				if (0.1f > Vector3.Distance(transform.position, endingPosition))
				{
					destinationReached();
					aiState = AIState.LOCATING_CHEESE;
				} else if (movementTimer != null && movementTimer.hasExpired())
				{
					Debug.Log("movement timer expired for bot "+this.botID.ToString()+", resetting to locating");
					setBotVelocityToZero();
					aiState = AIState.LOCATING_CHEESE;
				}
				break;
			}
		} else
		{
			setBotVelocityToZero();
		}
	}

	//--------------------------------------------------------------
	/// Sets the velocity based off the direction the bot is headed.
	//--------------------------------------------------------------
	private void setBotVelocity()
	{
		this.rigidbody.velocity = new Vector3(-(beginningPosition.x-endingPosition.x)*speed, 0.0f, -(beginningPosition.z-endingPosition.z)*speed);

		// Play the correct animation based off the velocity.
		animator.speed = 1;
		if (this.rigidbody.velocity.z > 0.0f)
		{
			animator.Play("Up");
		} else if (this.rigidbody.velocity.z < 0.0f)
		{
			animator.Play("Down");
		} else if (this.rigidbody.velocity.x < 0.0f)
		{
			animator.Play("Left");
		} else if (this.rigidbody.velocity.x > 0.0f)
		{
			animator.Play("Right");
		}
	}

	//--------------------------------------------------------------
	/// Called when the bot has reached the destination.
	//--------------------------------------------------------------
	private void destinationReached()
	{
		this.transform.position = endingPosition;
		setBotVelocityToZero();
	}

	//--------------------------------------------------------------
	/// Sets the bot velocity to zero and stops the current animation.
	//--------------------------------------------------------------
	private void setBotVelocityToZero()
	{
		this.rigidbody.velocity = Vector3.zero;
		animator.speed = 0;
	}

	//--------------------------------------------------------------
	/// Set the ID of the bot for later use.
	//--------------------------------------------------------------
	public void setBotID(int _id)
	{
		botID = _id;
		this.GetComponentInChildren<TextMesh>().text = "Bot " + _id.ToString();
	}

	//--------------------------------------------------------------
	/// Set the difficulty to ensure the correct pathing is used.
	//--------------------------------------------------------------
	public void setDifficulty()
	{
		difficultyLevel = (DifficultyLevel)BotAI.BOT_DIFFICULTY;
	}

	//--------------------------------------------------------------
	/// Use when a new piece of cheese is spawned.
	//--------------------------------------------------------------
	public void reassignCheese(GameObject _newCheese)
	{
		cheese = _newCheese;
		newCheeseHasSpawned = true;
	}

	//--------------------------------------------------------------
	/// Check for collision with cheese.
	//--------------------------------------------------------------
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "Cheese(Clone)" && enabled)
		{
			Debug.Log("cheese collected by bot");
			game.getObjectManager().removeCheese();
			Debug.Log("cheese removed by bot");
			BotAI.BOT_CHEESE_COUNT[botID]++;
		}
	}
}
