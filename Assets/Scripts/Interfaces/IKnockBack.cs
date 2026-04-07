using UnityEngine;

public interface IKnockBack
{
    void ApplyKnockBack(Vector2 projectileDirection, float distanceUnits, float durationSeconds);
}
