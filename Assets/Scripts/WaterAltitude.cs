using UnityEngine;

/// <summary>
/// Permite obtener la altura y fase de las olas en una posición dada,
/// basada en una función seno parametrizable.
/// Ideal para shaders de agua que animan vértices con la misma lógica.
/// </summary>
[DisallowMultipleComponent]
public class WaterAltitude : MonoBehaviour
{
    [Header("Parámetros de la ola")]
    [Tooltip("Altura máxima de las olas.")]
    public float waveAmplitude = 0.5f;

    [Tooltip("Frecuencia espacial de las olas (a mayor valor, más ondas por unidad).")]
    public float waveFrequency = 1.0f;

    [Tooltip("Velocidad a la que se propagan las olas (en el tiempo).")]
    public float waveSpeed = 1.0f;

    [Tooltip("Layer para ignorar el agua al hacer raycasts hacia el fondo.")]
    public LayerMask groundMask;

    /// <summary>
    /// Devuelve la altura de la ola en una posición (x,z) del mundo, sincronizada con el tiempo actual.
    /// </summary>
    public float GetWaterHeightAtPosition(Vector3 worldPosition)
    {
        float wave = Mathf.Sin((worldPosition.x + worldPosition.z) * waveFrequency + Time.time * waveSpeed);
        return transform.position.y + wave * waveAmplitude;
    }

    /// <summary>
    /// Devuelve la fase normalizada de la ola en una posición dada, sin amplitud ni altura base.
    /// Útil para sincronizar animaciones como el bamboleo o sonido del agua.
    /// </summary>
    public float GetWavePhaseAtPosition(Vector3 worldPosition)
    {
        return Mathf.Sin((worldPosition.x + worldPosition.z) * waveFrequency + Time.time * waveSpeed);
    }

    /// <summary>
    /// Realiza un raycast hacia abajo para obtener la altura del fondo marino en la posición dada.
    /// </summary>
    public float GetBottomHeightAtPosition(Vector3 worldPosition)
    {
        Vector3 rayOrigin = worldPosition + Vector3.up * 0.1f; // El float evita autocollision, modificar en base a la altura del jugador
        Debug.DrawRay(rayOrigin, Vector3.down * 5000f, Color.cyan);
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 5000f, groundMask))
        {
            //Debug.Log($"[!] Hit Ground at Y: {hit.point.y}");
            return hit.point.y;
        }
        else
        {
            //Debug.LogWarning($"[X] No hit at {rayOrigin} con groundMask: {groundMask.value}");
            return worldPosition.y - 5000f; // Valor por defecto si no golpea nada
        }
    }
    public Vector3 GetWaveNormalAtPosition(Vector3 worldPosition)
    {
        float dx = waveFrequency * Mathf.Cos((worldPosition.x + worldPosition.z) * waveFrequency + Time.time * waveSpeed);
        float dz = waveFrequency * Mathf.Cos((worldPosition.x + worldPosition.z) * waveFrequency + Time.time * waveSpeed);
        Vector3 normal = new Vector3(-dx, 1f, -dz).normalized;
        return normal;
    }
}


