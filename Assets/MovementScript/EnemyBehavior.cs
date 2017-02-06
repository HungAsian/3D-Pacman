using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour {
    Vector3 target;
    Vector3 steering;
    Vector3 velocity;
    Vector3 desired;
    public Transform player;
    public Transform pellet;
    public enum EnemyState
    {
        chase,
        Return,
        orbit 
    }
    public EnemyState currentState;
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pellet = FindClosestPellet().transform;
    }
	
	// Update is called once per frame
    void Update()
    {
        if (pellet != null)
        {
            target = player.transform.position;
            if (Vector3.Distance(transform.position, target) < 5)
            {
                currentState = EnemyState.chase;
            }
            else if (currentState != EnemyState.orbit)
            {
                currentState = EnemyState.Return;
            }
        }
        else
        {
            currentState = EnemyState.chase;
        }
        switch (currentState)
        {
            case EnemyState.chase:
                chase();
                break;
            case EnemyState.orbit:
                Orbit();
                break;
            case EnemyState.Return:
                Return();
                break;


        }
    }
    void chase()
    {
        if (pellet != null)
        {
            if (Vector3.Distance(transform.position, target) < 5)
            {
                transform.position = Vector3.Lerp(transform.position, player.position, 0.01f);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, player.position, 0.04f);
        }
 
    }

    void Return()
    {
        if (pellet != null)
        {
            transform.position = Vector3.Lerp(transform.position, pellet.position, 0.01f);
            if (Vector3.Distance(transform.position, pellet.position) < 5)
            {
                currentState = EnemyState.orbit;
            }
        }
    }
    void Orbit()
    {
            transform.RotateAround(pellet.position, Vector3.up, 20 * Time.deltaTime);
    }


    GameObject FindClosestPellet()
    {
        GameObject[] pellets;
        pellets = GameObject.FindGameObjectsWithTag("Super Pellet");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in pellets)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }
}