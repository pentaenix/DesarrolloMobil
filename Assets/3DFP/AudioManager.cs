using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

	public AudioSource gameMusic;
	public AudioSource menusound;
	public AudioSource hitsound;
	public AudioSource shootsound;
	public AudioSource gameoversound;

	private void Start() {
		instance = this;
	}
}
