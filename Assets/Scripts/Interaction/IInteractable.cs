// IInteractable.cs
using UnityEngine;

/// <summary>
/// Interfaz para objetos que pueden ser interactuados por el jugador.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Retorna el mensaje a mostrar al jugador (por ejemplo: "Press E to climb").
    /// </summary>
    string GetInteractionPrompt();

    /// <summary>
    /// Ejecuta la lógica específica al interactuar (por ejemplo: afirmar al jugador).
    /// </summary>
    void Interact(PlayerInteractor interactor);
}
