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

		if (spawnState == pelletBoard.activePellets) {

			respawnPellets = true;
		

			foreach (var item in pelletArr) {
			
				if (item.GetComponent<Renderer> ().enabled == true) {
					respawnPellets = false;
					break;
				}

			}

			if (respawnPellets == true) {

				spawnState = pelletBoard.noPellets;
				//boidObj.spawn();

			}

		}

		if (spawnState == pelletBoard.noPellets) {

			//if (respawnPellets == true) {
				foreach (var item in pelletArr) {
					item.GetComponent<Renderer> ().enabled = true;
					item.GetComponent<Collider> ().enabled = true;

				}
				


			spawnState = pelletBoard.activePellets;


			boidObj.spawn();
			boidObj.spawn();


				
			//}

		}
		
	}
}
