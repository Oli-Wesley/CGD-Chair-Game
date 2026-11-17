using UnityEngine;
using UnityEngine.EventSystems;

public class UISelectionHelper : MonoBehaviour
{
    [Header("SELECTED BUTTON")]
    [SerializeField] private GameObject firstButtonToSelect;

    private void OnEnable()
    {
        SetSelectedButton();
    }

    public void SetSelectedButton()
    {
        if (firstButtonToSelect != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButtonToSelect);
        }
    }
}