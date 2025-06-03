using UnityEngine;

/// <summary>
/// Permite calcular la altura de una superficie de agua que usa un shader con ondas senoidales
/// basadas en X, Z y el tiempo, como el WaterWavesShader.
/// </summary>
public class WaterAltitude : MonoBehaviour
{
    [Header("Parámetros de la ola (deben coincidir con el shader)")]
    public float waveFrequency = 1.0f;
    public float waveSpeed = 1.0f;
    public float waveHeight = 0.5f;

    /// <summary>
    /// Calcula la altura del agua en un punto del mundo replicando la lógica del shader.
    /// </summary>
    /// <param name="position">Posición en el mundo donde se quiere conocer la altura del agua</param>
    /// <returns>Altura Y del agua en ese punto</returns>
    public float GetWaterHeightAtPosition(Vector3 position)
    {
        float time = Time.time;

        // Calculamos la fase de la onda combinando X y Z
        float wavePhase = (position.x * waveFrequency) + (position.z * waveFrequency) + (time * waveSpeed);

        // Aplicamos la función seno y la escala de altura
        float offsetY = Mathf.Sin(wavePhase) * waveHeight;

        // Sumamos a la altura base del plano (asumimos que este objeto representa el plano de agua)
        return transform.position.y + offsetY;
    }
}