using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyRespawn : MonoBehaviour {
    public GameObject prefab;
    public int enemyCount=4;
    public Transform enemy; 

    float speed; 
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (Time.deltaTime != 0)
        {
            if (transform.childCount < enemyCount)
            {
                Spawn();
            }
        }
	}
    void Spawn()
    {
        GameObject enemy = Instantiate(prefab, transform.position, transform.rotation);
        enemy.transform.parent = transform; 

    }
}
