using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MovingLaser : MonoBehaviour
{
    [Header("Настройки движения")]
    public Vector3 direction = Vector3.forward;
    public float distance = 5f;
    public float speed = 2f;

    [Header("Настройки лазера")]
    public float laserLength = 10f;

    private Vector3 startPosition;
    private LineRenderer lineRenderer;

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

    void Start()
    {
        startPosition = transform.position;
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
            lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // Двигаем объект туда-сюда
        float move = Mathf.PingPong(Time.time * speed, distance);
        Vector3 currentPosition = startPosition + direction.normalized * move;
        transform.position = currentPosition;

        // Обновляем точки LineRenderer'а
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);  // Начало в позиции объекта
            lineRenderer.SetPosition(1, transform.position + direction.normalized * laserLength); // Конец — вперёд
        }
    }
}
