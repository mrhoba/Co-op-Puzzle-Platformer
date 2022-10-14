using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region playerStats
    [System.Serializable]
    public class PlayerStats
    {
        public int Health = 100;
    }
    #endregion

    #region Variables
    public PlayerStats playerStats = new();

    Rigidbody2D player;

    public Animator animator;

    public float speed;

    public float jumpForce;

    bool isGrounded = false;
    bool isUnderGround = false;

    public Transform isGroundedChecker;
    public float checkGroundRadius;

    public LayerMask groundLayer;
    public LayerMask burrowGroundLayer;
    public LayerMask undergroundLayer;
    public LayerMask hazardLayer;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    public float rememberGroundedFor;
    float lastTimeGrounded;

    public Transform camTarget;
    public float aheadAmount, aheadSpeed;

    private Vector2 movementInput = Vector2.zero;
    private bool jumped = false;

    private bool burrowed = false;
    private bool unburrowed = false;

    private bool candoublejump;

    private Collider2D trigger = null;

    private bool interacted = false;

    public int fallBoundary = -10;
    #endregion

    #region Start & Update
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Rigidbody2D>();

        var player1 = PlayerInput.all[0];
        var player2 = PlayerInput.all[1];

        // Discard existing assignments.
        player1.user.UnpairDevices();
        player2.user.UnpairDevices();

        // Assign devices and control schemes.
            InputUser.PerformPairingWithDevice(Keyboard.current, user: player1.user);
            InputUser.PerformPairingWithDevice(Keyboard.current, user: player2.user);

            player1.user.ActivateControlScheme("Arrows");
            player2.user.ActivateControlScheme("WASD");
    }

    // Update is called once per frame
    void Update()
    {
        BetterJump();
        CheckIfGrounded();
        CheckIfUnderGround();
        CheckIfHazard();
        CameraFollow();
        FallChecker();
    }
    #endregion

    #region Input Actions
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        Move();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumped = context.action.triggered;
        Jump();
    }

    public void OnBurrow(InputAction.CallbackContext context)
    {
        burrowed = context.action.triggered;
        Burrow();
    }

    public void OnUnBurrow(InputAction.CallbackContext context)
    {
        unburrowed = context.action.triggered;
        UnBurrow();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        interacted = context.action.triggered;
        Interact();
    }
    #endregion

    #region Movement
    //Handles Player Left And Right Movement
    void Move()
    {
        float x = movementInput.x;
        float moveBy = x * speed;
        animator.SetFloat("Speed", Mathf.Abs(moveBy));
        player.velocity = new Vector2(moveBy, player.velocity.y);
    }

    //Handles Player Jumping
    void Jump()
    {
        if (jumped)
        {
            if (isGrounded || Time.time - lastTimeGrounded <= rememberGroundedFor)
            {
                player.velocity = new Vector2(player.velocity.x, jumpForce);
                candoublejump = true;
            }
            else if (candoublejump)
            {
                candoublejump = false;
                player.velocity = new Vector2(player.velocity.x, jumpForce);
            }
        }
        
    }

    //Handles Player Burrowing
    void Burrow()
    {
        if (burrowed)
        {
            Color color = GetComponent<Renderer>().material.color;
            if (Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, burrowGroundLayer) != null)
            {
                color.a = 0.2f;
                player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 1.3f);

            }
            GetComponent<Renderer>().material.color = color;
        }
    }

    //Handles Player UnBurrowing
    void UnBurrow()
    {
        if (unburrowed)
        {
            Color color = GetComponent<Renderer>().material.color;
            if (isUnderGround)
            {
                color.a = 1;
                player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1.3f);
                burrowed = false;
            }
            GetComponent<Renderer>().material.color = color;
        }
    }

    //Handles Player Interaction
    void Interact()
    {
        if (trigger != null && interacted)
        {
            LayerMask layer = trigger.gameObject.layer;
            switch (layer)
            {
                case 10://Lever Layer
                    trigger.GetComponent<AnimationController>().PlayAnimation("Flip");
                    trigger.transform.GetChild(0).GetComponent<AnimationController>().PlayAnimation("Open");
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region Ground Checkers
    //Checks If Player Is On The Ground
    void CheckIfGrounded()
    {
        Collider2D colliders = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, groundLayer);
        if (colliders != null)
        {
            isGrounded = true;
        }
        else
        {
            if (isGrounded)
            {
                lastTimeGrounded = Time.time;
            }
            isGrounded = false;
        }
    }

    //Checks If Player Is Under The Ground
    void CheckIfUnderGround()
    {
        Collider2D colliders = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, undergroundLayer);
        if (colliders != null)
        {
            isUnderGround = true;
        }
        else
        {
            isUnderGround = false;
        }
    }

    //Checks If Player Is On Hazardous Ground
    void CheckIfHazard()
    {
        Collider2D colliders = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, hazardLayer);
        if (colliders != null)
        {
            //player.transform.position = new Vector2(0,0);
            DamagePlayer(999999);
        }
    }
    #endregion

    #region Smoothening
    //Smoother jumping with gravity
    void BetterJump()
    {
        if (player.velocity.y < 0)
        {
            player.velocity += Vector2.up * Physics2D.gravity * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (player.velocity.y > 0 && !jumped)
        {
            player.velocity += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    //Moves the Camera Target smoothly to the wanted postion and rotates the player
    void CameraFollow()
    {
        if(movementInput.x != 0)
        {
            camTarget.localPosition = new Vector3(Mathf.Lerp(camTarget.localPosition.x, aheadAmount * movementInput.x, aheadSpeed * Time.deltaTime), camTarget.localPosition.y, camTarget.localPosition.z);
            if (movementInput.x < 0)
            {
                player.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                player.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
    #endregion

    #region Misc
    //Manages Collision between the 2 players
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        trigger = collider;
    }

    void FallChecker()
    {
        if (transform.position.y <= fallBoundary)
        {
            DamagePlayer(999999);
        }
    }

    public void DamagePlayer(int damage)
    {
        playerStats.Health -= damage;
        if (playerStats.Health <= 0)
        {
            GameManager.KillPlayer(this);
        }
    }
    #endregion
}
