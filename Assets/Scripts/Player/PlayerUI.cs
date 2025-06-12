using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textPoints;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI MudText;
    [SerializeField]
    private TextMeshProUGUI SpikeText;
    [SerializeField]
    private Image weaponIcon;

    /// <summary>
    /// Le joueur que l'UI doit suivre.
    /// </summary>
    public Transform player;

    private RectTransform rectTransform;
    private Camera mainCamera;
    private Canvas parentCanvas;

    private Sprite SwordIcon;
    private Sprite BowIcon;

    public void UpdatePoints(int points) => textPoints.text = $"np : {points}";
    public void UpdateWeaponIcon(string weapon) => weaponIcon.sprite = weapon == "Sword" ? SwordIcon : BowIcon;
    public void UpdateTrapCounts(int mud, int spike)
    {
        MudText.text = mud.ToString();
        SpikeText.text = spike.ToString();
    }

    void Start()
    {
        nameText.text = GameManager.PlayerName;
        rectTransform = textPoints.transform.parent.GetComponent<RectTransform>();
        mainCamera = Camera.main;
        parentCanvas = textPoints.GetComponentInParent<Canvas>();

        // Chargement des icônes
        SwordIcon = Resources.Load<Sprite>("Props/Sprites/Sword");
        BowIcon = Resources.Load<Sprite>("Props/Sprites/Bow");
    }

    void LateUpdate()
    {
        Vector3 worldPos = player.position + new Vector3(0, 2.5f, 0); // Ajustez la hauteur ici
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // Conversion en coordonnées locales du Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(),
            screenPos,
            parentCanvas.worldCamera,
            out Vector2 localPos
        );

        // Ajustement final de position
        rectTransform.localPosition = localPos + new Vector2(0, 20f); // Offset en pixels si nécessaire
    }
}
