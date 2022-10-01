using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameSettingsButtonsHandler : MonoBehaviour
{
    public int sceneIndex = 0;

    public TextMeshProUGUI MPText;
    public TextMeshProUGUI AiText;
    public TextMeshProUGUI TurnTimeText;

    public string[] difficulties;
    public string[] turnTime;


    public void goBack()
    {
        SceneManager.LoadScene(3);
    }

    private void Start()
    {
        difficulties = new string[3];
        turnTime = new string[3];

        difficulties[0] = "Passive";
        difficulties[1] = "Aggressive";
        difficulties[2] = "Chaotic";

        turnTime[0] = "60";
        turnTime[1] = "90";
        turnTime[2] = "120";
    }

    public void ManualPlacement()
    {
        if(MPText.text.Equals("OFF"))
        {
            MPText.text = "ON";
        }
        else
        {
            MPText.text = "OFF";
        }
    }

    public void AiDifficultyRight()
    {
        for (int i = 0; i < 3; i++) 
        {
            if (difficulties[i].Equals(AiText.text)) 
            {
                if ((i + 1) >= 3)
                {
                    AiText.text = difficulties[0];
                    break;
                }

                else
                {
                    AiText.text = difficulties[(i + 1)];
                    break;
                }
            }
        }
    }

    public void AiDifficultyLeft()
    {
        for (int i = 0; i < 3; i++)
        {
            if (difficulties[i].Equals(AiText.text)) 
            {
                if ((i - 1) == -1)
                {
                    AiText.text = difficulties[2];
                    break;
                }

                else
                {
                    AiText.text = difficulties[(i - 1)];
                    break;
                }
            }
        }
    }

    public void TurnTimeRight()
    {
        for (int i = 0; i < 3; i++)
        {
            if (turnTime[i].Equals(TurnTimeText.text))
            {
                if ((i + 1) >= 3)
                {
                    TurnTimeText.text = turnTime[0];
                    break;
                }

                else
                {
                    TurnTimeText.text = turnTime[(i + 1)];
                    break;
                }
            }
        }
    }

    public void TurnTimeLeft()
    {
        for (int i = 0; i < 3; i++)
        {
            if (turnTime[i].Equals(TurnTimeText.text))
            {
                if ((i - 1) == -1)
                {
                    TurnTimeText.text = turnTime[2];
                    break;
                }

                else
                {
                    TurnTimeText.text = turnTime[(i - 1)];
                    break;
                }
            }
        }
    }

    public void StartGameplay()
    {
        PlayerPrefs.SetString("MP",MPText.text);
        PlayerPrefs.SetString("Ai",AiText.text);
        PlayerPrefs.SetString("TT",TurnTimeText.text);
        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}
