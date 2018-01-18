using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class SpeechControllerOVRLP : MonoBehaviour {

	public AudioClip[] Dialogs;
	public static string searchedDialog;
	public AudioSource source;
	//Potential Change from Diego
	private static SpeechControllerOVRLP instance;
	public SimpleDialogManager dialogManager;

	// Use this for initialization
	void Start () {
		instance = this;
		source = gameObject.GetComponent<AudioSource>();
	}

	public void Speak(string dialogName){
		source.Stop();
		searchedDialog = dialogName;
		AudioClip newDialog = Array.Find(Dialogs, SameName);
		if(newDialog!=null){
			source.clip = newDialog;
			//first inhale and wait for the exhale to play recording and animation
			source.Play();
			Debug.Log("Dialog: " +dialogName+ " with length: " + source.clip.length);
			Invoke("StopAnimation", source.clip.length);
		}
		else{
			Debug.Log("Dialog audio not found for: " + dialogName);
		}
	}

	float getDialogLength(string dialogName){
		AudioClip newDialog = Array.Find(Dialogs, SameName);
		if(newDialog!=null){
			source.clip = newDialog;
			return source.clip.length;
		}
		else{
			Debug.Log("Dialog audio not found for: " + dialogName);
			return 0.0f;
		}	
	}


    // Actually stops lipsync, not running animations. Change name.
	void StopAnimation(){
		dialogManager.isSpeaking = false;
	}

	// Weird method to find an element in the array. Comment with a reference of how it works.
	private static bool SameName(AudioClip dialog)
    {
		return dialog.name == searchedDialog;
	}
}
