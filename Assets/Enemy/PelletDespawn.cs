using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletDespawn : MonoBehaviour {

    public int lifetime = 600;
    Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        lifetime--;

        if (lifetime == 0) Destroy(gameObject);
	}

    void OnCollisionEnter(Collision collider)
    {
        rb.useGravity = false;
        rb.isKinematic = true;
        transform.position = new Vector3(transform.position.x, collider.transform.position.y + 1f, transform.position.z);
    }
}
