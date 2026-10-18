using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockLevelShow : MonoBehaviour
{
    [SerializeField]
    private RectTransform rect;
    [SerializeField]
    private Image image;

    public Image GetImage { get { if (image == null) image = GetComponent<Image>(); return image; } }

    public RectTransform GetRect { get { if (rect == null) rect = GetComponent<RectTransform>();  return rect; } }


    
}
