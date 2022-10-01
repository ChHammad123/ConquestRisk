using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingRoomSinglePlayer : MonoBehaviour
{
    public TextMeshProUGUI AiText;

    public Button ForwardButton; 

    public string[] difficulties;

    [SerializeField]
    private PlayerDataHandlerForSingleplayer[] allPlayers;

    public int sceneIndex = 0;

    private void Start()
    {
        string name = PlayerPrefs.GetString("username");

        allPlayers[0].parent.SetActive(true);

        allPlayers[0].name.text = name;

        string avatarName = PlayerPrefs.GetString("avatar");

        int AvatarNo = 1;

        for (int k = 0; k < allPlayers[0].Avatars.Length; k++)
        {
            allPlayers[0].Avatars[k].SetActive(false);
        }

        for (int k = 0; k < allPlayers[0].Avatars.Length; k++)
        {
            if (allPlayers[0].Avatars[k].Equals(avatarName))
            {
                AvatarNo = k;
                break;
            }
        }

        allPlayers[0].Avatars[AvatarNo].SetActive(true);

    }

    private void Update()
    {
        if(allPlayers[1].parent.activeSelf)
        {
            ForwardButton.image.color = Color.white;
            ForwardButton.interactable = true;
        }
        else
        {
            ForwardButton.image.color = Color.grey;
            ForwardButton.interactable = false;
        }
    }

    public void AddRobot()
    {
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (!allPlayers[i].parent.activeInHierarchy)
            {
                allPlayers[i].parent.SetActive(true);

                allPlayers[i].AiDifficulty.text = AiText.text;
                break;
            }
        }
    }

    public void RemoveRobot()
    {

        for (int i = 1; i < allPlayers.Length; i++)
        {
            if (!allPlayers[i].parent.activeInHierarchy)
            {
                if (i == 1)
                    return;
                else
                {
                    allPlayers[i - 1].parent.SetActive(false);
                }

                break;
            }
        }

        allPlayers[5].parent.SetActive(false);
    }

    public void goBack()
    {
        SceneManager.LoadScene(3);
    }

    public void StartGame()
    {
        int cnt = 0;

        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i].parent.activeInHierarchy)
            {
                cnt++;

                PlayerPrefs.SetInt("idSP" + i, allPlayers[i].player_id);
                PlayerPrefs.SetString("usernameSP" + i, allPlayers[i].name.text);
                PlayerPrefs.SetString("colorSP" + i, allPlayers[i].color);

                if (allPlayers[i].AiDifficulty != null)
                    PlayerPrefs.SetString("aiSP" + i, allPlayers[i].AiDifficulty.text);
                else
                    PlayerPrefs.SetString("aiSP" + i, "null");  

                for (int j = 0; j < 6; j++) 
                {
                    if (allPlayers[i].Avatars[j].activeInHierarchy)
                    {
                        PlayerPrefs.SetInt("avatarSP" + i, (j + 1));
                        break;
                    }
                }

                PlayerPrefs.Save();
            }
            else
                break;
        }

        PlayerPrefs.SetInt("allplayers",cnt);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneIndex);
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

}
