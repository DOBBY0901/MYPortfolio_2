using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
  

    public void SetEmpty()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(true);
            iconImage.enabled = false;             
        }

        if (countText != null)
        {
            countText.text = "";
            countText.gameObject.SetActive(false);
        } 
    }

    public void Set(Sprite icon, int count)
    {
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(true);  
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }
        if (countText != null)
        {
            countText.gameObject.SetActive(true); 
            countText.text = count.ToString();
        }

       
    }
}