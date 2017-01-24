using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

	// Use this for initialization
    public float H = 2.0F;
    
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float h = H * Input.GetAxis("Mouse X");
        transform.Rotate(0, h, 0); 
	}
}
