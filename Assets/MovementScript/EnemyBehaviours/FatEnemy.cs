using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatEnemy : MonoBehaviour {
    public GameObject character;
    public CollisionDetect stamina; 
	// Use this for initialization
	void Start () {
        stamina = character.GetComponent<CollisionDetect>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.deltaTime != 0)
        {
            if (stamina.Energy <= 0)
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(character.transform.position.x, character.transform.position.y - 10, character.transform.position.z), 0.01f);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(150.0f, 100.0f, 70.0f), 0.05f);
            }
        }
    }

    void OnCollisionStay(Collision collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            CollisionDetect script = collider.gameObject.GetComponent<CollisionDetect>();
            script.Health = 0;
        }
    }
}
