using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class whacamole : MonoBehaviour {
	enum Scene {
		Game,
		Menu,
		GameOver,
		none
	}

	//Time variables
	private int timeInGame = 0;
	private int TimeSinceSpawn = 0;
	private int startCoolDownValue = 8;
	public int startCooldown;

	//UI variables
	public RectTransform stars;
	public RectTransform world;
	public TMP_Text scoreText;
	public TMP_Text hp_text;
	public TMP_Text scoreTextGameOver;
	public TMP_Text winLoseText;
	public TMP_Text tutorialText;
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
	public GameObject GameScreen;
	public bool gameover = false;

	//Audio variables
	public AudioSource GameMusic;
	public AudioSource GameAmbient;
	public AudioSource MenuMusic;
	public AudioSource GameOverMusic;
	public AudioSource HitSound;
	bool muted = false;

	//Gameplay variables
	public Mole[] moles;
	public Color[] colors;
	public Color deactivatedColor;
	public SpriteRenderer lightcolor;
	public Color forvidenColor;
	private Color LastForvidenColor;
	private Color initialColor;
	public GameObject tutorial_UI;
	public textHit[] hitPoints;
	private int ConsecutiveMoles = 0;
	public float Speed = 10;
	private int spawnMoleTime = 0;
	public int score = 0;
	private int singleMoles = 0;
	private int minTime = 2;
	private int maxMoles = 2;
	public int health = 5;
	private int damageCoolDown = 3;
	private char hearth;
	private int toNextForvidenColor = 5;
	

	void Start() {
		initTutorialTextColor = tutorialText.color;
		TutorialGraphic = tutorial_UI.GetComponent<Image>();
		initTutotialImgColor = TutorialGraphic.color;
		startCooldown = startCoolDownValue;
		hearth = hp_text.text[0];
		initialColor = lightcolor.color;
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
		DisplayScore();
		tutorialUpdate();
		if (TimeSinceSpawn >= spawnMoleTime) {
			spawnMoleTime = Random.Range(minTime, 3);
			if (spawnMoleTime == 0) {
				ConsecutiveMoles++;
			} else {
				ConsecutiveMoles = 0;
			}

			singleMoles++;

			if (singleMoles > 4) {
				singleMoles = 0;
				if (minTime > 0) {
					minTime--;
				}
				if (maxMoles < 4) {
					maxMoles++;
				}
			}

			TimeSinceSpawn = 0;
			if (ConsecutiveMoles < maxMoles) {
				MoleSpawner();
			}
		}


		if (health <= 0) OnGameOver();
		if(toNextForvidenColor <= 0) {
			toNextForvidenColor = Random.Range(4, 7);
			LastForvidenColor = forvidenColor;
			forvidenColor = colors[Random.Range(1, colors.Length)];
			lightcolor.color = forvidenColor;
		}
	}

	public void MoleDown(Color color, bool pressed,Transform pos) {
		bool showPoints = true;
		bool positive = false;

		if (color == forvidenColor) {
			if (pressed) {
				Hit();
			} else {
				score += 3;
				positive = true;
			}
		} else {
			if (!pressed) {
				if (color != LastForvidenColor) {
					Hit();
				} else showPoints = false;
			} else {
				score += 3;
				positive = true;
			}
		}
		if (showPoints) {
			for (int i = 0; i < hitPoints.Length; i++) {
				if (hitPoints[i].CanUse) {
					hitPoints[i].Activate(pos, positive);
					break;
				}
			}
		}
	}



	void Hit() {
		if (damageCoolDown <= 0) {
			damageCoolDown = 3;
			health--;
		}
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

	void MoleSpawner() {
		if (startCooldown <= 0 && !gameover) {
			int tries = 0;
			while (tries < 15) {
				int index = Random.Range(0, moles.Length);
				if (!moles[index].IsActive) {
					moles[index].Activate(colors[Random.Range(0,colors.Length)]);
					break;
				}
				tries++;
			}
		}
	}


	void DisplayScore() {
		scoreText.text = "" + score.ToString("0000000");

		string hptxt = "";
		for (int i = 0; i < health; i++) hptxt += hearth;//'?';
		hp_text.text = hptxt;
	}

	void timeManager() {
		if (timeInGame != Mathf.Floor(Time.time)) {
			timeInGame = (int)Mathf.Floor(Time.time);
			TimeSinceSpawn++;
			if (startCooldown > 0) startCooldown--;
			if (damageCoolDown > 0) damageCoolDown--;
			if(toNextForvidenColor > 0 && startCooldown <= 0) toNextForvidenColor--;
		}

	}

	public void OnPlay() {
		singleMoles = 0;
		minTime = 2;
		maxMoles = 2;
		health = 5;
		damageCoolDown = 3;

		tutorialText.color = initTutorialTextColor;
		TutorialGraphic.color = initTutotialImgColor;

		score = 0;
		gameover = false;
		SetScene(Scene.Game);
		startCooldown = startCoolDownValue;
		lightcolor.color = forvidenColor = initialColor;

		foreach (Mole mole in moles) {
			mole.ResetMole();
		}
	}
	public void OnGameOver() {

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

}
