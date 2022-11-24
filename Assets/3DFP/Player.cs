using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour {
	public static Player Instance;

	private CharacterController controller;
	public GameObject bullet;
	public Transform gun;
	public Transform feet;
	public TextMeshProUGUI score;
	private int val = 0;
	public float groundDistance = 0.4f;
	public LayerMask groundMask;
	bool isGrounded = false;
	public float speed = 12f;
	public float force = 10f;
	public bool bubble = false;
	Vector3 velY;
	public float jumpStr = 100f;
	public float Gravity = -15f;
	public float accel = 0.05f;
	GameObject[] mag = new GameObject[50];
	public GameObject Cam;
	bool canJump = true;
	private float cooldown = 3;

	public TextMesh GO_text;
	public TextMesh GO_text2;

	public enemy spider;

	public GameObject gameScene;
	public GameObject menuScene;
	public GameObject gameOverScreen;

	public RectTransform tutorial;
	public float tutorialPos = 0;

	private Vector3 init_position;
	public float tutSpeed;

	// Start is called before the first frame update
	void Start() {
		Instance = this;
		init_position = transform.position;
		val = 0;
		controller = GetComponent<CharacterController>();
		for (int i = 0; i < mag.Length; i++) {
			mag[i] = Instantiate(bullet, gun.position, transform.rotation);
			mag[i].SetActive(false);
		}
		SetUpTutorial();
	}

	void SetUpTutorial() {
		tutorial.gameObject.SetActive(true);
		tutorial.localPosition = new Vector3(tutorial.localPosition.x, -200, tutorial.localPosition.z);
		tutorialPos = 0;
	}

	// Update is called once per frame
	void Update() {

		if(tutorialPos < 1) {
			tutorialPos += Time.deltaTime * tutSpeed / 10f;
			tutorial.localPosition = new Vector3(tutorial.localPosition.x,Mathf.Lerp(-200,500,tutorialPos),tutorial.localPosition.z);
		} else {
			tutorial.gameObject.SetActive(false);
		}

		score.text = "" + val;
		isGrounded = Physics.CheckSphere(feet.position, groundDistance, groundMask);
		//isGrounded = true;
		if (isGrounded) {
			Gravity = -15;
			
		} else {
			canJump = true;
			Gravity += accel;
		}
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		Vector3 move = transform.right * x + transform.forward * z;

		controller.Move(move * speed * Time.deltaTime);

		velY.y += Gravity * Time.deltaTime;
		controller.Move(velY * Time.deltaTime);

		if (Input.GetButtonDown("Jump") && isGrounded && canJump) {
			canJump = false;
			velY.y = Mathf.Sqrt(jumpStr * -2 * Gravity);
		}

		velY.y += Gravity * Time.deltaTime;
		controller.Move(velY * Time.deltaTime);

		fire();

		if(Typing.Instance.hp <=0) {
			Cursor.lockState = CursorLockMode.None;
			GO_text.text = GO_text2.text = "SCORE : " + Typing.Instance.score.ToString("00000");

			Typing.Instance.score = 0;
			Typing.Instance.hp = 5;
			gameScene.SetActive(false);
			gameOverScreen.SetActive(true);
			spider.resetPosition();
			transform.position = init_position;
			AudioManager.instance.gameMusic.Stop();
			AudioManager.instance.gameoversound.Play();
			SetUpTutorial();
		}
		if(cooldown > 0) cooldown-= Time.deltaTime;
	}



	public void fire() {


		if (Input.GetMouseButtonDown(0)) {
			for (int i = 0; i < mag.Length; i++) {
				if (!mag[i].activeInHierarchy) {
					AudioManager.instance.shootsound.Play();
					mag[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
					mag[i].GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity;
					mag[i].transform.SetPositionAndRotation(gun.position, Cam.transform.rotation);
					mag[i].SetActive(true);
					mag[i].GetComponent<Rigidbody>().velocity = Cam.transform.forward * force;
					break;
				}
			}
		}




	}
	private void OnCollisionEnter(Collision collision) {
		if (collision.collider.tag == "bullet" && cooldown <= 0) {
			Typing.Instance.hp--;
			cooldown = 3;
			AudioManager.instance.hitsound.Play();
		}
	}

	private void OnDrawGizmos() {
		Gizmos.DrawWireSphere(feet.position,groundDistance);
	}
}

