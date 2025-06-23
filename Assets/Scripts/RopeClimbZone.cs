using UnityEngine;

/// <summary>
/// Controla la zona de escalada, detecta en qué lado está el jugador
/// y gestiona la entrada y salida del modo escalada.
/// </summary>
public class RopeClimbZone : MonoBehaviour
{
    public Transform climbAnchorLeft;
    public Transform climbAnchorRight;
    public Transform climbExitPointLeft;
    public Transform climbExitPointRight;

    private GameObject playerInLeftZone = null;
    private GameObject playerInRightZone = null;

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.E))
    //    {
    //        // Primero comprobamos si el jugador está escalando para soltarlo
    //        PlayerClimb climbingPlayer = null;

    //        if (playerInLeftZone != null)
    //            climbingPlayer = playerInLeftZone.GetComponent<PlayerClimb>();
    //        else if (playerInRightZone != null)
    //            climbingPlayer = playerInRightZone.GetComponent<PlayerClimb>();

    //        if (climbingPlayer != null && climbingPlayer.IsClimbing())
    //        {
    //            climbingPlayer.ExitClimb();
    //        }
    //        else
    //        {
    //            // Si no está escalando, empezamos la escalada normalmente
    //            if (playerInLeftZone != null)
    //            {
    //                StartClimbing(playerInLeftZone, climbAnchorLeft, climbExitPointLeft);
    //            }
    //            else if (playerInRightZone != null)
    //            {
    //                StartClimbing(playerInRightZone, climbAnchorRight, climbExitPointRight);
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Se llama desde los triggers hijos para avisar si el jugador entró o salió.
    /// </summary>
    /// <param name="player">Jugador</param>
    /// <param name="isInZone">Entró (true) o salió (false)</param>
    /// <param name="trigger">Trigger que envía el evento</param>
    public void SetPlayerInZone(GameObject player, bool isInZone, RopeTrigger trigger)
    {
        // Dependiendo de cuál trigger se active, asigna jugador a zona izquierda o derecha
        if (trigger.gameObject.name == "LeftTrigger")
        {
            playerInLeftZone = isInZone ? player : null;
        }
        else if (trigger.gameObject.name == "RightTrigger")
        {
            playerInRightZone = isInZone ? player : null;
        }
    }

    //private void StartClimbing(GameObject player, Transform anchor, Transform exitPoint)
    //{
    //    PlayerClimb climb = player.GetComponent<PlayerClimb>();
    //    if (climb != null && !climb.IsClimbing())
    //    {
    //        climb.EnterClimb(anchor, exitPoint);
    //    }
    //}
}
