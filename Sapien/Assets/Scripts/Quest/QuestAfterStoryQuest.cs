using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using UnityEngine;

public class QuestAfterStoryQuest : MonoBehaviour
{
    public string questName;
    public CardInfo questForCard;
    private bool availible = false;
    private bool activated = false;

    private void Start()
    {
        QuestManager.instance.OnStoryComplete += TryOpen;
    }

    private void Update()
    {
        if (activated)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Complete();
        }
    }

    public void TryOpen(CardInfo card)
    {
        if (card != null && card.cardID == questForCard.cardID && !QuestManager.instance.completedQuest.TryGetValue(questName, out bool flag))
        {
            availible = true;
            Debug.Log($"<b>{questName}</b> <color=blue>Availible</color>");
        }
    }

    public void Activate()
    {
        if (availible)
        {
            activated = true;
            Debug.Log($"<b>{questName}</b> <color=green>Activated</color>");
        }
        else
        {
            Debug.Log($"<b>{questName}</b> <color=red>don't availible</color>");
        }
    }

    public void Complete()
    {
        if (activated)
        {
            QuestManager.instance.CompleteQuest(questName, false);
            FragmentCard.instance.GetEnergy(15);
            Debug.Log($"<b>{questName}</b> <color=yellow>Complete</color>");
        }
        else
        {
            Debug.Log($"<b>{questName}</b> <color=red>didn't activated</color>");
        }
    }

    private void OnDestroy()
    {
        QuestManager.instance.OnStoryComplete -= TryOpen;
    }
}
