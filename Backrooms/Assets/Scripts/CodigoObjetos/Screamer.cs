using System.Collections;
using UnityEngine;

public class Screamer : MonoBehaviour
{
    public GameObject Susto;
    public float tiempoDeActivacion = 3.0f; // Duración del temporizador en segundos
    private AudioSource audioSource;

    void Start()
    {
        audioSource = Susto.GetComponent<AudioSource>();
        Susto.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ActivarSustoConTemporizador());
        }
    }

    IEnumerator ActivarSustoConTemporizador()
    {
        // Activa la imagen y el sonido
        Susto.SetActive(true);
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Espera el tiempo especificado antes de desactivar la imagen y el sonido
        yield return new WaitForSeconds(tiempoDeActivacion);

        // Desactiva la imagen y el sonido después de que haya pasado el tiempo especificado
        Susto.SetActive(false);
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}