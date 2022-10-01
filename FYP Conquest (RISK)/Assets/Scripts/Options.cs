using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    private readonly string changeAvatarURL = "http://" + IpProvider.ip + "/RiskApi/api/Hammad/changeAvatar";

    public Slider slider;

    public Toggle[] allToggles;

    public string[] generalsNames;

    public GameObject chooseAvatarPanel;
    public GameObject OptionsPanel;

    public int CustomizeMapSceneIndex;

    public GameObject LoadingPanel;

    public Slider loadingSlider;

    public void goBack()
    {
        Debug.Log("Go Back Called");
        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }

    public void ResetVolume()
    {
        AudioListener.volume = slider.value;
    }

    public void changeAvatar()
    {
        StartCoroutine(tryChanging());
    }

    IEnumerator tryChanging()
    {
        WWWForm form = new WWWForm();

        bool isAvatarSelected = false;
        int index = 0;
        for(int i=0;i<allToggles.Length;i++)
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


        form.AddField("p_Name", PlayerPrefs.GetString("username"));
        form.AddField("p_Password", PlayerPrefs.GetString("password"));
        form.AddField("p_Avatar", generalsNames[index]);

        using (UnityWebRequest request = UnityWebRequest.Post(changeAvatarURL, form))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Not Changed Try Again ........ ");
            }
            else
            {
                PlayerPrefs.SetString("avatar", generalsNames[index]+"");
                PlayerPrefs.Save();
                UnityEngine.SceneManagement.SceneManager.LoadScene(3);
            }


        }
    }

    public void ActivateChooseAvatarPanel()
    {
        chooseAvatarPanel.SetActive(true);
        OptionsPanel.SetActive(false);
    }

    public void ActivateOptionsPanel()
    {
        chooseAvatarPanel.SetActive(false);
        OptionsPanel.SetActive(true);
    }

    public void TogglePressed(int index)
    {
        for(int i=0;i<allToggles.Length;i++)
        {
            if (i != index)
                allToggles[i].isOn = false;
        }
    }

    public void CustomizeMap()
    {
        LoadingPanel.SetActive(true);
        StartCoroutine(LoadCustomizeMapScene());
    }

    IEnumerator LoadCustomizeMapScene()
    {
        yield return new WaitForSeconds(1.5f);
        AsyncOperation operation = SceneManager.LoadSceneAsync(CustomizeMapSceneIndex);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = operation.progress;
            yield return null;
        }
    }
}
