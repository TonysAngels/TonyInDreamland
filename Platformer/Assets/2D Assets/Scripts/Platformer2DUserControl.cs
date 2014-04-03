using UnityEngine;


[RequireComponent(typeof(PlatformerCharacter2D))]
public class Platformer2DUserControl : MonoBehaviour 
{
	private PlatformerCharacter2D character;

	private int maxAirJumps = 1;
	private int airJumpsLeft = 0;
	public float shotSpeed;
	public GameObject shot;
	public Transform shotSpawn;
	public float fireRate;
	private float nextFire;
    public bool jump;

	void Awake()
	{
		character = GetComponent<PlatformerCharacter2D>();
	}

    void Update ()
    {

		if (character.grounded) {
			airJumpsLeft = maxAirJumps;
		}
		if (Input.GetKey (KeyCode.F) && Time.time > nextFire) {
						nextFire = Time.time + fireRate;
					
						if (character.facingRight) {
								GameObject shotInstance = Instantiate (shot, shotSpawn.position, Quaternion.Euler (new Vector3 (0, 0, 0)))  as GameObject;
								shotInstance.rigidbody2D.velocity = new Vector2 (shotSpeed, 0);
						} 
						else{
								GameObject shotInstance = Instantiate (shot, shotSpawn.position, Quaternion.Euler (new Vector3 (0, 0, 180f))) as GameObject;
								shotInstance.rigidbody2D.velocity = new Vector2 (-shotSpeed, 0);
						}
		}
		if (Input.GetButtonDown("Jump")) {
			if (character.grounded) {
				jump = true;
			} else if (airJumpsLeft > 0) {
				airJumpsLeft--;
				jump = true;
			}

		}


    }

	void FixedUpdate()
	{

		bool crouch = Input.GetKey(KeyCode.LeftControl);
		//	#if CROSS_PLATFORM_INPUT
		//	float h = CrossPlatformInput.GetAxis("Horizontal");
		//	#else
		float h = Input.GetAxis("Horizontal");
		//	#endif
		
		// Pass all parameters to the character control script.
		character.Move( h, crouch, jump);
		jump = false;
	}
}
