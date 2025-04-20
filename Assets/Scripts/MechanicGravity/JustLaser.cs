using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JustLaser : MonoBehaviour
{
    private Vector3 startPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
                Debug.Log("????? ????? ? ?????. ????????? PlayerHealth ?? ??????!");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                Debug.Log("????? ????? ? ?????. ????????? PlayerHealth ?? ??????!");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

        }
    }
}
