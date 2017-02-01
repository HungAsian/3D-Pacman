using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetect : MonoBehaviour {
    Movement player;
	// Use this for initialization
	void Start () {
        player = GetComponentInParent<Movement>(); 
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collider)
    {
        Debug.Log("Colliding");
        if (collider.gameObject.tag == "Enemy")
        {
            Debug.Log("Colliding with Enemy");
            if (player.currentState == Movement.PlayerState.MegaChomp) Destroy(collider.gameObject);
        }
    }
}
