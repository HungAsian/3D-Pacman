using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
public class CollisionDetect : MonoBehaviour {
    
    public Player player;
    Slider HealthSlider;
    Slider EnergySlider;
    public GameObject gameOverCanvas;
    // Status Variables
    public int Health;
    public int Energy;
    public int EnergyDrainTime;
    public int ENDrain;
    // Use this for initialization
	void Start () {
        player = GetComponent<Player>();
        HealthSlider = GameObject.FindGameObjectWithTag("HP").GetComponent<Slider>();
        EnergySlider = GameObject.FindGameObjectWithTag("Energy").GetComponent<Slider>();
        Health = 100;
        Energy = 100;
        EnergyDrainTime = 100;
        ENDrain = 100;
	}
	
	// Update is called once per frame
    void Update()
    {
        if (Time.deltaTime != 0)
        {
            if (Health > 100) Health = 100;
            if (Energy > 100) Energy = 100;

            HealthSlider.value = Health;
            EnergySlider.value = Energy;
            if(Energy < 0) ENDrain -= 1;
            if (ENDrain == 0)
            {
                Energy -= 1;
                ENDrain = EnergyDrainTime;
            }
            if (Energy > 0 && player.hitState == Player.HitState.Vincible) ENDrain -= 1;
            if (ENDrain == 0)
            {
                Energy -= 1;
                ENDrain = EnergyDrainTime;
            }
        }
        if (Health <= 0)
        {
            Debug.Log("ded");
            gameOverCanvas.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnControllerColliderHit(ControllerColliderHit collider)
    {
        if (collider.gameObject.tag == "Enemy")
        {
            if (player.currentState == Player.PlayerState.MegaChompTarget || player.hitState == Player.HitState.Invincible)
            {
                Destroy(collider.gameObject);
            }
            else
            {
                Vector3 recoil =  Vector3.Normalize(collider.transform.position - transform.position);
                recoil = -recoil * 4;
                if (recoil.y < 0) recoil.y = transform.position.y;
                transform.position += recoil;
                player.hitState = Player.HitState.Invincible;
                player.invincibilityTime = 50;
                Health -= 10;
            }
        }
        if (collider.gameObject.tag == "Pellet")
        {
            //Destroy(collider.gameObject);
			collider.gameObject.GetComponent<Renderer>().enabled = false;
			collider.gameObject.GetComponent<Collider>().enabled = false;
            Energy += 4;
        }
        if (collider.gameObject.tag == "Super Pellet")
        {
            if (player.currentState == Player.PlayerState.MegaChompTarget)
            {
                Destroy(collider.gameObject);
                player.hitState = Player.HitState.Invincible;
                player.invincibilityTime = 500;
            }
        }
        if (collider.gameObject.tag == "Big Pellet")
        {
            collider.gameObject.GetComponent<Renderer>().enabled = false;
            collider.gameObject.GetComponent<Collider>().enabled = false;
            Energy = 100;
        }
        if (collider.gameObject.tag == "Bad Pellet")
        {
            Destroy(collider.gameObject);
            if ((player.currentState != Player.PlayerState.MegaChompTarget || player.currentState != Player.PlayerState.MegaChomp) && player.hitState != Player.HitState.Invincible)
            {
                Health -= 5;
                player.hitState = Player.HitState.Invincible;
                player.invincibilityTime = 50;
            }
        }
        if (collider.gameObject.tag == "FatEnemy")
        {
            Health = 0; 
        }
    }
}