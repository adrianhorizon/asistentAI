using UnityEngine;
using System.Collections;

public class PressToStart : MonoBehaviour {
	private float adjustWidth = 0.34f;
	private float adjustHeight = 0.83f;
	public Texture2D textureToDisplay;
	//public bool display = false;
	public bool started = false;

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.S))
		{
			Debug.Log("Pressed S: " + started);
			started = true;
		}
	}
	
	void OnGUI() 
	{
		if(!started)
		{
			GUI.Label(new Rect((Screen.width * adjustWidth), (Screen.height * adjustHeight), 
			                   (textureToDisplay.width/2f), (textureToDisplay.height)/2f), textureToDisplay);
		}
	}
}
