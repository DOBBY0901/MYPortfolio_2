using UnityEngine;

public abstract class ItemEffectSO : ScriptableObject
{
    public abstract bool Apply(GameObject user);
}