using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class plataforms : MonoBehaviour
{
    public enum Direction
    {
        Left, Right, Up, Down, Forward, Backward
    }

    public enum WindDirection
    {
        Forward, Backward, Right, Left, Up, Down
    }

    #region Movement

    [Header("Movement Settings")]
    public bool CanMove = true;

    [SerializeField] private Direction direction = Direction.Right;
    [SerializeField, Min(0f)] private float distance = 20f;
    [SerializeField, Min(0f)] private float speed = 2f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingToEnd = true;

    public Vector3 DeltaThisFrame { get; private set; }

    #endregion

    #region Ventilador (Viento)

    [Header("Parámetros del Viento")]
    public bool CanUseWind = false;
    [SerializeField] private WindDirection windDirection = WindDirection.Forward;
    [SerializeField, Min(0f)] private float fuerza = 20f;
    [SerializeField, Min(0.1f)] private float alcance = 5f;
    [SerializeField, Min(0.1f)] private float ancho = 1f;
    [SerializeField, Min(0.1f)] private float alto = 1f;

    [Header("Configuración Intermitente")]
    [SerializeField] private bool esIntermitente = false;
    [SerializeField, Min(0.01f)] private float tiempoEncendido = 3f;
    [SerializeField, Min(0.01f)] private float tiempoApagado = 5f;

    [Header("Efectos Visuales")]
    [SerializeField] private ParticleSystem efectoParticulasViento;

    private BoxCollider colliderCuerpoSolido;
    private BoxCollider areaVientoTrigger;
    private bool estaActivo = true;
    private Coroutine corrutinaIntermitente;

    private readonly Dictionary<Movement, HashSet<Collider>> windPlayerColliders =
        new Dictionary<Movement, HashSet<Collider>>();

    #endregion

    #region Initialization

    private void Awake()
    {
        ConfigurarEstructuraVentilador();
    }

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + GetDirection() * distance;

        ActualizarAlcance();
        GestionarEstadoIntermitente();
    }

    private void Update()
    {
        UpdateMovement();
        GestionarEstadoIntermitente();
    }

    #endregion

    #region Movement Logic

    private void UpdateMovement()
    {
        DeltaThisFrame = Vector3.zero;

        if (!CanMove || distance <= 0f || speed <= 0f)
            return;

        Vector3 previousPosition = transform.position;
        Vector3 target = movingToEnd ? endPosition : startPosition;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        DeltaThisFrame = transform.position - previousPosition;

        if ((transform.position - target).sqrMagnitude < 0.000001f)
        {
            transform.position = target;
            DeltaThisFrame = transform.position - previousPosition;
            movingToEnd = !movingToEnd;
        }
    }

    private Vector3 GetDirection()
    {
        return direction switch
        {
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            Direction.Up => Vector3.up,
            Direction.Down => Vector3.down,
            Direction.Forward => Vector3.forward,
            Direction.Backward => Vector3.back,
            _ => Vector3.right
        };
    }

    #endregion

    #region Ventilador Logic

    private Vector3 GetWindDirectionLocal()
    {
        return windDirection switch
        {
            WindDirection.Forward => Vector3.forward,
            WindDirection.Backward => Vector3.back,
            WindDirection.Right => Vector3.right,
            WindDirection.Left => Vector3.left,
            WindDirection.Up => Vector3.up,
            WindDirection.Down => Vector3.down,
            _ => Vector3.forward
        };
    }

    private Vector3 GetWindDirectionWorld()
    {
        return transform.TransformDirection(GetWindDirectionLocal()).normalized;
    }

    private void OnValidate()
    {
        distance = Mathf.Max(0f, distance);
        speed = Mathf.Max(0f, speed);
        alcance = Mathf.Max(0.1f, alcance);
        ancho = Mathf.Max(0.1f, ancho);
        alto = Mathf.Max(0.1f, alto);
        tiempoEncendido = Mathf.Max(0.01f, tiempoEncendido);
        tiempoApagado = Mathf.Max(0.01f, tiempoApagado);

        AjustarColliderBase();

        Transform hijoArea = transform.Find("ZonaDeAire");
        if (hijoArea != null)
        {
            areaVientoTrigger = hijoArea.GetComponent<BoxCollider>();
        }

        ActualizarAlcance();
    }

    private void AjustarColliderBase()
    {
        if (colliderCuerpoSolido == null)
            colliderCuerpoSolido = GetComponent<BoxCollider>();

        if (colliderCuerpoSolido != null)
        {
            colliderCuerpoSolido.isTrigger = false;
            colliderCuerpoSolido.size = Vector3.one;
            colliderCuerpoSolido.center = Vector3.zero;
        }
    }

    private void ConfigurarEstructuraVentilador()
    {
        AjustarColliderBase();

        Transform hijoArea = transform.Find("ZonaDeAire");
        if (hijoArea == null)
        {
            GameObject objHijo = new GameObject("ZonaDeAire");
            objHijo.transform.SetParent(transform);
            objHijo.transform.localPosition = Vector3.zero;
            objHijo.transform.localRotation = Quaternion.identity;
            objHijo.transform.localScale = Vector3.one;
            hijoArea = objHijo.transform;
        }

        areaVientoTrigger = hijoArea.GetComponent<BoxCollider>();
        if (areaVientoTrigger == null)
        {
            areaVientoTrigger = hijoArea.gameObject.AddComponent<BoxCollider>();
        }

        areaVientoTrigger.isTrigger = true;

        ZonaVientoDetector detector = hijoArea.GetComponent<ZonaVientoDetector>();
        if (detector == null)
        {
            detector = hijoArea.gameObject.AddComponent<ZonaVientoDetector>();
        }
        detector.Inicializar(this);
    }

    private void GestionarEstadoIntermitente()
    {
        if (!CanUseWind)
        {
            if (corrutinaIntermitente != null)
            {
                StopCoroutine(corrutinaIntermitente);
                corrutinaIntermitente = null;
            }
            CambiarEstado(false);
            return;
        }

        if (esIntermitente && corrutinaIntermitente == null)
        {
            corrutinaIntermitente = StartCoroutine(RutinaIntermitente());
        }
        else if (!esIntermitente && corrutinaIntermitente != null)
        {
            StopCoroutine(corrutinaIntermitente);
            corrutinaIntermitente = null;
            CambiarEstado(true);
        }
    }

    private void ActualizarAlcance()
    {
        Vector3 dir = GetWindDirectionLocal();

        if (areaVientoTrigger != null)
        {
            if (dir == Vector3.forward)
            {
                areaVientoTrigger.center = new Vector3(0f, 0f, alcance / 2f);
                areaVientoTrigger.size = new Vector3(ancho, alto, alcance);
            }
            else if (dir == Vector3.back)
            {
                areaVientoTrigger.center = new Vector3(0f, 0f, -alcance / 2f);
                areaVientoTrigger.size = new Vector3(ancho, alto, alcance);
            }
            else if (dir == Vector3.right)
            {
                areaVientoTrigger.center = new Vector3(alcance / 2f, 0f, 0f);
                areaVientoTrigger.size = new Vector3(alcance, alto, ancho);
            }
            else if (dir == Vector3.left)
            {
                areaVientoTrigger.center = new Vector3(-alcance / 2f, 0f, 0f);
                areaVientoTrigger.size = new Vector3(alcance, alto, ancho);
            }
            else if (dir == Vector3.up)
            {
                areaVientoTrigger.center = new Vector3(0f, alcance / 2f, 0f);
                areaVientoTrigger.size = new Vector3(ancho, alcance, alto);
            }
            else
            {
                areaVientoTrigger.center = new Vector3(0f, -alcance / 2f, 0f);
                areaVientoTrigger.size = new Vector3(ancho, alcance, alto);
            }
        }

        if (efectoParticulasViento != null)
        {
            efectoParticulasViento.transform.localPosition = Vector3.zero;
            efectoParticulasViento.transform.localRotation = Quaternion.LookRotation(dir);
        }
    }

    private IEnumerator RutinaIntermitente()
    {
        while (esIntermitente && CanUseWind)
        {
            CambiarEstado(true);
            yield return new WaitForSeconds(tiempoEncendido);

            CambiarEstado(false);
            yield return new WaitForSeconds(tiempoApagado);
        }
    }

    private void CambiarEstado(bool activo)
    {
        bool estadoFinal = CanUseWind && activo;

        if (estaActivo == estadoFinal && areaVientoTrigger != null && areaVientoTrigger.enabled == estadoFinal)
            return;

        estaActivo = estadoFinal;

        if (areaVientoTrigger != null)
        {
            areaVientoTrigger.enabled = estaActivo;
        }

        if (efectoParticulasViento != null)
        {
            if (estaActivo)
            {
                if (!efectoParticulasViento.isPlaying)
                    efectoParticulasViento.Play();
            }
            else
            {
                if (efectoParticulasViento.isPlaying)
                    efectoParticulasViento.Stop();
            }
        }

        if (!estaActivo)
        {
            foreach (Movement jugador in windPlayerColliders.Keys)
            {
                if (jugador != null)
                    jugador.StopWind(this);
            }
            windPlayerColliders.Clear();
        }
    }

    public void ProcesarEmpuje(Collider other)
    {
        if (!CanUseWind || !estaActivo || other == null)
            return;

        Vector3 direccionAire = GetWindDirectionWorld();

        Movement jugador = other.GetComponentInParent<Movement>();
        if (jugador != null)
        {
            if (!windPlayerColliders.TryGetValue(jugador, out HashSet<Collider> colliders))
            {
                colliders = new HashSet<Collider>();
                windPlayerColliders.Add(jugador, colliders);
            }
            colliders.Add(other);

            jugador.ApplyWind(this, direccionAire * fuerza);
            return;
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(direccionAire * fuerza, ForceMode.Acceleration);
        }
    }

    public void SalirDelViento(Collider other)
    {
        if (other == null)
            return;

        Movement jugador = other.GetComponentInParent<Movement>();
        if (jugador == null)
            return;

        if (!windPlayerColliders.TryGetValue(jugador, out HashSet<Collider> colliders))
            return;

        colliders.Remove(other);

        if (colliders.Count == 0)
        {
            windPlayerColliders.Remove(jugador);
            jugador.StopWind(this);
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (!CanUseWind)
            return;

        Gizmos.color = estaActivo ? Color.cyan : Color.gray;
        Gizmos.DrawRay(transform.position, GetWindDirectionWorld() * alcance);
    }

    #endregion
}

public class ZonaVientoDetector : MonoBehaviour
{
    private plataforms ventiladorPadre;

    public void Inicializar(plataforms padre)
    {
        ventiladorPadre = padre;
    }

    private void OnTriggerStay(Collider other)
    {
        if (ventiladorPadre != null)
        {
            ventiladorPadre.ProcesarEmpuje(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ventiladorPadre != null)
        {
            ventiladorPadre.SalirDelViento(other);
        }
    }
}