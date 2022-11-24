using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroy : MonoBehaviour {
	bool shoot = true;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (shoot) {
			StartCoroutine(deactivate());
			shoot = false;
		}


	}
	public void deactivateBullet() {
		shoot = true;
		gameObject.SetActive(false);
	}

	public IEnumerator deactivate() {

		yield return new WaitForSeconds(5f);
		deactivateBullet();
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.collider.tag == "limit") {
			//Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
		}
	}
}
