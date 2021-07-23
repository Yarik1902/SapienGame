using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestForGiveCard : MonoBehaviour
{
    public CardInfo card;

    private bool availible = false;
    private bool activated = false;
    private void Start()
    {
        TakeCard(FragmentCard.GetCardByID(0));
        OpenQuest();
    }

    public void TakeCard(CardInfo card_)
    {
        card = card_;
    }

    public void OpenQuest()
    {
        availible = true;
        Debug.Log($"<b>{card.cardName}</b> <color=blue>card availible</color>");
    }

    public void Activate()
    {
        if (availible)
        {
            Debug.Log($"Quest for give card <b>{card.cardName}</b> <color=green>activated</color>");
            activated = true;
        }
    }

    public void CompleteQuest()
    {
        Debug.Log($"Quest for give card <b>{card.cardName}</b> <color=yellow>completed</color>");
        QuestManager.instance.TakeNewFragmentCard(card);
        Destroy(this);
    }

}
