using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YMovement : MonoBehaviour {
    public float V = 20.0F;
    public Transform player;
    public float minimumheight = .5f;
    public float maximumheight = 10f;
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
        float v = V * Input.GetAxis("Mouse Y");
        if (player.position.y - minimumheight < transform.position.y && v < 0 || player.position.y + maximumheight > transform.position.y && v > 0)
        {
            transform.Translate(0, v * Time.deltaTime, 0);
            transform.LookAt(player);
        }
	}
}
