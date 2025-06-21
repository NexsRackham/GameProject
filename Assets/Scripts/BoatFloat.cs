using UnityEngine;

/// <summary>
/// Sistema de flotación avanzada basado en el Principio de Arquímedes.
/// No depende directamente de la masa, sino del volumen sumergido.
/// Utiliza una malla secundaria como referencia de puntos de flotación.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BoatFloat : MonoBehaviour
{
    [Header("Referencia al agua")]
    public WaterAltitude waterAltitude;

    [Header("Malla de flotación")]
    public MeshFilter floatMesh;

    [Header("Parámetros físicos")]
    [Tooltip("Fuerza de flotación máxima por punto completamente sumergido.")]
    public float buoyancyForcePerPoint = 10f;

    [Tooltip("Densidad del agua (referencia: agua dulce ≈ 1000 kg/m3)")]
    public float waterDensity = 1f;

    [Tooltip("Profundidad a la que un vértice se considera completamente sumergido.")]
    public float fullSubmergeDepth = 1f;

    [Tooltip("Reducción de oscilaciones por rotación brusca.")]
    public float torqueDamping = 0.2f;

    private Rigidbody rb;
    private Vector3[] localVertices;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (floatMesh == null || waterAltitude == null)
        {
            Debug.LogError("[BoatFloat] Falta asignar el FloatMesh o WaterAltitude.");
            enabled = false;
            return;
        }

        localVertices = floatMesh.sharedMesh.vertices;
    }

    private void FixedUpdate()
    {
        float amp = waterAltitude.waveAmplitude;
        float freq = waterAltitude.waveFrequency;
        float speed = waterAltitude.waveSpeed;

        Transform meshTransform = floatMesh.transform;

        foreach (Vector3 localVert in localVertices)
        {
            // Posición mundial del vértice
            Vector3 worldVert = meshTransform.TransformPoint(localVert);

            // Altura actual del agua en esa posición
            float wave = Mathf.Sin((worldVert.x + worldVert.z) * freq + Time.time * speed);
            float waterHeight = waterAltitude.transform.position.y + wave * amp;

            float depth = waterHeight - worldVert.y;

            if (depth > 0f)
            {
                // Se calcula flotabilidad proporcional a la profundidad
                float submersion = Mathf.Clamp01(depth / fullSubmergeDepth);
                float forceMagnitude = submersion * buoyancyForcePerPoint * waterDensity;

                Vector3 force = Vector3.up * forceMagnitude;

                rb.AddForceAtPosition(force, worldVert, ForceMode.Force);

                // También podríamos aplicar estabilización aquí
            }
        }

        // Estabilización rotacional (amortiguador de torque)
        Vector3 angularVel = rb.angularVelocity;
        rb.AddTorque(-angularVel * torqueDamping, ForceMode.Force);
    }
}
