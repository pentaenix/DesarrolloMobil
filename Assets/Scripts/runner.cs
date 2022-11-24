using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class runner : MonoBehaviour {
	enum Scene {
		Game,
		Menu,
		GameOver,
		none
	}
	public GameObject player;

	//Time variables
	private int timeInGame = 0;
	private int TimeSinceSpawn = 0;
	private int startCoolDownValue = 8;
	public int startCooldown;

	//UI variables
	public RectTransform stars;
	public RectTransform world;
	public TMP_Text scoreText;
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
	public bool gameover = false;

	//Audio variables
	public AudioSource GameMusic;
	public AudioSource GameAmbient;
	public AudioSource MenuMusic;
	public AudioSource GameOverMusic;
	public AudioSource HitSound;
	bool muted = false;

	//Gameplay variables
	bool movingWheel = false;
	private Vector3 mouse_pos;
	public Transform wheel_transform;
	private Vector3 wheel_position;
	private float wheel_angle;
	public GameObject tutorial_UI;
	private int ConsecutiveStones = 0;
	public float Speed = 10;
	public Transform[] waves;
	private Vector3[] initialWavesPos;
	public Transform[] stones;
	private int spawnRockTime = 0;
	public int score = 0;
	Sequence tutorialAnim;

	void Start() {
		initTutorialTextColor = tutorialText.color;
		TutorialGraphic = tutorial_UI.GetComponent<Image>();
		initTutotialImgColor = TutorialGraphic.color;
		startCooldown = startCoolDownValue;
		for (int i = 0; i < stones.Length; i++) {
			stones[i].gameObject.SetActive(false);
		}
		initialWavesPos = new Vector3[waves.Length];
		for (int i =0; i < waves.Length; i++) {
			initialWavesPos[i] = waves[i].transform.position;
		}
		//SetUpTutorialAnim();
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
		enemyMover();
		DisplayScore();
		if (TimeSinceSpawn >= spawnRockTime) {
			spawnRockTime = Random.Range(0, 3);
			if (spawnRockTime == 0) {
				ConsecutiveStones++;
			} else {
				ConsecutiveStones = 0;
			}
			TimeSinceSpawn = 0;
			if (ConsecutiveStones < 3) RockSpawner();
		}

		for (int i = 0; i < waves.Length; i++) {
			waves[i].position += new Vector3(Mathf.Sin(i % 2 == 0 ? Time.time : -Time.time) * Time.deltaTime * 0.8f, Mathf.Sin(Time.time) * Time.deltaTime * 0.3f);
		}

		float PlayerY = player.transform.position.y - 7.4f;
		player.GetComponent<SpriteRenderer>().sortingOrder =
			PlayerY >= 1 ? -3 :
			PlayerY >= -1 ? -2 :
			PlayerY >= -3 ? -1 :
			PlayerY >= -5 ? 0 : 1;

		RotateWheel();
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

	void RotateWheel() {
		tutorial_UI.SetActive(startCooldown > 0);
		if (startCooldown>=2) {
			float transp = Mathf.Clamp(((Mathf.Sin(Time.time*3)) / 2 + 0.5f)-0.1f,0.2f,0.8f);
			TutorialGraphic.color = new Color(TutorialGraphic.color.r, TutorialGraphic.color.g, TutorialGraphic.color.b,transp);
		} else {
			TutorialGraphic.DOFade(0, 0.5f);
			tutorialText.DOFade(0,0.5f);
		}

		mouse_pos = Input.mousePosition;
		mouse_pos.z = -20;
		if (Input.GetMouseButtonDown(0) && mouse_pos.y < Screen.height / 2) {
			movingWheel = true;
		}
		if (!Input.GetMouseButton(0)) movingWheel = false;

		if (movingWheel && !gameover) {
			wheel_position = Camera.main.WorldToScreenPoint(wheel_transform.position);
			mouse_pos.x = mouse_pos.x - wheel_position.x;
			mouse_pos.y = mouse_pos.y - wheel_position.y;
			wheel_angle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
			if (wheel_angle < 0) wheel_angle *= -1;
			Mathf.Clamp(wheel_angle, 1, 175);
			wheel_transform.transform.rotation = Quaternion.Euler(0, 0, wheel_angle);
			float moveTo = Time.deltaTime * Mathf.Abs(90 - wheel_angle) * 0.2f * (wheel_angle < 90 ? -1 : 1);
			transform.position += new Vector3(0, moveTo, 0);
			transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 2, 9));

		}

	}
	void RockSpawner() {
		if (startCooldown <= 0 && !gameover) {
			int tries = 0;
			while (tries < 15) {
				int index = Random.Range(0, stones.Length);
				if (!stones[index].gameObject.activeInHierarchy) {
					stones[index].gameObject.SetActive(true);
					break;
				}
				tries++;
			}
		}
	}

	void enemyMover() {
		if (!gameover) {
			foreach (Transform rock in stones) {
				if (rock.gameObject.activeInHierarchy) {
					rock.position -= new Vector3(Speed * Time.deltaTime, 0, 0);
					if (rock.position.x < player.transform.position.x - 10) {
						if (!gameover) score++;
						rock.position = new Vector3(10, rock.position.y, 0);
						rock.gameObject.SetActive(false);
						break;
					}
				}
			}
		}
	}

	void DisplayScore() {
		scoreText.text = "" + score.ToString("0000000");
	}

	void timeManager() {
		if (timeInGame != Mathf.Floor(Time.time)) {
			timeInGame = (int)Mathf.Floor(Time.time);
			TimeSinceSpawn++;
			if (startCooldown > 0) startCooldown--;
		}

	}

	public void OnPlay() {
		
		tutorialText.color = initTutorialTextColor;
		TutorialGraphic.color = initTutotialImgColor;

		score = 0;
		gameover = false;
		SetScene(Scene.Game);
		wheel_transform.rotation = new Quaternion(0,0,0,0);
		startCooldown = startCoolDownValue;

		for (int i = 0; i < waves.Length; i++) {
			waves[i].transform.position = initialWavesPos[i];
		}

		foreach (Transform rock in stones) {
			if (rock != null) {
				rock.position = new Vector3(10, rock.position.y, 0);
				rock.gameObject.SetActive(false);
			}
		}
		//tutorialAnim.Rewind();
		//tutorialAnim.Play();
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



	private void OnTriggerEnter2D(Collider2D collision) {
		OnGameOver();
	}


}
