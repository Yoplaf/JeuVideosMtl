using UnityEngine;

public class PlatformerCharacter2D : MonoBehaviour 
{
	bool facingRight = true;							// For determining which way the player is currently facing.

	[SerializeField] float maxSpeed = 10f;				// The fastest the player can travel in the x axis.
	[SerializeField] float jumpForce = 800f;			// Amount of force added when the player jumps.	
    [SerializeField] float jumpForceLimit = 1600f;     //limite de force pour saut charge
    [SerializeField] int nombreSautMax = 3;
	[SerializeField] float wallForce = 800f;

	[Range(0, 1)]
	[SerializeField] float crouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	
	[SerializeField] bool airControl = false;			// Whether or not a player can steer while jumping;
	[SerializeField] LayerMask whatIsGround;			// A mask determining what is ground to the character
	[SerializeField] LayerMask whatIsWall;			
	
	Transform groundCheck;								// A position marking where to check if the player is grounded.
	float groundedRadius = .2f;							// Radius of the overlap circle to determine if grounded
	bool grounded = false;								// Whether or not the player is grounded.
	Transform ceilingCheck;								// A position marking where to check for ceilings
	float ceilingRadius = .01f;							// Radius of the overlap circle to determine if the player can stand up
	Animator anim;										// Reference to the player's animator component.
	int compteurSaut = 0;
	Transform wallCheck;
	float wallRadius = .5f;							
	bool wall = false;
	float crouchTime = 1; //pour saut charge
    


    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
		wallCheck = transform.Find("WallCheck");
		anim = GetComponent<Animator>();
	}


	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

		wall = Physics2D.OverlapCircle(wallCheck.position, wallRadius, whatIsWall);
		anim.SetBool("Wall", wall);

		// Set the vertical animation
		anim.SetFloat("vSpeed", GetComponent<Rigidbody2D>().velocity.y);
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
			move = (crouch ? move * crouchSpeed : move);

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat("Speed", Mathf.Abs(move));

			// Move the character
			GetComponent<Rigidbody2D>().velocity = new Vector2(move * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
			
			// If the input is moving the player right and the player is facing left...
			if(move > 0 && !facingRight)
				// ... flip the player.
				Flip();
			// Otherwise if the input is moving the player left and the player is facing right...
			else if(move < 0 && facingRight)
				// ... flip the player.
				Flip();
		}

        // SAUT CHARGE
        if (crouch)
        {
            // Si le personnage est accroupi, préparer le saut chargé
             crouchTime = crouchTime + (float)0.01;
        }

        // Relacher le temps d'accroupisseent et de chargement de saut dès que le joueur se relève
        if (!crouch)
        {
            crouchTime = 1;
        }

        // If the player should jump...
        if (grounded && jump) {
            // Add a vertical force to the player.
            anim.SetBool("Ground", false);
            
            //pour saut charge
            float jumpForceEffective = jumpForce;
            jumpForceEffective = (crouchTime > 1 ? crouchTime * jumpForce : jumpForce);
            if(jumpForceEffective > jumpForceLimit)
            {
                jumpForceEffective = jumpForceLimit;
            }
            Debug.Log("lo " + jumpForceEffective);

            GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForceEffective));

			compteurSaut = 1;
		}

		// SAUT MULTIPLE
		if (!grounded && jump && compteurSaut < nombreSautMax) {
			// Add a vertical force to the player.
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));
			compteurSaut++;
		}

		// SAUT MURAL
		if(!grounded && wall && jump) {
			// Add a vertical force to the player.

            
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));
			Flip();

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat("Speed", Mathf.Abs(move));

			// Move the character
			GetComponent<Rigidbody2D>().velocity = new Vector2(move * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
            

			// Add a horizontal force to the player.
			/*if(facingRight) {
				GetComponent<Rigidbody2D>().AddForce(new Vector2(-wallForce, 0f));
			}
			else {
				GetComponent<Rigidbody2D>().AddForce(new Vector2(+wallForce, 0f));
			}*/
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
