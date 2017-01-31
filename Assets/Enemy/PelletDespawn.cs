using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletDespawn : MonoBehaviour {

    public int lifetime = 100;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        lifetime--;

        if (lifetime == 0) Destroy(gameObject);
	}
}
