using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestForGiveCard : Quest
{
    public CardInfo card;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void TakeCard(CardInfo card_)
    {
        card = card_;
    }

    public void OpenQuest()
    {
        availible = true;
        Debug.Log($"Quest for give card <b>{card.cardName}</b> <color=blue>card availible</color>");
    }

    public void Activate()
    {
        if (availible)
        {
            Debug.Log($"Quest for give card <b>{card.cardName}</b> <color=green>activated</color>");
            activated = true;
        }
    }

    public override void QuestComplete()
    {
        if (activated)
        {
            //Debug.Log($"Quest for give card <b>{card.cardName}</b> <color=yellow>completed</color>");
            QuestManager.instance.CompleteQuest(questName , false);
            QuestManager.instance.TakeNewFragmentCard(card);
            base.QuestComplete();
            Destroy(this);
        }
    }


}
