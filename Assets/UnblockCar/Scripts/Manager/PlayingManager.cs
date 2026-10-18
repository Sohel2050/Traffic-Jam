using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class UndoAction
{
    public GameObject block;
    public Vector3 prevPosition;

    public void Action()
    {
        block.transform.position = prevPosition;
    }
}
public class PlayingManager : MonoHandler
{
    public static PlayingManager instance;
    [SerializeField]
    private List<GridContainer> gridInGame = new List<GridContainer>();
    [SerializeField]
    private List<GameObject> blocksPrefab = new List<GameObject>();
    [SerializeField]
    private Transform blockContainer;
    [SerializeField]
    private GameObject arrowImage;
    [SerializeField]
    private List<BlockObj> blockObjsInGame = new List<BlockObj>();
    [SerializeField]
    private List<Button> buttonsInGame = new List<Button>();

    private Vector2 FirstSelectPoint;
    private Vector2 FirstBlockPoint;
    private Vector3 selectedPositionBlock = Vector3.zero;
    private List<Vector3>  listStartPosition = new List<Vector3>();
    private List<UndoAction> undoActions = new List<UndoAction>();
  
    private BlockObj blockSpecial;
  
    [Header("Debug ")]
    [SerializeField] private GameObject selectedBlock;
    [SerializeField] private BlockObj shadowBlock;
    [SerializeField] private bool isMoveBeginGame = false;
    [SerializeField]
    private int blockMovePointMax;
    [SerializeField]
    private int blockMovePointMin;
    [SerializeField] private List<BlockObj> blockObjsWidth2 = new List<BlockObj>();
    [SerializeField] private List<BlockObj> blockObjsWidth3 = new List<BlockObj>();
    [SerializeField] private List<BlockObj> blockObjsHeight2 = new List<BlockObj>();
    [SerializeField] private List<BlockObj> blockObjsHeight3 = new List<BlockObj>();
    private bool blockDrag = false;
    private Vector3 originPositionBlock;
    private int countHint = 0;
    private bool isHint = false;
    private bool isPause = false;
    private float startDrag = 0;
    private float endDrag =  0;
    private Vector3 startInput;
    private Vector3 endInPut;
    [SerializeField]
    private bool autoMoveBlock = false;
    [SerializeField]
    private bool triggerAutoMoveBlock = false;
    private bool isWinGame = false;

    public List<GridContainer> GridInGame
    {
        get
        {
            return gridInGame;
        }
    }


  

