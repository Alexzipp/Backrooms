using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerImagen : MonoBehaviour
{
    public MeshRenderer meshRenderer; // Referencia al componente MeshRenderer

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entró en el trigger tiene el tag adecuado (si es necesario)
        if (other.CompareTag("Player"))
        {
            // Activar el MeshRenderer
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto que salió del trigger tiene el tag adecuado (si es necesario)
        if (other.CompareTag("Player"))
        {
            // Desactivar el MeshRenderer
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }
    }
}
