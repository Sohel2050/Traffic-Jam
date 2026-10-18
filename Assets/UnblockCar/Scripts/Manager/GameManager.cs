using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PopUpFrameWork;

[System.Serializable]
public class GridContainer
{
    public List<GameObject> grids = new List<GameObject>();
}
[System.Serializable]
public class GameModeInfor
{
    public string nameGameMode;
    public int idGameMode;
    public Sprite bannerGameMode;
    public int starRequest = 1;
    public int totalStarInMode = 0;
    public int currentLevel;
 
    public List<TextAsset> datas = new List<TextAsset>();
    public List<TextAsset> hintsData = new List<TextAsset>();

   


    public void SaveLevel()
    {
      
        PlayerPrefs.SetInt(string.Format("Mode_{0}",idGameMode),currentLevel);
    }
    public void LoadLevel()
    {
       currentLevel = PlayerPrefs.GetInt(string.Format("Mode_{0}", idGameMode), 1);
     
    }

    

    public void SaveTotalStar()
    {
        PlayerPrefs.SetInt(string.Format("TotalStar_{0}", idGameMode), totalStarInMode);
    }


    public void LoadTotalStar()
    {
        totalStarInMode = PlayerPrefs.GetInt(string.Format("TotalStar_{0}", idGameMode), 0);
    }
}
[System.Serializable]
public class Level
{
    public int levelStage;
    public int levelShowStage;
    public int gameMode;

    [HideInInspector]
    public int star;
    [HideInInspector]
    public int valueAddTotalStar;

    [HideInInspector]
    public string data;
    [HideInInspector]
    public string hintData;


    public LevelData levelData = new LevelData();
    public HintContainer hintContainer = new HintContainer();


    public void Load()
    {
        LoadHint();
        LoadBlock();
    }
    private void LoadHint()
    {
        if (hintContainer.hints.Count > 0) return;
        HintInfor[] hintInfors = JsonHelper.FromJson<HintInfor>(hintData);
       
       
        for (int i = 0; i < hintInfors.Length; i++)
        {
            hintContainer.hints.Add(hintInfors[i]);
        }
    }
    private void LoadBlock()
    {
        if (levelData.blockInfors.Count > 0) return;
        BlockInfor[] blocksInfor = JsonHelper.FromJson<BlockInfor>(data);

       
        for (int i = 0; i < blocksInfor.Length; i++)
        {
            levelData.blockInfors.Add(blocksInfor[i]);
        }
    }
    public void SaveStar(int starEarn)
    {
        valueAddTotalStar = 0;
        if (starEarn > star)
        {
            valueAddTotalStar = starEarn - star;
            star = starEarn;
            PlayerPrefs.SetInt(string.Format("Star_{0}_{1}", gameMode, levelStage), starEarn);
        }

    }
    public int GetStar()
    {
       return  PlayerPrefs.GetInt(string.Format("Star_{0}_{1}", gameMode, levelStage), 0);
    }


    public void SaveBestMove(int bestMove)
    {
        PlayerPrefs.SetInt(string.Format("Move_{0}_{1}", gameMode, levelStage), bestMove);
    }
    public int LoadBestMove()
    {
       return  PlayerPrefs.GetInt(string.Format("Move_{0}_{1}", gameMode, levelStage), 0);
    }
}
public class GameManager : MonoBehaviour
{
    public static string GAME_MODE;
    public const int gridWidth = 6;
    public const int gridHeight = 6;
    public const float speedMoveBlock = 0.5f;
    public const int totalLevelInPage = 15;
    public static GameManager instance;
    public const int maxWatchInterstitial = 2;



    [SerializeField]
    private List<GameModeInfor> gameModeInfors = new List<GameModeInfor>();

    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private TextMeshProUGUI packText;
    [SerializeField]
    private TextMeshProUGUI moveText;
    [SerializeField]
    private TextMeshProUGUI bestMoveText;

    private Dictionary<int, List<Level>> AllLevels = new Dictionary<int, List<Level>>();
    private int move;
    private int bestMove;
    private int totalStarEarnAllMode;
    private int totalStarAllMode;
    private int countInterstitial = 0;
    public DataInGame DataInGame { get; set; }

    public List<GameModeInfor> GetAllGameMode { get { return gameModeInfors; } }
    public Level CurrentLevel;//{ get; private set; }
    public GameModeInfor CurrentGameMode { get; set; }
    public int levelGameMode;//{ get; private set; }
    public List<Level> CurrentLevelsInGameMode;//{ get; private set; }
    [SerializeField]
    private bool testMode = false;
    public int GetTotalStarEarnAllMode { get { return totalStarEarnAllMode; } }

