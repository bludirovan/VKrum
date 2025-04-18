// Place this file in Assets/Scripts/Interfaces/Interfaces.cs
using System;
using UnityEngine;

/// <summary>
/// Implement this on any object that can trigger outputs (e.g., LaserSensor).
/// Provides events when the sensor is activated or deactivated.
/// </summary>
public interface IInput
{
    /// <summary>Fired when the input becomes active.</summary>
    event Action<IInput> onTriggered;
    /// <summary>Fired when the input becomes inactive.</summary>
    event Action<IInput> onUntriggered;
}

/// <summary>
/// Implement this on any object that responds to inputs (e.g., Laser).
/// Provides methods to activate or deactivate the output based on the input.
/// </summary>
public interface IOutput
{
    /// <summary>Called when the linked IInput triggers activation.</summary>
    /// <param name="source">The IInput source that triggered.</param>
    void Activate(IInput source);

    /// <summary>Called when the linked IInput triggers deactivation.</summary>
    /// <param name="source">The IInput source that triggered.</param>
    void Deactivate(IInput source);
}

/// <summary>
/// Implement this on any collider that reflects the laser beam.
/// </summary>
public interface ILaserReflective
{
    /// <summary>
    /// Called by Laser.CastBeam when the ray hits a reflective surface.
    /// Should compute the reflection and continue the beam as needed.
    /// </summary>
    /// <param name="laser">The Laser instance casting the beam.</param>
    /// <param name="incomingRay">The original ray that hit the surface.</param>
    /// <param name="hitInfo">The hit information (point, normal, collider, etc.).</param>
    void Reflect(Laser laser, Ray incomingRay, RaycastHit hitInfo);
}
