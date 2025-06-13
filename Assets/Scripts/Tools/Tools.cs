using UnityEngine;
using UnityEngine.SceneManagement;

public static class Tools
{
    public static void LoadScene(string name) => SceneManager.LoadScene(name);
}