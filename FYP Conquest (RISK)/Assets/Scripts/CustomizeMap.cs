using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomizeMap : MonoBehaviour
{
    [SerializeField]
    public CustomizeTerritoryDataHolder[] AllTerritories;

    public int LoadSceneIndex = 0;
    
    public int MainMenuSceneIndex = 0;

    private void Start()
    {
        int i = 0;

        i = PlayerPrefs.GetInt("IsMapCustomized");

        if (i == 1)
        {
            SetTerritories();
        }
        else
        {
            ResetTerritories();
        }

    }

    public void SetTerritories()
    {
        for (int i = 0; i < AllTerritories.Length; i++)
        {
            AllTerritories[i].Territory_id = PlayerPrefs.GetInt("Tid" + i);
            AllTerritories[i].Territory_name = PlayerPrefs.GetString("Tname" + i);

            int state = PlayerPrefs.GetInt("Tstate" + i);

            if (state == 1)
            {
                AllTerritories[i].TerritoryActiveState = true;

                AllTerritories[i].TerritoryImage.color = Color.white;
            }

            else
            {
                AllTerritories[i].TerritoryActiveState = false;

                string tempColor = "044954";

                Color OutputColor;
                ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                AllTerritories[i].TerritoryImage.color = OutputColor;
            }
        }
    }

    public void ResetTerritories()
    {
        for (int i = 0; i < AllTerritories.Length; i++)
        {
            AllTerritories[i].TerritoryActiveState = true;
            AllTerritories[i].TerritoryImage.color = Color.white;
        }
    }

    public void ToggleTerritoryState(int TerritoryId)
    {
        for (int i = 0; i < AllTerritories.Length; i++)
        {
            if (AllTerritories[i].Territory_id == TerritoryId)
            {
                string tempColor = "044954";

                Color OutputColor;
                ColorUtility.TryParseHtmlString("#" + tempColor, out OutputColor);

                AllTerritories[i].TerritoryImage.color = OutputColor;

                AllTerritories[i].TerritoryActiveState = false;

                /*if (AllTerritories[i].TerritoryActiveState)
                {
                }
                else
                {
                    AllTerritories[i].TerritoryImage.color = Color.white;
                    AllTerritories[i].TerritoryActiveState = true;
                }*/

                break;
            }
        }
    }   

    public void SaveCustomizeMapData()
    {
        PlayerPrefs.SetInt("IsMapCustomized", 1);

        for(int i=0;i<AllTerritories.Length;i++)
        {
            PlayerPrefs.SetInt("Tid" + i,AllTerritories[i].Territory_id);
            PlayerPrefs.SetString("Tname" + i,AllTerritories[i].Territory_name);

            if (AllTerritories[i].TerritoryActiveState)
                PlayerPrefs.SetInt("Tstate" + i, 1);
            else
                PlayerPrefs.SetInt("Tstate" + i, 0);
        }

        PlayerPrefs.Save();

        SceneManager.LoadScene(LoadSceneIndex);
    }

    public void DiscardCustomizeMapData()
    {
        SceneManager.LoadScene(MainMenuSceneIndex);
    }
}