    private void Awake()
    {
        instance = this;
    }
   
 
    private void Update()
    {
        SelectedAndDragBlock();
    }
    private void SelectedAndDragBlock()
    {
        if (!isMoveBeginGame) return;
        if (isPause) return;
        if (isWinGame) return;
        Vector2 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Ray2D ray2D = new Ray2D(touchPosition, Vector2.zero);
        RaycastHit2D raycastHit2D = Physics2D.Raycast(ray2D.origin, ray2D.direction, 0f, 1);
  

      
        if (Input.touchCount > 1) return;

        
        if (Input.GetMouseButtonDown(0) && !triggerAutoMoveBlock)
        {
            FirstSelectPoint = touchPosition;
            startDrag = Time.time;
            startInput = touchPosition;
            if (selectedBlock != null  )
            {
                return;
            }

            if (raycastHit2D.collider != null && raycastHit2D.transform.gameObject.tag == "Block"  )
            {
                selectedBlock = raycastHit2D.transform.parent.parent.gameObject;
                BlockObj blockObj = selectedBlock.GetComponent<BlockObj>();
                SoundManager.instance.TouchBlock();
                blockDrag = true;
             
                originPositionBlock = selectedBlock.transform.position;
          
                FirstBlockPoint = selectedBlock.transform.position;
                //Get Point Block Can Move In Line
                blockMovePointMax =  blockObj.PointCanMoveInLine(true);
                blockMovePointMin =  blockObj.PointCanMoveInLine(false);

            }

        }



        if (blockDrag && selectedBlock != null && !triggerAutoMoveBlock)
        {
            BlockObj blockObj = selectedBlock.GetComponent<BlockObj>();
            float x = selectedBlock.transform.position.x;
            float y = selectedBlock.transform.position.y;
            if (blockObj.GetHorizontal)
            {
                 x = (FirstSelectPoint - touchPosition).x;
                selectedPositionBlock = new Vector3(FirstBlockPoint.x - x, y, 0f);
                selectedBlock.transform.position = new Vector3(FirstBlockPoint.x - x, y, 0f);

                if (selectedBlock.transform.position.x < (float)blockMovePointMin)
                {
                    selectedPositionBlock = new Vector3((float)blockMovePointMin, y, 0f);
                    selectedBlock.transform.position = new Vector3((float)blockMovePointMin, y, 0f);

                }
                else if (selectedBlock.transform.position.x > (float)(blockMovePointMax))
                {
                    selectedPositionBlock = new Vector3((float)(blockMovePointMax), y, 0f);
                    selectedBlock.transform.position = new Vector3((float)(blockMovePointMax), y, 0f);

                }


             
            }
            else
            {
                y = (FirstSelectPoint - touchPosition).y;
                selectedPositionBlock = new Vector3(x, FirstBlockPoint.y- y, 0f);

             
                selectedBlock.transform.position = new Vector3(x, FirstBlockPoint.y- y, 0f);
             
                if (selectedBlock.transform.position.y < (float)blockMovePointMin)
                {
                    selectedPositionBlock = new Vector3(x, (float)blockMovePointMin, 0f);
                    selectedBlock.transform.position = new Vector3(x, (float)blockMovePointMin, 0f);

                }
                else if (selectedBlock.transform.position.y > (float)(blockMovePointMax))
                {
                    selectedPositionBlock = new Vector3(x, (float)(blockMovePointMax), 0f);
                    selectedBlock.transform.position = new Vector3(x, (float)(blockMovePointMax), 0f);
                 

                }
            }

            CheckWinGame(blockObj);
          
        }



        
        if (Input.GetMouseButtonUp(0) && !triggerAutoMoveBlock)
        {

          
            float timeDrag = Time.time - startDrag;
            if (selectedBlock == null)
            {
                return;
            }
            if (timeDrag>=0.1f&&  timeDrag <=0.25f && !isHint )
            {
                //Drag
                float dst =   Vector3.Distance(startInput, touchPosition);
                autoMoveBlock = true;
                triggerAutoMoveBlock = true;
                BlockObj blockObj = selectedBlock.GetComponent<BlockObj>();
              
                if(blockObj.GetHorizontal)
                {
                    int value = selectedPositionBlock.x>= originPositionBlock.x  ?1:-1;
                  //  Debug.Log(string.Format("{0}---{1}----{2}", value, selectedPositionBlock.x, originPositionBlock.x));
                    selectedPositionBlock.x = Mathf.RoundToInt(selectedPositionBlock.x + (dst*value));
                  
                    if (selectedPositionBlock.x > blockMovePointMax) selectedPositionBlock.x = blockMovePointMax;
                    if (selectedPositionBlock.x < blockMovePointMin) selectedPositionBlock.x = blockMovePointMin;
                }
                else
                {
                    int value = selectedPositionBlock.y >= originPositionBlock.y ? 1 : -1;
                  //  Debug.Log(string.Format("YYY: {0}---{1}----{2}", value, selectedPositionBlock.y, originPositionBlock.y));
                    selectedPositionBlock.y = Mathf.RoundToInt(selectedPositionBlock.y + (dst * value));
 
                    if (selectedPositionBlock.y > blockMovePointMax) selectedPositionBlock.y= blockMovePointMax;
                    if (selectedPositionBlock.y < blockMovePointMin) selectedPositionBlock.y = blockMovePointMin;
                }
                LeanTween.move(selectedBlock, selectedPositionBlock, 0.3f).setOnComplete(completed => {

                    autoMoveBlock = false;
                    triggerAutoMoveBlock = true;

                });
            }
            else
            {
                autoMoveBlock = false;
                triggerAutoMoveBlock = true;
            }
        }

        if(!autoMoveBlock && triggerAutoMoveBlock  )
        {
            NewPositionSelectedBlock();
            triggerAutoMoveBlock = false;
        }
    }

    private bool CheckWinGame( BlockObj blockObj)
    {
        if (blockObj.GetBlockSpecial && (int)selectedBlock.transform.position.x >= (GameManager.gridWidth - 4))
        {
            bool canWin = gridInGame[3].grids[4] == null && gridInGame[3].grids[5] == null;
            if (gridInGame[3].grids[4] != null && gridInGame[3].grids[5] == null)
            {
                GameObject obj = gridInGame[3].grids[4].transform.parent.parent.gameObject;
                if (blockObj.gameObject == obj)
                {
                    canWin = true;
                }
            }

            if (canWin)
            {
                isWinGame = true;
                //Win Game
                blockObj.WinGame();
                arrowImage.SetActive(false);
                Timer.Schedule(this, 1f, () => { WinGame(); });

                selectedBlock = null;
                return true;
            }
        }
        return false;
    }


