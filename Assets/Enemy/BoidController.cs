using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour {

    public float minVelocity = 5;
    public float maxVelocity = 20;
    public float randomness = 1;
    public int flockSize = 2;
    public GameObject prefab;
    public GameObject chasee;

    public Vector3 flockCenter;
    public Vector3 flockVelocity;

    //private GameObject[] boids;
	private List<GameObject> boids = new List<GameObject>();

	// Use this for initialization
	void Start () {
        chasee = GameObject.FindGameObjectWithTag("Player");
        //boids = new GameObject[flockSize];
        for (int i=0; i<flockSize; i++)
        {
            Vector3 position = new Vector3 (Random.Range(0f, 100f), 20f, Random.Range(0f, 100f));
            GameObject boid = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
            boid.transform.parent = transform;
            boid.transform.position = position;
            boid.GetComponent<BoidFlocking>().SetController (gameObject);
            //boids[i] = boid;
			boids.Add(boid);
	    }
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 theCenter = Vector3.zero;
        Vector3 theVelocity = Vector3.zero;

        foreach (GameObject boid in boids)
        {
            Rigidbody rb = boid.GetComponent<Rigidbody>();
            theCenter = theCenter + boid.transform.position;
            theVelocity = theVelocity + rb.velocity;
        }

        flockCenter = theCenter / (flockSize);
        flockVelocity = theVelocity / (flockSize);

        if (Input.GetMouseButtonDown(1)) spawn();
	}

    public void spawn()
    {
        Vector3 position = new Vector3(Random.Range(0f, 100f), 20f, Random.Range(0f, 100f));
        GameObject boid = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
        boid.transform.parent = transform;
        boid.transform.localPosition = position;
        boid.GetComponent<BoidFlocking>().SetController(gameObject);

        //boids[flockSize + 1] = boid;
		boids.Add(boid);
		flockSize++;
    }

}
