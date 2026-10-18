using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
namespace ScreenFrameWork
{

    public class DailyBonusScreen : Screen
    {
        public static DailyBonusScreen instance;
        [SerializeField]
        private int pools = 9;
        [SerializeField]
        private Transform dailyContent;

        [SerializeField]
        private GameObject dailyBonusObject;

        [SerializeField]
        private Sprite[] dailyBonusAvatar;
        [SerializeField]
        private int[] dailyBonusReward;

        [SerializeField]
        private Sprite[] ImageButtonCollect;
        [SerializeField]
        private Sprite[] ImageButtonCollectx2;
        [SerializeField]
        private Sprite[] boardDailyGetReward;

        [SerializeField]
        private Image btn_collect;
        [SerializeField]
        private Image btn_collect_x2;



        [SerializeField]
        private TextMeshProUGUI txtRemainTime;


        [SerializeField]
        private bool testMode;

        private const int starUnlockDailyBonus = 15;
        private const int TimeNextDay = 1;
        private DateTime tomorrow;
        [SerializeField]
        private List<DailyObject> dailyObjects = new List<DailyObject>();

        private int dayReward = 0;

        [Header("Debug")]
        [SerializeField]
        private bool pressButtonCollect;
        [SerializeField]
        private bool pressButtonCollectx2;

        private int firstOpenApp;

        private void Awake()
        {
            for (int i = 0; i < pools; i++)
            {
                GameObject daily = Instantiate(dailyBonusObject) as GameObject;
                daily.SetActive(true);
                daily.transform.SetParent(dailyContent, false);
                DailyObject dailyObject = daily.GetComponent<DailyObject>();
                dailyObject.Initialized(string.Format("DAY {0}", (i + 1)), dailyBonusAvatar[i], dailyBonusReward[i].ToString());
                dailyObjects.Add(dailyObject);
            }


        }


        private void Start()
        {
            dayReward = PlayerPrefs.GetInt("DayReward");



            for (int i = 0; i < dayReward; i++)
            {
               
                dailyObjects[i].VisibleHighLight(false);

                dailyObjects[i].EarnBonus(true);

            }

            //Set High Light Current Day
            dailyObjects[dayReward].VisibleHighLight(true);






            //Add Listener Button Collect
            pressButtonCollect = PlayerPrefs.GetInt("GetReward") == 1 ? true : false;
            if (pressButtonCollect)
            {
                DisableButtonCollect(false);
                HideBoardGetBonus();
            }


            //Add Listener Button Collect x2
            pressButtonCollectx2 = PlayerPrefs.GetInt("GetRewardDouble") == 1 ? true : false;
            if (pressButtonCollectx2)
            {
                DisableButtonCollect(true);
                HideBoardGetBonus();
            }

            firstOpenApp = PlayerPrefs.GetInt("FirstLoginApp");


            if (firstOpenApp == 0)
            {

                if (pressButtonCollect || pressButtonCollectx2)
                    //Get reward day 1 and get current time
                    PlayerPrefs.SetInt("FirstLoginApp", 1);

                PlayerPrefs.SetString("TimeGetReward", DateTime.Now.ToString());
                PlayerPrefs.SetString("TimeNextDay", DateTime.Now.ToString());

                dayReward = 0;

                PlayerPrefs.SetInt("DayReward", dayReward);

            }
            else
            {
                CheckEarnDailyBonus();
            }



        }


        private void FixedUpdate()
        {


            if (GameManager.instance.GetTotalStarEarnAllMode >= starUnlockDailyBonus)
            {
                TimeSpan span = GetRemainTime();

                if (-span.Minutes > 0 || -span.Seconds > 0)
                {
                    string hour = (-span.Hours < 10) ? "0" + -span.Hours : (-span.Hours).ToString();
                    string minute = (-span.Minutes < 10) ? "0" + -span.Minutes : (-span.Minutes).ToString();
                    string second = (-span.Seconds < 10) ? "0" + -span.Seconds : (-span.Seconds).ToString();
                    txtRemainTime.text = string.Format(" DAILY BONUS IN {0}:{1}:{2} ", hour, minute, second);

                }
                if (-span.Seconds <= 0)
                {

                    txtRemainTime.text = string.Format(" DAILY BONUS IN {0}:{1}:{2} ", "00", "00", "00");
                }
                CheckEarnDailyBonus();
            }
            else
            {


                txtRemainTime.text = string.Format("Earn {0} Star To Unlock",starUnlockDailyBonus);
                btn_collect.transform.GetChild(0).GetComponent<Image>().sprite = ImageButtonCollect[1];
                btn_collect_x2.sprite = ImageButtonCollectx2[1];
            }

        }

        private TimeSpan GetRemainTime()
        {
            tomorrow = DateTime.Parse(PlayerPrefs.GetString("TimeNextDay"));

            DateTime current = DateTime.Now;

            TimeSpan span = current.Subtract(tomorrow);

            return span;
        }

