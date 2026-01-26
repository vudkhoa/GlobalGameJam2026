using UnityEngine;

public abstract class BaseCollection : ScriptableObject
{
    public abstract void Collect(Transform collector, Transform itemTarget);
}
