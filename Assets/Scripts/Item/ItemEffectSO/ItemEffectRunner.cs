using UnityEngine;

public static class ItemEffectRunner
{
    public static bool TryApplyUseEffects(ItemDataSO item, GameObject user)
    {
        if (item == null || user == null) return false;

        var effects = item.OnUseEffects;
        if (effects == null || effects.Count == 0)
            return false;

        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            if (effect == null) continue;

            if (!effect.Apply(user))
                return false; // 하나라도 실패하면 소모 X
        }

        return true;
    }
}