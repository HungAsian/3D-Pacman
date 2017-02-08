using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    public Text counter;
    public float seconds, minutes;

	// Use this for initialization
	void Start () {
        //counter = GetComponent<Text>() as Text;	
	}
	
	// Update is called once per frame
	void Update () {
		minutes = (int)(Time. time/60f);
        seconds = (int)(Time.time % 60f);
        counter.text = minutes.ToString("00") + ":" + seconds.ToString("00");
	}
}
