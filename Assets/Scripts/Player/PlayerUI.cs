using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    private const string MobileButtonsName = "BUTTONS - COMMAND";
    private TextMeshProUGUI textPoints;
    private TextMeshProUGUI nameText;
    private Image weaponIcon;

    public RectTransform rectTransform;

    private static TextMeshProUGUI MudText;
    private static TextMeshProUGUI SpikeText;
    private static Canvas parentCanvas;
    private static Camera mainCamera;
    private static Sprite SwordIcon;
    private static Sprite BowIcon;

    public void UpdatePoints(int points) => textPoints.text = $"np : {points}";
    public void UpdateWeaponIcon(string weapon)
    {
        if (weaponIcon != null)
            weaponIcon.sprite = weapon == "Sword" ? SwordIcon : BowIcon;
    }
    public void UpdateTrapCounts(int mud, int spike)
    {
        MudText.text = mud.ToString();
        SpikeText.text = spike.ToString();
    }

    void Start()
    {
        if (rectTransform != null)
            Initialize(rectTransform);

        // Chargement des icônes
        if (SwordIcon == null)
            SwordIcon = Resources.Load<Sprite>("Props/Sprites/Sword");
        if (BowIcon == null)
            BowIcon = Resources.Load<Sprite>("Props/Sprites/Bow");
    }


    public static Vector2 ScreenToCanvasPosition(Vector2 screenPos)
    {
        if (parentCanvas == null)
            return Vector2.zero;

        // Conversion en coordonnées locales du Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(),
            screenPos,
            parentCanvas.worldCamera,
            out Vector2 localPos
        );
        return localPos;
    }

    void LateUpdate()
    {
        if (rectTransform == null)
            return;

        Vector3 worldPos = transform.position + new Vector3(0, 2.5f, 0); // Ajustez la hauteur ici
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // Ajustement final de position
        rectTransform.localPosition = ScreenToCanvasPosition(screenPos) + new Vector2(0, 20f); // Offset en pixels si nécessaire
    }

    public void Initialize(RectTransform RT)
    {
        rectTransform = RT;
        nameText = rectTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textPoints = rectTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
        weaponIcon = rectTransform.GetChild(2).GetComponent<Image>();

        var p = rectTransform.parent;
        if (parentCanvas == null)
        {
            parentCanvas = p.parent.GetComponent<Canvas>();
            mainCamera = Camera.main;
            MudText = p.GetChild(1).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            SpikeText = p.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();

            p.parent.GetChild(1).GetChild(3).GetComponent<Button>().onClick.AddListener(LogOut);
            p.GetChild(3).GetComponent<Button>().onClick.AddListener(LogOut);

            static void LogOut()
            {
                if (GameMaster.IsHost)
                    GameManager.networkManager?.StopHost();
                else
                    GameManager.networkManager?.StopClient();
                Tools.LoadScene("PlayerUI", "Menu");
            }
        }

        if (!TryGetComponent(out NPlayer nPlayer) || nPlayer.isLocalPlayer)
        {
            nameText.text = GameManager.PlayerName;
            foreach (Transform b in p.GetChild(1))
                b.GetComponent<Button>().onClick.AddListener(() => GetComponent<Player>().ThrowTrap(b.name));

            var obj = p.Find(MobileButtonsName);
            if (obj != null)
            {
                obj.GetChild(0).GetComponent<Button>().onClick.AddListener(() => GetComponent<Player>().Attack());
                obj.GetChild(1).GetComponent<Button>().onClick.AddListener(() => GetComponent<Player>().Jump());
            }
        }
    }
}