    private void NewPositionSelectedBlock()
    {

        blockDrag = false;

        if (selectedBlock == null)
        {
            return;
        }

        int startX = (int)originPositionBlock.x;
        int endX = (int)selectedPositionBlock.x;
        int startY = (int)originPositionBlock.y;
        int endY = (int)selectedPositionBlock.y;
        bool addUndo = true;
        if (startX == endX && startY == endY)
        {
            addUndo = false;
        }
        if (addUndo)
        {
            //Undo Action
            UndoAction undoAction = new UndoAction();
            undoAction.block = selectedBlock;
            undoAction.prevPosition = originPositionBlock;
            undoActions.Add(undoAction);
            if (!isHint)
                buttonsInGame[2].interactable = true;
            GameManager.instance.UpdateMove();
        }

        Vector3 lastPosition = new Vector3(Mathf.RoundToInt(selectedPositionBlock.x), Mathf.RoundToInt(selectedPositionBlock.y), 0);

        if (isHint)
        {
            startX = (int)lastPosition.x;
            startY = (int)lastPosition.y;
            int shadowX = shadowBlock.GetIntPositionX;
            int shadowY = shadowBlock.GetIntPositionY;
            if (startX == shadowX && startY == shadowY)
            {
                Level level = GameManager.instance.CurrentLevel;
                countHint++;
                if (countHint >= level.hintContainer.hints.Count)
                {
                    FinishHintGame();
                }
                else
                {
                    int position = level.hintContainer.hints[countHint].blockIndex;
                    ShowHintGame(position, countHint);
                }
                SoundManager.instance.BlockPlace();
                selectedBlock.transform.position = lastPosition;
            }
            else
            {
                SoundManager.instance.BlockPlace();
                selectedBlock.transform.position = originPositionBlock;
            }


        }
        else
        {
            SoundManager.instance.BlockPlace();
            selectedBlock.transform.position = lastPosition;
        }



        UpdateAllGrid();
        BlockObj blockObj = selectedBlock.GetComponent<BlockObj>();
        CheckWinGame(blockObj);
        selectedBlock = null;
        
    }


    private BlockObj GetBlockObj(bool isPortrait, int width, bool blockSpecial)
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

                    if (!blockObjsHeight3[i].gameObject.activeInHierarchy)
                    {
                        Debug.Log("blockObjsHeight3[i] : " + blockObjsHeight3[i].gameObject.activeInHierarchy);
                        return blockObjsHeight3[i];
                    }
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
        BlockObj blockObj = obj.GetComponent<BlockObj>();
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

    private void ShowHintGame(int position,int numberHint)
    {
        for (int i = 0; i < blockObjsInGame.Count; i++)
        {
            blockObjsInGame[i].VisibleFade(true);
        }
       
        blockObjsInGame[position].VisibleFade(false);
        Level level = GameManager.instance.CurrentLevel;

        if(shadowBlock)
        {
            
            shadowBlock.gameObject.SetActive(false);
            shadowBlock.VisibleImageObj(true);
            shadowBlock.VisibleHintObj(false);
  
        }
        shadowBlock = GetBlockObj(blockObjsInGame[position].GetHorizontal, blockObjsInGame[position].GetBlockRange, false);
        shadowBlock.name = "Block_Shadow_"+blockObjsInGame[position].name;
        shadowBlock.VisibleHintObj(true);
        shadowBlock.VisibleFade(false);
        shadowBlock.VisibleImageObj(false);
        shadowBlock.gameObject.SetActive(true);
      
        shadowBlock.transform.position = blockObjsInGame[position].GetHintPosition(level.hintContainer.hints[numberHint].numberMove);
        HandMove.instance.DeActiveHandMove();
        HandMove.instance.ActiveHandMove(blockObjsInGame[position].transform.position, shadowBlock.transform.position);
    }

    private void WinGame()
    {
        SoundManager.instance.WinGameSound();
       
        for (int i = 0; i <blockObjsInGame.Count ; i++)
        {
            blockObjsInGame[i].VisibleBoxs(false);
            blockObjsInGame[i].gameObject.SetActive(false);
        }

        VisibleButton(false);
        GameManager.instance.WinGame();
        
    }

    private void VisibleButton(bool visible)
    {
        for (int i = 0; i < buttonsInGame.Count; i++)
        {
            buttonsInGame[i].interactable = visible;
        }
    }

    private void FinishHintGame()
    {
        for (int i = 0; i < blockObjsInGame.Count; i++)
        {
            blockObjsInGame[i].VisibleFade(false);
        }
        isHint = false;
        VisibleButton(true);
        HandMove.instance.DeActiveHandMove();
        shadowBlock.gameObject.SetActive(false);
    }

   
  
