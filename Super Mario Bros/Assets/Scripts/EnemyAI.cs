using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

    private Vector3 size;

    public Vector2 velocity;
    public float gravity;
    public bool isWalkingLeft = true;

    private bool grounded = false;

    private enum EnemyState
    {
        walking,
        falling,
        dead
    }

    private EnemyState state = EnemyState.falling;

    public LayerMask wallMask;
    public LayerMask floorMask;

    // Use this for initialization
    void Start () {
        enabled = false;

        size = GetComponent<Renderer>().bounds.size;

        Fall();

        
    }
	
	// Update is called once per frame
	void Update () {
        UpdateEnemyPosition();

    }

    void UpdateEnemyPosition()
    {
        if (state != EnemyState.dead)
        {
            Vector3 pos = transform.localPosition;
            Vector3 scale = transform.localScale;

            if(state == EnemyState.falling)
            {
                Debug.Log("Entra");
                pos.y += velocity.y * Time.deltaTime;
                velocity.y -= gravity * Time.deltaTime;
                Debug.Log(velocity.y);
            }
                if (isWalkingLeft)
                {
                    pos.x -= velocity.x * Time.deltaTime;
                    scale.x = -1;
                }
                else
                {
                    pos.x += velocity.x * Time.deltaTime;
                    scale.x = 1;
                }

            if(velocity.y <= 0)
                pos = CheckGround(pos);

            CheckWallRays(pos, scale.x);

            transform.localPosition = pos;
            transform.localScale = scale;
        }
    }

    Vector3 CheckGround (Vector3 pos)
    {
        Vector2 originLeft = new Vector2(pos.x - size.x / 2 - 0.1f, pos.y - size.y / 2);
        Vector2 originMid = new Vector2(pos.x, pos.y - size.y / 2);
        Vector2 originRight = new Vector2(pos.x + size.x / 2 + 0.1f, pos.y - size.y / 2);

        RaycastHit2D floorLeft = Physics2D.Raycast(originLeft, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D floorMid = Physics2D.Raycast(originMid, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D floorRight = Physics2D.Raycast(originRight, Vector2.down, velocity.y * Time.deltaTime, floorMask);

        if (floorLeft.collider != null || floorMid.collider != null || floorRight.collider != null)
        {
            RaycastHit2D hitRay = floorMid;

            if (floorLeft)
            {
                hitRay = floorLeft;
            }
            else if (floorMid)
            {
                hitRay = floorMid;
            }
            else if (floorRight)
            {
                hitRay = floorRight;
            }
            if (hitRay.collider.tag == "Player")
            {
                Destroy(hitRay.collider.gameObject);
            }

            state = EnemyState.walking;

            grounded = true;

            velocity.y = 0;

            pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y / 2 + size.y / 2 - 0.01f;
        }
        else
        {
            if(state != EnemyState.falling)
                Fall();
        }
        return pos;
    }

    void CheckWallRays(Vector3 pos, float direction)
    {
        Vector2 originTop = new Vector2(pos.x + direction * 0.4f, pos.y + 0.4f);
        Vector2 originMid = new Vector2(pos.x + direction * 0.4f, pos.y);
        Vector2 originBot = new Vector2(pos.x + direction * 0.4f, pos.y - 0.4f);

        RaycastHit2D wallTop = Physics2D.Raycast(originTop, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallMid = Physics2D.Raycast(originMid, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallBot = Physics2D.Raycast(originBot, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);

        if (wallTop.collider != null || wallMid.collider != null || wallBot.collider != null)
        {
            RaycastHit2D hitRay = wallMid;

            if (wallTop)
            {
                hitRay = wallTop;
            }
            else if (wallMid)
            {
                hitRay = wallMid;
            }
            else if (wallBot)
            {
                hitRay = wallBot;
            }
            if (hitRay.collider.tag == "Player")
            {
                Destroy(hitRay.collider.gameObject);
            }

            isWalkingLeft = !isWalkingLeft;
        }
    }

    void OnBecameVisible()
    {
        enabled = true;
    }

    void Fall()
    {
        velocity.y = 0;
        state = EnemyState.falling;
        grounded = false;
    }
}
