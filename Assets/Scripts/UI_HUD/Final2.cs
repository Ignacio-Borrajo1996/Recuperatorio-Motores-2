using UnityEngine;
using UnityEngine.SceneManagement;

public class Final2 : MonoBehaviour
{
    [SerializeField] private string nombreEscenaVictoria = "";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
}