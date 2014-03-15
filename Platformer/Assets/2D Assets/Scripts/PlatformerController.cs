using UnityEngine;
using System.Collections;

/** Attach to Player object; Character Controller component **/

// Requires a character controller component to be attached to the same game object
[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("2D Platformer/Platformer Controller")]
public class PlatformerController : MonoBehaviour
{
    //Movement control bools
    //Meaning the player themselves can move the player in that direction as opposed to aim or have the game move the player
    //E.g. On the ground, the player can jump upwards but cannot move upwards | On the wall, the player can aim right but not move right
    bool canControl;
    bool canMoveH; 
    bool canMoveV;
    float hInput = 0.0f;
    float vInput = 0.0f;
    //Prefab References - set in Inspector menu
    public Transform spawnPoint;

    enum hitTypes { None, Left, Up, Right }
    int hitPos = (int) hitTypes.None;
    float characterWidth = 1f;  //##TODO: Width/Height should be set by the model width and height once we have one
    float characterHeight = 1f;

	Vector3 tempPos;
	
	

	// Public audioclip variables

	
	

    public class PlatformerControllerMovement
    {
        public float runSpeed = 13.0f;
        public float inAirControlAcceleration = 5.0f;
        public float gravity = 60f; //##NOT DEFINED YET? Physics.gravity.y; //The gravity for the character
        public float maxRunSpeed = 13.0f;
        public float maxFallSpeed = 20.0f;
        public float speedSmoothing = 10.0f; //Friction coefficient - higher means slows down faster
        public Vector3 direction = Vector3.zero; //Curent Move Vector
        public float verticalSpeed = 0.0f; // The current vertical speed
        public float horizontalSpeed = 0.0f; // The current movement speed.  This gets smoothed by speedSmoothing.
        public Vector3 velocity = Vector3.zero;  // Approx of vert+hor speeds
        public Vector3 inAirVelocity = Vector3.zero;
        public bool isMovingH = false;
        public bool isMovingV = false;
        public CollisionFlags collisionFlags; // The last collision flags returned from controller.Move
        public float hangTime = 0.0f; // This will keep track of how long we have we been in the air (not grounded)
        //Climbing
        public bool isClimbing = false;
        public bool canClimb = false;  //Collision-climb bool
        public float climbSpeed = 15f;
    }

    public class PlatformerControllerJumping
    {
        public bool canJump = true;
        public bool hasPressed = false;
        public float jumps = 2f; //number of jumps left
        public float height = 5.5f;  // How high do we jump when pressing jump and letting go immediately
        //public float extraHeight = 4.5; // We add extraHeight units (meters) on top when holding the button down longer while jumping
        public float repeatTime = 0.02f; // Prevent spamming of jump
        public float timeout = 0.15f; //?
        public bool isJumping = false;// Are we jumping? (Initiated with jump button and not grounded yet)
        //public bool reachedApex = false;
        public float lastButtonTime = -10.0f; // Last time the jump button was clicked down
        public float lastTime = -1.0f; // Last time we performed a jump
        //public float lastStartHeight = 0.0f;  // The height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
    }

 

    public PlatformerControllerMovement movement;
    public PlatformerControllerJumping jump;
    CharacterController controller;

    public void Start()
    {
        //Instantiate Player Attributes
        movement = new PlatformerControllerMovement();
        jump = new PlatformerControllerJumping();
        controller = new CharacterController();
        controller = GetComponent<CharacterController>();
        movement.direction = transform.TransformDirection(Vector3.forward);
        
        //Establish control on the ground
        canControl = true;
        canMoveV = false;
        canMoveH = true;



        //Lastly, spawn
        Spawn();
    }

    public void Spawn()
    {
        // Reset the character's speed
        movement.verticalSpeed = 0.0f;
        movement.horizontalSpeed = 0.0f;
        // Reset the character's position to the spawnPoint
        transform.position = spawnPoint.position;
    }

    public void OnDeath()
    {
        //##Play Sound/Animation; #Restart level moreso than respawn...
		// Respawn Audio
	
        Spawn();
    }
    //**********************************************************************************//
    //Used for physics updates, prevents player from leaving Z-plane
    public void FixedUpdate()
    {
        // Make sure we are always in the 2D plane.
        Vector3 temp = transform.position;
        temp.z = 0f;
        this.transform.position = temp;
    }

