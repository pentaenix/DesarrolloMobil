using UnityEngine;
using System.Collections;

public class Typing : MonoBehaviour {

	
	public static Typing Instance;
	public static bool close = false;
	[TextArea(3, 10)]
	public string text;
	public string hpIcon;
	public int score = 0;
	public int hp = 5;
	public float speed = 0.5f;
	public TextMesh tm1;
	public TextMesh tm2;
	public bool isGame = true;
	public bool isMenu = false;
	public Transform player;
	bool paused = false;
	// Use this for initialization
	void Start () {
		if (isGame) {
			Instance = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isGame) {
			string hpcount = "";
			for (int i = 0; i < hp; i++) hpcount += "" + hpIcon;
			text = "SCORE : " + score + "\n" + "HP : " + hpcount;
			timer();
		} else if (isMenu) {
			close = Vector3.Distance(player.position, transform.position) < 0.9f;
			if (close) {
				timer();
			}
		}
	}
	void timer() {
		int l = (int)(Time.time * speed);
		if (l > text.Length) l = text.Length;
		string out_ = text.Substring(0, l);

		tm1.text = out_;
		tm2.text = out_;
	}
}
