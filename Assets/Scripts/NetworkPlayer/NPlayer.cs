using UnityEngine;
using Mirror;
using TMPro;

public class NPlayer : NetworkBehaviour
{

    #region Synced Vars
    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName = string.Empty;

    [Command]
    void CmdSetName(string name) => playerName = name;

    void OnNameChanged(string _, string value)
    {
        if (NameText != null)
            NameText.text = value;
    }


    [SyncVar(hook = nameof(OnWeaponChanged))]
    private string weapon = "None";

    [Command(requiresAuthority = false)]
    public void CmdSetWeapon(string name) => weapon = name;

    void OnWeaponChanged(string _, string value) => player?.EquipWeapon(value);


    [SyncVar(hook = nameof(OnAnimChanged))]
    private string anim = "Idle";

    [Command(requiresAuthority = false)]
    public void CmdSetAnim(string name) => anim = name;

    void OnAnimChanged(string _, string value) => animator?.Play(value);


    [SyncVar(hook = nameof(OnPointsChanged))]
    public int BonusPoints;

    [Command(requiresAuthority = false)]
    public void CmdSetPoints(int value) => BonusPoints = value;

    void OnPointsChanged(int _, int value) => playerUI?.UpdatePoints(value);


    [Command(requiresAuthority = false)]
    public void CmdSpawnArrow(Vector3 position, Quaternion rotation, Vector3 XtendedTarget, NetworkConnectionToClient _ = null)
    {
        var arrow = Instantiate(Entity.ArrowPrefab, position, rotation);
        NetworkServer.Spawn(arrow, _);
        NetworkingManager.NetworkDestroy(arrow, 6);
        
        TargetReceiveSpawnedArrow(_, arrow, XtendedTarget);
    }

    [TargetRpc]
    private void TargetReceiveSpawnedArrow(NetworkConnection _, GameObject obj, Vector3 target)
    {
        if (isLocalPlayer)
            obj.AddComponent<Arrow>().Initialize(target, player, true);
    }

    [Command(requiresAuthority = false)]
    public void CmdSpawnObject(string name, Vector3 pos, Quaternion rot)
    {
        if (isServer)
            switch (name)
            {
                case "Coin":
                    NetworkServer.Spawn(Instantiate(GameManager.CoinPrefab, pos, rot));
                    break;
                case "Treasure":
                    NetworkServer.Spawn(Instantiate(GameManager.TreasurePrefab, pos, rot));
                    break;
                case "Mud":
                    NetworkServer.Spawn(Instantiate(Entity.MudPrefab, pos, rot));
                    break;
                case "Spike":
                    NetworkServer.Spawn(Instantiate(Entity.SpikePrefab, pos, rot));
                    break;
            }
    }



    [Command(requiresAuthority = false)]
    public void CmdDestroyObject(GameObject obj) => NetworkingManager.NetworkDestroy(obj, 0);
    #endregion


    private static Transform TextDataParent;
    private Animator animator;
    private PlayerUI playerUI;
    private Player player;
    private TextMeshProUGUI NameText = null;

    public override void OnStartClient()
    {
        tag = "Player";
        if (TextDataParent == null)
        {
            TextDataParent = FindFirstObjectByType<Canvas>().transform.GetChild(0);
            TextDataParent.GetChild(0).gameObject.SetActive(false);
        }

        animator = GetComponent<Animator>();
        playerUI = GetComponent<PlayerUI>();
        player = GetComponent<Player>();

        var txtCtn = Instantiate(TextDataParent.GetChild(0), TextDataParent);
        txtCtn.gameObject.SetActive(true);
        NameText = txtCtn.GetChild(0).GetComponent<TextMeshProUGUI>();
        NameText.text = playerName;
        playerUI.Initialize((RectTransform)txtCtn);

        if (isLocalPlayer)
            CmdSetName(GameManager.PlayerName);
        else
            Destroy(txtCtn.GetChild(2).gameObject);
    }

    public bool ImHosting() => isServer && isLocalPlayer;
}