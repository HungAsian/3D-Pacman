using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour {

    private GameObject[] flock;
    private Rigidbody[] flockrb;
    public Vector3 center;
    public Vector3 velocity;

	// Use this for initialization
	void Start () {
        flock = GetComponentsInChildren<GameObject>();
        flockrb = GetComponentsInChildren<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 currentCenter = Vector3.zero;
        Vector3 currentVelocity = Vector3.zero;
        foreach (GameObject boid in flock)
        {
            currentCenter += boid.transform.position;
        }
        foreach (Rigidbody boidrb in flockrb)
        {
            currentVelocity += boidrb.velocity;
        }

        velocity = currentVelocity;
        center = currentCenter;
	}
}
