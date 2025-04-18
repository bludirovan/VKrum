// MainController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody), typeof(GravityBody))]
public class MainController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _cam;

    [Header("UI Controls")]
    public Joystick moveJoystick;
    public Button jumpButton;
    public Button polarityButton;

    [Header("Movement Settings")]
    [SerializeField] private float _groundCheckRadius = 0.3f;
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _turnSpeed = 10f;
    [SerializeField] private float _jumpForce = 50f;

    private Rigidbody _rb;
    private GravityBody _gravityBody;
    private Vector3 _moveDirection;
    private bool _isFrozen = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();
        jumpButton.onClick.AddListener(Jump);
        polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        if (_isFrozen) return;

        // Read joystick input
        float rawX = moveJoystick.Horizontal;
        float rawZ = moveJoystick.Vertical;
        Vector3 input = new Vector3(rawX, 0f, rawZ);

        if (input.magnitude > 0.1f)
        {
            // Align movement to camera and gravity
            Vector3 gravityUp = -_gravityBody.GravityDirection.normalized;
            Vector3 camForward = Vector3.ProjectOnPlane(_cam.forward, gravityUp).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(_cam.right, gravityUp).normalized;
            _moveDirection = (camForward * input.z + camRight * input.x).normalized;
        }
        else
        {
            _moveDirection = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (_isFrozen) return;

        if (_moveDirection.magnitude > 0.1f)
        {
            // Move
            Vector3 newPos = _rb.position + _moveDirection * (_speed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            // Rotate smoothly towards move direction
            Quaternion targetRot = Quaternion.LookRotation(_moveDirection, -_gravityBody.GravityDirection);
            Quaternion slerped = Quaternion.Slerp(_rb.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(slerped);
        }
    }

    private void Jump()
    {
        if (_isFrozen) return;
        bool grounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);
        if (!grounded) return;

        Vector3 jumpDir = _rb.useGravity ? Vector3.up : -_gravityBody.GravityDirection.normalized;
        _rb.AddForce(jumpDir * _jumpForce, ForceMode.Impulse);
    }

    private void SwitchPolarity()
    {
        if (_isFrozen) return;
        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
        {
            area.LocalPolarity = !area.LocalPolarity;
            Debug.Log($"Polarity switched on: {area.gameObject.name}");
        }
    }

    public void FreezeMovement(float duration)
    {
        if (!_isFrozen)
            StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        _isFrozen = true;
        yield return new WaitForSeconds(duration);
        _isFrozen = false;
    }
}




/*
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _cam;

    [Header("Управление UI")]
    public Joystick moveJoystick;
    public Button jumpButton;
    public Button polarityButton;

    [Header("Параметры движения")]
    private float _groundCheckRadius = 0.3f;
    private float _speed = 8f;
    private float _turnSpeed = 1500f;
    private float _jumpForce = 50f;

    private Rigidbody _rigidbody;
    private GravityBody _gravityBody;
    private Vector3 _direction;
    private bool isFrozen = false;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();

        jumpButton.onClick.AddListener(Jump);
        polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        if (isFrozen) return;

        // Считаем сырые значения с джойстика
        float rawX = moveJoystick.Horizontal;
        float rawZ = moveJoystick.Vertical;

        // Запрещаем отрицательное значение по Z (назад)
        float forwardZ = Mathf.Max(0f, rawZ);

        // Собираем и нормализуем направление
        _direction = new Vector3(rawX, 0f, forwardZ).normalized;
    }


    void FixedUpdate()
    {
        if (isFrozen) return;

        bool isRunning = _direction.magnitude > 0.1f;
        if (isRunning)
        {
            // Движение вперёд/назад
            Vector3 movement = transform.forward * _direction.z;
            _rigidbody.MovePosition(_rigidbody.position + movement * (_speed * Time.fixedDeltaTime));

            // Поворот
            Quaternion turnRotation = Quaternion.Euler(0f, _direction.x * (_turnSpeed * Time.fixedDeltaTime), 0f);
            Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, _rigidbody.rotation * turnRotation, Time.fixedDeltaTime * 3f);
            _rigidbody.MoveRotation(newRotation);
        }
    }

    public void Jump()
    {
        if (isFrozen) return;

        bool isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);
        if (!isGrounded) return;

        if (_rigidbody.useGravity)
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        else
            _rigidbody.AddForce(-_gravityBody.GravityDirection * _jumpForce, ForceMode.Impulse);
    }

    public void SwitchPolarity()
    {
        if (isFrozen) return;

        var activeArea = _gravityBody.GetActiveGravityArea();
        if (activeArea != null)
        {
            activeArea.LocalPolarity = !activeArea.LocalPolarity;
            Debug.Log("Переключили полярность: " + activeArea.gameObject.name);
        }
    }

    public void FreezeMovement(float duration)
    {
        if (!isFrozen)
            StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
    }
}*/