using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopUpFrameWork;
using TMPro;
public class PopupWin : Popup
{

    [SerializeField]
    private TextMeshProUGUI moveText;
    [SerializeField]
    private TextMeshProUGUI bestMoveText;

    [SerializeField]
    private List<Animation> stars = new List<Animation>();


    public override void OnShowing(object[] inData)
    {
        HideAllStar();
        int star = (int)inData[0];
 
        StartCoroutine(ShowStar(star));
        moveText.text = string.Format("move:{0}",(int)inData[1]);
        bestMoveText.text = string.Format("best:{0}", (int)inData[2]);
        base.OnShowing(inData);
    }

    private IEnumerator ShowStar(int star)
    {
        for (int i = 0; i < star; i++)
        {
            yield return new WaitForSeconds(.35f);
            stars[i].gameObject.SetActive(true);
            stars[i].Play();
            Timer.Schedule(this, 0.25f * (i+1), () => { SoundManager.instance.StarCompletedSound(); });
        }
    }

    private void HideAllStar()
    {
        for (int i = 0; i < stars.Count; i++)
        {
            stars[i].gameObject.SetActive(false);
            stars[i].transform.localScale = Vector3.zero;
        }
    }
}