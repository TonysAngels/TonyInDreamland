using UnityEngine;
using System.Collections;

public abstract class ParentEnemy : MonoBehaviour {

	//int health;
	Transform player;

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.GetComponent<PlatformerCharacter2D>()) {
			collision.gameObject.SendMessage("attacked", true);
		}
		if (collision.gameObject.tag == "Player") {
			Debug.Log ("Player is hit!!!");

			player = collision.transform;
			if (player.position.x < this.gameObject.transform.position.x) {
				// Send message to player enemy hit method
				player.gameObject.GetComponent<PlatformerCharacter2D>().SendMessage("onEnemyHit", "1");
			//	collision.gameObject.SendMessage("onEnemyHit", "1");
			} else if (player.position.x > this.gameObject.transform.position.x) {
				player.gameObject.GetComponent<PlatformerCharacter2D>().SendMessage("onEnemyHit", "-1");
			}
		}
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
