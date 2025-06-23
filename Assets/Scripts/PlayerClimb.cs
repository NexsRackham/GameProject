//PlayerClimb.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerClimb : MonoBehaviour
{
    public float climbSpeed = 3f;
    public float exitTopThreshold = 0.5f;

    private ClimbableLadder currentLadder;
    private Collider ladderCollider;
    private Rigidbody rb;
    private PlayerMovement playerMovement;

    private bool isClimbing = false;
    private Vector3 lateralAnchorPos; // posición lateral fija (X,Z) para el jugador

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!isClimbing) return;

        if (currentLadder == null)
        {
            Debug.LogWarning("[PlayerClimb] currentLadder es null durante escalada. Detach forzado.");
            DetachFromLadder();
            return;
        }

        float verticalInput = Input.GetAxisRaw("Vertical");

        // Salida manual con E
        if (Input.GetKeyDown(KeyCode.E))
        {
            DetachFromLadder();
            return;
        }

        // Salida automática arriba o abajo
        float topY = ladderCollider.bounds.max.y;
        float bottomY = ladderCollider.bounds.min.y;
        float y = transform.position.y;

        if (verticalInput > 0 && y >= topY - exitTopThreshold)
        {
            JumpOffTop();
            return;
        }
        else if (verticalInput < 0 && y <= bottomY + 0.1f)
        {
            DetachFromLadder();
            return;
        }

        // Movimiento vertical
        if (Mathf.Approximately(verticalInput, 0f))
        {
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.velocity = new Vector3(0f, verticalInput * climbSpeed, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing && ladderCollider != null)
        {
            // Mantener X,Z en lateralAnchorPos, Y lo controla rb.velocity
            Vector3 pos = rb.position;
            rb.MovePosition(new Vector3(lateralAnchorPos.x, pos.y, lateralAnchorPos.z));
            //rb.velocity = Vector3.zero; // Para prevenir que cualquier fuerza residual se acumule
        }
    }

    public void AttachToLadder(ClimbableLadder ladder)
    {
        currentLadder = ladder;
        ladderCollider = ladder.GetComponent<Collider>();

        isClimbing = true;

        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        // Congelar rotación y posición lateral (X,Z)
        rb.constraints = RigidbodyConstraints.FreezeRotation |
                         RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ;

        if (playerMovement != null)
            playerMovement.enabled = false;

        // Guardar posición lateral donde agarramos
        Vector3 center = ladderCollider.bounds.center;
        lateralAnchorPos = new Vector3(center.x, 0, center.z);

        // Fijar X,Z en lateralAnchorPos
        rb.MovePosition(new Vector3(lateralAnchorPos.x, rb.position.y, lateralAnchorPos.z));

        // Orientar al jugador hacia la escalera
        if (currentLadder != null)
        {
            // Dirección en que queremos que mire el jugador (hacia la escalera)
            Vector3 lookDirection = -currentLadder.transform.forward;
            
            // Forzamos la rotación mirando hacia esa dirección
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            rb.angularVelocity = Vector3.zero; // eliminar cualquier rotación residual
        }

        Debug.Log("[PlayerClimb] Iniciado modo escalada.");
    }

    public void DetachFromLadder()
    {
        if (!isClimbing) return;

        isClimbing = false;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (playerMovement != null)
            playerMovement.enabled = true;

        currentLadder?.NotifyDetached(this);
        currentLadder = null;
        ladderCollider = null;

        Debug.Log("[PlayerClimb] Salido de escalada.");
    }

    private void JumpOffTop()
    {
        DetachFromLadder();

        Vector3 jumpForce = Vector3.up * 4f + transform.forward * 2f;
        rb.AddForce(jumpForce, ForceMode.VelocityChange);

        Debug.Log("[PlayerClimb] Salto desde la cima.");
    }

    public bool IsClimbing() => isClimbing;
}
