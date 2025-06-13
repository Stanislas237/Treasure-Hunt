using UnityEngine;
using UnityEngine.SceneManagement;

public static class Tools
{
    public static string PreviousScene { get; private set; } = string.Empty;
    public static string CurrentScene => SceneManager.GetActiveScene().name;
    public static void LoadScene(string name)
    {
        PreviousScene = CurrentScene;
        SceneManager.LoadScene(name);
    }
}