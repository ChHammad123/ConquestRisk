using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public int MultiplayerSceneIndex = 6;
    
    public int SinglePlayerSceneIndex = 5;
    
    public int OptionsSceneIndex = 4;

    public TextMeshProUGUI userName;

    public string[] generalsNames;

    public GameObject[] allAvatars;

    public void LogOut()
    {
        PlayerPrefs.SetInt("loginFirst", 1);
        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void Options()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(OptionsSceneIndex);    
    }


    public void SinglePlayer()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SinglePlayerSceneIndex);
    }


    public void Multiplayer()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(MultiplayerSceneIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Start()
    {
        string name = PlayerPrefs.GetString("username");
        userName.text = name;

        Debug.Log("Name is " + userName.text);

        string avatar = PlayerPrefs.GetString("avatar");

        Debug.Log("Avatar is " + avatar);

        for (int i = 0; i < generalsNames.Length; i++)
        {
            allAvatars[i].SetActive(false);

            if (generalsNames[i].Equals(avatar))
                allAvatars[i].SetActive(true);
        }
    }

}
