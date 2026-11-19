using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Final2 : MonoBehaviour
{
    [Tooltip("Escribe el nombre exacto de la escena de victoria. Si lo dejas vacío, cargará la siguiente escena en Build Settings.")]
    [SerializeField] private string nombreEscenaVictoria = "";

    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;

        if (other.CompareTag("Player"))
        {
            activado = true;
            StartCoroutine(CargarVictoriaConRetraso());
        }
    }

    private IEnumerator CargarVictoriaConRetraso()
    {
        yield return null;

        if (!string.IsNullOrEmpty(nombreEscenaVictoria))
        {
            SceneManager.LoadScene(nombreEscenaVictoria);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}