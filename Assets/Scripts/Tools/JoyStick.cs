using UnityEngine;

#if UNITY_ANDROID
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif

public class JoyStick : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
    [SerializeField]
    RectTransform joystickBase;
    [SerializeField]
    RectTransform joystickHandle;

    [Range(0f, 200f), SerializeField]
    float maxDistance = 100f;
    int fingerId = -1;

    Canvas canvas = null;

    // OutPut
    private Vector2 inputVector
    {
        set
        {
            if (OnMove != null)
                OnMove.Invoke(value);
        }
    }

    public System.Action<Vector2> OnMove = null;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
    }

    bool IsValidVector2(Vector2 v) => !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsInfinity(v.x)
        && !float.IsInfinity(v.y);

    public Vector2 ScreenToCanvasPosition(Vector2 screenPos)
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPos,
            canvas.worldCamera, out Vector2 localPos);
        return localPos;
    }

    private void OnFingerDown(Finger finger)
    {
        if (fingerId != -1)
            return; // déjà utilisé

        // Zone définie
        if (finger.screenPosition.x < Screen.width / 2f)
        {
            fingerId = finger.index;

            if (!IsValidVector2(finger.screenPosition))
                return;
            joystickBase.position = ScreenToCanvasPosition(finger.screenPosition);
            joystickHandle.position = ScreenToCanvasPosition(finger.screenPosition);
            joystickBase.gameObject.SetActive(true);
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (finger.index != fingerId)
            return;

        fingerId = -1;
        inputVector = Vector2.zero;
        joystickHandle.position = joystickBase.position;
        joystickBase.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (fingerId == -1)
        {
            inputVector = Vector2.zero;
            return;
        }

        var touch = Touch.activeTouches.FirstOrDefault(t => t.finger.index == fingerId);
        if (!touch.valid) return;

        Vector2 direction = ScreenToCanvasPosition(touch.screenPosition) - (Vector2)joystickBase.position;
        float distance = Mathf.Min(direction.magnitude, maxDistance);
        Vector2 clamped = direction.normalized * distance;
        joystickHandle.position = joystickBase.position + (Vector3)clamped;

        inputVector = clamped / maxDistance;
    }
#else
    private void Start()
    {
        foreach (var g in GameObject.FindGameObjectsWithTag("Mobile"))
            Destroy(g);
        Destroy(this);
    }
#endif
}
