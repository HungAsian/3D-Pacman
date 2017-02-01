using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

	// Use this for initialization
    public float H = 5.0F;
    
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // Cursor lock
        Cursor.lockState = CursorLockMode.Locked;

        float h = H * Input.GetAxis("Mouse X");
        transform.Rotate(0, h, 0);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //put something to close game here
        }
	}
}
