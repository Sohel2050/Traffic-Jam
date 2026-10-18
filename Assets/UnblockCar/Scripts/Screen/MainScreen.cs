using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopUpFrameWork;
namespace ScreenFrameWork {
    public class MainScreen : Screen
    {
        private void Awake() {
            Application.targetFrameRate = 60;
        }


        public void ShowPopupSeting()
        {
            PopupManager.instance.Show("settings");
        }
    }
}
