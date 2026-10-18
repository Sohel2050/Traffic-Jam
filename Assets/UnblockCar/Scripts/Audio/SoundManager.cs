using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    [SerializeField]
    private AudioSource audioSound;
    [SerializeField]
    private AudioSource BGM;
    [SerializeField]
    private AudioClip button;
    [SerializeField]
    private AudioClip clipWinGame;
    [SerializeField]
    private AudioClip clipBlockTouch;
    [SerializeField]
    private AudioClip clipBlockPlace;
    [SerializeField]
    private AudioClip starComplete;

    [SerializeField]
    private List<Image> buttonSound = new List<Image>();
    [SerializeField]
    private List<Image> buttonMusic= new List<Image>();
    [SerializeField]
    private Sprite musicOn;
    [SerializeField]
    private Sprite musicOff;
    [SerializeField]
    private Sprite soundOn;
    [SerializeField]
    private Sprite soundOff;

    private bool muteSound = false;
    private bool muteMusic = false;
    private void Awake()
    {
        instance = this;
 
    }

	private void Start()
	{
        muteMusic = PlayerPrefs.GetInt("Music") == 1 ? true : false;
        muteSound = PlayerPrefs.GetInt("Sound") == 1 ? true : false;

        if (BGM != null) { BGM.Stop(); BGM.mute = true; BGM.playOnAwake = false; }

        audioSound.volume = muteSound ? 0 : 1;
      

        for (int i = 0; i < buttonMusic.Count; i++)
        {
            buttonMusic[i].sprite = muteMusic ? musicOff : musicOn;
        }

        for (int i = 0; i < buttonSound.Count; i++)
        {
            buttonSound[i].sprite = muteSound ? soundOff : soundOn;
        }

    }

    public void OnEventMusic()
    {
        // background music removed
    }


    public void OnEventSound( )
    {

        muteSound = !muteSound;
        PlayerPrefs.SetInt("Sound", (muteSound == true ? 1 : 0));
        audioSound.volume = muteSound ? 0 : 1;
        for (int i = 0; i < buttonSound.Count; i++)
        {
            buttonSound[i].sprite = muteSound ? soundOff : soundOn;
        }


    }
    public void StarCompletedSound()
    {
        audioSound.PlayOneShot(starComplete);
    }


    public void WinGameSound()
    {
        audioSound.PlayOneShot(clipWinGame);
    }

   
    public void TouchBlock()
    {
        audioSound.PlayOneShot(clipBlockTouch);
    }
    public void BlockPlace()
    {
        audioSound.PlayOneShot(clipBlockPlace);
    }


    public void OnEventButtonSound()
	{
		audioSound.PlayOneShot(button);
	}

	 

	 

	 

	 
}
