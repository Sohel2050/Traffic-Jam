using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DailyObject : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI txtDay;
    [SerializeField]
    private GameObject goldIcon;
    [SerializeField]
    private TextMeshProUGUI txtReward;
    [SerializeField]
    private GameObject check;
    [SerializeField]
    private GameObject highLight;
 

    public void Initialized(string day, Sprite icon, string reward)
    {
        txtDay.text = day;

        txtReward.text = string.Format("+{0} Hint",reward);
        check.SetActive(false);
        
    }

    public void VisibleHighLight(bool visible)
    {
        highLight.SetActive(visible);
    }

  
    public void EarnBonus(bool earn)
    {
        check.SetActive(earn);
        goldIcon.gameObject.SetActive(!earn);
        //transform.GetChild(3).gameObject.SetActive(!earn);
    }
}
