using UnityEngine;

public class WaterSimpleMotion : MonoBehaviour
{
    [Header("Movimiento de textura")]
    public Vector2 scrollSpeed = new Vector2(0.05f, 0.03f);
    private Vector2 currentOffset = Vector2.zero;
    private Material waterMaterial;

    [Header("Oscilación vertical")]
    public float amplitude = 0.1f; // Altura del oleaje (ej. 0.1f)
    public float speed = 1.0f;     // Velocidad del oleaje (ej. 1.0f)
    private float baseY;          // Altura base inicial

    void Start()
    {
        // Instanciar el material para no afectar globalmente
        waterMaterial = GetComponent<MeshRenderer>().material = new Material(GetComponent<MeshRenderer>().material);

        // Guardar posición Y inicial del agua
        baseY = transform.position.y;
    }

    void Update()
    {
        // 1. Desplazamiento UV
        currentOffset += scrollSpeed * Time.deltaTime;
        waterMaterial.SetTextureOffset("_BaseMap", currentOffset);

        // 2. Movimiento vertical senoidal
        float waveY = Mathf.Sin(Time.time * speed) * amplitude;
        Vector3 pos = transform.position;
        pos.y = baseY + waveY;
        transform.position = pos;
    }
}