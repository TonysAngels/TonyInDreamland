using UnityEngine;
using System.Collections;

public class Goon : ParentEnemy {

	private int health = 1;
	public float xSpeed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.Translate(-xSpeed, 0, 0);

		if (health <= 0) {
			Destroy(gameObject);
		}
	}
}
