using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YMovement : MonoBehaviour {
    public float V = 10.0F;
    public Transform player;
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
        float v = V * Input.GetAxis("Mouse Y");
        if (transform.position.y > .5 && v < 0 || transform.position.y < 8.5 && v > 0)
        {
            transform.Translate(0, v * Time.deltaTime, 0);
            transform.LookAt(player);
        }
	}
}
