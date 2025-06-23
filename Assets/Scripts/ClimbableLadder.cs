// ClimbableLadder.cs
using UnityEngine;

/// <summary>
/// Define una escalera interactuable para el jugador.
/// </summary>
public class ClimbableLadder : MonoBehaviour, IInteractable
{
    [SerializeField] private Collider climbCollider;
    private PlayerClimb attachedPlayer;

    private void Awake()
    {
        if (climbCollider == null)
        {
            climbCollider = GetComponent<Collider>();
            if (climbCollider == null)
                Debug.LogError($"[ClimbableLadder] No se encontró un Collider en {gameObject.name}");
        }
    }

    public string GetInteractionPrompt()
    {
        return attachedPlayer != null ? "Press E to let go" : "Press E to climb";
    }

    public void Interact(PlayerInteractor interactor)
    {
        Debug.Log("Interact called on ClimbableLadder");

        PlayerClimb playerClimb = interactor.GetComponent<PlayerClimb>();
        if (playerClimb == null)
        {
            Debug.LogWarning("PlayerClimb no encontrado en el jugador");
            return;
        }

        if (attachedPlayer == null)
        {
            attachedPlayer = playerClimb;
            playerClimb.AttachToLadder(this);

            // Forzar mensaje permanente mientras escalamos
            interactor.ShowPersistentPrompt("Press E to let go");

            Debug.Log("Jugador afirmado a escalera");
        }
        else
        {
            playerClimb.DetachFromLadder();
            attachedPlayer = null;
            Debug.Log("Jugador soltado de escalera");
        }
    }

    /// <summary>
    /// Llamado por PlayerClimb al soltarse automáticamente por arriba o abajo.
    /// </summary>
    public void NotifyDetached(PlayerClimb player)
    {
        // También debemos ocultar el prompt manualmente aquí
        PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();
        interactor?.HidePrompt();

        if (attachedPlayer == player)
            attachedPlayer = null;
    }

}
