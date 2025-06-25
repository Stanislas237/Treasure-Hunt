using UnityEngine;

#if UNITY_ANDROID
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif

public class JoyStick : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_IOS
    [SerializeField]
    RectTransform joystickBase;
    int fingerId = -1;

    // OutPut
    private Vector2 _inputVector = Vector2.zero;
    private Vector2 inputVector
    {
        get => _inputVector;
        set
        {
            if (value != _inputVector && OnMove != null)
            {
                _inputVector = value;
                OnMove.Invoke(value);
            }
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

    private void OnFingerDown(Finger finger)
    {
        if (fingerId != -1)
            return; // déjà utilisé

        // Zone définie
        if (finger.screenPosition.x < Screen.width / 2f)
        {
            fingerId = finger.index;
            joystickBase.gameObject.SetActive(true);
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (finger.index != fingerId)
            return;

        fingerId = -1;
        inputVector = Vector2.zero;
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

        var newPos = PlayerUI.ScreenToCanvasPosition(touch.screenPosition);
        if (IsValidVector2(newPos))
            joystickBase.localPosition = newPos;
        
        var delta = (touch.screenPosition - touch.startScreenPosition).normalized;
        if (delta.sqrMagnitude > 0.25)
            inputVector = delta;
    }
#else
    private void Start()
    {
        foreach (var g in GameObject.FindGameObjectsWithTag("Mobile"))
            Destroy(g.gameObject);
        Destroy(this);
    }
#endif
}
