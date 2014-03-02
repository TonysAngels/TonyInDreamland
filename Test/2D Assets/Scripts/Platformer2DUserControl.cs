﻿using UnityEngine;

[RequireComponent(typeof(PlatformerCharacter2D))]
public class Platformer2DUserControl : MonoBehaviour 
{
	private PlatformerCharacter2D character;
    private bool jump;
	private bool doublejump= false;

	void Awake()
	{
		character = GetComponent<PlatformerCharacter2D>();
	}

    void Update ()
    {
        // Read the jump input in Update so button presses aren't missed.

       // if (CrossPlatformInput.GetButtonDown("Jump")) jump = true;

		if (Input.GetButtonDown("Jump")) jump = true;

		if (Input.GetButtonDown ("Jump") && (jump == true && doublejump == false))
			doublejump = true;
    }

	void FixedUpdate()
	{
		// Read the inputs.
		bool crouch = Input.GetKey(KeyCode.LeftControl);
	//	#if CROSS_PLATFORM_INPUT
	//	float h = CrossPlatformInput.GetAxis("Horizontal");
	//	#else
		float h = Input.GetAxis("Horizontal");
	//	#endif

		// Pass all parameters to the character control script.
		character.Move( h, crouch , jump , doublejump);
		doublejump = false;
        // Reset the jump input once it has been used.
	    jump = false;
	}
}
