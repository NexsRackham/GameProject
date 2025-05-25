using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;                 // Velocidad de movimiento
    public float rotationSpeed = 180f;           // Velocidad de rotación (modo libre)
    public Transform cameraTransform;            // Transform de la cámara principal

    private Rigidbody rb;
    private Vector3 moveVelocity;

    private CameraFollow cameraFollow;           // Referencia al script de cámara para saber en qué modo está

    public float jumpForce = 5f;        // Fuerza del salto
    public LayerMask groundMask;        // Máscara para definir qué es "suelo"
    public float groundCheckRadius = 0.3f;  // Radio de detección del suelo
    public Transform groundCheck;       // Punto desde donde se verifica el suelo

    private bool isGrounded;            // ¿Está en el suelo?

    public float groundDistance = 3f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform ??= Camera.main.transform;  // Asignar cámara principal si no se asignó
        cameraFollow = cameraTransform.GetComponent<CameraFollow>();
    }

    void Update()
    {
        // Detección de suelo usando Physics.CheckSphere
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        // Leer input de teclado
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        // Crear dirección en base al input, ajustada a la orientación de la cámara
        Vector3 inputDir = new Vector3(inputX, 0f, inputZ).normalized;

        if (!cameraFollow.IsFreeCamera()) // Modo seguimiento
        {
            Vector3 camForward = Vector3.Scale(cameraTransform.forward, Vector3.right + Vector3.forward).normalized;
            Vector3 camRight = Vector3.Scale(cameraTransform.right, Vector3.right + Vector3.forward).normalized;
            moveVelocity = (camForward * inputDir.z + camRight * inputDir.x).normalized * moveSpeed;
        }
        else // Modo libre
        {
            // Movimiento solo hacia adelante/atrás
            moveVelocity = transform.forward * inputDir.z * moveSpeed;

            // Rotación con A y D
            if (inputX != 0f)
                transform.Rotate(0f, inputX * rotationSpeed * Time.deltaTime, 0f);
        }

        // SALTO
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(groundCheck.position, Vector3.down * groundCheckRadius, rayColor);
    }

    void FixedUpdate()
    {
        // Movimiento horizontal
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

        // SOLO rotamos hacia la cámara si estamos en modo seguimiento
        if (!cameraFollow.IsFreeCamera() && !cameraFollow.CameraJustChanged())
        {
            Vector3 lookDir = Vector3.Scale(cameraTransform.forward, Vector3.right + Vector3.forward).normalized;
            if (lookDir != Vector3.zero)
                rb.MoveRotation(Quaternion.LookRotation(lookDir));
        }

        // CANCELAR deslizamiento en pendiente leve si no hay input de movimiento
        if (isGrounded)
        {
            RaycastHit hit;

            if (Physics.Raycast(groundCheck.position + Vector3.up * 0.1f, Vector3.down, out hit, groundDistance + 0.2f, groundMask))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                bool noInput = moveVelocity.magnitude < 0.1f;

                if (slopeAngle > 0.1f && slopeAngle < 45f && noInput)
                {
                    // Cancelamos toda la velocidad
                    rb.velocity = Vector3.zero;
                }
                else if (slopeAngle > 0.1f && slopeAngle < 45f)
                {
                    // Aplica un freno manual solo en Y para evitar deslizamiento vertical
                    Vector3 velocity = rb.velocity;
                    velocity.y = Mathf.Lerp(velocity.y, 0, Time.fixedDeltaTime * 5f); // más alto = más freno
                    rb.velocity = velocity;
                }
            }
        }
    }
}