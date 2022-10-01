using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartLoading : MonoBehaviour
{
    [SerializeField]
    private Slider loadingSlider;

    public int LoadSceneIndex = 1;

    public int delay = 2;

    private int index;

    public void Start()
    {
        checkedForLoginorSignUp();

        Invoke("LoadAfterDelay", delay);
    }   

    public void checkedForLoginorSignUp()
    {
        int val = 0;

        val = PlayerPrefs.GetInt("loginFirst");

        Debug.Log("Val = " + val);

        if (val == 0)
            index = 1;
        else
            index = LoadSceneIndex;
    }

    void LoadAfterDelay()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);

        while(!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = operation.progress;
            yield return null;
        }    
    }
}
