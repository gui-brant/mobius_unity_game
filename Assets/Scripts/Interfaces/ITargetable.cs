using UnityEngine;

public interface ITargetable
{
    Transform TargetTransform { get; }
    bool CanBeTargeted { get; }
}