        private void CheckEarnDailyBonus()
        {

            if (-GetRemainTime().Hours <= 0 && -GetRemainTime().Minutes <= 0 && -GetRemainTime().Seconds <= 0 && (pressButtonCollect || pressButtonCollectx2 || firstOpenApp == 0))
            {
                if (pressButtonCollect || pressButtonCollectx2)
                {
                    dayReward++;
                    if (dayReward >= pools) dayReward = 0;
                    PlayerPrefs.SetInt("DayReward", dayReward);
                }

                if (dayReward == 0)
                {
                    for (int i = 0; i < dailyObjects.Count; i++)
                    {
                      
                        dailyObjects[i].EarnBonus(false);
                    }

                }

 

                pressButtonCollect = false;
                pressButtonCollectx2 = false;
                PlayerPrefs.SetInt("GetReward", 0);
                PlayerPrefs.SetInt("GetRewardDouble", 0);
                btn_collect.transform.GetChild(0).GetComponent<Image>().sprite = ImageButtonCollect[0];
                btn_collect_x2.sprite = ImageButtonCollectx2[0];
                dailyObjects[dayReward].VisibleHighLight(true);





            }
        }


        public void HideBoardGetBonus()
        {
            // effect_gold.SetActive(true);
            //  effect_gold.GetComponent<ParticleSystem>().Play();


           
            dailyObjects[dayReward].VisibleHighLight(false);

            dailyObjects[dayReward].EarnBonus(true);

        }

        private void DisableButtonCollect(bool disableDouble)
        {


            btn_collect.transform.GetChild(0).GetComponent<Image>().sprite = ImageButtonCollect[1];
            if (disableDouble)
                btn_collect_x2.sprite = ImageButtonCollectx2[1];
        }

        private void CompleteEarnReward(bool earnDouble)
        {
            if (earnDouble)
            {



                if (!pressButtonCollectx2)
                {

                    if (dayReward >= dailyObjects.Count) dayReward = 0;

                    if (!pressButtonCollect)
                    {
                        if (testMode)
                        {
                            PlayerPrefs.SetString("TimeNextDay", DateTime.Now.AddSeconds(10).ToString());
                        }
                        else
                        {
                            PlayerPrefs.SetString("TimeNextDay", DateTime.Now.AddDays(TimeNextDay).ToString());
                        }

                    }
                    pressButtonCollectx2 = true;
                }
                DisableButtonCollect(true);
                btn_collect.GetComponent<Button>().onClick.RemoveAllListeners();
                btn_collect_x2.GetComponent<Button>().onClick.RemoveAllListeners();
                PlayerPrefs.SetInt("GetRewardDouble", 1);
            }
            else
            {
                if (!pressButtonCollect)
                {

                    if (dayReward >= dailyObjects.Count) dayReward = 0;

                    if (testMode)
                    {
                        PlayerPrefs.SetString("TimeNextDay", DateTime.Now.AddSeconds(10).ToString());
                    }
                    else
                    {
                        PlayerPrefs.SetString("TimeNextDay", DateTime.Now.AddDays(TimeNextDay).ToString());
                    }
                }
                DisableButtonCollect(false);
                btn_collect.GetComponent<Button>().onClick.RemoveAllListeners();
                PlayerPrefs.SetInt("GetReward", 1);
            }
            PlayerPrefs.SetInt("FirstLoginApp", 1);
            firstOpenApp = 1;
            HideBoardGetBonus();
        }


        public void OnDailyRewardPressed()
        {
            
            if (GameManager.instance.GetTotalStarEarnAllMode < starUnlockDailyBonus) return;
            if (pressButtonCollect || pressButtonCollectx2) return;


            BoosterManager.instance.AddHint(dailyBonusReward[dayReward]);
            //  GameManager.Instance.RewardButton.OnClaimButtonClicked();
         

        
            CompleteEarnReward(false);
            pressButtonCollect = true;
          


        }
        public void OnDailyDoubleRewardPressed()
        {

            if (GameManager.instance.GetTotalStarEarnAllMode < starUnlockDailyBonus) return;
            if (pressButtonCollectx2) return;
            int price = (pressButtonCollect) ? dailyBonusReward[dayReward] : dailyBonusReward[dayReward] * 2;

#if UNITY_EDITOR

            BoosterManager.instance.AddHint(price);
            CompleteEarnReward(true);
            Debug.Log("Add Hint");
            pressButtonCollectx2 = true;
#else

            //GoogleMobileAdsScript.instance.ShowRewardBasedVideo(() => {
              
            //    BoosterManager.instance.AddHint(price);
            //  CompleteEarnReward(true);
            //  pressButtonCollectx2 = true;
            //});
#endif


        }


    }
}
 
