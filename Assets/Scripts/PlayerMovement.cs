using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 5f;
    public float velocidadCorrer = 10f;
    public float alturaSalto = 3f;
    public float gravedad = -19.62f;

    [Header("Referencias")]
    public Transform camara;

    private Animator animator;
    private CharacterController cc;
    private Vector3 velocidadCaida;
    private bool estaEnSuelo;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        // Buscamos el animator en los hijos (donde suele estar el modelo de Maya/Mixamo)
        animator = GetComponentInChildren<Animator>();

        if (camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }
    }

    void Update()
    {
        MoverJugador();
        AplicarGravedadYSalto();
    }

    void MoverJugador()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = camara.forward;
        Vector3 right = camara.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 direccion = (forward * vertical + right * horizontal).normalized;

        bool corriendo = Input.GetKey(KeyCode.LeftShift);
        float velActual = corriendo ? velocidadCorrer : velocidad;

        if (direccion.magnitude >= 0.1f)
        {
            cc.Move(direccion * velActual * Time.deltaTime);

            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 10f);
        }

        // Control del Blend Tree (Idle -> Walk -> Run)
        if (animator != null)
        {
            float valorAnim = direccion.magnitude * (corriendo ? 1f : 0.5f);
            animator.SetFloat("Velocidad", valorAnim, 0.1f, Time.deltaTime);
        }
    }

    void AplicarGravedadYSalto()
    {
        estaEnSuelo = cc.isGrounded;

        if (estaEnSuelo && velocidadCaida.y < 0)
        {
            velocidadCaida.y = -2f;
        }

        // DETECCIÓN DE SALTO (Corregido para funcionar en movimiento)
        if (Input.GetKeyDown(KeyCode.Space) && estaEnSuelo)
        {
            // Fórmula física para salto preciso
            velocidadCaida.y = Mathf.Sqrt(alturaSalto * -2f * gravedad);

            // Avisamos al Animator para que use el trigger "Saltar" desde Any State
            if (animator != null)
            {
                animator.SetTrigger("Saltar");
            }
        }

        velocidadCaida.y += gravedad * Time.deltaTime;
        cc.Move(velocidadCaida * Time.deltaTime);
    }
}