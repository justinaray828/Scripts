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
	private bool hasWallJumped = true;
	private float wallJumpPosition;

	//Rates of falling
	[Range(0f,5f)]
	public float fallRate = 2.5f;
	[Range(0f,5f)]
	public float lowJumpFallRate = 2f;
	[Range(0f,5f)]
	public float wallFallRate = 4f;

	//Variables for falling detection
	//TODO: Rename whatIsGround
	public LayerMask whatIsGround;
	private bool isOnGround = false;
	//TODO: Rename groundCheck
	private Vector2 groundCheck;
	private Vector2 groundBoxSize;

	//Variables for wall detection
	public LayerMask whatIsWall;
	private Vector2 wallCheck;
	private Vector2 wallBoxSize;
	private bool isOnWall = false;
	private bool isOnWallLeft = false;

	//Components to be found
	private Animator animator;
	private Rigidbody2D rb2D;
	private BoxCollider2D bc2D;

	//====================================================
	//	Set Up
	//====================================================

	/**
	 * Setup Player Object
	 */
	void setupPlayer () {
		rb2D = GetComponent<Rigidbody2D> ();
		rb2D.freezeRotation = true;
		animator = GetComponent<Animator> ();
	}

	/**
	 * Setup Colliders
	 */
	void setupColliders () {
		//Gets BoxCollider2D from player and updates box sizes
		bc2D = GetComponent<BoxCollider2D> ();
		groundBoxSize = new Vector2 (bc2D.size.x - .5f, .2f);//Moves from front of player to not collide with all
		wallBoxSize = new Vector2 (bc2D.size.x + .1f, bc2D.size.y - .1f); //Shifts box up slightly to not clip with ground
	}

	// Initialize Game Objects
	void Start () {
		setupPlayer();
		setupColliders();
	}

	//====================================================
	//	Updates
	//====================================================
	
	// Updates on Physic Changes
	void FixedUpdate () {
		//Checks if player is on the ground and sets animation accordingly 
		GroundCheckUpdate ();
		//Checks if player is on a wall
		WallCheckUpdate();
		if (!timerBool) {
			movementUpdate ();
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

	/**
	 * Updates player movement
	 */
	void movementUpdate () {
		run ();
		jump ();
	}

	/*
	 * Check if player is running
	 */
	void updateRunning(float movement){
		bool isRunning = rb2D.velocity.x != 0 && movement != 0 && isOnGround && !isOnWall;
		setIsRunning (isRunning);
	}

	/*
	 * Get horozontal Movement Value
	 */
	float getHorozontalMovement() {
		float movement = 0.0f;
		if (Mathf.Abs(Input.GetAxis ("Horizontal")) > .01f) {
			movement = Input.GetAxis ("Horizontal");
		}
		return movement;
	}

	/*
	 * Updates character orientation
	 */
	void updateOrientation (float movement) {
		if (movement > 0 && !facingRight) {
			//If facing left and moving right, flip right
			Flip ();
		}else if (movement < 0 && facingRight) {
			//If facing left and moving right, flip right
			Flip ();
		}
	}

	/*
	 * Method for running physics and air dash
	 */
	void run(){
		float movement = getHorozontalMovement ();

		updateRunning(movement);

		//Checks if on the ground and updates horizontal speed
		//Returns before method can update the flip
		if (Input.GetButtonDown ("AirDash")) {
			AirDash (movement);
		} else if (isOnGround) {
			//movement speed
			rb2D.velocity = new Vector2 (movement * maxSpeed, rb2D.velocity.y);
		} else if(Mathf.Abs(movement) > 0){
			//Midair movement
			rb2D.velocity = new Vector2 (movement * airSpeed, rb2D.velocity.y);
			return;
		}

		updateOrientation (movement);
	}

	/*
	 * Performs Wall Jump Actions
	 */
	void wallJump (){
		if (rb2D.velocity.y < 0 && !isOnWall) {
			rb2D.velocity += Vector2.up * Physics2D.gravity.y * (fallRate - 1) * Time.deltaTime;
			//Normal fall speed
		}else if (rb2D.velocity.y < 0 && isOnWall) {
			rb2D.velocity += Vector2.up * Physics2D.gravity.y * (wallFallRate - 1) * Time.deltaTime;
			//Fall speed holding onto wall
		}else if (rb2D.velocity.y > 0 && !Input.GetButton ("Jump")) {
			rb2D.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpFallRate - 1) * Time.deltaTime;
			//Added push if jump is held
		}
	}

	/* 
	 * Adds velocity to make fall faster
	 * If jump is not held the jump will be smaller
	 */
	void performJump (){
		WallJumpCheck ();
		//Adds jumpForce to create jump
		if (isOnGround && Input.GetButtonDown ("Jump")) {
			rb2D.velocity = new Vector2 (rb2D.velocity.x, jumpForce);
		} else if (!isOnGround && isOnWall && Input.GetButtonDown ("Jump") && hasWallJumped && isOnWallLeft) {
			rb2D.velocity = new Vector2 (wallJumpForceHorizontal, wallJumpForceVertical);
			wallJumpPosition = transform.position.x;
			hasWallJumped = false;
		}else if (!isOnGround && isOnWall && Input.GetButtonDown ("Jump") && hasWallJumped && !isOnWallLeft) {
			rb2D.velocity = new Vector2 (-wallJumpForceHorizontal, wallJumpForceVertical);
			wallJumpPosition = transform.position.x;
			hasWallJumped = false;
		}
	}

	/*
	 * Method for jump physics
	 */
	void jump(){
		if (isOnGround) {
			performJump ();
		} else {
			wallJump ();
			performJump ();
		}
	}



	/*
	 * Makes player dash in specified direction
	 * @param direction
	 */
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

		timerBool = true;
		totalTime = airDashTimer;

	}

	//====================================================
	//	Checks if movement is possible
	//====================================================

	/*
	 * Detects if the player is able to do a wall jump
	 */
	void WallJumpCheck ()
	{
		//Resets wallJump if player lands on ground or gets certain distance from wall
		if (isOnGround) {
			hasWallJumped = true;
		}else if (!(transform.position.x < (wallJumpPosition + 1f) && transform.position.x > (wallJumpPosition - 1f))) {
			hasWallJumped = true;
		}
	}

	/**
	 * Checks if player is on the ground and sets jumping animation
	 */
	void GroundCheckUpdate ()
	{
		isOnGround = Physics2D.OverlapBox (groundCheck, groundBoxSize, 0, whatIsGround);
		if (isOnGround == true) {
			SetIsJumping (false);
		}else {
			SetIsJumping (true);
		}
	}

	/*
	 * Checks if player is on a wall and the direction of the wall
	 * in relation to player
	 */
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

	/*
	 * General global timer
	 */
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

	/**
	 * Flips direction of player sprite
	 */
	void Flip(){
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	/**
	 * Sets animation to running
	 * @param bool isRunning
	 */
	void setIsRunning(bool isRunning){
		animator.SetBool ("IsRunning", isRunning);
	}
		
	/**
	 * Sets animation to jumping
	 * @param bool isJumping
	 */
	void SetIsJumping(bool isJumping){
		animator.SetBool ("IsJumping", isJumping);
	}
}
