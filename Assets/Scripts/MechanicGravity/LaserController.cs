// LaserController.cs
// Красивый лазерный луч от одной точки к цели с эффектом свечения и импактом
// Требует:
// - LineRenderer (с Emission-материалом, URP/Universal Render Pipeline с Bloom)
// - (опционально) ParticleSystem для эффекта удара

using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserController : MonoBehaviour
{
    [Header("Laser Settings")]
    [Tooltip("Точка-источник луча")] public Transform origin;
    [Tooltip("Целевая точка")] public Transform target;
    [Tooltip("Скорость вылета луча (м/с)")] public float beamSpeed = 50f;
    [Tooltip("Время жизни луча после достижения цели (сек)")] public float beamLifetime = 0.2f;

    [Header("Beam Appearance")]
    [Tooltip("Ширина луча в начале")] public float startWidth = 0.1f;
    [Tooltip("Ширина луча в конце")] public float endWidth = 0.1f;
    [Tooltip("Кривая ширины вдоль длины луча")] public AnimationCurve widthCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [Tooltip("Градиент цвета и альфа-канала")] public Gradient colorGradient;

    [Header("Impact Effect (Optional)")]
    [Tooltip("Префаб ParticleSystem, располагаемый в точке попадания")] public ParticleSystem impactEffectPrefab;

    private LineRenderer lr;
    private Vector3 currentEnd;
    private bool isShooting;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthCurve = widthCurve;
        lr.colorGradient = colorGradient;
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;
        lr.enabled = false;
    }

    /// <summary>
    /// Запускает луч от origin к target
    /// </summary>
    public void Shoot()
    {
        if (origin == null || target == null) return;
        lr.enabled = true;
        isShooting = true;
        currentEnd = origin.position;
        UpdateLinePositions();
    }

    void Update()
    {
        if (!isShooting) return;

        // Расстояние до движения за кадром
        Vector3 direction = (target.position - origin.position).normalized;
        float step = beamSpeed * Time.deltaTime;
        currentEnd = Vector3.MoveTowards(currentEnd, target.position, step);
        UpdateLinePositions();

        // Когда достигли цели
        if (Vector3.Distance(currentEnd, target.position) < 0.01f)
        {
            isShooting = false;
            // Эффект попадания
            if (impactEffectPrefab != null)
            {
                ParticleSystem ps = Instantiate(impactEffectPrefab, target.position, Quaternion.identity);
                Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            // Отключить луч с задержкой
            Invoke(nameof(DisableBeam), beamLifetime);
        }
    }

    private void UpdateLinePositions()
    {
        lr.SetPosition(0, origin.position);
        lr.SetPosition(1, currentEnd);
    }

    private void DisableBeam()
    {
        lr.enabled = false;
    }
}

/*
Настройка в Unity:
1. Создайте GameObject "Laser" и добавьте к нему LineRenderer.
2. Установите Material на Unlit/Color или Shader Graph с Emission и подключите Bloom в Volume.
3. В параметрах LineRenderer:
   - Position Count = 2
   - Shadow Casting = Off, Receive Shadows = Off
4. Добавьте компонент LaserController:
   - Origin = Transform точки выстрела
   - Target = Transform цели (объекта)
   - Настройте Start/End Width, Width Curve и Color Gradient
   - (Опционально) подключите префаб ParticleSystem для эффекта удара
5. В сцене добавьте Volume с Bloom для усиления свечения.
6. Из вашего кода или UI вызовите laserController.Shoot();
*/
