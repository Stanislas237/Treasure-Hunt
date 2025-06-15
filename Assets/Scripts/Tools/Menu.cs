using TMPro;
using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private bool isSearchingServer = false;

    private CustomDiscovery discovery;

    [SerializeField]
    Animator PanelAnimator;

    [SerializeField]
    TMP_InputField inputField;

    void Start()
    {
        GameManager.SetName(PlayerPrefs.GetString("PlayerName", string.Empty));
        inputField.text = GameManager.PlayerName;
        inputField.onValueChanged.AddListener(newText =>
        {
            GameManager.SetName(newText);
            PlayerPrefs.SetString("PlayerName", newText);
            PlayerPrefs.Save();
        });

        var manager = FindFirstObjectByType<NetworkManager>(FindObjectsInactive.Include);
        if (manager)
            Destroy(manager.gameObject);
    }

    public void OnClickHost()
    {
        if (isSearchingServer || IncorrectName())
            return;

        GameMaster.GameType = typeof(NetworkingManager);
        GameMaster.IsHost = true;
        Tools.LoadScene("Waiting");
    }

    public void OnClickJoin() => StartCoroutine(Join());

    IEnumerator Join()
    {
        if (isSearchingServer || IncorrectName())
            yield break;

        isSearchingServer = true;
        PanelAnimator.Play("Searching");

        discovery = FindFirstObjectByType<CustomDiscovery>();
        discovery.OnServerFound.AddListener(response =>
        {
            GameMaster.IsHost = false;
            GameMaster.Address = response.uri;
            Tools.LoadScene("Waiting");
        });

        GameMaster.GameType = typeof(NetworkingManager);
        discovery.StartDiscovery();

        yield return null;
        yield return new WaitForSeconds(PanelAnimator.GetCurrentAnimatorStateInfo(0).length);
        
        discovery.StopDiscovery();
        isSearchingServer = false;
        Debug.Log("Recherche de serveurs terminée.");
    }

    public void OnClickPlay()
    {
        if (isSearchingServer || IncorrectName())
            return;

        GameMaster.GameType = typeof(GameManager);
        Tools.LoadScene("Game");
    }

    private bool IncorrectName()
    {
        if (string.IsNullOrEmpty(GameManager.PlayerName))
            PanelAnimator.Play("EnterName");
        return string.IsNullOrEmpty(GameManager.PlayerName);
    }
}


public static class Tools
{
    public static void LoadScene(string name) => SceneManager.LoadScene(name);
}
