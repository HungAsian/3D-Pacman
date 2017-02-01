using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionDetect : MonoBehaviour {
    
    Player player;
    Slider HealthSlider;
    Slider EnergySlider;
    
    // Status Variables
    public int Health;
    public int Energy;
	
    // Use this for initialization
	void Start () {
        player = GetComponentInParent<Player>();
        HealthSlider = GameObject.FindGameObjectWithTag("HP").GetComponent<Slider>();
        EnergySlider = GameObject.FindGameObjectWithTag("Energy").GetComponent<Slider>();
        Health = 100;
        Energy = 100;
	}
	
	// Update is called once per frame
	void Update () {
        HealthSlider.value = Health;
        EnergySlider.value = Energy;
	}

    void OnCollisionEnter(Collision collider)
    {
        if (collider.gameObject.tag == "Enemy")
        {
            if (player.currentState == Player.PlayerState.MegaChomp) Destroy(collider.gameObject);
            else Health -= 10;
        }
    }
}
