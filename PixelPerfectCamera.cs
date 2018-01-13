using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPerfectCamera : MonoBehaviour {
	
	public int pixelPerUnit;
	public int pixelPerUnitScale;

	private int verticalResolution;
	private float orthographicSize;

	void Start () {
		verticalResolution = Screen.height;
		SetOrthographicCamera ();
	}

	void Update(){
		verticalResolution = Screen.height;
		SetOrthographicCamera ();
	}

	public void SetOrthographicCamera(){
		orthographicSize = ((verticalResolution) / (pixelPerUnit * pixelPerUnitScale)) * 0.5f;
		GetComponent<Camera> ().orthographicSize = orthographicSize;
	}
}
