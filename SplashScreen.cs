using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour {

	public float splashTime = 15f;
	public float fadeTime = 1000f;
	public AudioClip splashJingle;

	public Image image;
	private float timeCounter = 0;
	private LevelManager levelManager;

	// Use this for initialization
	void Start () {
		FindLevelManager ();
		if (!image)
			Debug.LogError ("No Splash Image Added to SplashScreen.cs");
		image.canvasRenderer.SetAlpha (0);
		image.CrossFadeAlpha (255, fadeTime, false);
	}
	
	// Update is called once per frame
	void Update () {
		timeCounter += Time.deltaTime;
		if(timeCounter >= splashTime)
			levelManager.LoadNextLevel ();
	}

	void FindLevelManager(){
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager>();
		if (!levelManager)
			Debug.LogError ("No LevelManager Attached");
	}
}
