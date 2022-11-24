using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class mover : MonoBehaviour {
	private CharacterController controller;
	public float speed = 12f;
	public GameObject Cam;

	public GameObject Game;
	public GameObject Menu;

	// Start is called before the first frame update
	void Start() {
		controller = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	void Update() {

		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		Vector3 move = transform.right * x + transform.forward * z;

		controller.Move(move * speed * Time.deltaTime);

		if (Input.GetKeyDown(KeyCode.Space) && Typing.close) {
			Game.SetActive(true);
			Menu.SetActive(false);
			AudioManager.instance.menusound.Play();
			AudioManager.instance.gameMusic.Play();
		}
	}
}