    public void PlayGame(float waitTime  =0.06f)
    {

        isPause = false;
        blockObjsInGame.Clear();
        listStartPosition.Clear();
 
        Timer.Schedule(this, waitTime, () =>
        {


            Reset();
            Level currentLevel = GameManager.instance.CurrentLevel;
            for (int i = 0; i < currentLevel.levelData.blockInfors.Count-1; i++)
            {
                BlockInfor infor = currentLevel.levelData.blockInfors[i];
                if (i == 0)
                {
                    infor.blockMain = true;
                }
                BlockObj blockObj = GetBlockObj(infor.IsPortrait(), infor.GetMax(), infor.blockMain);
            
                blockObj.gameObject.SetActive(true);
              
                if (infor.y >= GameManager.gridHeight) infor.y = GameManager.gridHeight - 1;
                if (infor.x >= GameManager.gridWidth) infor.x = GameManager.gridWidth - 1;



                int x = infor.x;
               

                blockObj.name = string.Format("Block_{0}", (i));
                Vector3 position = new Vector3(x, infor.y, 0);
                blockObj.SetUp(position,i+1);
              
              
                blockObjsInGame.Add(blockObj);
                listStartPosition.Add(position);
            }

            
        });
 
    }

    public void Reset(bool move = false)
    {

        isWinGame = false;
        arrowImage.SetActive(true);
        isMoveBeginGame = move;
        if (!move)
        {
            VisibleButton(false);
        }
        undoActions.Clear();
        buttonsInGame[2].interactable = false;
        isHint = false;
        countHint = 0;
        //CLear All Obj In Grid
        selectedBlock = null;
        if (shadowBlock != null)
        {
            shadowBlock.gameObject.SetActive(false);
            shadowBlock = null;
        }
        Timer.Schedule(this, (GameManager.speedMoveBlock-0.05f) * ((float)GameManager.instance.CurrentLevel.levelData.blockInfors.Count/1.7f ), () =>
        {
            
            isMoveBeginGame = true;
            VisibleButton(isMoveBeginGame);
        });

    }
    public void HintGame()
    {
        ReplayGame(false);
        UpdateAllGrid();
        Level level = GameManager.instance.CurrentLevel;
        int position = level.hintContainer.hints[0].blockIndex ;
        buttonsInGame[1].interactable = false;
        buttonsInGame[2].interactable = false;
        isHint = true;
        ShowHintGame(position,0);
    }

    public void HideAllBlock()
    {
        for (int i = 0; i < blockObjsInGame.Count; i++)
        {
            blockObjsInGame[i].DeActiveMoveEffect();
              blockObjsInGame[i].VisibleBoxs(false);
            blockObjsInGame[i].gameObject.SetActive(false);
        }
        if (shadowBlock != null)
        {
            shadowBlock.gameObject.SetActive(false);
            shadowBlock = null;
        }
        HandMove.instance.DeActiveHandMove();
    }

    public void ReplayGame(bool move =true)
    {
        Reset(!move);
        ClearAllGrid();
        for (int i = 0; i < blockObjsInGame.Count; i++)
        {
            blockObjsInGame[i].SetUp(listStartPosition[i],i+1, false);
            blockObjsInGame[i].gameObject.SetActive(true);
          
        }

        HandMove.instance.DeActiveHandMove();
        if(shadowBlock!=null)
        shadowBlock.gameObject.SetActive(false);

     
       
        
        GameManager.instance.PlayGame();
    }

    public void OnEventPause(bool pause)
    {
        isPause = pause;
    }

    
    public void UndoGame()
    {
        if (undoActions.Count > 0)
        {
           
            undoActions[undoActions.Count - 1].Action();
            undoActions.RemoveAt(undoActions.Count - 1);
            
            UpdateAllGrid();
            GameManager.instance.UpdateMove();
        }
        if (undoActions.Count <= 0)
        {
            buttonsInGame[2].interactable = false;
        }
    }

    private void UpdateAllGrid()
    {
        ClearAllGrid();
        for (int i = 0; i < blockObjsInGame.Count; i++)
        {
            blockObjsInGame[i].UpdateBoxGrid();
        }
    }

    private void ClearAllGrid()
    {
        for (int i = 0; i < GameManager.gridHeight; i++)
        {

            for (int j = 0; j < GameManager.gridWidth; j++)
            {
                gridInGame[i].grids[j] = null;
            }

        }
    }
 
}
