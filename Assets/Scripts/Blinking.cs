using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class Blinking : MonoBehaviour {

	SkinnedMeshRenderer skin;
	Mesh mesh;
	bool morphing = false;

	int eyesClosedIndex = 9;
	int eyesOpenIndex = 10;

	float eyesClosedStart = 0f;
	float eyesClosedEnd = 100f;

	float eyesOpenStart = 80f;
	float eyesOpenEnd = 0f;

	float blinkDuration = 0.15f;

	// Use this for initialization
	void Start () 
	{
		skin = gameObject.GetComponent<SkinnedMeshRenderer>();
		mesh = skin.sharedMesh;

		float blinkTimer = Random.Range(3,11);
		InvokeRepeating("CloseEyes",0f,blinkTimer);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.B)){
			CloseEyes();
		}
	}

	void CloseEyes()
	{
		//Debug.Log ("Close Eyes");
		skin.SetBlendShapeWeight(eyesClosedIndex,eyesClosedEnd);
		skin.SetBlendShapeWeight(eyesOpenIndex,eyesOpenEnd);
		Invoke("OpenEyes",blinkDuration);
	}

	void OpenEyes()
	{
		//Debug.Log ("OpenedEyes");
		skin.SetBlendShapeWeight(eyesClosedIndex,eyesClosedStart);
		skin.SetBlendShapeWeight(eyesOpenIndex,eyesOpenStart);
	}
}
