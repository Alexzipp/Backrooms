using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleMove : MonoBehaviour
{
    Player playerController;
    [SerializeField] GameObject player;

    public Transform teleportTarget;
    public Transform posicionFinal;
    private Vector3 initialPlayerPosition;

    private void Start()
    {
        // Guarda la posición inicial del jugador al inicio
        if (player != null)
        {
            initialPlayerPosition = player.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            StartCoroutine(TeleportPlayer(player));
        }
    }

    private IEnumerator TeleportPlayer(GameObject player)
    {
        // Desactiva el controlador del jugador u realiza cualquier otra acción necesaria
         //playerController.disabled = true;

        yield return new WaitForSeconds(0.2f);

        // Teleporta al jugador al destino especificado
        player.transform.position = teleportTarget.position;

        // Si quieres que el jugador se teletransporte de vuelta a la posición inicial después de un tiempo, puedes usar corutinas.
        StartCoroutine(TeleportBack(player));
    }

    private IEnumerator TeleportBack(GameObject player)
    {
        yield return new WaitForSeconds(4.0f);

        // Teleporta al jugador de vuelta a la posición inicial
        player.transform.position = posicionFinal.position;

        // Reactiva el controlador del jugador u realiza cualquier otra acción necesaria
        //playerController.disabled = false;
    }
}
