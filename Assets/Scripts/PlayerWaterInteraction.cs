using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerWaterInteraction : MonoBehaviour
{
    [Header("Ajustes de agua")]
    public float waterDrag = 4f;                 // Mayor resistencia al moverse en agua
    public float floatForce = 9.8f;              // Fuerza de flotación hacia arriba
    public LayerMask waterLayer;                 // Asigna aquí el layer 'Water' desde el Inspector

    private Rigidbody rb;
    private bool isInWater = false;
    private float originalDrag;
    private bool originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Guardamos los valores originales para restaurarlos al salir del agua
        originalDrag = rb.drag;
        originalGravity = rb.useGravity;
    }

    void FixedUpdate()
    {
        if (isInWater)
        {
            // Simula flotación simple
            rb.useGravity = false;
            rb.drag = waterDrag;
            rb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);
        }
        else
        {
            // Restaura propiedades originales fuera del agua
            rb.useGravity = originalGravity;
            rb.drag = originalDrag;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsInWaterLayer(other.gameObject))
        {
            isInWater = true;
            Debug.Log("Jugador entró al agua");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsInWaterLayer(other.gameObject))
        {
            isInWater = false;
            Debug.Log("Jugador salió del agua");
        }
    }

    // Función auxiliar para verificar si el objeto está en el layer de agua
    private bool IsInWaterLayer(GameObject obj)
    {
        return (waterLayer.value & (1 << obj.layer)) > 0;
    }
}