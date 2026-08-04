using UnityEngine;

public class InspectorPanelToggle : MonoBehaviour
{
    [SerializeField] private MirrorSelectionController _selectionController;
    [SerializeField] private GameObject _inspectorPanel;

    private void OnEnable()
    {
        _selectionController.MirrorClicked += HandleMirrorClicked;
    }

    private void OnDisable()
    {
        _selectionController.MirrorClicked -= HandleMirrorClicked;
    }

    public void OnClickClose()
    {
        _inspectorPanel.SetActive(false);
    }

    private void HandleMirrorClicked(PlacedMirror mirror)
    {
        _inspectorPanel.SetActive(true);
    }
}
