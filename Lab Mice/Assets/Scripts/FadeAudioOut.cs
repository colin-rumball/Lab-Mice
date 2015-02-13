using UnityEngine;
using System.Collections;
// Need a source to fade
[RequireComponent(typeof(AudioSource))]
public class FadeAudioOut : MonoBehaviour 
{
	// Amount of time it will take to fade to 0 volume.
	private const float SECONDS_TO_FADE = 3.0f;

	//--------------------------------------------------------------
	/// Called every frame automatically. (Using fixed update to ensure an even fade)
	//--------------------------------------------------------------
	void FixedUpdate ()
	{
		// Get audio source for this object.
		AudioSource audioSource = this.GetComponent<AudioSource>();
		// Ensure the audio source exists.
		if (audioSource != null)
		{
			// If the source is still able to be heard then decrease the volume over time.
			if (audioSource.volume > 0.0f && audioSource.volume <= 1.0f )
			{
				audioSource.volume -= (Time.deltaTime/SECONDS_TO_FADE);
			}
			else
			{
				// Stop the source and remove this component when the volume has decreased.
				audioSource.Stop();
				Destroy(this);
			}
		} else
			// Remove this component if the source does not exist.
			Destroy(this);
	}
}
