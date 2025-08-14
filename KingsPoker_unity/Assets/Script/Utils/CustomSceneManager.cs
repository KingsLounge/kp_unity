using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager
{
    private static bool initialization = false;
    private static Stack<string> stack = new Stack<string>();

    public static void LoadScene(string scene)
    {
        if (!initialization)
        {
            Initialization();
        }
        Debug.Log("LoadScene Start");
        LoadingCircle.Instance.StartSpin();
        WebSocketManager.canTransitionPacket = false;
        SceneManager.LoadSceneAsync(scene);
    }

    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public static string GetLoginSceneName()
    {
        string loadScene = "Login";
#if Astar
        loadScene = "Login_Astar";
#elif Kingdom
        loadScene = "Login_KH";
#endif
        return loadScene;
    }

    public static void LoadLoginScene(bool reloadScene = true)
    {
        string loadScene = GetLoginSceneName();
        if (reloadScene || GetCurrentSceneName() != loadScene)
            LoadScene(loadScene);
        LoadingCircle.Instance.ReconnectEnd();
    }

    private static void Initialization()
    {
        initialization = true;
        SceneManager.sceneLoaded += delegate(Scene scene, LoadSceneMode mode)
        {
            LoadingCircle.Instance.StopSpin();
            WebSocketManager.canTransitionPacket = true;
            Debug.Log("Scene Loaded");
        };
    }
}
