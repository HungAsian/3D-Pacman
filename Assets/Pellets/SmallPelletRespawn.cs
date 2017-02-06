using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPelletRespawn : MonoBehaviour {

	public int totalRespawnTime;
	private int respawnTimer = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (GetComponent<Renderer>().enabled == false) 
		{
			respawnTimer++;
			if (respawnTimer == totalRespawnTime) {
				GetComponent<Renderer>().enabled = true;
				GetComponent<Collider>().enabled = true;
				respawnTimer = 0;
			}
		}
	}
}
