// PlayerInteractor.cs
using UnityEngine;
using TMPro;

/// <summary>
/// Detecta objetos interactuables al frente del jugador y permite interactuar con ellos.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Interacción")]
    public float interactRange = 2f;                    // Distancia máxima de interacción
    public LayerMask interactableMask;                 // Máscara de capas válidas

    [Header("UI")]
    public GameObject interactionPromptUI;             // UI de interacción (Canvas hijo)
    public TextMeshProUGUI interactionText;           // Texto dinámico ("Press E to ...")

    private IInteractable currentInteractable;

    private bool forcePrompt = false;

    void Update()
    {
        if (!forcePrompt)
        {
            HandleDetection();
        }

        HandleInput();
    }

    /// <summary>
    /// Detecta el objeto interactuable frente al jugador mediante raycast.
    /// </summary>
    void HandleDetection()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                interactionPromptUI.SetActive(true);
                interactionText.text = interactable.GetInteractionPrompt();
                return;
            }
        }

        // Si no detectamos nada, ocultamos UI
        currentInteractable = null;
        interactionPromptUI.SetActive(false);
    }

    /// <summary>
    /// Ejecuta la interacción si el jugador presiona E.
    /// </summary>
    void HandleInput()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact(this);
        }
    }
    public void ShowPersistentPrompt(string text)
    {
        interactionText.text = text;
        interactionPromptUI.SetActive(true);
        forcePrompt = true;
    }

    public void HidePrompt()
    {
        interactionPromptUI.SetActive(false);
        forcePrompt = false;
    }

}
