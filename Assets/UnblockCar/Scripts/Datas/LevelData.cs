using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BlockInfor
{

    public bool blockMain = false;
    public int x;
    public int y;
    public int width;
    public int height;


    public int GetImagePosition()
    {
        if (blockMain)
        {
            return 0;
        }
        return Mathf.Max(width, height) - 1;
    }

    public int GetMax()
    {
        return Mathf.Max(width, height);
    }

    public int GetWidth()
    {
        if (blockMain)
        {
           
            return 2;
        }
        return width;
    }

    public int GetHeight()
    {
        if (blockMain)
        {

            return 1;
        }
        return height;
    }

    public bool IsPortrait()
    {
        if (height > width) return false;
        return true;
    }
}
[System.Serializable]
public class LevelData
{
    public List<BlockInfor> blockInfors = new List<BlockInfor>();

    public string GetData()
    {
        return JsonHelper.ToJson<BlockInfor>(blockInfors.ToArray());
    }
   
}
