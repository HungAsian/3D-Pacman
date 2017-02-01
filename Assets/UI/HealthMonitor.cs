using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthMonitor : MonoBehaviour {

    CollisionDetect player;

    public int Health;
    public int Energy;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<CollisionDetect>();
        Health = player.Health;
        Energy = player.Energy;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
