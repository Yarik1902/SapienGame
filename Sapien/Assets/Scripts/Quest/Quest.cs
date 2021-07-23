using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public string questName;
    [HideInInspector] public bool activated = false;
    [HideInInspector] public bool availible = false;
    
    public event Action OnQuestComplete;

    public Action<CardInfo> destroyer;

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


    
    
}
