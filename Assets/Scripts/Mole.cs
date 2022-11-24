using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mole : MonoBehaviour {
	// Start is called before the first frame update
	public whacamole controller;
	public SpriteRenderer MoleImage;
	public Transform MolePosition;
	public Color MoleColor;
	[HideInInspector] public bool IsActive = false;
	[HideInInspector] public bool canTap = false;
	private float position;

	private int counter = 0;
	private int timeInGame = 0;
	private int mintime = 2;
	void Start() {
		position = MolePosition.transform.position.y;
	}

	// Update is called once per frame
	void Update() {
		if (IsActive && MolePosition.position.y == position) {
			IsActive = false;
		}
		
		if (IsActive)timeManager();
		if (counter >= mintime && canTap && IsActive) Deactivate();
	}

	public void Deactivate(bool pressed = false) {
		if (canTap) {
			controller.MoleDown(MoleColor,pressed,MolePosition);
			if(pressed) MoleImage.color = controller.deactivatedColor;
			MolePosition.DOMoveY(position, 0.2f);
			canTap = false;
		}
	}


	public void Activate(Color col) {
		if (!IsActive) {
			mintime = Random.Range(3, 4);
			counter = 0;
			canTap = true;
			MoleImage.color = col;
			MoleColor = col;
			IsActive = true;
			MolePosition.position += new Vector3(0, 0.01f);
			MolePosition.DOMoveY(position + 3.3f, 0.25f);
		}
	}

	public void ResetMole() {
		canTap = false;
		IsActive = false;
		canTap = false;
		MolePosition.DOMoveY(position, 0);
	}

	void timeManager() {
		if (timeInGame != Mathf.Floor(Time.time)) {
			timeInGame = (int)Mathf.Floor(Time.time);
			counter++;
		}
	}


}
