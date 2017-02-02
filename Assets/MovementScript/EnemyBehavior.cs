using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour {
    Vector3 target;
    Vector3 steering;
    Vector3 velocity;
    Vector3 desired;
    public Transform player; 
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
    }
	
	// Update is called once per frame
	void Update () {
        target = player.transform.position;
        Debug.Log(Vector3.Distance(transform.position, target)); 
        if (Vector3.Distance(transform.position, target) < 10)
        {
            transform.position = Vector3.Lerp(transform.position, player.position, 0.01f);  
        }
	}

}
