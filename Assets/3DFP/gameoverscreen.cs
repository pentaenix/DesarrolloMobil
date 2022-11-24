using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameoverscreen : MonoBehaviour
{
    public void OnClick() {
        Cursor.lockState = CursorLockMode.Locked;
		Player.Instance.gameOverScreen.SetActive(false);
		Player.Instance.menuScene.SetActive(true);
    }

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			OnClick();
		}
	}
}
