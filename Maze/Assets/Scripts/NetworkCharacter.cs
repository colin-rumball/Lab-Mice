using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour 
{
	// Stores the position provided by the network
	Vector3 realPosition = Vector3.zero;
	bool spawned = false;
	Animator anim;
	// Stores the character script associated with this network character
	Character myCharScript;

	//--------------------------------------------------------------
	/// Called automatically at Object Initialization.
	//--------------------------------------------------------------
	void Start () {
		myCharScript = this.GetComponent<Character>();
		anim = this.GetComponentInChildren<Animator>();
		if (anim == null)
			Debug.LogError("Animator Missing.");
		anim.speed = 0;
	}
	
	//--------------------------------------------------------------
	/// Called automatically every frame.
	//--------------------------------------------------------------
	void Update () 
	{
		if (!photonView.isMine)
		{
			// Lerp between the current position and the position provided from the server.
			transform.position = Vector3.Lerp (transform.position, realPosition, 0.1f);
		}
	}

	//--------------------------------------------------------------
	/// Called every time information is sent or recieved
	//--------------------------------------------------------------
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		// If sending information
		if (stream.isWriting) 
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(anim.speed);
			stream.SendNext(myCharScript.currentAnimation);
		}
		// If receiving information.
		else 
		{
			// If this character has not spawned yet then set it's name and position.
			if (!spawned)
			{
				this.GetComponentInChildren<TextMesh>().text = this.GetComponent<PhotonView>().owner.name;
				transform.position = (Vector3) stream.ReceiveNext();
				spawned = true;
			} else
			{
				realPosition = (Vector3) stream.ReceiveNext();
			}
			transform.rotation = (Quaternion) stream.ReceiveNext();
			anim.speed = (float) stream.ReceiveNext();
			anim.Play((string) stream.ReceiveNext());
		}
	}

	//--------------------------------------------------------------
	/// Used to add to the cheese counter of this character.
	//--------------------------------------------------------------
	public void addOneToCheeseCount()
	{
		Debug.Log("cheese collected");
		ExitGames.Client.Photon.Hashtable h = PhotonNetwork.player.customProperties;
		int newScore = int.Parse(h["CheeseCount"].ToString());
		h.Remove("CheeseCount");
		newScore++;
		h.Add("CheeseCount", newScore.ToString());
		PhotonNetwork.player.SetCustomProperties(h);
	}

	//--------------------------------------------------------------
	/// RPC event to remove the cheese from everyones game.
	//--------------------------------------------------------------
	[RPC]
	public void removeCheese()
	{
		GameObject cheese = GameObject.Find("Cheese(Clone)");
		if (cheese != null)
		{
			Debug.Log("cheese removed");
			Destroy(cheese);
		}
		Cheese.CHEESE_SPAWNED = false;
	}
}
