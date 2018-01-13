using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour {

	private LevelManager levelManager;

	void Start(){
		levelManager = GameObject.FindGameObjectWithTag ("LevelManager").GetComponent<LevelManager> ();
	}

	void OnCollisionEnter2D(Collision2D col){
		if (col.gameObject.tag == "Player") {
			levelManager.ResetLevel ();
		}
	}
}
