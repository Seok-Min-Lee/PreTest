using TMPro;
using UnityEngine;

public class MirrorInspectorController : MonoBehaviour
{
    [SerializeField] private MirrorSelectionController _selectionController;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private TMP_InputField _positionXField;
    [SerializeField] private TMP_InputField _positionYField;
    [SerializeField] private TMP_InputField _positionZField;
    [SerializeField] private TMP_InputField _rotationXField;
    [SerializeField] private TMP_InputField _rotationYField;
    [SerializeField] private TMP_InputField _rotationZField;
    [SerializeField] private TMP_InputField _nameField;
    [SerializeField] private TMP_InputField _descriptionField;

    private PlacedMirror _target;
    private Vector3 _lastPosition;
    private Vector3 _lastEulerAngles;

    private void OnEnable()
    {
        _selectionController.SelectionChanged += HandleSelectionChanged;
        _gameManager.ModeChanged += HandleModeChanged;

        HandleSelectionChanged(_selectionController.SelectedMirror);
    }

    private void OnDisable()
    {
        _selectionController.SelectionChanged -= HandleSelectionChanged;
        _gameManager.ModeChanged -= HandleModeChanged;
    }

    private void Update()
    {
        if (_target == null)
        {
            return;
        }

        RefreshFieldsIfChanged();
    }

    private void HandleSelectionChanged(PlacedMirror mirror)
    {
        _target = mirror;
        UpdateInteractable();

        if (_target == null)
        {
            ClearFields();
            return;
        }

        ForceRefreshFields();
    }

    private void HandleModeChanged(AppMode mode)
    {
        UpdateInteractable();
    }

    private void UpdateInteractable()
    {
        bool interactable = _gameManager.Mode == AppMode.Edit && _target != null;

        _positionXField.interactable = interactable;
        _positionYField.interactable = interactable;
        _positionZField.interactable = interactable;
        _rotationXField.interactable = interactable;
        _rotationYField.interactable = interactable;
        _rotationZField.interactable = interactable;
        _nameField.interactable = interactable;
        _descriptionField.interactable = interactable;
    }

    public void OnEndEditPositionX(string value)
    {
        ApplyPosition(0, value);
    }

    public void OnEndEditPositionY(string value)
    {
        ApplyPosition(1, value);
    }

    public void OnEndEditPositionZ(string value)
    {
        ApplyPosition(2, value);
    }

    public void OnEndEditRotationX(string value)
    {
        ApplyRotation(0, value);
    }

    public void OnEndEditRotationY(string value)
    {
        ApplyRotation(1, value);
    }

    public void OnEndEditRotationZ(string value)
    {
        ApplyRotation(2, value);
    }

    public void OnEndEditName(string value)
    {
        if (_target == null)
        {
            return;
        }

        _target.DisplayName = value;
    }

    public void OnEndEditDescription(string value)
    {
        if (_target == null)
        {
            return;
        }

        _target.Description = value;
    }

    private void ApplyPosition(int axisIndex, string value)
    {
        if (_target == null)
        {
            return;
        }

        if (!float.TryParse(value, out float parsed))
        {
            ForceRefreshFields();
            return;
        }

        Vector3 position = _target.transform.position;
        position[axisIndex] = parsed;
        _target.transform.position = position;
        ForceRefreshFields();
    }

    private void ApplyRotation(int axisIndex, string value)
    {
        if (_target == null)
        {
            return;
        }

        if (!float.TryParse(value, out float parsed))
        {
            ForceRefreshFields();
            return;
        }

        Vector3 eulerAngles = _target.transform.rotation.eulerAngles;
        eulerAngles[axisIndex] = parsed;
        _target.transform.rotation = Quaternion.Euler(eulerAngles);
        ForceRefreshFields();
    }

    private void RefreshFieldsIfChanged()
    {
        Vector3 position = _target.transform.position;
        Vector3 eulerAngles = _target.transform.rotation.eulerAngles;

        if (position == _lastPosition && eulerAngles == _lastEulerAngles)
        {
            return;
        }

        ForceRefreshFields();
    }

    private void ForceRefreshFields()
    {
        Vector3 position = _target.transform.position;
        Vector3 eulerAngles = _target.transform.rotation.eulerAngles;

        _lastPosition = position;
        _lastEulerAngles = eulerAngles;

        SetFieldValue(_positionXField, position.x);
        SetFieldValue(_positionYField, position.y);
        SetFieldValue(_positionZField, position.z);
        SetFieldValue(_rotationXField, eulerAngles.x);
        SetFieldValue(_rotationYField, eulerAngles.y);
        SetFieldValue(_rotationZField, eulerAngles.z);
        SetFieldValue(_nameField, _target.DisplayName);
        SetFieldValue(_descriptionField, _target.Description);
    }

    private void ClearFields()
    {
        _positionXField.SetTextWithoutNotify(string.Empty);
        _positionYField.SetTextWithoutNotify(string.Empty);
        _positionZField.SetTextWithoutNotify(string.Empty);
        _rotationXField.SetTextWithoutNotify(string.Empty);
        _rotationYField.SetTextWithoutNotify(string.Empty);
        _rotationZField.SetTextWithoutNotify(string.Empty);
        _nameField.SetTextWithoutNotify(string.Empty);
        _descriptionField.SetTextWithoutNotify(string.Empty);
    }

    private static void SetFieldValue(TMP_InputField field, float value)
    {
        if (field.isFocused)
        {
            return;
        }

        field.SetTextWithoutNotify(value.ToString("F2"));
    }

    private static void SetFieldValue(TMP_InputField field, string value)
    {
        if (field.isFocused)
        {
            return;
        }

        field.SetTextWithoutNotify(value);
    }
}
