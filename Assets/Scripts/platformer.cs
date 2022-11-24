using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class platformer : MonoBehaviour {
	enum Scene {
		Game,
		Menu,
		GameOver,
		none
	}

	public enum Dead {
		Pig,
		Fall,
		Win
	}

	public static platformer instance;

	//Time variables
	private int timeInGame = 0;
	private int startCoolDownValue = 8;
	public int startCooldown;

	//UI variables
	public RectTransform stars;
	public RectTransform world;
	public TMP_Text scoreTextGameOver;
	public TMP_Text winLoseText;
	public TMP_Text tutorialText;
	private Image TutorialGraphic;
	private Color initTutorialTextColor;
	private Color initTutotialImgColor;
	public GameObject tutorial_UI;

	//Sceen Variables
	public Image blackdrop;
	private bool swapingScene = true;
	private float blackStat = 1.1f;
	private int blackscreenDir = 0;
	private Scene scene = Scene.Menu;
	private Scene newscene = Scene.Menu;
	public GameObject GameOverScreen;
	public GameObject MenuScreen;
	public GameObject GameScreen;
	public bool gameover = false;

	//Audio variables
	public AudioSource GameMusic;
	public AudioSource MenuMusic;
	public AudioSource GameOverMusic;
	public AudioSource HitSound;
	bool muted = false;

	//Gameplay variables
	public int score = 0;
	public GameObject moonDead;
	public GameObject pigDead;
	public GameObject fallDead;

	void Start() {
		instance = this;
		initTutorialTextColor = tutorialText.color;
		TutorialGraphic = tutorial_UI.GetComponent<Image>();
		initTutotialImgColor = TutorialGraphic.color;
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
		GameScreen.SetActive(scene == Scene.Game);
	}

	void MenuUpdate() {
		stars.Rotate(new Vector3(0, 0, Time.deltaTime * 2));
		world.Rotate(new Vector3(0, 0, -Time.deltaTime * 2));

	}

	void GameUpdate() {
		timeManager();
		tutorialUpdate();
	}


	void tutorialUpdate() {
		tutorial_UI.SetActive(startCooldown > 0);
		if (startCooldown >= 2) {
			float transp = Mathf.Clamp(((Mathf.Sin(Time.time * 3)) / 2 + 0.5f) - 0.1f, 0.2f, 0.8f);
			TutorialGraphic.color = new Color(TutorialGraphic.color.r, TutorialGraphic.color.g, TutorialGraphic.color.b, transp);
		} else {
			TutorialGraphic.DOFade(0, 0.7f);
			tutorialText.DOFade(0, 0.7f);
		}
	}

	void timeManager() {
		if (timeInGame != Mathf.Floor(Time.time)) {
			timeInGame = (int)Mathf.Floor(Time.time);
			if (startCooldown > 0) startCooldown--;
		}
	}

	public void OnPlay() {
		PlayerMover.instance.ResetPlayer();
		tutorialText.color = initTutorialTextColor;
		TutorialGraphic.color = initTutotialImgColor;

		score = 0;
		gameover = false;
		SetScene(Scene.Game);
		startCooldown = startCoolDownValue;
		
	}
	public void OnGameOver(Dead TypeOfDead) {
		fallDead.SetActive(TypeOfDead == Dead.Fall);
		pigDead.SetActive(TypeOfDead == Dead.Pig);
		moonDead.SetActive(TypeOfDead == Dead.Win);
		if (TypeOfDead == Dead.Pig) winLoseText.text = "Beware of the pigs, they dont seem that friendly...";
		if(TypeOfDead == Dead.Fall) winLoseText.text = "Try not to fall from too high, or... launch yourself to the ground.";
		if (TypeOfDead == Dead.Win) {
			winLoseText.text = "Congratulations, you reached the moon.";
		}else HitSound.Play();
		SetScene(Scene.GameOver);
		gameover = true;
		if (PlayerPrefs.GetInt("maxScore") < score) {
			PlayerPrefs.SetInt("maxScore", score);
		} 
		scoreTextGameOver.text = "" + PlayerPrefs.GetInt("maxScore")+"m";
	}

	public void OnReturnToMenu() {
		SetScene(Scene.Menu);
	}

	void AudioSystem() {
		switch (scene) {
			case Scene.Game:
				GameMusic.Play();

				MenuMusic.Stop();
				GameOverMusic.Stop();
				break;
			case Scene.Menu:
				GameMusic.Stop();

				MenuMusic.Play();
				GameOverMusic.Stop();
				break;
			case Scene.GameOver:
				GameMusic.Stop();

				MenuMusic.Stop();
				GameOverMusic.Play();
				break;
			default:
				break;
		}
	}


}