    void Update()
    {
       
        Debug.Log("> " + canMoveH + "," + canMoveV);

        checkMovement();
        handleAction();
        handleMovement();
        checkPostMove();

      
    }
    
    public void checkMovement()
    {
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        //If no access, ignore axis input
        if (!canControl)
        {
            hInput = 0.0f;
            vInput = 0.0f;
            canMoveH = false;
            canMoveV = false;
        }

        //Determine if moving with controls
        movement.isMovingH = Mathf.Abs(hInput) > 0.1f;
        movement.isMovingV = Mathf.Abs(vInput) > 0.1f;

        //Set direction (note this ignores the movement locks -- that is handled in the handleMovement() func)
        movement.direction = new Vector3(hInput, -vInput, 0f);
    }

    public void handleAction()
    {
        handleJump();
    }

    private void handleJump()
    {
        //Read input
        if (Input.GetKeyDown("x"))
        {
            jump.hasPressed = true;
            jump.lastButtonTime = Time.time;
			
        }
    }

    
    public void handleMovement()
    {
        // Apply jumping logic
        ApplyJumping();

        // Horizontal controls
        if (canMoveH)
        {
            // Determine smooth speed based on current dT
            float curSmooth = movement.speedSmoothing * Time.deltaTime;

            // Determine target speed on 
            float targetSpeed = Mathf.Min(Mathf.Abs(hInput), 1.0f);

            //Movement
            if (canControl)
                targetSpeed *= movement.runSpeed;

            //Limit max run speed
            if (movement.horizontalSpeed > movement.maxRunSpeed)
                movement.horizontalSpeed = movement.maxRunSpeed;
            
            //Smooth horizontal velocity
            movement.horizontalSpeed = Mathf.Lerp(movement.horizontalSpeed, targetSpeed, curSmooth);

            //Set hang time to zero
            movement.hangTime = 0.0f;
        }

        //Calculate vertical speed
        ApplyGravity();
        
        //Char move
        // Save lastPosition for velocity calculation.
        Vector3 lastPosition = transform.position;

        // Calculate actual motion by adding horizontal and vertical velocity
     
        /** The logic below is as follows:
         * If the player is/was grounded or on the ceiling, they can only move horizontally.  
         *   The vertical direction is multiplied by 1 because the player can jump and gravity must act on the player
         *   The ceiling has an added +8 movement speed b/c the system constantly collides the two
         * 
         * If the player is attached to the wall, they can only move vertically.
         *   The horizontal direction is multiplied by 0 because we want the player to "stop" when they collide with the wall
         * 
         * Note: This still allows the player to "aim" in the respective directions, it just doesn't affect movement
         **/
        Vector3 currentMovementOffset = new Vector3(0f, 0f, 0f);
        if (hitPos == (int)hitTypes.None) //canMoveH && !canMoveV && 
        {
            //##Some reason, jumping off re-collides and resets these values
            canMoveH = true;
            canMoveV = false;
            currentMovementOffset = new Vector3((movement.direction.x * movement.horizontalSpeed), (1 * movement.verticalSpeed), 0f);
        }
        else if (hitPos == (int)hitTypes.Up) //canMoveH && !canMoveV
        {
            currentMovementOffset = new Vector3((movement.direction.x * (movement.horizontalSpeed+8)), (0 * movement.verticalSpeed), 0f);
        }
        else if (hitPos == (int)hitTypes.Left || hitPos == (int)hitTypes.Right) //!canMoveH && canMoveV
        {
            currentMovementOffset = new Vector3((1 * movement.horizontalSpeed), (movement.direction.y * movement.verticalSpeed), 0f);
        }else
        {
            currentMovementOffset = new Vector3((movement.horizontalSpeed), (movement.verticalSpeed), 0f);
        }

        // We always want the movement to be framerate independent.  Multiplying by Time.deltaTime does this.
        currentMovementOffset *= Time.deltaTime;

        // Move our character!
        movement.collisionFlags = controller.Move(currentMovementOffset);

        // Calculate the velocity based on the current and previous position.  
        // This means our velocity will only be the amount the character actually moved as a result of collisions.
        movement.velocity = (transform.position - lastPosition) / Time.deltaTime;
    }

    private void ApplyGravity()
    {
       
        movement.verticalSpeed -= movement.gravity * Time.deltaTime;

        movement.verticalSpeed = Mathf.Max(movement.verticalSpeed, -movement.maxFallSpeed);

    }

