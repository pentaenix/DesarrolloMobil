using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMover : MonoBehaviour {
    // Start is called before the first frame update
    bool Dragging = false;
    public Animator anim;
    public SpriteRenderer sprite;
    public TMP_Text height;
    Rigidbody2D RB2D;
    public BoxCollider2D Collider2D;

    //Swaping variables
    Vector2 StartTouchPosition;
    Vector2 EndTouchPosition;

    public float SwipeRange;

    private float framesSwiping;
    float initialCamY;
    public Transform campos;
    public Transform Foot;
    int jumps = 1;
    public LayerMask groundLayer;
    public bool IsAlive = true;
    private float airTime;
    private bool FallDamage = false;

    public static PlayerMover instance;

    private Vector2 camStart;
    private Vector2 playStart;

    void Start() {
        instance = this;
        Collider2D = GetComponent<BoxCollider2D>();
        RB2D = GetComponent<Rigidbody2D>();
        initialCamY = campos.position.y;
        camStart = campos.position;
        playStart = transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (IsAlive) {
            Mover();
            Swipe();
        }
        MoveCamera();
        CheckGround();
    }

	

	void CheckGround() {
		if (IsGrounded()) {
            if(airTime >= 0.05f && !FallDamage) {
                anim.SetTrigger("Fall");
			}
            airTime = 0;
            jumps = 1;
            if (FallDamage) Dead();
		} else {
            airTime += Time.deltaTime;
		}
        
        if(RB2D.velocity.y < -15) {
            FallDamage = true;
        }
        
        height.text = ""+Mathf.Ceil(transform.position.y+3)+"m";

    }

    bool IsGrounded() {
        bool feet = Physics2D.OverlapCircle(Foot.position, 0.18f, groundLayer);
        return feet;
	}

	void MoveCamera() {
        if(transform.position.y > initialCamY) {
            campos.position = new Vector3(campos.position.x,transform.position.y, campos.position.z);
		}
	}

    void Swipe() {
        if(Input.GetMouseButtonDown(0)) {
            StartTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        }

        if(Input.GetMouseButton(0)) {
            sprite.flipX = RB2D.velocity.x < 0;
            framesSwiping += Time.deltaTime;
		}

        if(Input.GetMouseButtonUp(0)) {
            EndTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 Distance = EndTouchPosition - StartTouchPosition;

            if (jumps > 0 && Distance.magnitude > SwipeRange  && framesSwiping < 0.25f) {
                jumps--;
                Vector2 addedForce = Distance.normalized*400;
                var upDir = Vector2.Dot(addedForce,Vector2.up);
                addedForce = SetLengthOnAxis(addedForce, Vector3.up, 0f);
                addedForce = Vector3.ClampMagnitude(addedForce, 100);
                addedForce += upDir * Vector2.up;
                RB2D.AddForce(addedForce) ;
                anim.SetTrigger("Jump");
            }

            framesSwiping = 0;
        }
	}


    void Mover() {
        
        if (Input.GetMouseButtonDown(0)) Dragging = true;

        Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        point.z = 0;
        if (Dragging && !nearZero(point.x - transform.position.x)) {
            sprite.flipX = RB2D.velocity.x < 0;
            Vector2 forceToAdd = new Vector2((point.x > transform.position.x ? 1f : -1f), 0);
            RB2D.AddForce(forceToAdd*2);
        }
        
        if (!Input.GetMouseButton(0)) {
            Dragging = false;
        }


        anim.SetFloat("Speed", Mathf.Abs(RB2D.velocity.x));
    }

    void Dead(platformer.Dead dead = platformer.Dead.Fall) {
        platformer.instance.score = (int)Mathf.Ceil(transform.position.y + 3);
        if (IsAlive) {
            anim.SetTrigger("Dead");
            IsAlive = false;
            RB2D.velocity = Vector2.zero;
            if (dead == platformer.Dead.Pig) {
                GetComponent<Collider2D>().enabled = false;
                RB2D.gravityScale = 0;
            }
		}
        platformer.instance.OnGameOver(dead);
	}

    public void ResetPlayer() {
        IsAlive = true;
        anim.SetTrigger("Reset");
        GetComponent<Collider2D>().enabled = true;
        RB2D.gravityScale = 1;
        FallDamage = false;
        transform.position = playStart;
        campos.position = new Vector3(camStart.x,camStart.y,campos.position.z);
    }

    public void OnClick() {
        Dragging = true;
    }

    bool nearZero(float val) {
        return Mathf.Abs(val) < 0.1f;
	}

    public static Vector3 SetLengthOnAxis(Vector3 v, Vector3 axis, float len) {
        axis.Normalize();
        var d = len - Vector3.Dot(v, axis);
        return v + axis * d;
    }
	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.collider.CompareTag("Enemy")){
            Dead(platformer.Dead.Pig);
        }
        
	}
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Doorway")) {
            Dead(platformer.Dead.Win);

        }
    }
	private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(Foot.position, 0.18f);
	}
}
