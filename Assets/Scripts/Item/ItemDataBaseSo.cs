using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    //인스펙터에서 넣는 아이템 목록
    [SerializeField] private List<ItemDataSO> items = new List<ItemDataSO>();

    //런타임에서 빠르게 찾기 위한 딕셔너리
    private Dictionary<string, ItemDataSO> itemDic;

    /// <summary>
    /// id로 ItemData 가져오기
    /// </summary>
    public ItemDataSO Get(string id)
    {
        // 방어 코드: id가 비어있으면 찾을 수 없으니 null 반환
        if (string.IsNullOrEmpty(id)) return null;

        // itmeDic이 아직 안 만들어졌으면 만들기
        EnsureCache();

        itemDic.TryGetValue(id, out var data);
        return data; 
    }

    /// <summary>
    /// 해당 id가 DB에 있는지 확인
    /// </summary>
    public bool Contains(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        EnsureCache();
        return itemDic.ContainsKey(id);
    }

    /// <summary>
    /// 캐시(itemDic) 만드는 함수
    /// items 리스트를 보고 Dictionary를 구성함
    /// </summary>
    private void EnsureCache()
    {
        // 이미 만들어졌으면 다시 만들 필요 없음
        if (itemDic != null) return;

        itemDic = new Dictionary<string, ItemDataSO>(items.Count);

        // items 리스트를 순회하며 id를 key로 등록
        for (int i = 0; i < items.Count; i++)
        {
            var data = items[i];
            if (data == null) continue; // 리스트에 빈 칸(Null)이 있으면 스킵

            var id = data.Id;
            if (string.IsNullOrEmpty(id)) continue; // id가 비었으면 스킵

            itemDic[id] = data;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        itemDic = null;
        var set = new HashSet<string>();

        for (int i = 0; i < items.Count; i++)
        {
            var data = items[i];
            if (data == null) continue;

            if (string.IsNullOrEmpty(data.Id))
            {
                Debug.LogWarning($"[ItemDatabase] 빈 ID가 있습니다: {data.name}", this);
                continue;
            }

            if (!set.Add(data.Id))
            {
                Debug.LogError($"[ItemDatabase] ID 중복 발견: '{data.Id}' (중복 에셋이 리스트에 존재)", this);
            }
        }
    }
#endif
}
