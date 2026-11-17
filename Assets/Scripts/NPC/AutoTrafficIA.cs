using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AutoTrafficIA : MonoBehaviour
{
    [Header("Ruta de Patrulla (Waypoints)")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private int indiceInicial = 0;
    [SerializeField] private float distanciaCambioWaypoint = 2.5f;

    [Header("Detección y Frenado Reactivo")]
    [SerializeField] private float distanciaDeteccion = 9f;
    [SerializeField] private float distanciaFrenadoTotal = 3.5f; 
    [SerializeField] private float radioDeteccion = 1.2f;
    [SerializeField] private float alturaSensor = 0.8f;
    [SerializeField] private float adelantamientoSensor = 1.8f;
    [SerializeField] private LayerMask capasObstaculos;
    [SerializeField] private float velocidadNormal = 11f;
    [SerializeField] private float suavizadoFrenado = 15f;

    private NavMeshAgent agente;
    private int indiceActual;
    private bool obstaculoDetectado;
    private bool frenadoEmergencia;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[AutoTrafficIA] No hay waypoints asignados en {gameObject.name}");
            enabled = false;
            return;
        }

        indiceActual = Mathf.Clamp(indiceInicial, 0, waypoints.Length - 1);
        agente.speed = velocidadNormal;
        IrAlSiguienteWaypoint();
    }

    private void Update()
    {
        if (!agente.isOnNavMesh || waypoints.Length == 0)
            return;

        DetectarObstaculoAdelante();

        if (frenadoEmergencia)
        {
            agente.speed = 0f;
            agente.velocity = Vector3.zero;
            agente.isStopped = true;
        }
        else
        {
            float velocidadObjetivo = obstaculoDetectado ? 0f : velocidadNormal;
            agente.speed = Mathf.Lerp(agente.speed, velocidadObjetivo, Time.deltaTime * suavizadoFrenado);
            agente.isStopped = obstaculoDetectado && agente.speed < 0.3f;
        }

        if (!agente.pathPending && agente.remainingDistance <= distanciaCambioWaypoint)
        {
            indiceActual = (indiceActual + 1) % waypoints.Length;
            IrAlSiguienteWaypoint();
        }
    }

    private void IrAlSiguienteWaypoint()
    {
        if (waypoints[indiceActual] != null)
        {
            agente.SetDestination(waypoints[indiceActual].position);
        }
    }

    private void DetectarObstaculoAdelante()
    {
        Vector3 origen = transform.position + Vector3.up * alturaSensor + transform.forward * adelantamientoSensor;

        if (Physics.SphereCast(origen, radioDeteccion, transform.forward, out RaycastHit hit, distanciaDeteccion, capasObstaculos, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != transform && !hit.transform.IsChildOf(transform))
            {
                obstaculoDetectado = true;
                frenadoEmergencia = hit.distance <= distanciaFrenadoTotal;
                return;
            }
        }

        obstaculoDetectado = false;
        frenadoEmergencia = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = frenadoEmergencia ? Color.magenta : (obstaculoDetectado ? Color.red : Color.green);
        Vector3 origen = transform.position + Vector3.up * alturaSensor + transform.forward * adelantamientoSensor;
        Vector3 fin = origen + transform.forward * distanciaDeteccion;
        Gizmos.DrawLine(origen, fin);
        Gizmos.DrawWireSphere(fin, radioDeteccion);
    }
}