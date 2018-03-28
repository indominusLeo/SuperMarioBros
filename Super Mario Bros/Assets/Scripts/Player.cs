using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    private Vector3 size;

    public Vector2 velocity;
    public float jumpVelocity;
    public float gravity;
    private bool jumped;

    bool stoppedJumping;
    public float jumpTime;
    public float jumpTimeCounter;

    public LayerMask wallMask;
    public LayerMask floorMask;

    private bool walk, walk_left, walk_right, jump;

    public enum PlayerState
    {
        jumping,
        idle,
        walking
    }

    private PlayerState playerState = PlayerState.idle;

    public bool grounded = false;

	// Use this for initialization
	void Start () {
        Fall();
	}
	
	// Update is called once per frame
	void Update () {

        size = GetComponent<Renderer>().bounds.size;

        CheckPlayerInput();

        UpdatePlayerPosition();

        UpdateAnimationStates();
    }

    void UpdatePlayerPosition()
    {
        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        if (walk)
        {
            if (walk_left)
            {
                pos.x -= velocity.x * Time.deltaTime;
                scale.x = -1;
            }

            if (walk_right)
            {
                pos.x += velocity.x * Time.deltaTime;
                scale.x = 1;
            }

            pos = CheckWallRays(pos, scale.x);
        }

        /*if (jump && playerState != PlayerState.jumping)
        {
            playerState = PlayerState.jumping;
            //VIDEO   velocity = new Vector2(velocity.x, jumpVelocity);
            velocity.y = jumpVelocity;
            jumped = true;
        }
        if(playerState == PlayerState.jumping)
        {
            pos.y += velocity.y * Time.deltaTime;

            velocity.y -= gravity * Time.deltaTime;
        }*/

        //Jump mechanics


        if (grounded)
        {
            jumpTimeCounter = jumpTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            playerState = PlayerState.jumping;
            velocity.y = jumpVelocity;
            stoppedJumping = false;
            jumped = true;
            grounded = false;
        }
        if(Input.GetKey(KeyCode.Space) && !stoppedJumping && jumpTimeCounter > 0)
        {
            velocity.y = jumpVelocity;
            jumpTimeCounter -= 9.5f * Time.deltaTime;
            jumped = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpTimeCounter = 0;
            stoppedJumping = true;
        }
        if (playerState == PlayerState.jumping)
        {
            pos.y += velocity.y * Time.deltaTime;

            velocity.y -= gravity * Time.deltaTime;
        }
        pos.y += velocity.y * Time.deltaTime;


        if (velocity.y <= 0)
            pos = CheckFloorRays(pos);

        if (velocity.y >= 0)
            pos = CheckCeilingRays(pos);

        transform.localPosition = pos;
        transform.localScale = scale;
    }

    void UpdateAnimationStates()
    {
        if(grounded && !walk)
        {
            GetComponent<Animator>().SetBool("isJumping", false);
            GetComponent<Animator>().SetBool("isRunning", false);
        }

        if (grounded && walk)
        {
            GetComponent<Animator>().SetBool("isJumping", false);
            GetComponent<Animator>().SetBool("isRunning", true);
        }
        if(jumped)
        {
            GetComponent<Animator>().SetBool("isJumping", true);
            GetComponent<Animator>().SetBool("isRunning", false);
        }
    }

    void CheckPlayerInput()
    {
        bool input_left = Input.GetKey(KeyCode.LeftArrow);
        bool input_right = Input.GetKey(KeyCode.RightArrow);
        //bool input_space = Input.GetKeyDown(KeyCode.Space);

        walk = input_left || input_right;

        walk_left = input_left && !input_right;
        walk_right = input_right && !input_left;
        //jump = input_space;
    }

    Vector3 CheckWallRays (Vector3 pos, float direction)
    {
        Vector2 originTop = new Vector2(pos.x + direction * 0.4f, pos.y + 0.4f);   //Add for big mario
        Vector2 originMid = new Vector2(pos.x + direction * 0.4f, pos.y);   //Add for big mario
        Vector2 originBot = new Vector2(pos.x + direction * 0.4f, pos.y - 0.4f);   //Add for big mario

        RaycastHit2D wallTop = Physics2D.Raycast(originTop, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallMid = Physics2D.Raycast(originMid, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallBot = Physics2D.Raycast(originBot, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);

        if (wallTop.collider != null || wallMid.collider != null || wallBot.collider != null)
        {
            pos.x -= velocity.x * Time.deltaTime * direction;
        }

        return pos;
    }

    Vector3 CheckFloorRays(Vector3 pos)
    {
        Vector2 originLeft = new Vector2(pos.x - size.x/2, pos.y - size.y/2);   //Add for big mario
        Vector2 originMid = new Vector2(pos.x, pos.y - size.y/2);   //Add for big mario
        Vector2 originRight = new Vector2(pos.x + size.x/2, pos.y - size.y/2);   //Add for big mario

        RaycastHit2D floorLeft = Physics2D.Raycast(originLeft, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D floorMid = Physics2D.Raycast(originMid, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D floorRight = Physics2D.Raycast(originRight, Vector2.down, velocity.y * Time.deltaTime, floorMask);

        if (floorLeft.collider != null || floorMid.collider != null || floorRight.collider != null)
        {
            RaycastHit2D hitRay = floorMid;

            if (floorLeft)
            {
                hitRay = floorLeft;
            } else if (floorMid)
            {
                hitRay = floorMid;
            } else if (floorRight)
            {
                hitRay = floorRight;
            }
            if(hitRay.collider.tag == "Enemy")
            {
                Destroy(hitRay.collider.gameObject);
            }

            playerState = PlayerState.idle;

            grounded = true;

            velocity.y = 0;

            jumped = false;

            stoppedJumping = true;

            pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y/2 + size.y/2 - 0.01f; //Add for big mario
        } else
        {
            if (playerState != PlayerState.jumping)
            {
                Fall();
            }
        }

        return pos;
    }

    Vector3 CheckCeilingRays(Vector3 pos)
    {
        Vector2 originLeft = new Vector2(pos.x - size.x / 2, pos.y + size.y / 2);   //Add for big mario
        Vector2 originMid = new Vector2(pos.x, pos.y + size.y / 2);   //Add for big mario
        Vector2 originRight = new Vector2(pos.x + size.x / 2, pos.y + size.y / 2);   //Add for big mario

        RaycastHit2D ceilLeft = Physics2D.Raycast(originLeft, Vector2.up, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D ceilMid = Physics2D.Raycast(originMid, Vector2.up, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D ceilRight = Physics2D.Raycast(originRight, Vector2.up, velocity.y * Time.deltaTime, floorMask);

        if (ceilLeft.collider != null || ceilMid.collider != null || ceilRight.collider != null)
        {
            RaycastHit2D hitRay = ceilMid;

            if (ceilLeft)
            {
                hitRay = ceilLeft;
            }
            else if (ceilMid)
            {
                hitRay = ceilMid;
            }
            else if (ceilRight)
            {
                hitRay = ceilRight;
            }

            pos.y = hitRay.collider.bounds.center.y - hitRay.collider.bounds.size.y/2 - size.y/2; //Add for big mario

            Fall();

            stoppedJumping = true;
        }

        return pos;
    }

    void Fall()
    {
        velocity.y = 0;

        playerState = PlayerState.jumping;

        grounded = false;
    }
}
