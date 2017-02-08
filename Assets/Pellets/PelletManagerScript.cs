﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletManagerScript : MonoBehaviour {

	private GameObject[] pelletArr;

	private bool respawnPellets = false;

	BoidController boidObj;



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

                if (respawnPellets == true)
                {

                    spawnState = pelletBoard.noPellets;
                }

            }

            if (spawnState == pelletBoard.noPellets)
            {

                //if (respawnPellets == true) {
                foreach (var item in pelletArr)
                {
                    item.GetComponent<Renderer>().enabled = true;
                    item.GetComponent<Collider>().enabled = true;

<<<<<<< HEAD
                }
=======
			spawnState = pelletBoard.activePellets;

			boidObj.spawn();


				
			//}
>>>>>>> af7531bd98e24a9f71c1077fe2c63ffc1345d2c3

                spawnState = pelletBoard.activePellets;

                //}

            }
        }
	}
}
