using UnityEngine;
using UnityEngine.Audio;

public class Coleccionable : MonoBehaviour
{
    [Header("Configuración de Tipo")]
    [SerializeField] private bool esMeta = false;

    [Header("Efectos Visuales")]
    [SerializeField] private bool rotar = true;
    [SerializeField] private float velocidadRotacion = 90f;

    [Header("Punto de Reaparición")]
    [SerializeField] private Transform puntoDeInicio;

    [Header("Audio y Efectos (SFX)")]
    [SerializeField] private AudioClip clipSonidoAgarrar;
    [SerializeField] private AudioClip clipSonidoVictoria;
    [SerializeField] private AudioMixerGroup grupoAudioMixer;

    [Header("Audio y Voces (Bloque E)")]
    [SerializeField] private AudioSource fuenteAudioExterna;
    [SerializeField] private AudioClip clipVoz5;
    [SerializeField] private AudioClip clipVoz6;
    [SerializeField] private AudioClip clipVoz7;
    [SerializeField] private AudioClip clipVoz8;

    private bool yaRecogido = false;

    private void Update()
    {
        if (rotar)
        {
            transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaRecogido) return;

        if (other.CompareTag("Player"))
        {
            yaRecogido = true;

            if (esMeta)
            {
                ReproducirClipSeguro(clipSonidoVictoria);
                ReproducirVozMetaAleatoria();
            }
            else
            {
                ReproducirClipSeguro(clipSonidoAgarrar);
                ReproducirVozAleatoria();

                if (ContadorContrarreloj.Instancia != null)
                {
                    ContadorContrarreloj.Instancia.RegistrarColeccionable();
                }

                TeletransportarJugador(other.gameObject);
                Destroy(gameObject);
            }
        }
    }

    private void ReproducirVozAleatoria()
    {
        AudioClip clipElegido = (Random.value < 0.5f) ? clipVoz5 : clipVoz6;
        ReproducirClipSeguro(clipElegido);
    }

    private void ReproducirVozMetaAleatoria()
    {
        AudioClip clipElegido = (Random.value < 0.5f) ? clipVoz7 : clipVoz8;
        ReproducirClipSeguro(clipElegido);
    }

    private void ReproducirClipSeguro(AudioClip clip)
    {
        if (clip == null) return;

        if (fuenteAudioExterna != null && fuenteAudioExterna.gameObject != this.gameObject)
        {
            if (grupoAudioMixer != null)
            {
                fuenteAudioExterna.outputAudioMixerGroup = grupoAudioMixer;
            }
            fuenteAudioExterna.PlayOneShot(clip);
            return;
        }

        GameObject audioTemp = new GameObject("SFX_Coleccionable_" + clip.name);
        DontDestroyOnLoad(audioTemp);

        AudioSource sourceTemp = audioTemp.AddComponent<AudioSource>();
        sourceTemp.clip = clip;
        sourceTemp.spatialBlend = 0f;

        if (grupoAudioMixer != null)
        {
            sourceTemp.outputAudioMixerGroup = grupoAudioMixer;
        }

        sourceTemp.Play();
        Destroy(audioTemp, clip.length + 0.1f);
    }

    private void TeletransportarJugador(GameObject jugador)
    {
        Transform destino = puntoDeInicio;

        if (destino == null)
        {
            GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");
            if (spawnObj != null)
            {
                destino = spawnObj.transform;
            }
        }

        if (destino != null)
        {
            CharacterController controller = jugador.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                jugador.transform.position = destino.position;
                jugador.transform.rotation = destino.rotation;
                controller.enabled = true;
            }
            else
            {
                jugador.transform.position = destino.position;
                jugador.transform.rotation = destino.rotation;
            }

            Rigidbody rb = jugador.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}