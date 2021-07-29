using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friend : MonoBehaviour
{
    public string friendName;
    public Sprite friendAvatar;
    public FriendRelations friendRelations;

    public FriendQuest quest;

    private void Update()
    {
        if (quest.activated)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                quest.QuestComplete();
                quest.activated = false;
            }
        }
    }

    public void GiveQuest()
    {
        quest.questGiverAvatar = friendAvatar;
        quest.OpenQuest();
        if (GameObject.Find("PhoneButton").GetComponent<QuestPanelManager>().AddQuestToActiveList(quest.questName))
        {
            //quest.Activate();
            quest.OnQuestComplete += () => friendRelations.GetFriendRelationPoints(quest.relationPoints , true);
        }
    }
    
}
