using UnityEngine;

public interface ISlowable
{
    // used to apply slow debuff

    void ApplySlow(float multiplier, float duration);
}
