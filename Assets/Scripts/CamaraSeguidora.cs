using UnityEngine;

public class CamaraSeguidora : MonoBehaviour
{
    [Header("Objetivos")]
    public Transform objetivo;
    public Vector3 offsetPersonaje = new Vector3(0, 1.5f, 0); // Hacia dónde mira la cámara (la cabeza)

    [Header("Configuración")]
    public float distancia = 6f;
    public float sensibilidad = 3f;
    public float suavizadoPosicion = 10f; // Qué tan rápido sigue al jugador

    [Header("Límites")]
    public float limiteYMin = -20f;
    public float limiteYMax = 60f;

    [Header("Colisiones")]
    public bool detectarParedes = true;
    public LayerMask capasObstaculos; // Selecciona "Default" o las capas de tu mapa

    private float anguloX = 0f;
    private float anguloY = 0f;
    private float distanciaActual;

    void Start()
    {
        distanciaActual = distancia;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inicializar ángulos para que la cámara no salte al empezar
        Vector3 angles = transform.eulerAngles;
        anguloX = angles.y;
        anguloY = angles.x;
    }

    void LateUpdate()
    {
        if (!objetivo) return;

        // 1. Entrada del ratón
        anguloX += Input.GetAxis("Mouse X") * sensibilidad;
        anguloY -= Input.GetAxis("Mouse Y") * sensibilidad;
        anguloY = Mathf.Clamp(anguloY, limiteYMin, limiteYMax);

        // 2. Rotación de la cámara
        Quaternion rotacion = Quaternion.Euler(anguloY, anguloX, 0);

        // 3. Cálculo de posición ideal
        Vector3 posicionDeseada = objetivo.position + offsetPersonaje - (rotacion * Vector3.forward * distancia);

        // 4. Detección de colisiones (Raycast)
        if (detectarParedes)
        {
            RaycastHit hit;
            Vector3 direccionDesdeObjetivo = posicionDeseada - (objetivo.position + offsetPersonaje);

            if (Physics.Raycast(objetivo.position + offsetPersonaje, direccionDesdeObjetivo, out hit, distancia, capasObstaculos))
            {
                // Si hay algo en medio, acercamos la cámara
                distanciaActual = Mathf.Clamp(hit.distance * 0.9f, 1f, distancia);
            }
            else
            {
                // Si no hay nada, volvemos suavemente a la distancia original
                distanciaActual = Mathf.Lerp(distanciaActual, distancia, Time.deltaTime * 5f);
            }
            posicionDeseada = objetivo.position + offsetPersonaje - (rotacion * Vector3.forward * distanciaActual);
        }

        // 5. Aplicar posición y rotación con suavizado
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, Time.deltaTime * suavizadoPosicion);
        transform.rotation = rotacion;
    }
}