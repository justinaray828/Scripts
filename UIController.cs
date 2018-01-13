using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour {

	public GameObject pauseMenuCanvas;
	private int currentSceneIndex;
	private bool inMenu = false;

	// Use this for initialization
	void Start () {
		Time.timeScale = 1;
		//pauseMenuCanvas = GameObject.Find ("PauseMenuCanvas");
		if (!pauseMenuCanvas)
			Debug.LogError ("No PauseMenuCanvasFound");
		pauseMenuCanvas.SetActive (false); //Insure that the pause menu is closed
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Cancel") && !inMenu) {
			Time.timeScale = 0;
			pauseMenuCanvas.SetActive (true);
			inMenu = true;
		} else if (Input.GetButtonDown ("Cancel")) {
			Time.timeScale = 1;
			pauseMenuCanvas.SetActive (false);
			inMenu = false;
		}
	}

}