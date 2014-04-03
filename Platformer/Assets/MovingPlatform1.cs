using UnityEngine;
using System.Collections;

public class MovingPlatform1 : MonoBehaviour {

	public float xSpeed;
	public float ySpeed;
	int counter;

	int spot1=0, spot2=0, spot3=0, spot4=0, spotEnd=0;

	   
	// Use this for initialization
	void Start () {
		spot1 = 0;
		spot2 = 640;
		spot3 = 2500;
		spot4 = spot3 + spot2;
		spotEnd = spot4 + spot3;
	}
	
	// Update is called once per frame
	void Update () {
		counter++;
		if (spot1 <= counter && counter <= spot2) { 
			this.transform.Translate(0, ySpeed, 0);
		}
		if (spot2 < counter && counter <= spot3) {
			this.transform.Translate(xSpeed, 0, 0);
		}
		if (spot3 < counter && counter <= spot4) {
			this.transform.Translate(0, -ySpeed, 0);
		}
		if (spot4 < counter) {
			this.transform.Translate(-xSpeed, 0, 0);
		}
		if (counter > spotEnd) {
			counter = 0;
		}
	}
}
