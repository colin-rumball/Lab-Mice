using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Game))]

public class AudioManager : MonoBehaviour 
{
	// Stores the audio files themselves.
	public AudioClip[] audioClips = new AudioClip[7];
	// Stores the names of the clips.
	private string[] clipNames = {
		"MAIN_MENU_MUSIC",
		"GAMEPLAY_MUSIC",
		"WIN_SOUND",
		"LOSE_SOUND",
		"COLLECT_SOUND",
		"SQUEAK_SOUND",
		"EAT_SOUND",
		"BEEP_SOUND",
		"MENU_SOUND"
	};
	// Represents the Game.
	private Game game;
	// Represents the State Manager from Game.
	private GameStateManager gameStateManager;
	// Stores the sources after creation for easy management.
	private List<AudioSource> audioSources = new List<AudioSource>();
	
	//--------------------------------------------------------------
	/// Called automatically at application start.
	//--------------------------------------------------------------
	void Start()
	{
		game = this.GetComponent<Game>();
		gameStateManager = game.getGameStateManager();
		if (gameStateManager == null)
			Debug.LogError("Game State Manager not found.");
	}

	//--------------------------------------------------------------
	/// Called every frame automatically.
	//--------------------------------------------------------------
	void Update()
	{
		// Checks the current game state and decides when to increase the pitch of the game play music
		if (gameStateManager.getGameState() == GameStateManager.GameState.PLAYING)
		{
			AudioSource music = getAudioSource("GAMEPLAY_MUSIC");
			if (music != null)
			{
				Timer roundTimer = game.getTimerManager().getTimer("ROUND_COUNTDOWN");
				if (roundTimer != null)
				{
					if (roundTimer.getTimeLeft() > game.getLengthOfRound()*0.25 && music.pitch < 1.125f)
					{
						music.pitch = 1.125f;
					} else if (roundTimer.getTimeLeft() > game.getLengthOfRound()*0.75 && music.pitch < 1.25f)
					{
						music.pitch = 1.25f;
					}
				}
			}
		}
	}

	//--------------------------------------------------------------
	/// Called for every game state change automatically.
	//--------------------------------------------------------------
	public void newStateChange(GameStateManager.GameState _newState)
	{
		// Play the appropriate music based on the new state
		switch(_newState)
		{
		case GameStateManager.GameState.MAIN_MENU:
			stopAllClips("MAIN_MENU_MUSIC");
			playClip("MAIN_MENU_MUSIC", true);
			break;
		case GameStateManager.GameState.PLAYING:
			stopAllClips();
			playClip("GAMEPLAY_MUSIC", true);
			break;
		case GameStateManager.GameState.GAME_OVER:
			stopAllClips("GAMEPLAY_MUSIC");
			fadeClipOut("GAMEPLAY_MUSIC");
			if (game.getWinnerName().Contains(PhotonNetwork.player.name))
				playClip("WIN_SOUND");
			else
				playClip("LOSE_SOUND");
			break;
		}
	}

	//--------------------------------------------------------------
	/// Plays a clip unless if is already playing.
	//--------------------------------------------------------------
	public void playClip(string _clipName, bool _loop = false)
	{
		// Loop through all audio sources to see if the clip has already been created
		foreach(AudioSource source in audioSources)
		{
			// If the clip has played before then it is already created and we don't need to create it again
			if (source.name == _clipName)
			{
				// If the sound is not playing then we play it.
				if (!source.isPlaying)
				{
					// If for some reason a fade component still exists then remove it.
					FadeAudioOut fade = source.GetComponent<FadeAudioOut>();
					if (fade != null)
						Destroy(fade);
					source.Play();
					source.volume = 1.0f;
					source.pitch = 1.0f;
				}
				return;
			}
		}

		// If the source wasn't found above then we create it to play it
		int clipIndex = getClipIndex(_clipName);
		if (clipIndex >= 0)
		{
			GameObject obj = new GameObject();
			obj.AddComponent<AudioSource>();
			AudioSource audioSource = obj.GetComponent<AudioSource>();
			audioSource.clip = audioClips[clipIndex];
			audioSource.name = _clipName;
			audioSource.loop = _loop;
			audioSources.Add(audioSource);
			audioSources[audioSources.Count-1].Play();
		} else
			// The clip index does not exist so the name provided was wrong
			Debug.LogError("Incorrect clip name provided. Named: "+_clipName);

	}


	//--------------------------------------------------------------
	/// Returns the audio source with the provided name. (Can return Null)
	//--------------------------------------------------------------
	private AudioSource getAudioSource(string _clipName)
	{
		foreach(AudioSource source in audioSources)
		{
			if (source.name == _clipName)
			{
				return source;
			}
		}
		return null;
	}

	//--------------------------------------------------------------
	/// Returns the index of the stored clip file with the provided name. (Returns negative if not found)
	//--------------------------------------------------------------
	private int getClipIndex(string _clipName)
	{
		for (int i = 0; i < clipNames.Length; i++)
		{
			if (clipNames[i] == _clipName)
			{
				return i;
			}
		}
		return -1;
	}

	//--------------------------------------------------------------
	/// Fades out the audio source with the provided name then stops it.
	//--------------------------------------------------------------
	public void fadeClipOut(string _clipName)
	{
		foreach(AudioSource source in audioSources)
		{
			if (source.name == _clipName)
			{
				source.gameObject.AddComponent<FadeAudioOut>();
				return;
			}
		}
	}

	//--------------------------------------------------------------
	/// Stops the audio source with the provided name.
	//--------------------------------------------------------------
	public void stopClip(string _clipName)
	{
		foreach(AudioSource source in audioSources)
		{
			if (source.name == _clipName)
			{
				source.Stop();
				return;
			}
		}
	}

	//--------------------------------------------------------------
	/// Stops all of the audio sources, except the one with the provided name.
	//--------------------------------------------------------------
	public void stopAllClips(string _exception = "")
	{
		foreach(AudioSource source in audioSources)
		{
			if (source.name != _exception)
				source.Stop();
		}
	}
}
