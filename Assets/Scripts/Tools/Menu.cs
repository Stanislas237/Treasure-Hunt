using TMPro;
using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private bool isSearchingServer = false;

    private CustomDiscovery discovery = null;

    [SerializeField]
    Animator PanelAnimator;

    [SerializeField]
    TMP_InputField inputField;

    void Start()
    {
        GameMaster.Instance.AddSoundOnButtons();
        GameManager.SetName(PlayerPrefs.GetString("PlayerName", string.Empty));
        inputField.text = GameManager.PlayerName;
        inputField.onValueChanged.AddListener(newText =>
        {
            GameManager.SetName(newText);
            PlayerPrefs.SetString("PlayerName", newText);
            PlayerPrefs.Save();
        });

        discovery = GetComponent<CustomDiscovery>();
        var manager = FindFirstObjectByType<NetworkManager>(FindObjectsInactive.Include);
        if (manager)
            Destroy(manager.gameObject);
    }

    public void OnClickHost()
    {
        if (isSearchingServer || IncorrectName())
            return;

        isSearchingServer = true;
        StartCoroutine(Host());
    }

    IEnumerator Host()
    {
        yield return new WaitForSeconds(0.3f);

        GameMaster.GameType = typeof(NetworkingManager);
        GameMaster.IsHost = true;
        Tools.LoadScene(name, "Waiting");        
    }

    public void OnClickJoin()
    {
        if (isSearchingServer || IncorrectName())
            return;

        isSearchingServer = true;
        StartCoroutine(Join());
    }

    IEnumerator Join()
    {
        PanelAnimator.Play("Searching");
        discovery.onServerDiscovered = address =>
        {
            GameMaster.IsHost = false;
            GameMaster.Address = address;
            Tools.LoadScene(name, "Waiting");
        };

        GameMaster.GameType = typeof(NetworkingManager);
        discovery?.StartDiscovery();

        yield return null;
        yield return new WaitForSeconds(PanelAnimator.GetCurrentAnimatorStateInfo(0).length);
        
        discovery?.StopDiscovery();
        isSearchingServer = false;
    }

    public void OnClickPlay()
    {
        if (isSearchingServer || IncorrectName())
            return;

        isSearchingServer = true;
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        yield return new WaitForSeconds(0.3f);

        GameMaster.GameType = typeof(GameManager);
        Tools.LoadScene(name, "Game");
    }

    private bool IncorrectName()
    {
        if (string.IsNullOrEmpty(GameManager.PlayerName))
        {
            PanelAnimator.Play("EnterName");
            GameMaster.PlayClip2D("Error");
        }
        return string.IsNullOrEmpty(GameManager.PlayerName);
    }
}


public static class Tools
{
    public static void LoadScene(string MyName, string name)
    {
        SceneManager.LoadScene(name);
    }
}
