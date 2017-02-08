using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Player FSM
    public enum PlayerState
    {
        Grounded,
        Jumping,
        MegaChomp,
        MegaChompTarget
    }
    public enum HitState
    {
        Vincible,
        Invincible
    }

    // Movement Variables
    public float speed;
    public float jumpSpeed = 8.0F;
    public float gravity = 15.0F;
    public float floatingmultiplier = 0.1f;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 jumpDirection;
    float verticalgrav;
    private CharacterController control;
    public PlayerState currentState;
    public HitState hitState;
    public int invincibilityTime;
    public Transform child;
    public CollisionDetect childScript;
    public Transform respawn;
    public Renderer childRenderer;
    public bool isCrouching = false;
    // Mega Chomp Variables
    private Vector3 goalposition;
    public float MegaChompDistance = 5f;
    public int MegaChompDetectionRange = 30;

    void Start()
    {
        control = GetComponent<CharacterController>();
        control.detectCollisions = false;
        if (control.isGrounded)
        {
            currentState = PlayerState.Grounded;
        }
        else
        {
            currentState = PlayerState.Jumping;
        }
        hitState = HitState.Vincible;
        invincibilityTime = 0;
        child = transform.GetChild(0);
        childScript = GetComponent<CollisionDetect>();
        childRenderer = child.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (childScript.Energy > 50) speed = 8.0f;
        else if (childScript.Energy > 25) speed = 5.5f;
        else speed = 4.0f;
        // Gets input from player
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        // Activate Mega Chomp
        if (Input.GetMouseButtonDown(0) && currentState != PlayerState.MegaChomp && childScript.Energy > 5)
        {
            GameObject target = FindEnemyinRange();
            GameObject superTarget = FindPelletinRange();
            childScript.Energy -= 5;
            if (target)
            {
                goalposition = target.transform.position;
                currentState = PlayerState.MegaChompTarget;
            }
            else if (superTarget)
            {
                goalposition = superTarget.transform.position;
                currentState = PlayerState.MegaChompTarget;
            }
            else
            {
                goalposition = transform.position + transform.forward * MegaChompDistance;
                currentState = PlayerState.MegaChomp;
            }
        }

        //if player is not undersomething get back up
        if (isCrouching && !Input.GetKey(KeyCode.C))
        {
            if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), 1f))
            {
                child.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                control.height += 0.5f;
                isCrouching = false;
            }

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
            case PlayerState.MegaChompTarget:
                MegaChompTarget();
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
            respawn = GameObject.FindGameObjectWithTag("Respawn").transform;
            transform.position = respawn.position;
        }

        if (invincibilityTime > 0)
        {
            invincibilityTime--;
            childRenderer.enabled = !childRenderer.enabled;
            if (invincibilityTime == 0)
            {
                hitState = HitState.Vincible;
                childRenderer.enabled = true;
            }
        }

        // Look at pellets
        Collider[] localColliders = Physics.OverlapSphere(transform.position, 2f);
        Transform lookat = null;
        foreach (Collider CollidingObject in localColliders)
        {
            if (CollidingObject.tag == "Pellet") lookat = CollidingObject.transform;
        }
        if (lookat) child.transform.LookAt(lookat);
        else child.transform.rotation = transform.rotation;
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
        if (Input.GetKeyDown(KeyCode.C) && isCrouching == false)
        {
            child.position = new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z);
            control.height -= 0.5f;
            isCrouching = true;
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
        RaycastHit hit;
        if (Physics.SphereCast(child.position, .3f, transform.TransformDirection(Vector3.forward), out hit, .7f))
        {
            if (hit.rigidbody.tag == "Pellet")
            {
                GameObject pellet = hit.transform.gameObject;

                pellet.GetComponent<Renderer>().enabled = false;
                pellet.GetComponent<Collider>().enabled = false;
                childScript.Energy += 4;
            }
        }
        if (Vector3.Magnitude(transform.position - goalposition) < .5 || Physics.Raycast(child.position, transform.TransformDirection(Vector3.forward), 0.8f))
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
        else
        {
            transform.position = Vector3.Lerp(transform.position, goalposition, .2f);
            //rb.MovePosition(goalposition);
        }
    }

    void MegaChompTarget()
    {
        if (Vector3.Magnitude(transform.position - goalposition) < .5)
        {
            if (control.isGrounded)
            {
                currentState = PlayerState.Grounded;
                child.transform.rotation = transform.rotation;
            }
            else
            {
                currentState = PlayerState.Jumping;
                child.transform.rotation = transform.rotation;
            }
        }
        else
        {
            child.transform.LookAt(goalposition);
            transform.position = Vector3.Lerp(transform.position, goalposition, .2f);
            //rb.MovePosition(goalposition);
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
    GameObject FindPelletinRange()
    {
        GameObject[] pellets;
        pellets = GameObject.FindGameObjectsWithTag("Super Pellet");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in pellets)
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