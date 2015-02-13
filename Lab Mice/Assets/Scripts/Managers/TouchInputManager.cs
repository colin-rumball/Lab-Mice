using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchInputManager : MonoBehaviour 
{
	// Store the player's mouse.
	public GameObject myCharacter;
	// Stores the character's animator
	private Animator anim;
	private Character characterComponent;
	private Vector2 tapLocation;
	private string currentAnimation = "Down";
	private GameObject mainCamera;
	private List<GameObject> tappedWalls = new List<GameObject>();
	private bool removingWalls, addingWalls;
	private float movementScale = 50.0f;
	// Represents the Game.
	private Game game;
	// Walking speed of the character.
	private const float SPEED = 2.4f;

	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	public void Start()
	{
		game = this.GetComponent<Game>();
		mainCamera = game.getMainCamera();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			movementInput();
			if (game.getGameStateManager().getGameState() == GameStateManager.GameState.LEVEL_EDITOR)
				levelEditorInput();
		}
	}

	private void levelEditorInput()
	{
		if (game.getGUIManager().levelEditorIsScrolling())
		{
			if (Input.GetMouseButtonDown(0))
			{
				tapLocation = new Vector2(Input.mousePosition.x, Input.mousePosition.y);


			} else if (Input.GetMouseButton(0))
			{
				if (Input.mousePosition.x > tapLocation.x + 20.0f || 
				    Input.mousePosition.x < tapLocation.x - 20.0f || 
				    Input.mousePosition.y > tapLocation.x + 20.0f || 
				    Input.mousePosition.y < tapLocation.x - 20.0f)
				{
					if (mainCamera.transform.position.x+(tapLocation.x-Input.mousePosition.x)/movementScale > 6.5f &&
					    mainCamera.transform.position.x+(tapLocation.x-Input.mousePosition.x)/movementScale < (6.5f+Maze.MAZE_COLUMNS))
						mainCamera.transform.position = new Vector3(mainCamera.transform.position.x+(tapLocation.x-Input.mousePosition.x)/movementScale, 
						                                            mainCamera.transform.position.y, 
						                                            mainCamera.transform.position.z);
					if (mainCamera.transform.position.z+(tapLocation.y-Input.mousePosition.y)/movementScale < -1.0f &&
					    mainCamera.transform.position.z+(tapLocation.y-Input.mousePosition.y)/movementScale > (-1.0f-Maze.MAZE_ROWS))
						mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, 
						                                            mainCamera.transform.position.y, 
						                                            mainCamera.transform.position.z+(tapLocation.y-Input.mousePosition.y)/movementScale);

					tapLocation = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				}
			}

		} else if (!game.getGUIManager().optionsIsMenuOpen())
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100.0f))
			{
				if (hit.collider.transform.tag == "Wall")
				{
					GameObject wall = hit.collider.gameObject;
					if (!wall.GetComponent<WallComponent>().isHard() && wall.GetComponent<WallComponent>().getIsAlive() && Input.GetMouseButton(0) && !tappedWalls.Contains(wall) && !addingWalls)
					{
						wall.GetComponent<WallComponent>().kill();
						Cell c = game.getMaze().getCell(wall.GetComponent<WallComponent>().getCol(), wall.GetComponent<WallComponent>().getRow());
						Wall w;
						if (wall.GetComponent<WallComponent>().getWallType() == WallComponent.WallType.left)
						{
							w = c.getLeftWall();
							w.bringDown();
						}
						else if (wall.GetComponent<WallComponent>().getWallType() == WallComponent.WallType.top)
						{
							w = c.getTopWall();
							w.bringDown();
						}
						tappedWalls.Add(wall);
						removingWalls = true;
					}	else if (!wall.GetComponent<WallComponent>().isHard() && !wall.GetComponent<WallComponent>().getIsAlive() && Input.GetMouseButton(0) && !tappedWalls.Contains(wall) && !removingWalls)
					{
						wall.GetComponent<WallComponent>().revive();
						Cell c = game.getMaze().getCell(wall.GetComponent<WallComponent>().getCol(), wall.GetComponent<WallComponent>().getRow());
						Wall w;
						if (wall.GetComponent<WallComponent>().getWallType() == WallComponent.WallType.left)
						{
							w = c.getLeftWall();
							w.buildUp();
						}
						else if (wall.GetComponent<WallComponent>().getWallType() == WallComponent.WallType.top)
						{
							w = c.getTopWall();
							w.buildUp();
						}
						tappedWalls.Add(wall);
						addingWalls = true;
					}
					
				}
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			tappedWalls.Clear();
			removingWalls = false;
			addingWalls = false;
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
			anim.speed = 1;

			if (Input.GetMouseButton(0) && Input.mousePosition.y > Screen.height*0.8f)
			{
				ztranslation = SPEED;
				anim.Play("Up");
				currentAnimation = "Up";
			} else if (Input.GetMouseButton(0) && Input.mousePosition.y < Screen.height*0.2f)
			{
				ztranslation = -SPEED;
				anim.Play("Down");
				currentAnimation = "Down";
			} else if (Input.GetMouseButton(0) && Input.mousePosition.x < Screen.width*0.2f)
			{
				xtranslation = -SPEED;
				anim.Play("Left");
				currentAnimation = "Left";
			} else if (Input.GetMouseButton(0) && Input.mousePosition.x > Screen.width*0.8f)
			{
				xtranslation = SPEED;
				anim.Play("Right");
				currentAnimation = "Right";
			} else
			{
				anim.speed = 0;
			}
			characterComponent.setCurrentAnimation(currentAnimation);
		}

		if (characterComponent != null)
		{
			characterComponent.setVelocity(new Vector3(xtranslation, 0.0f, ztranslation));

		}
	}

	public void setComponents()
	{
		anim = myCharacter.GetComponentInChildren<Animator>();
		characterComponent = myCharacter.GetComponentInChildren<Character>();
	}
}
