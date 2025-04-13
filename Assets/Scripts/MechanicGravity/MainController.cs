using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _cam;

    [Header("Компоненты управления")]
    public Joystick moveJoystick;   // Ссылка на UI-джойстик
    public Button jumpButton;       // Кнопка прыжка
    public Button polarityButton;   // Кнопка смены полярности зоны

    private float _groundCheckRadius = 0.3f;
    private float _speed = 8f;
    private float _turnSpeed = 1500f;
    private float _jumpForce = 50f;

    private Rigidbody _rigidbody;
    private Vector3 _direction;
    private GravityBody _gravityBody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();

        if (jumpButton != null)
            jumpButton.onClick.AddListener(Jump);

        if (polarityButton != null)
            polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        _direction = new Vector3(moveJoystick.Horizontal, 0f, moveJoystick.Vertical).normalized;
    }

    void FixedUpdate()
    {
        bool isRunning = _direction.magnitude > 0.1f;

        if (isRunning)
        {
            Vector3 movement = transform.forward * _direction.z;
            _rigidbody.MovePosition(_rigidbody.position + movement * (_speed * Time.fixedDeltaTime));

            // Поворот персонажа вокруг вертикальной оси
            Quaternion turnRotation = Quaternion.Euler(0f, _direction.x * (_turnSpeed * Time.fixedDeltaTime), 0f);
            Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, _rigidbody.rotation * turnRotation, Time.fixedDeltaTime * 3f);
            _rigidbody.MoveRotation(newRotation);
        }
    }

    public void Jump()
    {
        // Проверяем, на земле ли персонаж.
        bool isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);

        if (isGrounded)
        {
            if (_rigidbody.useGravity)
            {
                // Стандартный прыжок
                _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            }
            else
            {
                // Прыжок с учётом направления кастомной гравитации
                _rigidbody.AddForce(-_gravityBody.GravityDirection * _jumpForce, ForceMode.Impulse);
            }
        }
    }

    // Переключаем полярность активной зоны гравитации
    public void SwitchPolarity()
    {
        GravityArea activeArea = _gravityBody.GetActiveGravityArea();
        if (activeArea != null)
        {
            activeArea.SwitchPolarity();
            Debug.Log("Переключена локальная полярность зоны: " + activeArea.gameObject.name);
        }
        else
        {
            Debug.Log("Активная зона гравитации не найдена.");
        }
    }
}
