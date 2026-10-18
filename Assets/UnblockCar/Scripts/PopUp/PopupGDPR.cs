using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopUpFrameWork;
using UnityEngine.UI;
using TMPro;
public class PopupGDPR : Popup
{
	public static PopupGDPR instance;


	protected void Awake()
	{
		instance = this;
	}
}
