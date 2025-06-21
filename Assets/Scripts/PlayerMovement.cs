using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;                 // Velocidad de movimiento
    public float rotationSpeed = 180f;           // Velocidad de rotación (modo libre)
    public Transform cameraTransform;            // Transform de la cámara principal

    private Rigidbody rb;
    private Vector3 moveVelocity;

    private CameraFollow cameraFollow;           // Referencia al script de cámara para saber en qué modo está

    [Header("Salto y Suelo")]
    public float jumpForce = 5f;                 // Fuerza del salto
    public LayerMask groundMask;                 // Máscara para definir qué es "suelo"
    public float groundCheckRadius = 0.3f;       // Radio de detección del suelo
    public Transform groundCheck;                // Punto desde donde se verifica el suelo
    private bool isGrounded;                     // ¿Está en el suelo?
    public float groundDistance = 3f;            // Distancia para detectar pendientes

    [Header("Nado")]
    private PlayerWaterInteraction waterInteraction;  // Referencia al sistema de agua

    //Variables para vuelo
    private bool isFlying = false;
    private bool shiftHeld = false;
    public float flightSpeed = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform ??= Camera.main.transform;
        cameraFollow = cameraTransform.GetComponent<CameraFollow>();
        waterInteraction = GetComponent<PlayerWaterInteraction>();
    }

    void Update()
    {
        // Verificar si está tocando suelo
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        // Si está nadando, no hacemos nada en Update. El nado se maneja desde otro script
        if (waterInteraction != null && waterInteraction.IsSwimming)
        {
            moveVelocity = Vector3.zero;
            return;
        }

        // Capturar input del jugador
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(inputX, 0f, inputZ).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            if (cameraFollow != null && cameraFollow.IsFreeCamera())
            {
                // MODO CÁMARA LIBRE: rotar con teclas A/D y moverse adelante/atrás
                float rotation = inputX * rotationSpeed * Time.deltaTime;
                transform.Rotate(0f, rotation, 0f);
                moveVelocity = transform.forward * inputZ * moveSpeed;
            }
            else
            {
                // MODO SEGUIMIENTO: mover y rotar según la cámara
                Vector3 camForward = Vector3.Scale(cameraTransform.forward, Vector3.right + Vector3.forward).normalized;
                Vector3 camRight = Vector3.Scale(cameraTransform.right, Vector3.right + Vector3.forward).normalized;

                Vector3 moveDir = (camForward * inputZ + camRight * inputX).normalized;

                // Rotación suave hacia la dirección de movimiento
                if (moveDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
                }

                moveVelocity = moveDir * moveSpeed;
            }
        }
        else
        {
            moveVelocity = Vector3.zero;
        }

        // SALTO
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Visualización de la esfera de detección del suelo
        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(groundCheck.position, Vector3.down * groundCheckRadius, rayColor);

        // Detectar combinación ALT + V para cambiar modo vuelo
        shiftHeld = Input.GetKey(KeyCode.LeftShift);

        if (shiftHeld && Input.GetKeyDown(KeyCode.V))
        {
            isFlying = !isFlying;
            rb.useGravity = !isFlying;

            if (isFlying)
            {
                Debug.Log("Modo vuelo activado.");
            }
            else
            {
                Debug.Log("Modo vuelo desactivado.");
            }
        }

    }

    void FixedUpdate()
    {
        if (isFlying)
        {
            Vector3 moveDir = Vector3.zero;

            // Movimiento horizontal
            float h = Input.GetAxis("Horizontal"); // A/D
            float v = Input.GetAxis("Vertical");   // W/S
            moveDir += transform.forward * v;
            moveDir += transform.right * h;

            // Subir y bajar
            if (Input.GetKey(KeyCode.Space)) moveDir += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl)) moveDir += Vector3.down;

            rb.velocity = moveDir.normalized * flightSpeed;

            return; // Omitimos movimiento terrestre si estamos volando
        }

        // Si está nadando, la física es controlada por el sistema de nado
        if (waterInteraction != null && waterInteraction.IsSwimming)
            return;

        // Aplicar movimiento horizontal (preservando la velocidad vertical actual)
        Vector3 velocity = moveVelocity;
        velocity.y = rb.velocity.y;
        rb.velocity = velocity;

        // ROTAR automáticamente hacia la cámara si estamos en modo seguimiento y no hubo cambio de cámara reciente
        if (!cameraFollow.IsFreeCamera() && !cameraFollow.CameraJustChanged())
        {
            Vector3 lookDir = Vector3.Scale(cameraTransform.forward, Vector3.right + Vector3.forward).normalized;
            if (lookDir != Vector3.zero)
                rb.MoveRotation(Quaternion.LookRotation(lookDir));
        }

        // CONTROL DE DESLIZAMIENTO EN PENDIENTES
        if (isGrounded)
        {
            if (Physics.Raycast(groundCheck.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, groundDistance + 0.2f, groundMask))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                bool noInput = moveVelocity.magnitude < 0.1f;

                if (slopeAngle > 0.1f && slopeAngle < 45f && noInput)
                {
                    rb.velocity = Vector3.zero;
                }
                else if (slopeAngle > 0.1f && slopeAngle < 45f)
                {
                    Vector3 vel = rb.velocity;
                    vel.y = Mathf.Lerp(vel.y, 0, Time.fixedDeltaTime * 5f);
                    rb.velocity = vel;
                }
            }
        }
    }
}