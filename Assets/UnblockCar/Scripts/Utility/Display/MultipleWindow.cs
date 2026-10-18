using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleWindow : MonoBehaviour
{

    public static MultipleWindow instance;
    private int currentWidth;
    private int currentHeight;


    private float beginUpdate = 0;

    private float durationUpdate = 1;
    private List<IWindowChangeAction> windows = new List<IWindowChangeAction>();

    public void AddWindowChange(IWindowChangeAction window)
    {
        windows.Add(window);
    }

    
    private void Awake()
    {
        instance = this;
        currentHeight = Screen.height;
        currentWidth = Screen.width;
    }

    private void Update()
    {
        if(currentHeight!=Screen.height || currentWidth != Screen.width)
        {
            currentHeight = Screen.height;
            currentWidth = Screen.width;

           
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].WindowChangeStart();
            }

            beginUpdate = Time.time + durationUpdate;





        }

        if(beginUpdate-Time.time>=0)
        {

            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].WindowChangeUpdate();
            }
        }
    }
}
