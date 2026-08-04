using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraViewController : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 0.2f;
    [SerializeField] private float _zoomSpeed = 0.02f;
    [SerializeField] private float _panSpeed = 0.02f;
    [SerializeField] private float _minPitch = -80f;
    [SerializeField] private float _maxPitch = 80f;

    private float _yaw;
    private float _pitch;
    private Vector3 _initialPosition;
    private float _initialYaw;
    private float _initialPitch;

    private void Awake()
    {
        Vector3 eulerAngles = transform.eulerAngles;
        _yaw = eulerAngles.y;
        _pitch = eulerAngles.x;

        _initialPosition = transform.position;
        _initialYaw = _yaw;
        _initialPitch = _pitch;
    }

    public void OnClickResetView()
    {
        _yaw = _initialYaw;
        _pitch = _initialPitch;

        transform.SetPositionAndRotation(_initialPosition, Quaternion.Euler(_pitch, _yaw, 0f));
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        HandleRotation();
        HandleZoom();
        HandlePan();
    }

    private void HandleRotation()
    {
        if (!Mouse.current.rightButton.isPressed)
        {
            return;
        }

        Vector2 delta = Mouse.current.delta.ReadValue();
        _yaw += delta.x * _rotationSpeed;
        _pitch -= delta.y * _rotationSpeed;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Approximately(scroll, 0f))
        {
            return;
        }

        transform.position += transform.forward * (scroll * _zoomSpeed);
    }

    private void HandlePan()
    {
        if (!Mouse.current.middleButton.isPressed)
        {
            return;
        }

        Vector2 delta = Mouse.current.delta.ReadValue();
        transform.position -= (transform.right * delta.x + transform.up * delta.y) * _panSpeed;
    }
}
