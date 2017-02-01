using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetect : MonoBehaviour {
    
    Player player;
    
    // Status Variables
    public int Health;
    public int Energy;
	
    // Use this for initialization
	void Start () {
        player = GetComponentInParent<Player>(); 
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
            if (player.currentState == Player.PlayerState.MegaChomp) Destroy(collider.gameObject);
        }
    }
}
