using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysChaseEnemy : MonoBehaviour {
    public Transform player;
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
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, .4f, transform.TransformDirection(Vector3.forward), out hit, 2f))
        {
            if (hit.transform.tag != "Player")
            {
                transform.position = Vector3.Lerp(transform.position, transform.parent.position, speed);
            }
            transform.position = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x, .5f, player.transform.position.z), speed); 
        }
        else transform.position = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x, .5f, player.transform.position.z), speed);
        transform.rotation = Quaternion.identity;
	}

}   

