using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Drop Table")]
public class DropTableSO : ScriptableObject
{
    [SerializeField] private List<DropEntry> drops = new();

    public List<(ItemDataSO data, int amount)> Roll()
    {
        var result = new List<(ItemDataSO, int)>();

        foreach (var e in drops)
        {
            if (e.item == null) continue;
            if (Random.value > e.chance) continue;

            int amount = Random.Range(e.minAmount, e.maxAmount + 1);
            if (amount > 0)
                result.Add((e.item, amount));
        }

        return result;
    }
}