    public void ApplyJumping()
    {
        // Drops out if no button press 
        if (!jump.hasPressed)
        {
            return;
        }

        //If jump pressed, disengage lock
        if (movement.canClimb)
        {
            releaseClimbLock();
            movement.canClimb = false; //###Will be set in onCollisionExit which handles cases in which player jumps AND when they run off edge
            hitPos = (int)hitTypes.None;

            //Give some quick speed to get off wall
            movement.direction = new Vector3(1f, 1f, 0f);
            movement.horizontalSpeed = movement.runSpeed;
            movement.verticalSpeed += 20f;
        }
        //Drop out if jumping too soon - excluding from detaching from wall
        else if (jump.lastTime + jump.repeatTime > Time.time)
        {
            return;
        }

        if (jump.jumps > 0)
        {
            // Jump
            // - Only when pressing the button down
            // - You can jump
            // - With a timeout so you can press the button slightly before landing		
            if (jump.hasPressed && jump.canJump && Time.time < jump.lastButtonTime + jump.timeout)
            {
                movement.verticalSpeed = CalculateJumpVerticalSpeed(jump.height);
                SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
				//Jumping Audio
			
                jump.jumps--; 
            }
        }

        //Reset the button press
        jump.hasPressed = false;

    }

    private void DidJump()
    {
        jump.isJumping = true;
        //jump.reachedApex = false;
        jump.lastTime = Time.time;
        //jump.lastStartHeight = transform.position.y;
        jump.lastButtonTime = -10;
    }

    private float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * targetJumpHeight * movement.gravity);
    }


    public void checkPostMove()
    {

        //If floating
        if (!controller.isGrounded)
        {
            if (jump.jumps == 2)
            {
                jump.jumps = 1;
            }
        }
        else
        {
            jump.jumps = 2;
            movement.inAirVelocity = Vector3.zero;
            if (jump.isJumping)
            {
                jump.isJumping = false;
                SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);

                Vector3 jumpMoveDirection = movement.direction * movement.horizontalSpeed + movement.inAirVelocity;
                if (jumpMoveDirection.sqrMagnitude > 0.01)
                    movement.direction = jumpMoveDirection.normalized;
            }
        }

        


    }

    void releaseClimbLock()
    {
        //Release lock
        if (!canMoveH && canMoveV)
        {
            canMoveH = true;
            canMoveV = false;
        }

        if (canMoveH && !canMoveV)
        {
            
            canMoveH = false;
            canMoveV = true;
        }
    }

    
    bool OnControllColliderExit()
    {
        if(hitPos == (int) hitTypes.Left){
            if (Physics.Linecast(transform.position, transform.position - new Vector3(characterWidth, 0, 0)))
            {
                return false;
            }
            else
            {
                movement.canClimb = false;
                hitPos = (int)hitTypes.None; //Reset hit position
                releaseClimbLock();
                return true;
            }
        }else if(hitPos == (int) hitTypes.Right){
            if (Physics.Linecast(transform.position, transform.position + new Vector3(characterWidth, 0, 0)))
            {
                return false;
            }
            else
            {
                movement.canClimb = false;
                hitPos = (int) hitTypes.None;
                return true;
            }
        }else if(hitPos == (int) hitTypes.Up){
            if (Physics.Linecast(transform.position, transform.position + new Vector3(0, characterHeight*10, 0)))
            {
                return false;
            }
            else
            {
                movement.canClimb = false;
                hitPos = (int) hitTypes.None;
                return true;
            }
        }
        return false;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Trigger climbing lock
        if (hit.gameObject.tag == "Climbable")
        {
            //Hit from the side
            if (hit.moveDirection.x != 0)
            {
                //Set hit position for onExit casting
                if (hit.point.x < transform.position.x)
                {
                    hitPos = (int) hitTypes.Left; //hit left side
                }
                else
                {
                    hitPos = (int)hitTypes.Right; //hit right side
                }

                canMoveH = false;
                canMoveV = true;
                movement.canClimb = true;
                movement.horizontalSpeed = 0;
            }
            //Hit from above
            else if(hit.moveDirection.y == 1)
            {
                hitPos = (int)hitTypes.Up; //hit top side
                canMoveV = false;
                canMoveH = true;
                movement.canClimb = true;
                
            }
            //Else hit from bottom or in z dir
        }
        //End if Climable
		
		if (hit.gameObject.tag == "Enemy") {
			OnDeath();
		}
    }
	
}