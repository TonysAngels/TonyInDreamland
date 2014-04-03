using UnityEngine;
using System.Collections;

public class TreeFallen : MonoBehaviour {

	public static bool fallen = false;
	// Use this for initialization
	void Start () {
		this.collider2D.isTrigger = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (this.collider2D.isTrigger == false) { // TODO: Replace this if parameter with checking if the "button was pressed" to make the tree fall
			//this.collider2D.isTrigger = false; //TODO: Uncomment when button logic is present
			fallen = true;
		}
	}
}
