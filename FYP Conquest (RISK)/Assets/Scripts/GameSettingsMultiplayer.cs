using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using SimpleJSON;

public class GameSettingsMultiplayer : MonoBehaviour
{
    public int sceneIndex = 0;

    public TextMeshProUGUI MPText;
    public TextMeshProUGUI TurnTimeText;

    public string[] turnTime;

    private readonly string createRoomURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/createRoom";

    private void Start()
    {
        turnTime = new string[3];

        turnTime[0] = "60";
        turnTime[1] = "90";
        turnTime[2] = "120";
    }

    public void goBack()
    {
        SceneManager.LoadScene(3);
    }

    public void ManualPlacement()
    {
        if (MPText.text.Equals("OFF"))
        {
            MPText.text = "ON";
        }
        else
        {
            MPText.text = "OFF";
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

    public void CreateRoom()
    {
        StartCoroutine(tryCreating());
    }

    IEnumerator tryCreating()
    {
        WWWForm form = new WWWForm();

        form.AddField("ROOM_ID", 1);
        form.AddField("TURN_TIME", "90");
        form.AddField("PLAYER_STATUS", "waiting");
        form.AddField("JOINED_PLAYER_ID", PlayerPrefs.GetInt("id"));
        form.AddField("ROOM_OWNER", PlayerPrefs.GetInt("id"));

        using (UnityWebRequest request = UnityWebRequest.Post(createRoomURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error Creating the Room ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);

                PlayerPrefs.SetString("roomKey", data[0]);
                PlayerPrefs.Save();

                Debug.Log("Room key is " + PlayerPrefs.GetString("roomKey"));
                
                SceneManager.LoadScene(8);
            }
        }
    }

}
