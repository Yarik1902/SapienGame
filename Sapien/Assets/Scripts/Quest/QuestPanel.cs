using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    [HideInInspector]public Quest targetQuest;
    public Text questDescription;
    public Text questPrice;
    public Image questImage;

    public void ShowInfo(Quest newQuest)
    {
        targetQuest = newQuest;
        if (newQuest != null)
        {
            questDescription.text = targetQuest.questDescription;
            questPrice.text = targetQuest.price.ToString();
            questImage.sprite = newQuest.questGiverAvatar;
        }
    }
}
