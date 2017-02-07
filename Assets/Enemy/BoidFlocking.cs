using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidFlocking : MonoBehaviour {
    public GameObject Projectile;
    private GameObject Player;
    private Rigidbody bulletRB;
    public float velocity = 10f;
    private Rigidbody rb;

    private GameObject Controller;
    private bool inited = false;
    private float minVelocity;
    private float maxVelocity;
    private float randomness;
    private GameObject chasee;

	// Use this for initialization
	void Start () {
        Player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        StartCoroutine("BoidSteering");
        StartCoroutine("fire");
	}
	
	// Update is called once per frame
	void Update () {

	}

    IEnumerator fire()
    {
        while (true)
        {
            Vector3 direction = Vector3.Normalize(Player.transform.position - transform.position);
            GameObject bullet = Instantiate(Projectile, transform.position + direction * 1.25f, Quaternion.identity);
            bulletRB = bullet.GetComponent<Rigidbody>();
            Vector3 trajectory = (direction * 3) + Random.insideUnitSphere;
            bulletRB.velocity = trajectory * velocity;

            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator BoidSteering()
    {
        while (true)
        {
            if (inited)
            {
                rb.velocity = rb.velocity + Calc() * Time.deltaTime;

                // enforce minimum and maximum speeds for the boids
                float speed = rb.velocity.magnitude;
                if (speed > maxVelocity)
                {
                    rb.velocity = rb.velocity.normalized * maxVelocity;
                }
                else if (speed < minVelocity)
                {
                    rb.velocity = rb.velocity.normalized * minVelocity;
                }
            }

            float waitTime = Random.Range(0.3f, 0.5f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private Vector3 Calc()
    {
        Vector3 randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);

        randomize.Normalize();
        BoidController boidController = Controller.GetComponent<BoidController>();
        Vector3 flockCenter = boidController.flockCenter;
        Vector3 flockVelocity = boidController.flockVelocity;
        Vector3 follow = new Vector3(chasee.transform.position.x, chasee.transform.position.y + 20f, chasee.transform.position.z);

        flockCenter = flockCenter - transform.position;
        flockVelocity = flockVelocity - rb.velocity;
        follow = follow - transform.position;

        return (flockCenter * .5f + flockVelocity + (follow * 3) + randomize * randomness);
    }

    public void SetController(GameObject theController)
    {
        Controller = theController;
        BoidController boidController = Controller.GetComponent<BoidController>();
        minVelocity = boidController.minVelocity;
        maxVelocity = boidController.maxVelocity;
        randomness = boidController.randomness;
        chasee = boidController.chasee;
        inited = true;
    }
}
