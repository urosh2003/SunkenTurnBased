using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float edgeScrollSpeed = 15f;
    public float edgeThickness = 25f;

    [Header("Zoom")]
    public CinemachineVirtualCamera virtualCamera;
    public float zoomSpeed = 3f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    private Vector3 moveInput;
    private Camera mainCam;
    private Vector3 lastManualPosition;
    private bool isLockedOnTarget = false;

    private void Awake()
    {
        mainCam = Camera.main;
        if (!virtualCamera) virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (isLockedOnTarget) return;

        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0f) * moveSpeed * Time.deltaTime;
        move += EdgeScroll() * edgeScrollSpeed * Time.deltaTime;
        transform.position += move;

        lastManualPosition = transform.position;

        HandleZoom();
    }

    private Vector3 EdgeScroll()
    {
        Vector3 dir = Vector3.zero;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (mousePos.x < edgeThickness) dir.x -= 1;
        else if (mousePos.x > screenSize.x - edgeThickness) dir.x += 1;

        if (mousePos.y < edgeThickness) dir.y -= 1;
        else if (mousePos.y > screenSize.y - edgeThickness) dir.y += 1;

        return dir.normalized;
    }

    private void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        float newSize = virtualCamera.m_Lens.OrthographicSize - scroll * zoomSpeed * Time.deltaTime;
        virtualCamera.m_Lens.OrthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    public void LockCamera()
    {
        isLockedOnTarget = true;
    }
    public void ToggleCamera()
    {
        isLockedOnTarget = !isLockedOnTarget;
    }


    public void UnlockCamera()
    {
        isLockedOnTarget = false;
    }

    public void SnapTo(Vector3 position)
    {
        transform.position = position;
        lastManualPosition = position;
    }

    public Vector3 GetLastManualPosition() => lastManualPosition;
}
