using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(GridLayoutGroup))]
public class GridLayoutGroupSupport : MonoBehaviour,IWindowChangeAction
{
    //Grid Layout Group Support Cell Size full Screen
    private GridLayoutGroup layoutGroup;

    [SerializeField]
    private RectTransform rectDefault;
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        layoutGroup = GetComponent<GridLayoutGroup>();
        MultipleWindow.instance.AddWindowChange(this);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectDefault);
        layoutGroup.cellSize = new Vector2(rectDefault.rect.width, rectDefault.rect.height);
    }

    public void WindowChangeStart()
    {
        
    }

    public void WindowChangeUpdate()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectDefault);
        layoutGroup.cellSize = new Vector2(rectDefault.rect.width, rectDefault.rect.height);
    }


}
