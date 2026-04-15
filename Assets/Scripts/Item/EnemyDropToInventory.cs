using UnityEngine;

public class EnemyDropToInventory : MonoBehaviour
{
    [SerializeField] private DropTableSO dropTable;
    [SerializeField] private ItemDatabaseSO database;
    [SerializeField] private Inventory inventory;

    private bool _done;

    private void Awake()
    {
        ResolveInventory();
    }

    private void ResolveInventory()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>(FindObjectsInactive.Include);
        }
    }

    public void DropOnce()
    {
        if (_done) return;
        _done = true;

        var rolled = dropTable.Roll();

        foreach (var (data, amount) in rolled)
        {
            //ItemDatabase에서 id만 꺼내쓰기
            bool ok = inventory.AddItem(database, data.Id, amount);
            Debug.Log($"Drop {data.DisplayName} x{amount} => {ok}");
        }

    }
}