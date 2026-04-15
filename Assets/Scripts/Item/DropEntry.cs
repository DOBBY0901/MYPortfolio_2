using UnityEngine;

[System.Serializable]
public class DropEntry
{
    public ItemDataSO item; 

    [Range(0f, 1f)]
    public float chance = 1f; // 아이템 드랍 확률

    public int minAmount = 1; //드랍 최소 수량
    public int maxAmount = 1; //드랍 최대 수량
}