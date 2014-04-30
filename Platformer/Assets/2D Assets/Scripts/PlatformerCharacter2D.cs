using UnityEngine;
using System.Collections;
public class PlatformerCharacter2D : MonoBehaviour 
{
	public bool facingRight = true;							// For determining which way the player is currently facing.

	float maxSpeed = 8.5f;				// The fastest the player can travel in the x axis.
	[SerializeField] float jumpForce = 400f;			// Amount of force added when the player jumps.	

	[Range(0, 1)]
	[SerializeField] float crouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%

	[SerializeField] bool airControl = false;			// Whether or not a player can steer while jumping;
	[SerializeField] LayerMask whatIsGround;			// A mask determining what is ground to the character

	float momentum;
	bool isMoving;


	Transform groundCheck;								// A position marking where to check if the player is grounded.
	float groundedRadius = .2f;							// Radius of the overlap circle to determine if grounded
	public bool grounded = false;								// Whether or not the player is grounded.
	Transform ceilingCheck;								// A position marking where to check for ceilings
	float ceilingRadius = .01f;							// Radius of the overlap circle to determine if the player can stand up
	Animator anim;										// Reference to the player's animator component.

	//properties
	public int maxHealth = 3; //Hearts?
	public int health; 
	public float invincibleTime = 0.0f;
	public float invincTimer = 2f;
	
	public object[] subweapons; //TODO: Deal with subweapon objects, which is selected and what are unlocked, blah blah blah...
	



    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
		anim = GetComponent<Animator>();
		Spawn();
	}

	public void Spawn()
	{
		health = maxHealth; //Reset Health
	}

	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

		// Set the vertical animation
		anim.SetFloat("vSpeed", rigidbody2D.velocity.y);
	}

	public void onEnemyHit(int dir)
	{
		Debug.Log (health);
		if (Time.time < invincibleTime)
		{
			// Invincible to enemies
		}
		else
		{
			// Set invincibility time
			invincibleTime = Time.time + invincTimer;
			
			health--;
			if (health <= 0) {
				Spawn();
			}
			
			// Create vector for changing position
			Vector3 tempPos = this.transform.position;
			
			// Hit stun
			if (dir == -1)
			{
				tempPos.x += 3.0f; //hit left side of player and send right
			}
			else
			{
				tempPos.x -= 3.0f; //hit right side and send left
			}
			this.transform.position = tempPos;
		}
	}
	
	public void attacked(bool isAttacked)
	{
		if (isAttacked) {
			Debug.Log("Enemy has collided with player");
		}
	}
	
	void checkInvincible()
	{
		// Grant temporary immunity with timer
		if (Time.time < invincibleTime)
		{
			//TODO: Change color or something
			
			// Disable collision
			this.gameObject.collider.enabled = false;
		}
		else if (Time.time >= invincibleTime)
		{
			
			//TODO: Change color back
			
			// Enable collision
			this.gameObject.collider.enabled = true;
		}
	}

	public void Move(float move, bool crouch, bool jump)
	{

		// If crouching, check to see if the character can stand up
		if(!crouch && anim.GetBool("Crouch"))
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if( Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
				crouch = true;
		}

		// Set whether or not the character is crouching in the animator
		anim.SetBool("Crouch", crouch);

		//only control the player if grounded or airControl is turned on
		if(grounded || airControl)
		{
			// Reduce the speed if crouching by the crouchSpeed multiplier
			float maxMove = (crouch ? move * crouchSpeed : move);

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat("Speed", Mathf.Abs(maxMove));

			// Move the character
			/**
			 * 
			 *	Momentum: needs work 
			 * 
			 **/
			if (grounded) {
				rigidbody2D.AddForce(new Vector2(maxMove * Time.deltaTime * 1000f, 0));
			} else {
				rigidbody2D.AddForce(new Vector2(maxMove * Time.deltaTime * 500f, 0));
			}
			if (Mathf.Abs(rigidbody2D.velocity.x) > Mathf.Abs(maxMove * maxSpeed)) {
				rigidbody2D.velocity = new Vector2(move * maxSpeed, rigidbody2D.velocity.y);
			}

			/**
			 * Regular
			 * 
			rigidbody2D.velocity = new Vector2(move * maxSpeed, rigidbody2D.velocity.y);
			*/

			// If the input is moving the player right and the player is facing left...
			if(move > 0 && !facingRight)
				// ... flip the player.
				Flip();
			// Otherwise if the input is moving the player left and the player is facing right...
			else if(move < 0 && facingRight)
				// ... flip the player.
				Flip();
		}

        // If the player should jump...
        if (jump) {
            // Add a vertical force to the player.
            anim.SetBool("Ground", false);
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
            rigidbody2D.AddForce(new Vector2(0f, jumpForce));

        }
	}


	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
