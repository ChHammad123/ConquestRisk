using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;

public class CreateAccount : MonoBehaviour
{
    public GameObject response;

    public GameObject createAccountPanel;
    public GameObject chooseAvatarPanel;

    public Text[] inputFields;

    public Toggle[] allToggles;

    public string[] generalsNames;

    public string[] colors;

    private readonly string addDataURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/addPlayerData";
    private readonly string checkUsernameURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/CheckUsername";

    public void CheckUser()
    {
        StartCoroutine(tryChecking());
    }

    public void TogglePressed(int index)
    {
        for (int i = 0; i < allToggles.Length; i++)
        {
            if (i != index)
                allToggles[i].isOn = false;
        }
    }

    IEnumerator tryChecking()
    {
        WWWForm form = new WWWForm();

        form.AddField("p_Name", inputFields[0].text+"");

        using (UnityWebRequest request = UnityWebRequest.Post(checkUsernameURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                response.SetActive(true);

                Invoke("ResetResponseGameObject", 2f);
            }
            else
            {
                JSONNode data = JSON.Parse(request.downloadHandler.text);
                
                if (data.Count!=0)
                {
                    response.SetActive(true);
                    Invoke("ResetResponseGameObject", 2f);
                }
                else
                {
                    createAccountPanel.SetActive(false);
                    chooseAvatarPanel.SetActive(true);
                }
            }
        }
    }

    public void CreatePlayer()
    {
        StartCoroutine(tryAdding());
    }

    IEnumerator tryAdding()
    {

        WWWForm form = new WWWForm();

        bool isAvatarSelected = false;
        int index = 0;
        for (int i = 0; i < allToggles.Length; i++)
        {
            if (allToggles[i].isOn)
            {
                isAvatarSelected = true;
                index = i;
            }
        }

        if (!isAvatarSelected)
        {
            Debug.Log("Avatar Not Selected");
            yield return null;
        }


        string name = inputFields[0].text.ToString();
        string pass = inputFields[1].text.ToString();

        int r = Random.Range(0, colors.Length);
        string tempColor = colors[r];


        form.AddField("p_Name", name + "");
        form.AddField("p_Password", pass + "");
        form.AddField("p_Color", colors[index] + "");
        form.AddField("p_Avatar", generalsNames[index] + "");

        using (UnityWebRequest request = UnityWebRequest.Post(addDataURL, form))
        {
            Debug.Log("Sending Request");

            yield return request.SendWebRequest();

            Debug.Log("Request Completed");

            if (request.isHttpError || request.isNetworkError)
            {
                createAccountPanel.SetActive(true);
                chooseAvatarPanel.SetActive(false);
            }
            else
            {
                response.SetActive(false);

                JSONNode data = JSON.Parse(request.downloadHandler.text);

                if(data.Count!=0)
                {
                    for(int i=0;i<data.Count;i++)
                    {
                        Debug.Log("Data is " + data[i]);
                    }
                }
                

                PlayerPrefs.SetInt("id", data[0]);
                PlayerPrefs.SetString("username", inputFields[0].text);
                PlayerPrefs.SetString("password", inputFields[1].text);
                PlayerPrefs.SetString("avatar", generalsNames[index] + "");
                PlayerPrefs.SetString("color", colors[index] + "");
                PlayerPrefs.SetInt("loginFirst", 2);
                PlayerPrefs.Save();

                Debug.Log("Player id is " + PlayerPrefs.GetInt("id"));

                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }

    public void AlreadyHaveAccount()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void ResetResponseGameObject()
    {
        response.SetActive(false);
    }

    public void ActivateCreateAccountPanel()
    {
        chooseAvatarPanel.SetActive(false);
        createAccountPanel.SetActive(true);
    }
}
