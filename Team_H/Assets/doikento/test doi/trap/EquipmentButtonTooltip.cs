using UnityEngine;
using UnityEngine.EventSystems;
public class EquipmentButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string description;   // ê›îıÇÃê‡ñæï∂

    private UIManager ui;//éQè∆Ç∑ÇÈ

    private void Start()
    {
        ui = FindAnyObjectByType<UIManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui?.ShowDescription(description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui?.HideDescription();
    }
}
