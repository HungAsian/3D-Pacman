using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

	// Use this for initialization
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    public float rotateSpeed;
    public Transform camera; 
    private Vector3 moveDirection = Vector3.zero;
	void Start () {
	    
	}
	
	// Update is called once per frame
    void Update()
    {
        //Vector3 camerarotate = new Vector3(0, camera.position.y, 0);
        CharacterController control = GetComponent<CharacterController>();
        if (control.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        control.Move(moveDirection * Time.deltaTime);
        if (transform.position.y < -10)
        {
            transform.position = new Vector3(0, 0, 0); 
        }
        transform.eulerAngles = new Vector3(0, camera.eulerAngles.y, 0); 
        
    }

}
