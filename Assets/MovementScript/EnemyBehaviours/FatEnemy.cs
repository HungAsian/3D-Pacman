using System.Collections;
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
        if (character.speed < 3.0f)
        {
            transform.LookAt(character.transform.position);
            transform.position = Vector3.Lerp(transform.position, character.transform.position, 0.01f);
        }
        else
        {
            transform.position = new Vector3(150.0f, 100.0f, 70.0f); 
        }
	}
}
