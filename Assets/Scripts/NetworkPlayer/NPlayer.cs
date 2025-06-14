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
        playerUI.Initialize((RectTransform)txtCtn);

        if (!isLocalPlayer)
            Destroy(txtCtn.GetChild(2).gameObject);
    }


    [SyncVar(hook = nameof(OnWeaponChanged))]
    private string weapon = "None";

    [Command]
    public void CmdSetWeapon(string name) => weapon = name;

    void OnWeaponChanged(string _, string value) => player.EquipWeapon(value);


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
    public void CmdSpawnArrow(Vector3 pos, Quaternion rot, Vector3 target, NetworkConnectionToClient _ = null)
    {
        var arrow = Instantiate(Entity.ArrowPrefab, pos, rot);
        NetworkServer.Spawn(arrow);
        TargetReceiveSpawnedObj(_, arrow, target);
    }

    [TargetRpc]
    private void TargetReceiveSpawnedObj(NetworkConnection _, GameObject obj, Vector3 target)
    {
        // Reçu uniquement par le client appelant
        Debug.Log($"Objet spawné avec ID: {obj.GetComponent<NetworkIdentity>().netId}");
        if (target != null)
        {
            CmdInitArrow(obj, target);
            obj.GetComponent<Arrow>().Initialize(target, player);
        }
    }

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
                case "Spike":
                    NetworkServer.Spawn(Instantiate(Entity.SpikePrefab, pos, rot));
                    break;
            }
    }

    [Command]
    public void CmdDestroyObject(GameObject obj) => NetworkServer.Destroy(obj);


    [Command]
    public void CmdInitArrow(GameObject Arrow, Vector3 target) => RpcInitArrow(Arrow, target);

    [ClientRpc]
    private void RpcInitArrow(GameObject Arrow, Vector3 target) => Arrow.GetComponent<Arrow>().Initialize(target, player);
    #endregion

    private Transform TextDataParent;
    private Animator animator;
    private PlayerUI playerUI;
    private Player player;

    void Start()
    {
        TextDataParent = FindFirstObjectByType<Canvas>().transform.GetChild(0);
        TextDataParent.GetChild(0).gameObject.SetActive(false);
        tag = "Player";

        animator = GetComponent<Animator>();
        playerUI = GetComponent<PlayerUI>();
        player = GetComponent<Player>();
    }

    public override void OnStartLocalPlayer() => CmdSetName(GameManager.PlayerName);
}