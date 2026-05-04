using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;

    public float moveRange = 100f;

    [SerializeField] InputReader inputReader; 

    Vector2 inputVector;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        pos = Vector2.ClampMagnitude(pos, moveRange);

        handle.anchoredPosition = pos;

        inputVector = pos / moveRange;

        inputReader.MobileMoveInput = inputVector;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero;

        inputReader.MobileMoveInput = Vector2.zero;
    }
}