using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button button; 

    private int _index;
    private Action<int> _onClick;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();

        //클릭 연결
        if (button != null)
            button.onClick.AddListener(() => _onClick?.Invoke(_index));

        SetEmpty();
    }

    public void Bind(int index, Action<int> onClick)
    {
        _index = index;
        _onClick = onClick;
    }

    //빈 상태로 설정
    public void SetEmpty()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }

        if (countText != null)
        {
            countText.text = "";
            countText.gameObject.SetActive(false);
        }
    }

    //아이템 슬롯 설정
    public void Set(ItemDataSO data, int count)
    {
      
        iconImage.gameObject.SetActive(true);
        iconImage.sprite = data.Icon;

        //아이템 1개면 수량은 끄기
        if (count <= 1)
        {
            countText.gameObject.SetActive(false);
        }
        else
        {
            countText.gameObject.SetActive(true);
            countText.text = count.ToString();
        }
    }
}