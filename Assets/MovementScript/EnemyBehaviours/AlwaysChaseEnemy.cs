using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysChaseEnemy : MonoBehaviour {
    public Transform player;
    public GameObject enemy; 
    public float speed = 0.02f;
	// Use this for initialization
	void Start () {
        if (Time.deltaTime != 0)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.Slerp(transform.position, player.position, speed); 
	}

}   

