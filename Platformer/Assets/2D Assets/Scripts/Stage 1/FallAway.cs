using UnityEngine;
using System.Collections;

public class FallAway : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.collider2D.isTrigger = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (TreeFallen.fallen) {
			this.collider2D.isTrigger = true;
		}
	}
}
