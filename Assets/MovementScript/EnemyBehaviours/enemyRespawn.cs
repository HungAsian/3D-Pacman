using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyRespawn : MonoBehaviour {
    public GameObject prefab;
    public int enemyCount=4;
    public int cooldown = 100;

    float speed; 
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (Time.deltaTime != 0)
        {
            if (cooldown > 0) cooldown--;
            if (transform.childCount < enemyCount && cooldown == 0)
            {
                Spawn();
                cooldown = 100;
            }
        }
	}
    void Spawn()
    {
        GameObject enemy = Instantiate(prefab, transform.position, Quaternion.identity);
        enemy.transform.parent = transform;
        Renderer enemyRenderer = enemy.GetComponent<Renderer>();
        enemyRenderer.material.color = Random.ColorHSV();

    }
}