    private void Awake()
    {


        //PopupManager.instance.Show("gdpr");

        instance = this;
        totalStarAllMode = 0;
        int totalStage = 0;
        for (int i = 0; i < gameModeInfors.Count; i++)
        {
            GameModeInfor game = gameModeInfors[i];

            game.LoadTotalStar();
            totalStarAllMode += (game.datas.Count * 3);
            totalStarEarnAllMode += game.totalStarInMode;
            AllLevels.Add(i, new List<Level>());
            if (i > 0)
            {
                totalStage += gameModeInfors[i - 1].datas.Count;

            }
            game.LoadLevel();
            for (int j = 0; j < game.datas.Count; j++)
            {
                Level level = new Level();
                level.data = game.datas[j].text;
                level.hintData = game.hintsData[j].text;
                level.levelStage = j + 1;
                level.levelShowStage = totalStage + j + 1;
                level.gameMode = game.idGameMode;
                AllLevels[i].Add(level);
            }
        }
        if (testMode)
        {
            totalStarEarnAllMode = 1000;
        }
        DataInGame = new DataInGame();
        DataInGame = DataInGame.Load();
    }


    private int CaculatorStarLevel()
    {
        int limitMove = CurrentLevel.hintContainer.hints.Count + 1;

        if (move <= limitMove) return 3;
        if (move <= (limitMove + 3)) return 2;
        return 1;
    }
    public void SelectedGameMode(int position)
    {
        CurrentGameMode = gameModeInfors[position];
        levelGameMode = position;
        CurrentLevelsInGameMode = AllLevels[position];
    }

    public List<Level> GetListLevelPage(int position)
    {
        List<Level> levels = new List<Level>();
        int start = position * totalLevelInPage;
        int end = (position + 1) * totalLevelInPage;
        for (int i = start; i < end; i++)
        {
            if (i >= CurrentLevelsInGameMode.Count) break;
            levels.Add(CurrentLevelsInGameMode[i]);
        }
        return levels;
    }

    public int GetTotalPageMode()
    {
        float total = CurrentLevelsInGameMode.Count + 5;
        //Debug.Log("GetTotalPageMode :" + total);
        return Mathf.RoundToInt(total / totalLevelInPage);
    }
    public string TotalStarInGameMode(int position)
    {

        int totalStarMode = AllLevels[position].Count * 3;

        return string.Format("{0}/{1}", gameModeInfors[position].totalStarInMode, totalStarMode);

    }

    public string TotalStarInAllMode()
    {

        return string.Format("{0}/{1}", totalStarEarnAllMode, totalStarAllMode);

    }

    public string TotalStarEarnAllMode()
    {
        return string.Format("{0}", totalStarEarnAllMode);
    }


    public void PlayGame()
    {
        levelText.text = string.Format("{0}", CurrentLevel.levelShowStage);
        packText.text = GameManager.GAME_MODE;
        moveText.text = string.Format("0");
        move = 0;
        bestMove = CurrentLevel.LoadBestMove();
        bestMoveText.text = string.Format("{0}/{1}", (bestMove==0?"--":bestMove.ToString()),CurrentLevel.hintContainer.hints.Count);
    }
    public void UpdateMove()
    {
        move++;
        moveText.text = string.Format("{0}", move);
    }


    public void SetBestMove()
    {
        if(bestMove==0)
        {
            bestMove = move;
            
           
            CurrentLevel.SaveBestMove(bestMove);
        }
        else if (bestMove > move)
        {
            bestMove = move;
            CurrentLevel.SaveBestMove(bestMove);
        }


    }
    public List<Level> GetLevelGameMode(int position)
    {
        return AllLevels[position];

    }

    public void NextLevel()
    {
        countInterstitial++;
        if(AdsConfig.EnableLevelInterstitial && countInterstitial >= maxWatchInterstitial)
        {
            countInterstitial = 0;
            AdsManager.Interstitial();
        }
        int level = CurrentLevel.levelStage;
        int nextLevel = level++;
        int gameMode = CurrentLevel.gameMode;
        if (nextLevel>AllLevels[gameMode].Count)
        {
            nextLevel = 0;
            gameMode++;
            if(gameMode>= gameModeInfors.Count)
            {
                gameMode = 0; 
            }
        }
        CurrentLevel = AllLevels[gameMode][nextLevel];
        CurrentLevel.Load();
        PlayGame();
        PlayingManager.instance.PlayGame();
        
    }


 

    public void WinGame()
    {
        SetBestMove();
        System.Object[] datasSend = new System.Object[3];
        int star = CaculatorStarLevel();
       
        datasSend[0] = star;
        datasSend[1] = move;
        datasSend[2] = bestMove;

        PopupManager.instance.Show("win",datasSend);


        CurrentLevel.SaveStar(star);

        int level = CurrentLevel.levelStage;
       
        int nextLevel = level + 1;
           
        int gameMode = CurrentLevel.gameMode;
        gameModeInfors[gameMode].totalStarInMode += CurrentLevel.valueAddTotalStar;
        totalStarEarnAllMode+= CurrentLevel.valueAddTotalStar;
        gameModeInfors[gameMode].SaveTotalStar();


        if (nextLevel > gameModeInfors[gameMode].datas.Count)
        {
            nextLevel = gameModeInfors[gameMode].datas.Count - 1;
        }
        
        if (gameModeInfors[gameMode].currentLevel < nextLevel)
        {
            gameModeInfors[gameMode].currentLevel = nextLevel;
            gameModeInfors[gameMode].SaveLevel();
        }
    }

    public int GetLevelCompleteMode(int position)
    {
        int levelComplete = gameModeInfors[position].currentLevel;

        return levelComplete;
    }
   
}
