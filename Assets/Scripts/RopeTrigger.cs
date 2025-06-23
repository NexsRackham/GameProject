using UnityEngine;

/// <summary>
/// Este script está en los triggers laterales (LeftTrigger, RightTrigger).
/// Detecta cuando el jugador entra o sale del trigger y notifica al padre RopeClimbZone.
/// </summary>
public class RopeTrigger : MonoBehaviour
{
    private RopeClimbZone parentZone;

    private void Start()
    {
        // Busca el componente RopeClimbZone en el padre
        parentZone = GetComponentInParent<RopeClimbZone>();
        if (parentZone == null)
            Debug.LogError("RopeClimbZone no encontrado en padres de " + gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parentZone.SetPlayerInZone(other.gameObject, true, this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parentZone.SetPlayerInZone(other.gameObject, false, this);
        }
    }
}
