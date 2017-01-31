using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

	// Use this for initialization
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 jumpDirection;
    float verticalgrav; 
	void Start () {
		
	}
	
	// Update is called once per frame
    void Update()
    {
        CharacterController control = GetComponent<CharacterController>();

            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (control.isGrounded)
            {
                verticalgrav = -gravity * Time.deltaTime;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    verticalgrav = jumpSpeed;
                }
            }
            else
            {
                verticalgrav -= gravity * Time.deltaTime; 
            }
        moveDirection.y = verticalgrav;
        control.Move(moveDirection * Time.deltaTime); 
        //moveDirection.y -= gravity * Time.deltaTime;
      
        if (transform.position.y < -10)
        {
            transform.position = new Vector3(0, 0, 0); 
        }
    }

}
