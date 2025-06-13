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
        var txtCtn = Instantiate(TextDataParent.GetChild(0), TextDataParent);
        txtCtn.gameObject.SetActive(true);
        txtCtn.GetChild(0).GetComponent<TextMeshProUGUI>().text = value;
        GetComponent<PlayerUI>().Initialize((RectTransform)txtCtn);

        if (!isLocalPlayer)
            Destroy(txtCtn.GetChild(2).gameObject);
    }


    [SyncVar(hook = nameof(OnAnimChanged))]
    private string anim = "Idle";

    [Command]
    public void CmdSetAnim(string name) => anim = name;

    void OnAnimChanged(string _, string value) => animator.Play(value);


    [SyncVar(hook = nameof(OnPointsChanged))]
    private int BonusPoints;

    [Command]
    public void CmdSetPoints(int value) => BonusPoints = value;

    void OnPointsChanged(int _, int value) => playerUI.UpdatePoints(value);


    [Command]
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
                case "Arrow":
                    NetworkServer.Spawn(Instantiate(Entity.ArrowPrefab, pos, rot));
                    break;
                case "Spike":
                    NetworkServer.Spawn(Instantiate(Entity.SpikePrefab, pos, rot));
                    break;
            }
    }


    // [Command]
    // public void CmdInitArrow(GameObject Arrow, Vector3 target) => RpcInitArrow(Arrow, target);

    // [ClientRpc]
    // private void RpcInitArrow(GameObject Arrow, Vector3 target) => Arrow.GetComponent<Arrow>().Initialize(target, gameObject);
    #endregion

    private Transform TextDataParent;
    private Animator animator;
    private PlayerUI playerUI;

    void Start()
    {
        TextDataParent = FindFirstObjectByType<Canvas>().transform.GetChild(0);
        TextDataParent.GetChild(0).gameObject.SetActive(false);
        tag = "Player";

        animator = GetComponent<Animator>();
        playerUI = GetComponent<PlayerUI>();
    }

    public override void OnStartLocalPlayer() => CmdSetName(GameManager.PlayerName);
}