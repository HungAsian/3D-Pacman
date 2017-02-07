using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyType1 : MonoBehaviour {
    public GameObject Projectile;
    private GameObject Player;
    private Rigidbody bulletRB;
    public float velocity = 10f;
    private Rigidbody rb;
    private GameObject[] flock;

	// Use this for initialization
	void Start () {
        Player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        flock = GameObject.FindGameObjectsWithTag("Enemy Type 1");
        StartCoroutine("fire");
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 boidCenter = Vector3.zero;
        Vector3 boidHeading = Vector3.zero;

        foreach(GameObject boid in flock)
        {
            Rigidbody boidRB = boid.GetComponent<Rigidbody>();
            boidCenter += boid.transform.position / flock.Length;
            boidHeading += boidRB.velocity / flock.Length;
        }

        Vector3 target = new Vector3(Player.transform.position.x, Player.transform.position.y + 20, Player.transform.position.z) - transform.position;

        rb.velocity = boidHeading + boidCenter + target * 2;

	}

    IEnumerator fire()
    {
        Vector3 direction = Vector3.Normalize(Player.transform.position - transform.position);
        GameObject bullet = Instantiate(Projectile, transform.position + direction * 1.25f, Quaternion.identity);
        bulletRB = bullet.GetComponent<Rigidbody>();
        Vector3 trajectory = (direction * 3) + Random.insideUnitSphere;
        bulletRB.velocity = trajectory * velocity;

        yield return new WaitForSeconds(2f);
    }
}
