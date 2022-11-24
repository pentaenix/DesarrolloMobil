using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class enemy : MonoBehaviour {
	public NavMeshAgent ene;
	public GameObject player;
	public Transform[] pos;
	public Transform gun;
	public GameObject bullet;
	public GameObject[] mag = new GameObject[50];
	public TextMeshProUGUI score;
	private int val = 0;
	public int currentObj;
	private float seconds;
	private float limiter = 0;
	public int speed = 30;
	private int bulCount = 0;
	private Vector3 init_pos;

	// Start is called before the first frame update
	void Start() {
		init_pos = transform.position;
		val = 0;
		currentObj = (int)Random.Range(0, pos.Length);
		for (int i = 0; i < mag.Length; i++) {
			mag[i] = Instantiate(bullet, gun.position, transform.rotation);
			mag[i].SetActive(false);
		}
	}

	// Update is called once per frame
	void Update() {
		score.text = "" + val;
		ene.SetDestination(pos[currentObj].position);
		if (Vector3.Distance(ene.transform.position, pos[currentObj].position) < 0.5f) {
			ene.isStopped = true;
			gameObject.transform.LookAt(player.transform);
			if (bulCount++ < 3) shoot();
			
			if (limiter > 2) {
				ene.isStopped = false;
				currentObj = Random.Range(0, pos.Length);
				limiter = 0;
				bulCount = 0;
			}
		}

		if (seconds != (int)Time.time) {
			seconds = (int)Time.time;
			limiter += 1;
		}

	}

	public void shoot() {
		for (int i = 0; i < mag.Length; i++) {
			if (!mag[i].activeInHierarchy) {
				mag[i].transform.SetPositionAndRotation(gun.position, transform.rotation);
				mag[i].SetActive(true);
				mag[i].GetComponent<Rigidbody>().velocity = transform.forward * speed;
				break;
			}
		}
	}

	public void resetPosition() {
		transform.position = init_pos;
	}
	private void OnTriggerEnter(Collider other) {
		if (other.tag == "arrow") {
			other.gameObject.GetComponent<destroy>().deactivateBullet();
			Typing.Instance.score++;
		}
	}



}
