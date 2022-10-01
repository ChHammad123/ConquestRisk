using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    public Text roomId;

    public int ROOM_ID;
    public string ROOM_Key;
    public int TURN_TIME;
    public string PLAYER_STATUS;
    public int JOINED_PLAYER_ID;
    public int ROOM_OWNER;

    private readonly string searchRoomURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/getRoom";
    private readonly string joinRoomURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/joinRoom";
    private readonly string checkRoomStatusURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/checkRoomStatus";

    public void goBack()
    {
        SceneManager.LoadScene(3);
    }

    public void goCreateRoom()
    {
        SceneManager.LoadScene(7);
    }

    public void JoinRoom()
    {
        StartCoroutine(TrySearching());
    }

    IEnumerator TrySearching()
    {
        WWWForm form = new WWWForm();

        string roomKey = roomId.text;

        Debug.Log("Making Request with room key " + roomKey);

        form.AddField("ROOM_Key", roomKey+"");

        using (UnityWebRequest request = UnityWebRequest.Post(searchRoomURL, form))
        {
            Debug.Log("Sending Request");
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("No Room Found ..... ");
            }
            else
            {
                Debug.Log("Request Data");

                JSONNode data = JSON.Parse(request.downloadHandler.text);

                Debug.Log("Checking Data");

                if (data.Count != 0)
                {
                    PlayerPrefs.SetString("roomKey", roomId.text);
                    PlayerPrefs.Save();

                    Debug.Log(data[1]);

                    Debug.Log("1");
                    ROOM_ID = int.Parse(data[0]);

                    Debug.Log("2");
                    ROOM_Key = data[1];

                    Debug.Log("3");
                    TURN_TIME = int.Parse(data[2]);


                    Debug.Log("6");
                    ROOM_OWNER = int.Parse(data[4]);

                    StartCoroutine(CheckRoomStatus());      
                }
                else
                {
                    Debug.Log("No Room Found ..... ");
                }
            }
        }
    }

    IEnumerator CheckRoomStatus()
    {
        Debug.Log("Checking Room Status with Room id + "+ROOM_ID);

        WWWForm form = new WWWForm();

        form.AddField("ROOM_KEY", ROOM_Key);

        using (UnityWebRequest request = UnityWebRequest.Post(checkRoomStatusURL, form))
        {
            Debug.Log("Sending Request");

            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Joining the Room ...  ");
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);

                Debug.Log("Request Completed");

                if (data.Count != 0) 
                {

                    for (int i = 0; i < data.Count; i++)
                        Debug.Log(data[i]);

                    if (data[0].Equals("waiting"))
                    {

                        Debug.Log("Room Not Started Joining it");
                        StartCoroutine(TryJoining());
                    }
                    else
                    {
                        Debug.Log("Room is Already Started");
                    }
                }
                else
                {

                    Debug.Log("Error While Joining the Room ...  ");
                }
            }
        }
    }
 
    IEnumerator TryJoining()
    {
        WWWForm form = new WWWForm();


        form.AddField("ROOM_ID", ROOM_ID);
        form.AddField("ROOM_Key", ROOM_Key);
        form.AddField("TURN_TIME", TURN_TIME);
        form.AddField("PLAYER_STATUS", "waiting");
        form.AddField("JOINED_PLAYER_ID", PlayerPrefs.GetInt("id"));
        form.AddField("ROOM_OWNER", ROOM_OWNER);

        using (UnityWebRequest request = UnityWebRequest.Post(joinRoomURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Error While Joining the Room ...  ");
            }
            else
            {
                Debug.Log("Room key is " + PlayerPrefs.GetString("roomKey"));

                SceneManager.LoadScene(8);
            }
        }
    }
}
