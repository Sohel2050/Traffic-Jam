using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class DataInGame 
{
  


    public int starInGame = 0;
    public int goldInGame = 0;
    public int sound = 0;
    public int music = 0;
    public int currentBeginnerLevel = 0;
    public int currentMediumLevel = 0;
    public int currentAdvanceLevel = 0;
    public int currentExpertLevel = 0;
    public int currentMasterLevel = 0;


  
    public void SaveData (DataInGame  data)
    {
       string json = JsonUtility.ToJson(data);
       PlayerPrefs.SetString("DataInGame", json);
    }

   


    public DataInGame Load()
    {
        DataInGame dataInGame = new DataInGame();
        string json = PlayerPrefs.GetString("DataInGame", string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            dataInGame = SetValueDefault();
        }
        else
        {
            dataInGame = JsonUtility.FromJson<DataInGame>(json);
        }
        return dataInGame;
    }

    private DataInGame SetValueDefault()
    {
        DataInGame data = new DataInGame();
        data.starInGame = 0;

        data.goldInGame = 100;
        data.sound = 1;
        data.music = 1;
        data.currentBeginnerLevel = 0;
        data.currentMediumLevel = 0;
        data.currentAdvanceLevel = 0;
        data.currentExpertLevel = 0;
        data.currentMasterLevel = 0;

        return data;
    }
}
