using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

	// Use this for initialization
    public float H = 2.0F;
    public Transform player;
    public Vector3 movement; 
	void Start () {
        movement = new Vector3(player.position.x, player.position.y + 8.0f, player.position.z - 8.0f); 
	}
	
	// Update is called once per frame
	void Update () {
        float h = H * Input.GetAxis("Mouse X");
        movement = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * H, Vector3.up) * movement;
        transform.position = player.position + movement;
        transform.LookAt(player.position);
        
	}
}