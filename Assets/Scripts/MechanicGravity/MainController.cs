// MainController.cs
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

        _direction = new Vector3(moveJoystick.Horizontal, 0f, moveJoystick.Vertical).normalized;
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
}
