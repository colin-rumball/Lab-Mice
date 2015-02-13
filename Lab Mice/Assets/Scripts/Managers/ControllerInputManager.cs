using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Game))]

public class ControllerInputManager : MonoBehaviour 
{
	// Represents the Game.
	private Game game;
	// Records whether the user is using a controller or not.
	private bool controllerActive = false;
	// Stores the controller tilt from the last frame for menu navigation purposes.
	private float vertTiltLastFrame = 0.0f;
	private float horiTiltLastFrame = 0.0f;

	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	public void Start()
	{
		game = this.GetComponent<Game>();
	}

	//--------------------------------------------------------------
	/// Called every frame automatically.
	//--------------------------------------------------------------
	public void Update () 
	{
		if (usingController())
		{
			MenuManager menuManager = game.getMenuManager();
			if (Input.GetButtonDown("Controller_Fire"))
			{
				menuManager.buttonPushed();
			}
			if (vertTiltLastFrame != Input.GetAxis("Controller_Vertical"))
			{
				if (Input.GetAxis("Controller_Vertical") < 0)
				{
					menuManager.nextMenuItem();
				} else if (Input.GetAxis("Controller_Vertical") > 0)
				{
					menuManager.prevMenuItem();
				}
			}
			game.getMenuManager().resetLeftRight();
			if (horiTiltLastFrame != Input.GetAxis("Controller_Horizontal"))
			{
				if (Input.GetAxis("Controller_Horizontal") < 0)
				{
					menuManager.menuLeft();
				} else if (Input.GetAxis("Controller_Horizontal") > 0)
				{
					menuManager.menuRight();
				}
			}
		}
		vertTiltLastFrame = Input.GetAxis("Controller_Vertical");
		horiTiltLastFrame = Input.GetAxis("Controller_Horizontal");

		if (Input.GetAxis("Controller_Horizontal") != 0.0f || Input.GetAxis("Controller_Vertical") != 0.0f
		    || Input.GetButtonDown("Controller_Fire"))
			controllerActive = true;
		if (Input.GetMouseButton(0))
			controllerActive = false;
	}

	//--------------------------------------------------------------
	/// Returns true when the user is using a controller.
	//--------------------------------------------------------------
	public bool usingController()
	{
		return controllerActive;
	}
}
