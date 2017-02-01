using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    //Player FSM
    public enum PlayerState
    {
        Grounded,
        Jumping,
        MegaChomp
    }

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 15.0F;
    public float floatingmultiplier = 0.1f;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 jumpDirection;
    float verticalgrav;
    private CharacterController control;
    public PlayerState currentState;

    // Mega Chomp Variables
    private Vector3 goalposition;
    public int MegaChompDistance = 5;
    public int MegaChompDetectionRange = 30;
    public Transform child; 

    void Start()
    {
        control = GetComponent<CharacterController>();
        if (control.isGrounded)
        {
            currentState = PlayerState.Grounded;
        }
        else
        {
            currentState = PlayerState.Jumping;
        }
        child = transform.GetChild(0); 
	}
	
	// Update is called once per frame
    void Update()
    {
        // Gets input from player
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        // Activate Mega Chomp
        if (Input.GetMouseButtonDown(0) && currentState != PlayerState.MegaChomp)
        {
            GameObject target = FindEnemyinRange();
            if (target)
            {
                goalposition = target.transform.position;
            }
            else goalposition = transform.position + transform.forward * MegaChompDistance;
            currentState = PlayerState.MegaChomp;
        }
        

        switch (currentState)
        {
            case PlayerState.Grounded:
                grounded();
                break;
            case PlayerState.Jumping:
                jumping();
                break;
            case PlayerState.MegaChomp:
                MegaChomp();
                break;
        }

        // Actual Movement Takes Place
        if (currentState != PlayerState.MegaChomp)
        {
            control.Move(moveDirection * Time.deltaTime);
        }
        //moveDirection.y -= gravity * Time.deltaTime;
      

        // Respawn
        if (transform.position.y < -10)
        {
            transform.position = new Vector3(0, 0, 0); 
        }
    }

    void grounded()
    {
        // Grounding Force
        verticalgrav = -gravity * Time.deltaTime;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            verticalgrav = jumpSpeed;
            currentState = PlayerState.Jumping;
        }

        // Falling
        if (!control.isGrounded)
            currentState = PlayerState.Jumping;

        //Crouch
        if (Input.GetKeyDown(KeyCode.C))
        {
            child.position = new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z); 
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            child.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        }
        moveDirection.y = verticalgrav;

    }

    void jumping()
    {

        // Falling Gravity
        verticalgrav -= gravity * Time.deltaTime;
        moveDirection.y = verticalgrav;

        // "Float"
        if (Input.GetKey(KeyCode.Space) && moveDirection.y < 0) moveDirection.y *= floatingmultiplier;

        // Transition from Jumping to Landing
        if (control.isGrounded)
        {
            currentState = PlayerState.Grounded;
        }
    }

    void MegaChomp()
    {
        transform.position = Vector3.Lerp(transform.position, goalposition, .2f);
        if (Vector3.Magnitude(transform.position - goalposition) < .5)
        {
            if (control.isGrounded)
            {
                currentState = PlayerState.Grounded;
            }
            else
            {
                currentState = PlayerState.Jumping;
            }
        }
    }

    GameObject FindEnemyinRange()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        if (distance <= MegaChompDetectionRange) return closest;
        else return null;
    }

    
}
