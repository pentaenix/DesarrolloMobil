using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform sensor;
    public bool canMove = false;
    public float speed;
    private Rigidbody2D RB2D;
    private bool flipping = false;
    private bool mustFlip;
    public LayerMask ground;
    public Animator anim;
    private PlayerMover player;
    

	void Start()
    {
        player = PlayerMover.instance;
        RB2D = GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        anim.SetBool("CanMove", canMove);
        if (player == null) player = PlayerMover.instance;
        if (Mathf.Abs(player.transform.position.y - transform.position.y) < 11) {
            anim.enabled = true;
            if (canMove && !flipping) {
                move();
            }
            mustFlip = !Physics2D.OverlapCircle(sensor.position, 0.1f, ground);
        } else {
            anim.enabled = false;
            RB2D.velocity = Vector2.zero;
        }
    }

	void move() {

        if (mustFlip) flip();
        RB2D.velocity = new Vector2(speed*Time.deltaTime,0);

	}

    void flip() {
        flipping = true;
        transform.localScale = new Vector2(transform.localScale.x*-1, transform.localScale.y);
        speed *= -1;
        flipping = false;
	}
}
