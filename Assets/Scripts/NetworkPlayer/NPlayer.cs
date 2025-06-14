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

    [Command]
    public void CmdSetWeapon(string name) => weapon = name;

    void OnWeaponChanged(string _, string value) => player?.EquipWeapon(value);


    [SyncVar(hook = nameof(OnAnimChanged))]
    private string anim = "Idle";

    [Command]
    public void CmdSetAnim(string name) => anim = name;

    void OnAnimChanged(string _, string value) => animator?.Play(value);


    [SyncVar(hook = nameof(OnPointsChanged))]
    private int BonusPoints;

    [Command]
    public void CmdSetPoints(int value) => BonusPoints = value;

    void OnPointsChanged(int _, int value) => playerUI?.UpdatePoints(value);


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


    #region Custom NetworkTransform
    [SyncVar]
    private Vector3 _syncedPosition;
    
    [SyncVar]
    private Quaternion _syncedRotation;

    [Header("Custom Network Transform Settings")]
    [SerializeField]
    private float _positionLerpSpeed = 15f;
    [SerializeField]
    private float _rotationLerpSpeed = 15f;
    [SerializeField]
    private float _threshold = 0.1f; // Seuil de sync

    private void Update()
    {
        if (isLocalPlayer)
            UpdateServerTransform();
        else
            ApplyInterpolatedTransform();
    }

    [ServerCallback]
    private void UpdateServerTransform()
    {
        // Synchronise seulement si le changement dépasse le seuil
        if (Vector3.Distance(_syncedPosition, transform.position) > _threshold)
        {
            _syncedPosition = transform.position;
            _syncedRotation = transform.rotation;
        }
    }

    private void ApplyInterpolatedTransform()
    {
        transform.position = Vector3.Lerp(
            transform.position, 
            _syncedPosition, 
            _positionLerpSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            _syncedRotation, 
            _rotationLerpSpeed * Time.deltaTime
        );
    }
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
}