using UnityEngine;

[RequireComponent(typeof(PlatformerCharacter2D))]
public class Platformer2DUserControl : MonoBehaviour 
{
	private PlatformerCharacter2D character;

	private int maxAirJumps = 1;
	private int airJumpsLeft = 0;

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
