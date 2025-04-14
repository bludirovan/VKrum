using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GravityField : MonoBehaviour
{
    [Header("Параметры гравитационного поля")]
    [Tooltip("Сила гравитационного поля. Чем больше значение, тем сильнее воздействие.")]
    public float gravitationalForce = 10f;

    [Tooltip("Радиус действия поля, в пределах которого персонаж начинает подвержен воздействию.")]
    public float effectRadius = 10f;

    [Tooltip("Полярность поля. true – поле притягивает, false – отталкивает.")]
    public bool isAttractive = true;

    // Отладочное отображение области действия в редакторе:
    private void OnDrawGizmos()
    {
        // Если поле притягивает – синяя, если отталкивает – красная.
        Gizmos.color = isAttractive ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}
