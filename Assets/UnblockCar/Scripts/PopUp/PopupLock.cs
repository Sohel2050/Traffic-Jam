using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopUpFrameWork;
using UnityEngine.UI;
using TMPro;
public class PopupLock : Popup
{

	public static PopupLock instance;


	protected void Awake()
	{
		instance = this;
	}

	[SerializeField]
    private TextMeshProUGUI starHaveText;


    public void ShowStarHaveText(string numStar)
    {
		starHaveText.text = numStar;

    }
}
