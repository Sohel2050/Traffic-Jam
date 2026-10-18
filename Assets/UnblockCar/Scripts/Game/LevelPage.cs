using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelPageContainer
{
    public List<LevelPage> pages = new List<LevelPage>();
}

public class LevelPage : MonoBehaviour
{
    [SerializeField]
    private GameObject levelItemPrefab;
    [SerializeField]
    private List<Level> levels = new List<Level>();
   
    [SerializeField]
    private List<LevelListItem> levelListItems = new List<LevelListItem>();
    public void Initialized()
    {
        levelListItems.Clear();
        for (int i = 0; i < GameManager.totalLevelInPage; i++)
        {
            GameObject obj = Instantiate(levelItemPrefab) as GameObject;
            obj.transform.SetParent(transform, false);
            LevelListItem level = obj.GetComponent<LevelListItem>();
            levelListItems.Add(level);

        }
    }

    public void Show(List<Level> levelsInfor)
    {
        levels = levelsInfor;
        for (int i = 0; i < levelListItems.Count; i++)
        {
            if(i>= levelsInfor.Count)
            {
                levelListItems[i].gameObject.SetActive(false);
                continue;
            }
            else
            {
                levelListItems[i].gameObject.SetActive(true);
            }
            levelListItems[i].Setup(levelsInfor[i]);
        }
    }
}
