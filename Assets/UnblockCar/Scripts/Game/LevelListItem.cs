using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScrollRectFrameWork;
using UnityEngine.UI;
using ScreenFrameWork;
using TMPro;
public class LevelListItem : MonoBehaviour
{
 
    [SerializeField]
    private TextMeshProUGUI levelText ;
    [SerializeField]
    private GameObject starObj;
    [SerializeField]
    private GameObject completeObj;
    [SerializeField]
    private GameObject lockedObj;
    [SerializeField]
    private GameObject fadeObj;
    [SerializeField]
    private GameObject highLightObj;

    [SerializeField]
    private Transform blockContainer;
    [SerializeField]
    private GameObject[] starsLight;
 

    [Space()]
 
 
    [SerializeField]
    private List<GridContainer> gridContainers = new List<GridContainer>();
 
    [SerializeField]
    private bool locked = false;
    [SerializeField]
    private bool current = false;
    [SerializeField]
    private bool completed = false;

    private Level dataObject;
    [SerializeField]

    private List<GameObject> blocksPrefab = new List<GameObject>();
    [SerializeField]
    private List<BlockLevelShow> blockObjsInLevel = new List<BlockLevelShow>();
    private List<BlockLevelShow> blockObjsWidth2 = new List<BlockLevelShow>();
    private List<BlockLevelShow> blockObjsWidth3 = new List<BlockLevelShow>();
    private List<BlockLevelShow> blockObjsHeight2 = new List<BlockLevelShow>();
    private List<BlockLevelShow> blockObjsHeight3 = new List<BlockLevelShow>();
    private BlockLevelShow blockSpecial;
 

    public  void Setup(Level dataObject)
    {


        VisibleBlocks(false);
        levelText.text = dataObject.levelShowStage.ToString();

        dataObject.Load();
        this.dataObject = dataObject;
        SetUpLevel(dataObject);
        SetUpBoard(dataObject.levelData);

    }

    private void SetUpLevel(Level dataObject)
    {
        int levelCurrent = GameManager.instance.GetLevelCompleteMode(dataObject.gameMode) ;
        
        int starEarn = dataObject.GetStar();
        locked = dataObject.levelStage > (levelCurrent) && !Premium.IsPremium;
        current = levelCurrent == dataObject.levelStage;
        completed = levelCurrent > dataObject.levelStage;
        lockedObj.SetActive(locked);
        fadeObj.SetActive(locked);
        starObj.SetActive(completed);
        completeObj.SetActive(completed);
        highLightObj.SetActive(current);
        if (completed)
        {
            for (int i = 0; i < starsLight.Length; i++)
            {
                starsLight[i].SetActive(true);
            }
            for (int i = starEarn; i < starsLight.Length; i++)
            {
                starsLight[i].SetActive(false);
            }
        }
    }

    private void SetUpBoard(LevelData levelData)
    {
        Timer.Schedule(this, .02f, () =>
        {
            for (int i = 0; i < levelData.blockInfors.Count-1; i++)
            {
                BlockInfor infor = levelData.blockInfors[i];
                if(i==0)
                {
                    infor.blockMain = true;
                }
                BlockLevelShow blockObj = GetBlockObj(infor.IsPortrait(), infor.GetMax(), infor.blockMain);
                blockObjsInLevel.Add(blockObj);
              
                if (infor.y >= GameManager.gridHeight) infor.y = GameManager.gridHeight - 1;
                if (infor.x >= GameManager.gridWidth) infor.x = GameManager.gridWidth - 1;

                blockObj.gameObject.SetActive(true);
                blockObj.transform.SetParent(blockContainer, false);
                int x = infor.x;
                blockObj.transform.position = gridContainers[infor.y].grids[x].transform.position;

            }
        });
    }

 

    private BlockLevelShow GetBlockObj(bool isPortrait, int width, bool blockSpecial)
    {
        int type = GetBlockType(isPortrait, width, blockSpecial);
        switch (type)
        {
            case 0:
                for (int i = 0; i < blockObjsWidth2.Count; i++)
                {
                    if (!blockObjsWidth2[i].gameObject.activeInHierarchy) return blockObjsWidth2[i];
                }
                break;
            case 1:
                for (int i = 0; i < blockObjsWidth3.Count; i++)
                {
                    if (!blockObjsWidth3[i].gameObject.activeInHierarchy) return blockObjsWidth3[i];
                }
                break;
            case 2:
                for (int i = 0; i < blockObjsHeight2.Count; i++)
                {
                    if (!blockObjsHeight2[i].gameObject.activeInHierarchy) return blockObjsHeight2[i];
                }
                break;
            case 3:
                for (int i = 0; i < blockObjsHeight3.Count; i++)
                {
                    if (!blockObjsHeight3[i].gameObject.activeInHierarchy) return blockObjsHeight3[i];
                }
                break;
            case 4:
                if (this.blockSpecial != null)
                {
                    return this.blockSpecial;
                }
                break;
        }



        GameObject obj = Instantiate(blocksPrefab[type]);
        obj.transform.SetParent(blockContainer, false);
        BlockLevelShow blockObj = obj.GetComponent<BlockLevelShow>();
        switch (type)
        {
            case 0:
                blockObjsWidth2.Add(blockObj);
                break;
            case 1:
                blockObjsWidth3.Add(blockObj);
                break;
            case 2:
                blockObjsHeight2.Add(blockObj);
                break;
            case 3:
                blockObjsHeight3.Add(blockObj);
                break;
            case 4:
                this.blockSpecial = blockObj;
                break;
        }


        return blockObj;
    }

    private int GetBlockType(bool isPortrait, int width, bool blockSpecial)
    {

        if (blockSpecial)
        {
            return 4;
        }
        //Get Block Type
        int type = 0; //default block width 2
        if (isPortrait)
        {
            //block width
            if (width == 3)
            {
                type = 1;
            }
        }
        else
        {
            //block height
            if (width == 2)
            {
                type = 2;
            }
            else
            {
                type = 3;
            }
        }

        return type;
    }

    private void VisibleBlocks(bool visible)
    {
        for (int i = 0; i < blockObjsInLevel.Count; i++)
        {
            blockObjsInLevel[i].gameObject.SetActive(visible);
        }

        blockObjsInLevel.Clear();
    }


  
    public void OnEventClicked()
    {
        if (locked) return;
       
        ScreenManager.Instance.Show("game");
        GameManager.instance.CurrentLevel = dataObject;
        GameManager.instance.PlayGame();
    }


}
