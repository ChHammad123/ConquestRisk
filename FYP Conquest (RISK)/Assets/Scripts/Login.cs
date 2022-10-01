using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public GameObject response;

    public Text[] inputFields;

    
    private readonly string loginURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/login";

    public void LoginPlayer()
    {
        StartCoroutine(tryLogin());
    }

    IEnumerator tryLogin()
    {
        WWWForm form = new WWWForm();
        string name = inputFields[0].text;
        string pass = inputFields[1].text;

        form.AddField("p_Name", name);
        form.AddField("p_Password", pass);

        using (UnityWebRequest request = UnityWebRequest.Post(loginURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError )
            {
                inputFields[0].text = " ";
                inputFields[1].text = " ";

                response.SetActive(true);

                Invoke("ResetResponseGameObject", 2f);
            }
            else
            {

                response.SetActive(false);

                JSONNode data = JSON.Parse(request.downloadHandler.text);

                if (data.Count != 0)
                {
                    for(int i=0;i<data.Count;i++)
                        Debug.Log("Every Data is " + data[i]);

                    PlayerPrefs.SetInt("id", data[0]);
                    PlayerPrefs.SetString("username", data[1]);
                    PlayerPrefs.SetString("password", data[2]);
                    PlayerPrefs.SetString("color", data[3]);
                    PlayerPrefs.SetString("avatar", data[4]);
                    PlayerPrefs.SetInt("loginFirst", 2);
                    PlayerPrefs.Save();

                    Debug.Log("Player Id is "+PlayerPrefs.GetInt("id"));

                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
                else
                {
                    Debug.Log("Login Error");
                }
            }


        }
    }

    public void dontHaveAccount()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    public void ResetResponseGameObject()
    {
        response.SetActive(false);
    }
}
