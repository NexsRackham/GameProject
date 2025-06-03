using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerWaterInteraction : MonoBehaviour
{
    [Header("Ajustes de agua")]
    public float waterDrag = 4f;                  // Resistencia al movimiento en el agua
    public float floatForce = 9.8f;               // Fuerza de flotación básica (no usada en modo de nado libre)
    public LayerMask waterLayer;                  // Layer para identificar objetos de agua

    [Header("Nado libre")]
    public float swimSpeed = 4f;                  // Velocidad base general de nado
    public float verticalSwimMultiplier = 2.5f;  // Multiplicador de velocidad para ascenso/descenso

    [Header("Oxígeno")]
    public float maxOxygen = 10f;                 // Cantidad máxima de oxígeno bajo el agua
    public float oxygenConsumptionRate = 1f;     // Oxígeno consumido por segundo bajo el agua
    public float oxygenRecoveryRate = 2f;        // Oxígeno recuperado por segundo fuera del agua

    private float currentOxygen;

    private Rigidbody rb;
    private bool isInWater = false;
    private float originalDrag;
    private bool originalGravity;

    private WaterAltitude currentWater;
    private Vector3 swimInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalDrag = rb.drag;
        originalGravity = rb.useGravity;
        currentOxygen = maxOxygen;
    }

    void Update()
    {
        if (isInWater)
        {
            // Entrada horizontal y profundidad
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Entrada vertical para nadar hacia arriba/abajo con multiplicador para mayor velocidad
            float up = 0f;
            if (Input.GetKey(KeyCode.Space)) up += 1f;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) up -= 1f;

            // Asignar swimInput con multiplicador aplicado en el eje Y directamente aquí
            swimInput = new Vector3(horizontal, up * verticalSwimMultiplier, vertical);
        }
        else
        {
            swimInput = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (isInWater && currentWater != null)
        {
            rb.useGravity = false;
            rb.drag = waterDrag;

            float waterSurfaceY = currentWater.GetWaterHeightAtPosition(transform.position);

            bool isSwimming = swimInput != Vector3.zero;
            bool isSubmerged = transform.position.y < waterSurfaceY - 0.1f;

            if (isSwimming)
            {
                // Movimiento horizontal relativo a cámara
                Vector3 horizontalDir = Camera.main.transform.TransformDirection(new Vector3(swimInput.x, 0f, swimInput.z));

                // Movimiento combinado: horizontal + vertical (ya con multiplicador)
                Vector3 moveDir = horizontalDir + Vector3.up * swimInput.y;

                // Aplicar fuerza SIN normalizar, para respetar magnitud y velocidad vertical mayor
                rb.AddForce(moveDir * swimSpeed, ForceMode.Acceleration);
            }
            else if (!isSubmerged)
            {
                // Flotación pasiva en superficie cuando no hay input
                Vector3 targetPos = new Vector3(transform.position.x, waterSurfaceY, transform.position.z);
                float smoothSpeed = 2.5f;
                Vector3 smoothedPos = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * smoothSpeed);
                rb.MovePosition(new Vector3(transform.position.x, smoothedPos.y, transform.position.z));
            }

            // Oxígeno bajo el agua
            if (isSubmerged)
            {
                currentOxygen -= oxygenConsumptionRate * Time.fixedDeltaTime;
                currentOxygen = Mathf.Max(0f, currentOxygen);

                if (currentOxygen <= 2f)
                    Debug.LogWarning("¡Oxígeno bajo!");

                if (currentOxygen <= 0f)
                    Debug.LogError("¡Sin oxígeno! (falta aplicar daño)");
            }
            else
            {
                // Recuperación fuera del agua
                currentOxygen += oxygenRecoveryRate * Time.fixedDeltaTime;
                currentOxygen = Mathf.Min(maxOxygen, currentOxygen);
            }
        }
        else
        {
            // Fuera del agua, restaurar drag y gravedad
            rb.useGravity = originalGravity;
            rb.drag = originalDrag;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsInWaterLayer(other.gameObject))
        {
            isInWater = true;
            currentWater = other.GetComponent<WaterAltitude>();
            if (currentWater == null)
                Debug.LogWarning("El objeto de agua no tiene WaterAltitude.");

            Debug.Log("Jugador entró al agua");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsInWaterLayer(other.gameObject))
        {
            isInWater = false;
            currentWater = null;
            Debug.Log("Jugador salió del agua");
        }
    }

    private bool IsInWaterLayer(GameObject obj)
    {
        return (waterLayer.value & (1 << obj.layer)) > 0;
    }
}