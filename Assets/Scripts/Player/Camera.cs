using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    #region Cam Values
    [Header("Referencias Cinemachine")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform playerBody;

    [Header("Sensibilidad y Límites")]
    [SerializeField] private float lookSpeed = 8f;
    [SerializeField] private float lookXLimitMin = -40f;
    [SerializeField] private float lookXLimitMax = 70f;

    [Header("Rotación del Cuerpo")]
    [SerializeField] private bool rotatePlayerWithCamera = true;
    [SerializeField] private float bodyRotationSpeed = 8f;
    [SerializeField] private float angleThreshold = 60f;

    [Header("Efectos de Cámara")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float runFOV = 78f;
    [SerializeField] private float fovSpeed = 8f;
    [SerializeField] private float tiltSpeed = 8f;

    private float rotationX;
    private float rotationY;
    private float currentTilt;
    private float targetTilt;
    private float targetFOV;
    private bool isMoving;
    private bool canMove = true;
    private bool lockPlayerRotation = false;
    #endregion

    public bool CanMove
    {
        get => canMove;
        set => canMove = value;
    }

    public void SetLockPlayerRotation(bool lockRotation)
    {
        lockPlayerRotation = lockRotation;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerBody == null)
            playerBody = transform;

        if (virtualCamera != null)
        {
            targetFOV = normalFOV;
            virtualCamera.Lens.FieldOfView = normalFOV;
        }

        if (playerBody != null)
        {
            rotationY = playerBody.eulerAngles.y;
            rotationX = playerBody.eulerAngles.x;
        }
    }

    private void Update()
    {
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        if (virtualCamera != null)
        {
            virtualCamera.Lens.FieldOfView = Mathf.Lerp(
                virtualCamera.Lens.FieldOfView,
                targetFOV,
                Time.deltaTime * fovSpeed
            );
        }

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        isMoving = (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputY) > 0.1f);
    }

    private void LateUpdate()
    {
        if (!canMove || virtualCamera == null || playerBody == null)
            return;

        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, lookXLimitMin, lookXLimitMax);
        rotationY += mouseX;

        if (rotatePlayerWithCamera && !lockPlayerRotation)
        {
            Quaternion targetBodyRotation = Quaternion.Euler(0f, rotationY, 0f);
            float angleDifference = Quaternion.Angle(playerBody.rotation, targetBodyRotation);

            if (isMoving || angleDifference > angleThreshold)
            {
                float speedMultiplier = isMoving ? bodyRotationSpeed * 2.5f : bodyRotationSpeed;
                playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetBodyRotation, Time.deltaTime * speedMultiplier);
            }
        }

        virtualCamera.transform.rotation = Quaternion.Euler(rotationX, rotationY, currentTilt);
    }

    public void SetTilt(float angle)
    {
        targetTilt = angle;
    }

    public void SetFOV(bool isRunning)
    {
        targetFOV = isRunning ? runFOV : normalFOV;
    }
}