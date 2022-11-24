using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class draganddrop : MonoBehaviour {
	enum Scene {
		Game,
		Menu,
		GameOver,
		none
	}

	enum col {
		Red = 0,
		Green = 1,
		Blue = 2,
		Yellow = 3
	}
	public GameObject target;

	//Time variables
	private int timeInGame = 0;
	private int TimeSinceActivatedColor = 0;
	private int startCoolDownValue = 8;
	public int startCooldown;

	//UI variables
	public RectTransform stars;
	public RectTransform world;
	public TMP_Text scoreText;
	public TMP_Text timeText;
	public TMP_Text scoreTextGameOver;
	public TMP_Text winLoseText;
	public TMP_Text tutorialText;
	public TMP_Text pointText;
	private Image TutorialGraphic;
	private Color initTutorialTextColor;
	private Color initTutotialImgColor;

	//Sceen Variables
	public Image blackdrop;
	private bool swapingScene = true;
	private float blackStat = 1.1f;
	private int blackscreenDir = 0;
	private Scene scene = Scene.Menu;
	private Scene newscene = Scene.Menu;
	public GameObject GameOverScreen;
	public GameObject MenuScreen;
	public bool gameover = false;

	//Audio variables
	public AudioSource GameMusic;
	public AudioSource GameAmbient;
	public AudioSource MenuMusic;
	public AudioSource GameOverMusic;
	public AudioSource HitSound;
	bool muted = false;

	//Gameplay variables
	bool Dragging = false;
	public float ballRad = 10;
	public GameObject[] BallArray;
	public SpriteRenderer[] Tubes;
	public Transform[] tubesTransform;
	public Color[] tubeColors;
	public Color inactiveColor;
	public GameObject tutorial_UI;
	public float Speed = 10;
	public int score = 0;
	int colorActivateTime = 10;
	int level = 0;
	int gameTime = 10;

	void Start() {
		initTutorialTextColor = tutorialText.color;
		TutorialGraphic = tutorial_UI.GetComponent<Image>();
		initTutotialImgColor = TutorialGraphic.color;
		startCooldown = startCoolDownValue;

	}


	// Update is called once per frame
	void Update() {
		SwapScene();
		switch (scene) {
			case Scene.Menu:
				MenuUpdate();
				break;
			case Scene.Game:
				GameUpdate();
				break;
			default:
				break;
		}

	}

	void MenuUpdate() {
		stars.Rotate(new Vector3(0, 0, Time.deltaTime * 2));
		world.Rotate(new Vector3(0, 0, -Time.deltaTime * 2));

	}

	void GameUpdate() {
		timeManager();
		DisplayScore();
		transform.Rotate(new Vector3(0, 0, Time.deltaTime * (50 + score / 50)));

		DragAndDrop();
	}

	void SetScene(Scene scene) {
		newscene = scene;
		swapingScene = true;
	}

	void SwapScene() {
		if (swapingScene) {
			blackdrop.gameObject.SetActive(true);
			blackStat += Time.deltaTime * 1f * blackscreenDir;
			blackdrop.color = Color.Lerp(Color.clear, Color.black, Mathf.Clamp(blackStat, 0, 1));
			if (blackStat > 1) {
				scene = newscene;
				blackStat = 1;
				blackscreenDir = -1;
				AudioSystem();
			}
			if (blackStat < 0) {
				swapingScene = false;
				blackStat = 0;
				blackscreenDir = 1;

			}
		} else {
			blackdrop.gameObject.SetActive(false);
		}
		MenuScreen.SetActive(scene == Scene.Menu);
		GameOverScreen.SetActive(scene == Scene.GameOver);
	}

	void DragAndDrop() {
		if (gameTime <= 0) OnGameOver();

		if (TimeSinceActivatedColor >= colorActivateTime && !gameover) {
			colorActivateTime = Random.Range(SetMaxLevel(), 3);
			ActivateTube();
			TimeSinceActivatedColor = 0;
		}

		tutorial_UI.SetActive(startCooldown > 0);
		if (startCooldown >= 2) {
			float transp = Mathf.Clamp(((Mathf.Sin(Time.time * 3)) / 2 + 0.5f) - 0.1f, 0.2f, 0.8f);
			TutorialGraphic.color = new Color(TutorialGraphic.color.r, TutorialGraphic.color.g, TutorialGraphic.color.b, transp);
		} else {
			TutorialGraphic.DOFade(0, 1);
			tutorialText.DOFade(0, 1);
		}



		Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		point.z = 0;
		if (Dragging && !gameover && movingBall != null) {
			movingBall.transform.position = point;
		}

		if (!Input.GetMouseButton(0)) {
			Dragging = false;
			if (movingBall != null) {
				movingBall.transform.DOScale(0.6f, 0.5f);
				movingBall = null;
			}
		}
		for (int i = 0; i < activeTubesIndex.Count; i++) {
			if (Tubes[activeTubesIndex[i]].color == inactiveColor) {
				gameTime -= 2;
				ResetTube(i);
				break;
			}
		}


		for (int b = 0; b < BallArray.Length; b++) {
			if (BallArray[b].GetComponent<SpriteRenderer>().color.a <= 0.3f) {
				ResetBall(b);
			}
			if (BallArray[b].transform.localScale.x <= 0.6f) {
				for (int i = 0; i < activeTubesIndex.Count; i++) {
					if (Vector2.Distance(BallArray[b].transform.position, tubesTransform[activeTubesIndex[i]].transform.position) < 1f) {
						print("Entered " + (col)takenColors[i]);
						bool Point = b == takenColors[i];
						if (Point) {
							score += 3;
							gameTime += 3;
						} else gameTime -= 5;
						ShowPoints(tubesTransform[activeTubesIndex[i]].transform.position, Point, Point ? 3 : 5);
						ResetTube(i, false);
						ResetBall(b, true);
						break;
					}
				}
				ResetBall(b);
			}
		}

		checkButtons();




	}

	void ShowPoints(Vector2 position, bool isPositive, int ammount) {
		pointText.rectTransform.position = position;
		pointText.text = (isPositive ? "+" + ammount : "-" + ammount);
		pointText.DOColor(isPositive ? Color.yellow : Color.gray, 0);
		pointText.DOFade(0, 0.5f);
		pointText.rectTransform.DOMoveY(pointText.rectTransform.position.y + 2, 1f);
	}

	void ResetBall(int i, bool fade = false) {
		if (!fade) {
			BallArray[i].transform.position = new Vector2(15, 15);
			BallArray[i].transform.localScale = new Vector3(1.53f, 1.53f, 1.53f);
			BallArray[i].GetComponent<SpriteRenderer>().DOFade(1, 0);
		} else {
			BallArray[i].GetComponent<SpriteRenderer>().DOFade(0, 0.5f);
		}
	}
	void ResetTube(int i, bool missed = true) {
		if (missed && activeTubesIndex.Contains(i)) {
			Tubes[activeTubesIndex[i]].DOColor(inactiveColor, 0);
			ShowPoints(tubesTransform[activeTubesIndex[i]].position, false, 2);
		}
		freeTubesIndex.Add(activeTubesIndex[i]);
		activeTubesIndex.RemoveAt(i);
		freeColors.Add(takenColors[i]);
		takenColors.RemoveAt(i);

	}

	List<GameObject> activeBalls = new List<GameObject>();

	List<int> takenColors = new List<int>();
	List<int> freeColors = new List<int>();

	List<int> activeTubesIndex = new List<int>();
	List<int> freeTubesIndex = new List<int>();
	void ActivateTube() {
		if (takenColors.Count < 4 && activeTubesIndex.Count < 8 && !gameover && startCooldown <= 0) {
			int tubeFound = GetRandomIndex();
			int colorFound = GetRandomColor();
			StartTube(tubeFound, colorFound);
		}
	}

	int GetRandomColor() {
		return freeColors[Random.Range(0, freeColors.Count)];
	}

	int GetRandomIndex() {
		return freeTubesIndex[Random.Range(0, freeTubesIndex.Count)];
	}

	void StartTube(int index, int col) {
		if (!gameover) {
			takenColors.Add(col);
			freeColors.Remove(col);
			activeTubesIndex.Add(index);
			freeTubesIndex.Remove(index);
			Tubes[index].color = tubeColors[col];
			Tubes[index].DOColor(inactiveColor, 5);
		}
	}

	int SetMaxLevel() {
		return level++ > 3 ? 0 : 1;
	}

	GameObject movingBall;
	bool PressedRed = false;
	bool PressedGreen = false;
	bool PressedBlue = false;
	bool PressedYellow = false;
	public SpriteRenderer[] buttons;

	void checkButtons() {
		if (buttons[0].color.a == 1) PressedRed = false;
		if (buttons[1].color.a == 1) PressedGreen = false;
		if (buttons[2].color.a == 1) PressedBlue = false;
		if (buttons[3].color.a == 1) PressedYellow = false;
	}

	public void OnRedBallPress() {
		if (!PressedRed) {
			PressedRed = true;
			Dragging = true;
			SetFadeButton((int)col.Red);
			movingBall = BallArray[(int)col.Red];
			activeBalls.Add(movingBall);
		}
	}
	public void OnGreenBallPress() {
		if (!PressedGreen) {
			PressedGreen = true;
			Dragging = true;
			SetFadeButton((int)col.Green);
			movingBall = BallArray[(int)col.Green];
			activeBalls.Add(movingBall);
		}
	}
	public void OnBlueBallPress() {
		if (!PressedBlue) {
			PressedBlue = true;
			Dragging = true;
			SetFadeButton((int)col.Blue);
			movingBall = BallArray[(int)col.Blue];
			activeBalls.Add(movingBall);
		}
	}
	public void OnYellowBallPress() {
		if (!PressedYellow) {
			PressedYellow = true;
			Dragging = true;
			SetFadeButton((int)col.Yellow);
			movingBall = BallArray[(int)col.Yellow];
			activeBalls.Add(movingBall);
		}
	}

	void SetFadeButton(int i) {
		Color tmp = buttons[i].color;
		tmp.a = 0;
		buttons[i].color = tmp;
		tmp.a = 1;
		buttons[i].DOColor(tmp, 1.5f);
	}

	List<int> FilterCache;
	int FilteredRandomValue(int min, int max) {
		int safeguard = 0;
		if (FilterCache == null) FilterCache = new List<int>();
		int randomValue;
		do {
			randomValue = Random.Range(min, max);
			bool notConsecutive = FilterCache.Count > 0 && FilterCache[FilterCache.Count] != randomValue;
			//add other rules as needed
			if (notConsecutive) {
				FilterCache.Add(randomValue);
				if (FilterCache.Count > 3) FilterCache.RemoveAt(0);
				break;
			}
		} while (safeguard++ < 100);

		return randomValue;
	}

	void DisplayScore() {
		scoreText.text = "" + score.ToString("0000000");
		timeText.text = "" + Mathf.Max(0, gameTime).ToString("0000000");
	}

	void timeManager() {
		if (timeInGame != Mathf.Floor(Time.time)) {
			timeInGame = (int)Mathf.Floor(Time.time);
			TimeSinceActivatedColor++;
			if (startCooldown > 0) startCooldown--;
			else gameTime--;
		}

	}

	public void OnPlay() {

		movingBall = null;
		PressedRed = false;
		PressedGreen = false;
		PressedBlue = false;
		PressedYellow = false;

		tutorialText.color = initTutorialTextColor;
		TutorialGraphic.color = initTutotialImgColor;

		if (FilterCache != null) FilterCache.Clear();

		gameover = false;
		SetScene(Scene.Game);

		freeColors.Clear();
		takenColors.Clear();
		for (int i = 0; i < 4; i++) {
			freeColors.Add(i);
		}

		freeTubesIndex.Clear();
		activeTubesIndex.Clear();
		for (int i = 0; i < 8; i++) {
			freeTubesIndex.Add(i);
		}

		for (int i = 0; i < BallArray.Length; i++) {
			ResetBall(i);
		}
		for (int i = 0; i < Tubes.Length; i++) {
			Tubes[i].DOColor(inactiveColor, 0);
		}
		gameTime = 10;
		startCooldown = 8;
		score = 0;
	}
	public void OnGameOver() {
		//DOTween.KillAll();
		pointText.rectTransform.position = new Vector2(15, 15);
		SetScene(Scene.GameOver);
		HitSound.Play();
		gameover = true;
		if (PlayerPrefs.GetInt("maxScore") < score) {
			PlayerPrefs.SetInt("maxScore", score);
			winLoseText.text = "New highscore!";
		} else {
			winLoseText.text = "Your score: " + score.ToString("00000");
		}
		scoreTextGameOver.text = "" + PlayerPrefs.GetInt("maxScore").ToString("0000000");
	}

	public void OnReturnToMenu() {
		SetScene(Scene.Menu);
	}

	public void OnMute() {
		muted = !muted;
		if (muted) AudioListener.volume = 0;
		else AudioListener.volume = 1;
	}

	void AudioSystem() {
		switch (scene) {
			case Scene.Game:
				GameMusic.Play();
				GameAmbient.Play();

				MenuMusic.Stop();
				GameOverMusic.Stop();
				break;
			case Scene.Menu:
				GameMusic.Stop();
				GameAmbient.Stop();

				MenuMusic.Play();
				GameOverMusic.Stop();
				break;
			case Scene.GameOver:
				GameMusic.Stop();
				GameAmbient.Stop();

				MenuMusic.Stop();
				GameOverMusic.Play();
				break;
			default:
				break;
		}
	}

	private void OnDrawGizmos() {
		Gizmos.DrawWireSphere(tubesTransform[0].position, 1f);
		Gizmos.DrawWireSphere(tubesTransform[1].position, 1f);
		Gizmos.DrawWireSphere(tubesTransform[2].position, 1f);
		Gizmos.DrawWireSphere(tubesTransform[3].position, 1f);
		Gizmos.DrawWireSphere(tubesTransform[4].position, 1f);
		Gizmos.DrawWireSphere(tubesTransform[5].position, 1f);
		Gizmos.DrawWireSphere(tubesTransform[6].position, 1f);
		Gizmos.DrawWireSphere(tubesTransform[7].position, 1f);
		//float tmp = Vector2.Distance(BallArray[0].transform.position, tubesTransform[0].position);
		//if(tmp < 0.65f)print("Distance " + tmp);
	}

}
