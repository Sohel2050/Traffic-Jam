using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScrollRectFrameWork;
namespace ScreenFrameWork
{
  
    public class LevelScreen : Screen
    {

        [SerializeField]
        private GameObject levelPagePrefab;
        [SerializeField]
        private List<ScrollRect> levelsScrollRect = new List<ScrollRect>();

        [SerializeField]
        private List<ScrollSnapRect> scrollSnapRects = new List<ScrollSnapRect>();
        [SerializeField]
        private List<LevelPageContainer>  levelPageContainers = new List<LevelPageContainer>();

        [Space()]
        [SerializeField]
        private TextMeshProUGUI title;
        [SerializeField]
        private TextMeshProUGUI pageText;
        [SerializeField]
        private TextMeshProUGUI starInGameMode;
        
        private int currentPositionMode = 0;
        public override void Initialize()
        {

            for (int i = 0; i < GameManager.instance.GetAllGameMode.Count; i++)
            {
                levelPageContainers.Add(new LevelPageContainer());

            }
            base.Initialize();
        }
        public override void Show(bool back, bool immediate)
        {
            HideAllScrollRect();
            title.text = GameManager.GAME_MODE;
            int position = GameManager.instance.levelGameMode;
            currentPositionMode = position;
            starInGameMode.text = GameManager.instance.TotalStarInGameMode(position);
            levelsScrollRect[position].gameObject.SetActive(true);
            if(levelPageContainers[position].pages.Count==0)
            {
                scrollSnapRects[position].Initialized(GameManager.instance.GetTotalPageMode());
                LevelPageContainer container = new LevelPageContainer();
                for (int i = 0; i < 3; i++)
                {
                    GameObject page = Instantiate(levelPagePrefab) as GameObject;
                 
                    LevelPage levelPage = page.GetComponent<LevelPage>();
                    levelPage.Initialized();
                    //levelPage.Show(GameManager.instance.GetListLevelPage(i));
                   // scrollSnapRects[position].AddParent(levelPage.GetComponent<RectTransform>(), i);
               
                    container.pages.Add(levelPage);
                }
                levelPageContainers[position] = container;



                ShowCurrentLevelPage(position);

                scrollSnapRects[position].AddAction(() => { ResetPage(); });
            }
            else
            {
                ShowCurrentLevelPage(position);
           
               
            }
            pageText.text = string.Format("{0}/{1}", scrollSnapRects[currentPositionMode].GetCurrentPage+1, scrollSnapRects[currentPositionMode].GetTotalPage);
            base.Show(back, immediate);

          
        }

        public override void Hide(bool back, bool immediate)
        {
           
                base.Hide(back, immediate);
        }


        private void ShowCurrentLevelPage(int position)
        {
            int currentLevel = GameManager.instance.CurrentGameMode.currentLevel;
            int newPage = Mathf.CeilToInt((float)currentLevel / 9);
            //  Debug.Log("new Page 1 :" + Mathf.RoundToInt((float)currentLevel / 9));
            //  Debug.Log("new Page 2 :" + Mathf.FloorToInt((float)currentLevel / 9));
            //  Debug.Log("new Page 3 :" + Mathf.CeilToInt((float)currentLevel / 9));
            //  Debug.Log("currentLevel :" + currentLevel);
           // Debug.Log("Reset All Page");
            scrollSnapRects[position].SetPage(newPage-1);
            ResetAllPage();

        }

        #region Page
        private void ResetPage()
        {
            int currentPage = scrollSnapRects[currentPositionMode].GetCurrentPage;
            if (currentPage > 0 && currentPage < scrollSnapRects[currentPositionMode].GetDataObjects.Count - 1)
            {
                List<RectTransform> rects = scrollSnapRects[currentPositionMode].GetDataObjects;
                int prevPage = currentPage - 1;
                int nextPage = currentPage + 1;
                if (rects[prevPage].transform.childCount == 0)
                {
                    int next = nextPage + 1;
                    RectTransform rectPrev = rects[next].transform.GetChild(0).GetComponent<RectTransform>();
                    LevelPage levelPage = rectPrev.GetComponent<LevelPage>();
                    
                    levelPage.Show(GameManager.instance.GetListLevelPage(prevPage));
                    scrollSnapRects[currentPositionMode].AddParent(rectPrev, prevPage);
                }
                else if (rects[nextPage].transform.childCount == 0)
                {
                    int prev = prevPage - 1;
                    RectTransform rectNext = rects[prev].transform.GetChild(0).GetComponent<RectTransform>();
                    LevelPage levelPage = rectNext.GetComponent<LevelPage>();
                   
                    levelPage.Show(GameManager.instance.GetListLevelPage(nextPage));
                    scrollSnapRects[currentPositionMode].AddParent(rectNext, nextPage);
                }
            }
            pageText.text = string.Format("{0}/{1}", scrollSnapRects[currentPositionMode].GetCurrentPage+1, scrollSnapRects[currentPositionMode].GetTotalPage);
        }
        private void ResetAllPage()
        {
            int currentPage = scrollSnapRects[currentPositionMode].GetCurrentPage;

          
            int prevPage = currentPage - 1;
            prevPage = prevPage < 0 ? 2 : prevPage; 
            int nextPage = currentPage + 1;
            nextPage = nextPage >= scrollSnapRects[currentPositionMode].GetDataObjects.Count ? currentPage - 2 : nextPage;

            List<RectTransform> rects = new List<RectTransform>();
            rects.Add(scrollSnapRects[currentPositionMode].GetDataObjects[currentPage]);
            rects.Add(scrollSnapRects[currentPositionMode].GetDataObjects[nextPage]);
            rects.Add(scrollSnapRects[currentPositionMode].GetDataObjects[prevPage]);

            List<int> positions = new List<int>();
            positions.Add(currentPage);
            positions.Add(prevPage);
            positions.Add(nextPage);
            for (int i = 0; i < levelPageContainers[currentPositionMode].pages.Count; i++)
            {
                LevelPage levelPage = levelPageContainers[currentPositionMode].pages[i];
                RectTransform rectPage = levelPage.GetComponent<RectTransform>();
                scrollSnapRects[currentPositionMode].AddParent(rectPage, positions[i]);
                levelPage.Show(GameManager.instance.GetListLevelPage(positions[i]));
            }
            pageText.text = string.Format("{0}/{1}", scrollSnapRects[currentPositionMode].GetCurrentPage + 1, scrollSnapRects[currentPositionMode].GetTotalPage);
        }
        #endregion
 
        private void HideAllScrollRect()
        {
            for (int i = 0; i < levelsScrollRect.Count; i++)
            {
                levelsScrollRect[i].gameObject.SetActive(false);
            }
        }

    }
}
