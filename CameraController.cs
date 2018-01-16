using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public float followSpeed;
	public float xOffset = 4f;
	public float yOffset = 5f;
	public bool xIsLocked = false;
	public bool yIsLocked = false;

	private GameObject player;

	// Use this for initialization
	void Start () {
		FindPlayer ();
	}
	
	// Update is called once per frame
	void Update () {
		float xTarget = player.transform.position.x + xOffset;
		float yTarget = player.transform.position.y + yOffset;

		float xNew = player.transform.position.x;
		float yNew = player.transform.position.y;

		if(!xIsLocked)
			xNew = Mathf.Lerp(transform.position.x, xTarget, Time.deltaTime * followSpeed);
		if(!yIsLocked)
			yNew = Mathf.Lerp(transform.position.y, yTarget, Time.deltaTime * followSpeed);

		transform.position = new Vector3(xNew, yNew, transform.position.z);
	}

	void FindPlayer(){
		player = GameObject.FindGameObjectWithTag ("Player");
		if (!player)
			Debug.Log ("Camera Control could not find Player");
	}

	void onTriggerEnter(Collider col){
		Debug.Log (col);
	}
}
