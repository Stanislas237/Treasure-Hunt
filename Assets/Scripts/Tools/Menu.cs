using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private bool isSearchingServer = false;

    private CustomDiscovery discovery;

    public void OnClickHost()
    {
        if (isSearchingServer)
            return;

        GameMaster.GameType = typeof(NetworkingManager);
        GameMaster.IsHost = true;
        Tools.LoadScene("Waiting");
    }

    public void OnClickJoin()
    {
        if (isSearchingServer)
            return;

        isSearchingServer = true;

        discovery = FindFirstObjectByType<CustomDiscovery>(FindObjectsInactive.Include);
        discovery.OnServerFound.AddListener(response =>
        {
            GameMaster.IsHost = false;
            GameMaster.Address = response.uri;
            Tools.LoadScene("Waiting");
        });

        GameMaster.GameType = typeof(NetworkingManager);
        discovery.StartDiscovery();
        Invoke(nameof(StopSearch), 3);
    }

    public void OnClickPlay()
    {
        if (isSearchingServer)
            return;

        GameMaster.GameType = typeof(GameManager);
        Tools.LoadScene("Game");
    }

    private void StopSearch()
    {
        discovery.StopDiscovery();
        isSearchingServer = false;
        Debug.Log("Recherche de serveurs terminée.");
    }
}


public static class Tools
{
    public static void LoadScene(string name) => SceneManager.LoadScene(name);
}
