using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JustLaser : MonoBehaviour
{
    private Vector3 startPosition;
    // [SerializeField] private Vector3 teleportTarget = new(37.45f, 17.3291f, -129.11f);
    [SerializeField] private Vector3 teleportTarget;
    public float freezeDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
                Debug.Log("????? ????? ? ?????. ????????? PlayerHealth ?? ??????!");
                // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                TeleportPlayer(other);

            }
            else
            {
                Debug.Log("????? ????? ? ?????. ????????? PlayerHealth ?? ??????!");
                // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                TeleportPlayer(other);
            }

        }
    }


// 37.45 17.3291 -129.11
    private void TeleportPlayer(Collider other)
    {
        other.transform.position = teleportTarget;

        // Сброс скорости
        if (other.TryGetComponent<Rigidbody>(out var rb))
            rb.velocity = Vector3.zero;

        // Блокировка управления
        if (other.TryGetComponent<MainController>(out var player))
            player.FreezeMovement(freezeDuration);
    }
}
