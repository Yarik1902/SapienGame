using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using UnityEngine;

public class QuestAfterStoryQuest : Quest
{
    [HideInInspector] public List<CardInfo> ignoreCards;
    public KeyCode key;
    private void Start()
    {
        QuestManager.instance.OnStoryComplete += TryOpen;
        TryOpen(QuestManager.instance.card);
    }

    private void Update()
    {
        if (activated)
        {
            if (Input.GetKeyDown(key))
                QuestComplete();
        }
    }

    public override void OpenQuest()
    {
        TryOpen(QuestManager.instance.card);
    }

    public void TryOpen(CardInfo card)
    {
        bool completed;
        QuestManager.instance.completedQuest.TryGetValue(questName, out completed);
        if (CanActivateCardOnFragmentCard(card) && !completed && QuestManager.instance.storyQuestStage == StoryQuestStage.Complete)
        {
            availible = true;
            Debug.Log($"<b>{questName}</b> <color=blue>Availible</color>");
        }
        else
        {
            Debug.Log($"<b>{questName}</b> <color=red>isn't availible , complete story mission</color>");
        }
    }

    public bool CanActivateCardOnFragmentCard(CardInfo card)
    {
        bool flag = ignoreCards.Contains(card);
        return !flag;
    }

    public override void QuestComplete()
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
        base.QuestComplete();
    }

    private void OnDestroy()
    {
        QuestManager.instance.OnStoryComplete -= TryOpen;
    }

    public void AddCardToIgnoreList(CardInfo card)
    {
        ignoreCards.Add(card);
    }
}
