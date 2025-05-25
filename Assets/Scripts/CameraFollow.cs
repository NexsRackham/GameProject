using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                                 // Jugador a seguir
    public Vector3 offset = new Vector3(0f, 3f, -10f);        // Offset para modo seguimiento
    public float rotationSpeed = 5f;

    private float currentYaw = 0f;
    private float currentPitch = 20f;
    public float pitchMin = -90f;
    public float pitchMax = 70f;

    private bool isFreeCamera = false;                        // Estado de cámara libre o seguimiento
    private bool cameraJustChanged = false;

    // Zoom
    public float zoomDistance = 10f;                          // Distancia inicial
    public float zoomMin = 5f;                                // Zoom mínimo
    public float zoomMax = 20f;                               // Zoom máximo
    public float zoomSpeed = 2f;                              // Velocidad del zoom

    public bool CameraJustChanged() => cameraJustChanged;
    public void ResetCameraJustChanged() => cameraJustChanged = false;
    public bool IsFreeCamera() => isFreeCamera;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        zoomDistance = offset.magnitude;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Alternar modo cámara con tecla H
        if (Input.GetKeyDown(KeyCode.H))
        {
            isFreeCamera = !isFreeCamera;
            cameraJustChanged = true;

            if (!isFreeCamera) // Si volvemos a modo seguimiento
            {
                // Alineamos la cámara detrás del personaje
                Vector3 forward = target.forward;
                forward.y = 0f; // Evitar inclinaciones verticales
                currentYaw = Quaternion.LookRotation(forward).eulerAngles.y; // Obtener la rotación horizontal del jugador para usar como nueva dirección base
            }
        }

        bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        if (!altHeld)
        {
            // Leer movimiento del mouse para rotar la cámara
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            currentYaw += mouseX;
            currentPitch = Mathf.Clamp(currentPitch - mouseY, pitchMin, pitchMax);
        }

        // Ajustar zoom con el scroll del mouse
        zoomDistance = Mathf.Clamp(zoomDistance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, zoomMin, zoomMax);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position + rotation * (isFreeCamera ? new Vector3(0f, 0f, -zoomDistance) : offset.normalized * zoomDistance);

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);

        Cursor.lockState = altHeld ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = altHeld;

        cameraJustChanged = false; // Solo dura un frame
    }
}