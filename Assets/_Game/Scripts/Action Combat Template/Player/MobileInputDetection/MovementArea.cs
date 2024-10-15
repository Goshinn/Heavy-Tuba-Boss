using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

/// <summary>
/// Whenever touch is detected in the movement area, a floating joystick appears which allows players to move ther character.
/// The floating joystick position shifts to the pointerDown position and joystick handle moves appropriately according to touch input.
/// </summary>
public class MovementArea : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Setup")]
    [SerializeField] private RectTransform floatingJoystick;
    [SerializeField] private RectTransform floatingJoystickHandle;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out m_PointerDownPos);

        // Fade in floating joystick here
        floatingJoystick.anchoredPosition = m_PointerDownPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localPosition);
        var delta = localPosition - m_PointerDownPos;

        delta = Vector2.ClampMagnitude(delta, movementRange);

        var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
        SendValueToControl(newPos);

        // Handle visual representation of joystick position
        floatingJoystickHandle.anchoredPosition = delta;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SendValueToControl(Vector2.zero);

        // Reset visual position of joystick
        floatingJoystickHandle.anchoredPosition = Vector2.zero;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public float movementRange
    {
        get => m_MovementRange;
        set => m_MovementRange = value;
    }

    [FormerlySerializedAs("movementRange")]
    [SerializeField]
    private float m_MovementRange = 50;

    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;

    [Header("Members")]
    private RectTransform rectTransform;
    private Vector2 m_PointerDownPos;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }
}