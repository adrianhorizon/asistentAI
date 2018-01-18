using UnityEngine;
using System.Collections;

public class PlayMovie : MonoBehaviour 
{
	AudioSource videoAudio; 


	void Start()
	{
		videoAudio = gameObject.GetComponent<AudioSource>();
		PlayVideo();
	}

	void PlayVideo() 
	{
		//if (Input.GetButtonDown ("Jump")) {
		Renderer r = GetComponent<Renderer>();
		MovieTexture movie = (MovieTexture)r.material.mainTexture;
		
		if (movie.isPlaying) 
		{
			movie.Pause();
			videoAudio.Pause();
		}
		else 
		{
			movie.Play();
			videoAudio.Play();
		}
	}
	void FadeInSelf()
	{

	}
	void FadeToBalckSelf()
	{

	}
}
