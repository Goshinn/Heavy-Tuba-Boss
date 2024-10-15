using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.EventSystems;

public class CameraLookArea : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        previousPointerPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        Vector2 delta = eventData.position - previousPointerPosition;
        SendValueToControl(delta);

        previousPointerPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SendValueToControl(Vector2.zero);
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;

    [Header("Members")]
    private RectTransform rectTransform;
    private Vector2 previousPointerPosition;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }
}
