﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatEnemy : MonoBehaviour {
    public Player character;
    public CollisionDetect stamina; 
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (Time.deltaTime != 0)
        {
            if (stamina.Energy <= 0)
            { 
                transform.position = Vector3.Lerp(transform.position, new Vector3(character.transform.position.x, character.transform.position.y - 10, character.transform.position.z), 0.01f);
                transform.LookAt(-(character.transform.position - transform.position));
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(150.0f, 100.0f, 70.0f), 0.05f);
            }
        }
    }
}
