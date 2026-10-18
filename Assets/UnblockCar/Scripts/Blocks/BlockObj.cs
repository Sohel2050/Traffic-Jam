using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class BlockObj : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer image;
    [SerializeField]
    private int blockRange = 1;
    [SerializeField]
    private bool horizontal = false;
    [SerializeField]
    private bool blockSpecial = false;
    [SerializeField]
    private GameObject fadeObj;
    [SerializeField]
    private GameObject hintObj;
    [SerializeField]
    private GameObject boxsObj;
    [SerializeField]
    private List<GameObject> boxs = new List<GameObject>();
 
    public int GetIntPositionY { get { return (int)transform.position.y; } }
    public int GetIntPositionX { get { return (int)transform.position.x; } }
    public int GetBlockRange { get { return blockRange; } }
    public bool GetHorizontal { get { return horizontal; } }
    public bool GetBlockSpecial { get { return blockSpecial; } }

    private Vector3 winTarget = new Vector3(8, 3, 0);
    private Vector3 velocity = Vector3.zero;
    private float smoothSpeed = 0.75f;
    private bool isUpdate = false;

    private Vector3 originPosition = Vector3.zero;

    

    private void Update()
    {
        if (!isUpdate) return;
        transform.position = Vector3.SmoothDamp(transform.position, winTarget, ref velocity, smoothSpeed);
    }
    public void SetUp(Vector3 position,int index,bool move =false)
    {
        isUpdate = false;
        originPosition = position;
        Vector3 newPosition = originPosition;
        newPosition.x += 10;
        transform.position =(move)? newPosition:originPosition;
        if (move)
        {
            LeanTween.move(gameObject, originPosition, GameManager.speedMoveBlock).setDelay(0.2f*index).setOnComplete(completed=> {
                UpdateBoxGrid();

            });
        }
        else
        {
            UpdateBoxGrid();
              
        }
        VisibleImageObj(true);
        VisibleHintObj(false);
        VisibleFade(false);
    }


    public void DeActiveMoveEffect()
    {
        isUpdate = false;
    }
    public void UpdateBoxGrid()
    {
        int startX = GetIntPositionX;

        int startY = GetIntPositionY;
        for (int i = 0; i < boxs.Count; i++)
        {
           
            if (horizontal)
            {
                int x = startX + i;
                PlayingManager.instance.GridInGame[startY].grids[x] = boxs[i];
                boxs[i].transform.name = string.Format("Grid_{0}_{1}", startY, x);
            }
            else
            {
                int y = startY + i;
                PlayingManager.instance.GridInGame[y].grids[startX] = boxs[i];
                boxs[i].transform.name = string.Format("Grid_{0}_{1}", y, startX);
            }
         
        }
    }


    public Vector3 GetHintPosition(int move)
    {
        Vector3 position = transform.position;
        if(horizontal)
        {
            position.x += move;
        }
        else
        {
            position.y += move;
        }
        return position;

    }

    public void WinGame()
    {
        isUpdate = true;
    }

    /// <summary>
    /// Limit position can move
    /// </summary>
    public int PointCanMoveInLine(bool max)
    {
        int startX = GetIntPositionX;
        int startY = GetIntPositionY;
        int y = GetIntPositionY;
        int width = GameManager.gridWidth;
        int height = GameManager.gridHeight;
        int limit = 0;
        if (max)
        {

            if (horizontal)
            {
                startX += blockRange;
                int count = 0;
                for (int i = startX; i < width; i++)
                {
                    if (PlayingManager.instance.GridInGame[y].grids[i] != null)
                    {
                        return GetIntPositionX + count;
                    }
                    count++;
                }
                limit = GetIntPositionX + count;
            }
            else
            {
                startY += blockRange;
                int count = 0;
                for (int i = startY; i < height; i++)
                {
                    if (PlayingManager.instance.GridInGame[i].grids[startX] != null)
                    {
                        return GetIntPositionY + count;
                    }
                    count++;
                }
               
                limit = GetIntPositionY + count;
            }
        }
        else
        {
            if (horizontal)
            {
                for (int i = startX - 1; i >= 0; i--)
                {
                    if (PlayingManager.instance.GridInGame[y].grids[i] != null)
                    {
                        return i + 1;
                    }
                }
                limit = 0;
            }
            else
            {
                for (int i = startY - 1; i >= 0; i--)
                {
                    if (PlayingManager.instance.GridInGame[i].grids[startX] != null)
                    {
                        return i + 1;
                    }
                }
                limit = 0;
            }
        }

        return limit;
    }

    public void VisibleImageObj(bool visible)
    {
        boxsObj.SetActive(visible);
        image.gameObject.SetActive(visible);
    }
    public void VisibleHintObj(bool visible)
    {
     
        hintObj.SetActive(visible);
    }

    public void VisibleFade(bool visible)
    {
        fadeObj.SetActive(visible);
        boxsObj.SetActive(!visible); 
       
    }

    public void VisibleBoxs(bool visible)
    {
        
        boxsObj.SetActive(visible);

    }
}
