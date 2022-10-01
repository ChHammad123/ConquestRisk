using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingRoom : MonoBehaviour
{
    public GameObject ForwardButton;

    public TextMeshProUGUI roomKeyText;

    public TextMeshProUGUI totalPlayers;

    public int gameplaySceneIndex;

    public string[] allAvatars;

    public PlayerDataHolder[] allPlayers;

    private readonly string synchDataURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/getDataForWaitingRoom";
    private readonly string leaveRoomURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/leaveRoom";
    private readonly string RemoveRoomURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/RemoveRoom";
    private readonly string getRoomHostURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/getRoomHost";

    private readonly string startRoomURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/StartRoom";
    private readonly string checkRoomStatusURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/checkRoomStatus";
    private readonly string ForwardButtonURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/getRoom";
    private readonly string changePlayerColorURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/changeColor";



    private int id;
    private string name;
    private string color;

    void Start()
    {
        ForwardButton.SetActive(false);

        StartCoroutine(SynchData());
        StartCoroutine(checkForwardButton());
        StartCoroutine(checkRoomStatus()); 
    }

    // For Synching Room Data

    IEnumerator SynchData()
    {
        bool flag = true;

        while (flag)
        {
            WWWForm form = new WWWForm();

            string roomKey = PlayerPrefs.GetString("roomKey");

            Debug.Log("Making Request with room key " + roomKey);

            form.AddField("ROOM_Key", roomKey);

            using (UnityWebRequest request = UnityWebRequest.Post(synchDataURL, form))
            {
                yield return request.SendWebRequest();

                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("Error Getting the Room Data ...  ");
                }
                else
                {
                    JSONNode data = JSON.Parse(request.downloadHandler.text);

                    totalPlayers.text = data.Count.ToString() + "/6";

                    if (data.Count != 0)
                    {
                        roomKeyText.text = roomKey;

                        int i=0;

                        for (i = 0; i < data.Count; i++)
                        {

                            allPlayers[i].player_id = data[i]["p_ID"];

                            // Activating Player Slot
                            allPlayers[i].parent.SetActive(true);

                            //string tempColor = data[i]["p_Color"];

                            //Color OutputColor;
                            //ColorUtility.TryParseHtmlString(tempColor, out OutputColor);

                            //allPlayers[i].bg.color = OutputColor;

                            // Assigning name
                            allPlayers[i].name.text = data[i]["p_Name"];

                            // Deactivating Avatar

                            for (int k = 0; k < allPlayers[i].Avatars.Length; k++)
                            {
                                allPlayers[i].Avatars[k].SetActive(false);
                            }

                            // Activating Avatar

                            string avatarName = data[i]["p_Avatar"];
                            int AvatarNo = 1;

                            for (int k=0;k<allAvatars.Length;k++)
                            {
                                if(allAvatars[k].Equals(avatarName))
                                {
                                    AvatarNo = k;
                                    break;
                                }

                            }

                            allPlayers[i].Avatars[AvatarNo].SetActive(true);

                        }
                        // Making Extra slots unactive
                        for(; i<6;i++)
                        {
                            allPlayers[i].parent.SetActive(false);
                        }
                    }

                    else
                        SceneManager.LoadScene(3);  // If Room is Deleted
                }
            }
            
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator checkForwardButton()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        Debug.Log("Making Request with room key " + roomKey);

        form.AddField("ROOM_Key", roomKey);

        using (UnityWebRequest request = UnityWebRequest.Post(ForwardButtonURL, form))
        {
            Debug.Log("Sending Request");
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Setting the Forward Button");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);

                int playerId = PlayerPrefs.GetInt("id");

                if (data[4].Equals(playerId))
                {
                    ForwardButton.SetActive(true);
                }
            }
        }
    }


    // For Leaving The Room

    public void LeaveRoom()
    {
        StartCoroutine(tryCheckingForHost());
    }

    IEnumerator tryCheckingForHost()
    {
        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);

        using (UnityWebRequest request = UnityWebRequest.Post(getRoomHostURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Leaving the Room ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);

                int playerId = PlayerPrefs.GetInt("id");

                if (data[4].Equals(playerId))
                {
                    StartCoroutine(tryRemoving());
                }
                else
                {
                    StartCoroutine(tryLeaving());
                }

                SceneManager.LoadScene(3);
            }
        }
    }

    IEnumerator tryLeaving()
    {
        Debug.Log("Leaving Room");

        WWWForm form = new WWWForm();

        int playerId = PlayerPrefs.GetInt("id");

        form.AddField("JOINED_PLAYER_ID", playerId);

        using (UnityWebRequest request = UnityWebRequest.Post(leaveRoomURL, form))
        {
            Debug.Log("Sending Request");

            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Leaving the Room ...  ");
            }
            else
            {
                Debug.Log("Request Completed");

                SceneManager.LoadScene(3);
            }
        }
    }

    IEnumerator tryRemoving()
    {

        Debug.Log("Removing Room");

        WWWForm form = new WWWForm();

        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);


        using (UnityWebRequest request = UnityWebRequest.Post(RemoveRoomURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Leaving the Room ...  ");
            }
            else
            {
                SceneManager.LoadScene(3);
            }
        }
    }


    // For Starting The Room

    public void StartGame()
    {
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (!allPlayers[i].parent.activeInHierarchy)
                break;

            id = allPlayers[i].player_id;
            name = allPlayers[i].name.text;
            color = allPlayers[i].color;

            ChangeColorForPlayers();
        }

        StartCoroutine(tryStarting());
    }


    public void ChangeColorForPlayers()
    {
        StartCoroutine(tryChangingColors());
    }


    IEnumerator tryChangingColors()     // Colors will be assigned to players before starting the room
    {
        WWWForm form = new WWWForm();

        form.AddField("p_ID", id);
        form.AddField("p_Name", name);
        form.AddField("p_Color", color);

        using (UnityWebRequest request = UnityWebRequest.Post(changePlayerColorURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Starting the Room ...  ");
            }
            else
            {
                Debug.Log("Color Changed for player name " + name + " of color " + color);
            }
        }

    }


    IEnumerator tryStarting()
    {
        WWWForm form = new WWWForm();

        int playerId = PlayerPrefs.GetInt("id");
        string roomKey = PlayerPrefs.GetString("roomKey");

        form.AddField("ROOM_Key", roomKey);
        form.AddField("JOINED_PLAYER_ID", playerId);

        using (UnityWebRequest request = UnityWebRequest.Post(startRoomURL, form))
        {
            Debug.Log("Sending Request");

            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Starting the Room ...  ");
            }
            else
            {
                Debug.Log("Request Completed");

                SceneManager.LoadScene(gameplaySceneIndex);
            }
        }
    }

    IEnumerator checkRoomStatus()
    {
        while (true)
        {
            WWWForm form = new WWWForm();

            form.AddField("ROOM_KEY", PlayerPrefs.GetString("roomKey"));

            using (UnityWebRequest request = UnityWebRequest.Post(checkRoomStatusURL, form))
            {
                yield return request.SendWebRequest();

                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("Error While Starting the Room ...  ");
                }
                else
                {
                    JSONNode data = JSON.Parse(request.downloadHandler.text);

                    string status = data[0];

                    if (status.Equals("ingame")) 
                        SceneManager.LoadScene(gameplaySceneIndex);
                    else
                        Debug.Log("Status is " + status);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

}
