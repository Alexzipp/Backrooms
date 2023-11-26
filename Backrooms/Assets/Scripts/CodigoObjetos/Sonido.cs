using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonido : MonoBehaviour
{
    public AudioClip sonido;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Asegúrate de que el AudioSource tenga el clip de sonido asignado
        if (audioSource != null && sonido != null)
        {
            audioSource.clip = sonido;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ReproducirSonido();
        }
    }

    private void ReproducirSonido()
    {
        if (audioSource != null && sonido != null)
        {
            audioSource.Play();
        }
    }
}