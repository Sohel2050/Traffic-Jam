using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using System;

[System.Serializable]
public class HintInfor
{
    public int blockIndex = 0;
    public int numberMove = 0;
}

[System.Serializable]
public class HintContainer
{
    public List<HintInfor> hints = new List<HintInfor>();
}
public class BoosterManager : MonoHandler
{
    public static BoosterManager instance;
    [SerializeField]
    private TextMeshProUGUI hintText;
    [SerializeField]
    private bool testMode;
    private int maxWatchAds = 6;
   


    private int countAds = 0;
    private static int hintPressCount = 0;

    private int hintNumber = 0;

   

    private void Awake()
    {
        instance = this;
        countAds = 0;
        hintNumber = PlayerPrefs.GetInt("Hint", (testMode) ? 300 : 2);
        AddHint(0);
        Premium.Changed += () => { if (this != null) UpdateTextHint(); };
    }
    public void OnEventHint()
    {
        if (Premium.IsPremium)
        {
            PlayingManager.instance.HintGame();
        }
        else if (hintNumber > 0)
        {
            AddHint(-1);
            PlayingManager.instance.HintGame();

            hintPressCount++;
            if (hintPressCount >= AdsConfig.InterstitialEveryHints)
            {
                hintPressCount = 0;
                AdsManager.Interstitial();
            }
        }
        else
        {
            // out of hints: an interstitial unlocks more hints
            AdsManager.Interstitial(() => AddHint(AdsConfig.HintsPerInterstitial));

        }
    }

    private void CompleteMethod(bool completed)
    {
        if (completed == true)
        {
            AddHint(2);
        }
        else
        {
            Debug.Log("NO REWARD");
        }
    }

            public void AddHint(int number)
    {
      
        hintNumber += number;
        PlayerPrefs.SetInt("Hint", hintNumber);
        UpdateTextHint();
    }

    private void UpdateTextHint()
    {
        if (Premium.IsPremium)
        {
            hintText.text = "FREE";
        }
        else if(hintNumber==0)
        {
            hintText.text = "AD";
        }
        else
        {
            hintText.text = hintNumber.ToString() ;
        }
    }

    public void OnEventUndo()
    {
   
        PlayingManager.instance.UndoGame();
    }

    public void OnEventReplay()
    {
//        GoogleMobileAdsScript.instance.ShowInterstitialAd();
        //Advertisements.Instance.ShowInterstitial();
        PlayingManager.instance.ReplayGame();
    }


 

}
