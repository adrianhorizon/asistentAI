﻿using UnityEngine;
using System.Collections;

public class PlayAudio : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Play(){

		gameObject.GetComponent<AudioSource>().Play();
	}

	public void Stop(){
		
		gameObject.GetComponent<AudioSource>().Stop();
	}
}
