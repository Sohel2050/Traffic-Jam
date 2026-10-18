using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {

            SoundManager.instance.OnEventButtonSound();
        });
    }
}
