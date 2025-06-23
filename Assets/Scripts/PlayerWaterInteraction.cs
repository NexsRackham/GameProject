//PlayerWaterInteraction.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerWaterInteraction : MonoBehaviour
{
    [Header("Ajustes de agua")]
    public float waterDrag = 4f;                  // Resistencia al movimiento en el agua
    public float floatForce = 9.8f;               // Fuerza de flotación básica
    public LayerMask waterLayer;                  // Layer para identificar objetos de agua

    [Header("Nado libre")]
    public float swimSpeed = 3f;                  // Velocidad base general de nado
    public float verticalSwimMultiplier = 2.5f;   // Multiplicador de velocidad para ascenso/descenso
    public float rotationSpeed = 5f;              // Velocidad de rotación para alinear con la dirección de nado

    [Header("Oxígeno")]
    public float maxOxygen = 10f;                 // Cantidad máxima de oxígeno bajo el agua
    public float oxygenConsumptionRate = 1f;      // Oxígeno consumido por segundo bajo el agua
    public float oxygenRecoveryRate = 2f;         // Oxígeno recuperado por segundo fuera del agua

    [Header("Bamboleo pasivo")]
    [Range(0f, 1f)]
    public float bobbingAmount = 0.7f;

    private float currentOxygen;
    private Rigidbody rb;
    private bool isInWater = false;
    private float originalDrag;
    private bool originalGravity;

    private WaterAltitude currentWater;
    private Vector3 swimInput;

    private float floatBaseHeight; // Base flotante para oscilación

    private float initialWavePhase; // Para almacenar la fase de entrada de la ola

    private PlayerClimb playerClimb;


    // Señal para avisar al PlayerMovement que está nadando
    public bool IsSwimming { get; private set; } = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalDrag = rb.drag;
        originalGravity = rb.useGravity;
        currentOxygen = maxOxygen;
        floatBaseHeight = transform.position.y;
        playerClimb = GetComponent<PlayerClimb>();
    }

    void Update()
    {
        if (isInWater)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            float up = 0f;
            if (Input.GetKey(KeyCode.Space)) up += 1f;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) up -= 1f;

            swimInput = new Vector3(horizontal, up * verticalSwimMultiplier, vertical);
        }
        else
        {
            swimInput = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        IsSwimming = false;

        // Si estamos escalando, ignoramos completamente el agua
        if (playerClimb != null && playerClimb.IsClimbing())
            return;

        if (isInWater && currentWater != null)
        {
            float waterSurfaceY = currentWater.GetWaterHeightAtPosition(transform.position);
            Vector3 currentPos = transform.position;

            bool isSwimming = swimInput != Vector3.zero;
            bool isSubmerged = currentPos.y < waterSurfaceY - 0.1f;

            if (isSwimming)
            {
                rb.useGravity = false;
                rb.drag = waterDrag;

                Transform cam = Camera.main.transform;
                Vector3 inputDir = swimInput.normalized;
                Vector3 moveDir = cam.right * inputDir.x + cam.up * inputDir.y + cam.forward * inputDir.z;
                moveDir = moveDir.normalized;

                if (currentPos.y >= waterSurfaceY - 0.1f && moveDir.y > 0f)
                    moveDir.y = 0f;

                rb.AddForce(moveDir * swimSpeed, ForceMode.Acceleration);

                Vector3 lookDir = cam.forward;
                lookDir.y = Mathf.Clamp(lookDir.y, -1f, 1f);
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
                }

                floatBaseHeight = currentPos.y; // Actualizamos el centro de flotación

                IsSwimming = true;
            }
            else if (isSubmerged)
            {
                rb.useGravity = true;
                rb.drag = waterDrag;

                // Si el jugador no nada verticalmente, mantenemos la altura base y aplicamos oscilación
                if (Mathf.Abs(swimInput.y) < 0.01f)
                {
                    // Sincronizamos con la fase del shader de agua
                    float rawPhase = currentWater.GetWavePhaseAtPosition(currentPos);
                    float relativePhase = rawPhase - initialWavePhase;

                    // Obtenemos el fondo marino real en esta posición
                    float bottomY = currentWater.GetBottomHeightAtPosition(currentPos);
                    float depthToBottom = Mathf.Max(0f, currentPos.y - bottomY);             // Distancia vertical al fondo
                    float totalWaterDepth = Mathf.Max(0.001f, waterSurfaceY - bottomY);        // Profundidad total (evita división por cero)
                    float depthFactor = Mathf.Clamp01(depthToBottom / totalWaterDepth);      // 0 en el fondo, 1 cerca de la superficie

                    // Calculamos el bamboleo de la ola según la fase, atenuado por cercanía al fondo
                    float waveBump = relativePhase * currentWater.waveAmplitude * bobbingAmount * depthFactor;

                    // Limitamos el bamboleo para no atravesar el fondo marino
                    float maxAllowedBump = Mathf.Max(0f, currentPos.y - bottomY); // Distancia libre entre el jugador y el fondo
                    waveBump = Mathf.Clamp(waveBump, -maxAllowedBump, maxAllowedBump);

                    // Eliminar el bamboleo al estar cerca del lecho marino
                    if (depthToBottom > 0.65f)
                    {
                        float currentY = currentPos.y;
                        float targetY = floatBaseHeight + waveBump;
                        float smoothedY = Mathf.Lerp(currentY, targetY, Time.fixedDeltaTime * 2f); // Ajustar Xf para interpolar más rápido/lento
                        Vector3 adjustedPos = new Vector3(currentPos.x, smoothedY, currentPos.z);
                        rb.MovePosition(adjustedPos);
                    }
                    //Debug.Log($"Pos jugador Y: {currentPos.y:F2} | Fondo Y: {bottomY:F2} | Superficie Y: {waterSurfaceY:F2} | depthToBottom: {depthToBottom:F2} | totalWaterDepth: {totalWaterDepth:F2} | depthFactor: {depthFactor:F2}");
                }

                float depth = waterSurfaceY - currentPos.y;
                float floatForceApplied = floatForce * Mathf.Clamp(depth, 0f, 1f);
                rb.AddForce(Vector3.up * floatForceApplied, ForceMode.Acceleration);

                Vector3 forward = transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(forward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
                }
            }
            else
            {
                rb.useGravity = true;
                rb.drag = originalDrag;
            }

            // Manejo de oxígeno
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
                currentOxygen += oxygenRecoveryRate * Time.fixedDeltaTime;
                currentOxygen = Mathf.Min(maxOxygen, currentOxygen);
            }
        }
        else
        {
            rb.useGravity = originalGravity;
            rb.drag = originalDrag;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsInWaterLayer(other.gameObject))
        {
            isInWater = true;

            if (currentWater == null)
            {
                Debug.LogWarning("El objeto de agua no tiene WaterAltitude.");
                currentWater = other.GetComponent<WaterAltitude>();
                if (currentWater != null)
                {
                    initialWavePhase = currentWater.GetWavePhaseAtPosition(transform.position);
                }
            }
            

            // No movemos la posición, solo ajustamos velocidad vertical
            if (rb != null)
            {
                Vector3 vel = rb.velocity;
                vel.y = 0f;
                rb.velocity = vel;
            }

            floatBaseHeight = transform.position.y; // Inicializamos base para oscilación

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