using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CharacterController2DPlatformer : MonoBehaviour {
	[Range(0f,20f)]
	public float maxSpeed = 10f;
	[Range(0f,20f)]
	public float airSpeed = 5f;
	public bool facingRight = true;

	//Timing variables
	private float  totalTime;
	private float  timerCounter = 0f;
	private bool timerBool = false;

	//Air dash variables
	[Range(0f,40f)]
	public float dashForce = 20f;
	//public float totalAirDash; Have not implemented
	[Range(0f,.1f)]
	public float   airDashTimer = .05f;

	//Jump Variables
	//public float wallJumpPhysicsTimer = .2f;
	[Range(0f,20f)]
	public float jumpForce = 20f;
	[Range(0f,20f)]
	public float wallJumpForceVertical = 20f;
	[Range(0f,20f)]
	public float wallJumpForceHorizontal = 20f;
	private bool canWallJump = true;
	private float wallJumpPosition;

	//Rates of falling
	[Range(0f,5f)]
	public float fallRate = 2.5f;
	[Range(0f,5f)]
	public float lowJumpFallRate = 2f;
	[Range(0f,5f)]
	public float wallFallRate = 4f;

	//Variables for falling detection
	public LayerMask whatIsGround;
	private bool isOnGround = false;
	private Vector2 groundCheck;
	private Vector2 groundBoxSize;

	//Variables for wall detection
	public LayerMask whatIsWall;
	private Vector2 wallCheck; //TODO check if this works facing both directions
	private Vector2 wallBoxSize;
	private bool isOnWall = false;
	private bool isOnWallLeft = false;

	//Components to be found
	private Animator animator;
	private Rigidbody2D rb2D;
	private BoxCollider2D bc2D;


	// Use this for initialization
	void Start () {
		rb2D = GetComponent<Rigidbody2D> ();
		rb2D.freezeRotation = true;
		animator = GetComponent<Animator> ();

		//Gets BoxCollider2D from player and updates box sizes
		bc2D = GetComponent<BoxCollider2D> ();
		groundBoxSize = new Vector2 (bc2D.size.x - .5f, .2f);//Moves from front of player to not collide with all
		wallBoxSize = new Vector2 (bc2D.size.x + .1f, bc2D.size.y - .1f); //Shifts box up slightly to not clip with ground
	}

	//====================================================
	//	Updates
	//====================================================
	
	// Fixed update does not need Time.DeltaTime
	void FixedUpdate () {
		//Checks if player is on the ground and sets animation accordingly 
		GroundCheckUpdate ();
		//Checks if player is on a wall
		WallCheckUpdate();
		if (!timerBool) {
			Run ();
			Jump ();
		}
	}

	void Update(){
		//Sets the ground and wall checking objects location
		groundCheck = new Vector2 (bc2D.transform.position.x, bc2D.transform.position.y - bc2D.size.y / 2);
		wallCheck = new Vector2 (bc2D.transform.position.x, bc2D.transform.position.y + .1f);
		TimerUpdate ();
	}

	//====================================================
	//	Movements
	//====================================================

	//Handles all running code and airdash
	void Run(){
		float move = Input.GetAxis ("Horizontal");

		//Updates animation
		if (rb2D.velocity.x != 0 && Input.GetButton("Horizontal") && isOnGround && !isOnWall) {
			IsRunning (true);
		} else {
			IsRunning (false);
		}

		//Checks if on the ground and updates horizontal speed
		//Returns before method can update the flip
		if (Input.GetButtonDown ("AirDash")) {
			AirDash (move);
			timerBool = true;
			totalTime = airDashTimer;
		} else if (isOnGround) {
			rb2D.velocity = new Vector2 (move * maxSpeed, rb2D.velocity.y);
		} else if(Input.GetButton("Horizontal")){
			rb2D.velocity = new Vector2 (move * airSpeed, rb2D.velocity.y);
			return;
		}

		//Updates character orientation
		if (move > 0 && !facingRight) {
			Flip ();
		} else if (move < 0 && facingRight) {
			Flip ();
		}
	}

	//Handles all jumping code
	void Jump(){
		WallJumpCheck ();

		//Adds jumpForce to create jump
		if (isOnGround && Input.GetButtonDown ("Jump")) {
			rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce);
		} else if (!isOnGround && isOnWall && Input.GetButtonDown("Jump") && canWallJump && isOnWallLeft) {
			rb2D.velocity = new Vector2(wallJumpForceHorizontal, wallJumpForceVertical);
			wallJumpPosition = transform.position.x;
			canWallJump = false;
		}else if (!isOnGround && isOnWall && Input.GetButtonDown("Jump") && canWallJump && !isOnWallLeft) {
			rb2D.velocity = new Vector2(-wallJumpForceHorizontal, wallJumpForceVertical);
			wallJumpPosition = transform.position.x;
			canWallJump = false;
		}
		//Input.GetAxis("Horizontal")<0

		//Adds velocity to make fall faster
		//If jump is not held the jump will be smaller
		if (rb2D.velocity.y < 0 && !isOnWall) {
			rb2D.velocity += Vector2.up * Physics2D.gravity.y * (fallRate - 1) * Time.deltaTime; //Normal fall speed
		}else if (rb2D.velocity.y < 0 && isOnWall){
			rb2D.velocity += Vector2.up * Physics2D.gravity.y * (wallFallRate - 1) * Time.deltaTime; //Fall speed holding onto wall
		} else if (rb2D.velocity.y > 0 && !Input.GetButton ("Jump")) {
			rb2D.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpFallRate - 1) * Time.deltaTime; //Added push if jump is held
		}
	}

	void AirDash(float direction){
		if (direction > 0) {
			rb2D.velocity = new Vector2 (rb2D.velocity.x + dashForce, rb2D.velocity.y);
		} else if (direction < 0) {
			rb2D.velocity = new Vector2 (rb2D.velocity.x - dashForce, rb2D.velocity.y);
		} else if (facingRight) {
			rb2D.velocity = new Vector2 (rb2D.velocity.x + dashForce, rb2D.velocity.y);
		} else {
			rb2D.velocity = new Vector2 (rb2D.velocity.x - dashForce, rb2D.velocity.y);
		}
	}

	//====================================================
	//	Checks if movement is possible
	//====================================================

	void WallJumpCheck ()
	{
		//Resets wallJump if player lands on ground or gets certain distance from wall
		if (isOnGround) {
			canWallJump = true;
		}
		else if (!(transform.position.x < (wallJumpPosition + 1f) && transform.position.x > (wallJumpPosition - 1f))) {
				canWallJump = true;
			}
	}

	//Checks if player is on the ground and sets animation accordingly 
	void GroundCheckUpdate ()
	{
		isOnGround = Physics2D.OverlapBox (groundCheck, groundBoxSize, 0, whatIsGround);
		if (isOnGround == true) {
			IsJumping (false);
		}else {
			IsJumping (true);
		}
	}

	//Checks if player is on wall and sets animation accordingly 
	void WallCheckUpdate(){
		isOnWall = Physics2D.OverlapBox (wallCheck, wallBoxSize, 0, whatIsWall);
		float velocityDirection = rb2D.velocity.x;
		if (velocityDirection < 0 && isOnWall) {
			isOnWallLeft = true;
		} else if (velocityDirection > 0 && isOnWall) {
			isOnWallLeft = false;
		}
		if (isOnWall && !isOnWall) {
			//TODO Update animation
		}
	}

	//General timer to allow physics to be stalled
	void TimerUpdate (){
		if (timerBool) {
			if (timerCounter <= totalTime) {
				timerCounter += Time.deltaTime;
			} else {
				timerCounter = 0;
				timerBool = false;
			}
		}
	}

	//====================================================
	//	Animation Update
	//====================================================

	//Flips character sprite
	void Flip(){
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	//Sets animation to running
	void IsRunning(bool boolean){
		animator.SetBool ("IsRunning", boolean);
	}

	//Sets animation to jumping
	void IsJumping(bool boolean){
		animator.SetBool ("IsJumping", boolean);
	}
}
