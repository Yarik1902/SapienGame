using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryQuest : MonoBehaviour
{
    public string questName;
    public CardInfo questFromCard;
    public int questOrder;
    [HideInInspector] public bool activated = false;
    [HideInInspector] public bool availible = false;
    public event Action OnQuestComplete;

    private Action<CardInfo> destroyer;


    private void Start()
    {
        destroyer = (CardInfo card) =>
        {
            if (card == questFromCard)
                Destroy(this);
        };
    }

    public virtual void OpenQuest()
    {
        availible = true;
        Debug.Log($"<b>{questName}</b> <color=blue>Availible</color>");
    }

    public virtual void Activate()
    {
        if (availible)
        {
            activated = true;
            Debug.Log($"<b>{questName}</b> <color=green>Activated</color>");

            QuestManager.instance.OnStoryComplete += destroyer;
        }
        else
        {
            Debug.Log($"<b>{questName}</b> <color=red>don't availible , complete all quests</color>");
        }
    }

    public virtual void QuestComplete()
    {
        if (activated)
        {
            OnQuestComplete?.Invoke();
            Debug.Log($"<b>{questName}</b> <color=yellow>Complete</color>");
        }
        else
        {
            Debug.Log($"<b>{questName}</b> <color=red>didn't activated</color>");
        }
    }

    private void OnDestroy()
    {
        QuestManager.instance.OnStoryComplete -= destroyer;
    }
}

