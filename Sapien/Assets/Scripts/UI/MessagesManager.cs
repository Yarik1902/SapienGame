using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MessagesManager : MonoBehaviour
{
    public GameObject ChatIcon;
    public GameObject ChatPanel;
    public GameObject QuestText;
    public Vector3 FirstChatPos = new Vector3(0f, 425f, 0f);
    public bool QuestAvailable = false;
    public float Increment = 130f;
    public GameObject chat;
    public GameObject Content;
    public GameObject[] Quests;
    public int n = 0;
    public int OpenedQuest = 100;

    public void OnClickChatOpener(GameObject target)
    {
        ChatIcon.SetActive(false);
        ChatPanel.SetActive(true);
        
        string[] tokens = target.name.Split(' ');
        OpenedQuest = Int32.Parse(tokens[0]);
        tokens = tokens.Skip(1).ToArray();
        QuestText.GetComponent<Text>().text = string.Join(" ",tokens);
    }

    public void OnClickChatDelete()
    {
        ChatIcon.SetActive(true);
        ChatPanel.SetActive(false);
        GameObject.Find("PhoneButton").GetComponent<QuestPanelManager>().OnClickQuestIconDecline(Quests[OpenedQuest].tag);
        Quests[OpenedQuest].SetActive(false);
        Destroy(Quests[OpenedQuest]);
        for (int k = OpenedQuest; k >= 0; k--)
        {
            if (k < Quests.Length && Quests[k] != null)
            {
                Vector3 CurrentPos = Quests[k].GetComponent<RectTransform>().position;
                Quests[k].GetComponent<RectTransform>().Translate(0,Increment,0);
                //Quests[k].GetComponent<RectTransform>().position = new Vector3(CurrentPos.x, CurrentPos.y + Increment, CurrentPos.z);
            }
        }
        OpenedQuest = 100;
    }

    public void OnClickChatActivate()
    {
        string[] tokens = Quests[OpenedQuest].name.Split(' ');
        tokens = tokens.Skip(1).ToArray();
        GameObject.Find("PhoneButton").GetComponent<QuestPanelManager>().AddQuestToActiveList(string.Join(" ",tokens), Quests[OpenedQuest].tag);
    }
    
    public void AddQuest(string name, string type)
    {
        for (int k = 0; k <= n; k++)
        {
            if (k < Quests.Length && Quests[k] != null)
            {
                Vector3 CurrentPos = Quests[k].GetComponent<RectTransform>().localPosition;
                Debug.Log(CurrentPos + $" position of quest {k}");
                //Quests[k].GetComponent<RectTransform>().Translate(0,-Increment,0);
                Quests[k].GetComponent<RectTransform>().localPosition = new Vector3(0, CurrentPos.y - Increment, 0);
                Debug.Log(Quests[k].GetComponent<RectTransform>().position + $" next position of quest {k}");
            }
        }

        Quests[n] = GameObject.Instantiate(chat, FirstChatPos, Quaternion.identity) as GameObject;
        GameObject.Find("Chat1(Clone)/Name").GetComponent<Text>().text = name;
        GameObject.Find("Chat1(Clone)/Type").GetComponent<Text>().text = type;
        Quests[n].name = n.ToString() + " " + name;
        Quests[n].tag = type;
        Quests[n].GetComponent<RectTransform>().SetParent(Content.GetComponent<RectTransform>(), false);
        OnClickChatOpener(Quests[n]);
        n++;
    }

}
