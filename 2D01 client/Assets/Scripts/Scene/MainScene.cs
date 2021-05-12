using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public static string CurrentSceneName {get;set;}

    void Start()
    {
        CurrentSceneName = "Login";
        SceneManager.LoadScene("Login", LoadSceneMode.Additive);
        SceneManager.LoadScene("Remote", LoadSceneMode.Additive);
    }


    void Update()
    {
        
    }
}
