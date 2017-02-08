using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletManagerScript : MonoBehaviour {

	private GameObject[] pelletArr;

	private bool respawnPellets = false;

	public BoidController boidObj;



	public enum pelletBoard
	{
		activePellets,
		noPellets
	}

	public pelletBoard spawnState;


	// Use this for initialization
	void Start () {

		pelletArr = GameObject.FindGameObjectsWithTag ("Pellet");

	}
	
	// Update is called once per frame
	void Update () {
        if (Time.deltaTime != 0)
        {
            if (spawnState == pelletBoard.activePellets)
            {

                respawnPellets = true;


                foreach (var item in pelletArr)
                {

                    if (item.GetComponent<Renderer>().enabled == true)
                    {
                        respawnPellets = false;
                        break;
                    }

                }

<<<<<<< HEAD
				spawnState = pelletBoard.noPellets;
				//boidObj.spawn();

			}
=======
                if (respawnPellets == true)
                {
>>>>>>> origin/master

                    spawnState = pelletBoard.noPellets;
                }

            }

            if (spawnState == pelletBoard.noPellets)
            {

<<<<<<< HEAD
				}
				

=======
                //if (respawnPellets == true) {
                foreach (var item in pelletArr)
                {
                    item.GetComponent<Renderer>().enabled = true;
                    item.GetComponent<Collider>().enabled = true;
>>>>>>> origin/master

                }
			spawnState = pelletBoard.activePellets;


			boidObj.spawn();
			boidObj.spawn();


				
			//}

                spawnState = pelletBoard.activePellets;

                //}

            }
        }
	}
}
