using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletDespawn : MonoBehaviour {

    public int lifetime = 600;
    Rigidbody rb;
    int timeGrounded;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        timeGrounded = 0;
	}
	
	// Update is called once per frame
	void Update () {
        lifetime--;

        if (lifetime == 0) Destroy(gameObject);

        if (Physics.Raycast(transform.position, Vector3.down, .5f) && timeGrounded > 5)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            transform.position = new Vector3(transform.position.x, GetComponent<Collider>().transform.position.y + 1f, transform.position.z);
        }
        else if (Physics.Raycast(transform.position, Vector3.down, .5f))
        {
            timeGrounded++;
        }
        else timeGrounded = 0;
	}

}
