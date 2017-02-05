using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyType1 : MonoBehaviour {

    public enum EnemyState
    {
        Idle,
        Firing
    }
    public GameObject Projectile;
    private GameObject Player;
    public EnemyState state;
    public int cooldown;
    private Rigidbody bulletRB;
    public int velocity = 10;

	// Use this for initialization
	void Start () {
        Player = GameObject.FindGameObjectWithTag("Player"); 
        cooldown = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Magnitude(Player.transform.position - transform.position) < 20 && cooldown == 0)
        {
            state = EnemyState.Firing;
        }
        else state = EnemyState.Idle;

        if (state == EnemyState.Firing)
        {
            Vector3 direction = Vector3.Normalize(Player.transform.position - transform.position);
            GameObject bullet = Instantiate(Projectile, transform.position + direction * 1.25f, Quaternion.identity);
            bulletRB = bullet.GetComponent<Rigidbody>();
            Vector3 trajectory = (direction * 3) + Random.insideUnitSphere;
            bulletRB.velocity = trajectory * velocity;

            cooldown = 50;
        }
        else if(cooldown > 0) cooldown--;
	}
}
