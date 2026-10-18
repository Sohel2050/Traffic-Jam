using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ScrollRectFrameWork;
using TMPro;

namespace ScreenFrameWork
{
    public class PackScreen : Screen
    {
       
        [SerializeField]
        private PackListItem packItemPrefab;
        [SerializeField]
        private RectTransform packListContainer;
        [SerializeField]
        private ScrollRect packScrollRect;
        [SerializeField]
        private TextMeshProUGUI starAllModeText;
        private float expandAnimDuration = 0.5f;

        private ExpandableListHandler<GameModeInfor> expandableListHandler;

      

        public override void Show(bool back, bool immediate)
        {
            if (expandableListHandler == null)
            {
                expandableListHandler = new ExpandableListHandler<GameModeInfor>(GameManager.instance.GetAllGameMode, packItemPrefab, packListContainer, packScrollRect, expandAnimDuration);
                expandableListHandler.Setup();
            }
            else
            {
                expandableListHandler.Refresh();
            }
            starAllModeText.text = GameManager.instance.TotalStarInAllMode();
            base.Show(back, immediate);
        }


       
    }
}
