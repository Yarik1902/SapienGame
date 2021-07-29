using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanelManager : MonoBehaviour
{
    public GameObject[] QuestIcon;
    public GameObject[] QuestPrice;
    public GameObject[] QuestDescription;
    public Animator[] anim;
    public GameObject[] FadePanel;
    public int LastActiveQuest;
    public GameObject Warning;
    public GameObject[] QuestDecline;

    public bool[] isHidden = new bool[3];
    public bool QuestAdded = false;

    private PhoneManager phoneManager;

    private void Start()
    {
        phoneManager = GameObject.Find("PhoneButton").GetComponent<PhoneManager>();
    }

    public bool AddQuestToActiveList(string name, string tag = "")
    {
//        Debug.Log(name + " " + tag);
        Quest quest = QuestManager.instance.GetQuestByName(name);
        bool startOfFragment = (QuestManager.instance.GetQuestForGiveCardByName(name) != null)
            , StoryQuest = (QuestManager.instance.GetStoryQuestByName(name) != null)
            , AfterStory = (QuestManager.instance.GetQuestAfterStoryQuestByName(name) != null);
        if (quest.availible)
        {
            if (LastActiveQuest < 3)
            {
                QuestIcon[LastActiveQuest].SetActive(true);
                QuestIcon[LastActiveQuest].GetComponent<QuestPanel>().ShowInfo(quest);
                //QuestPrice[LastActiveQuest].GetComponent<Text>().text = Random.Range(100, 1000).ToString();
                //QuestDescription[LastActiveQuest].GetComponent<Text>().text = quest.questDescription;

                if (startOfFragment)
                {
                    quest?.Activate();
                    ClosePanelAfterQuestComplete(quest);
                    //QuestManager.instance.GetQuestForGiveCardByName(name).OnQuestComplete += 
                    //    () => GameObject.Find("Messages").GetComponent<MessagesManager>().DeleteQuest(name, tag);
                }
                else if (StoryQuest)
                {
                    if (quest != null)
                    {
                        quest.Activate();
                        ClosePanelAfterQuestComplete(quest);
                        phoneManager.StartStoryQuestChain();
                        //QuestManager.instance.GetQuestForGiveCardByName(name).OnQuestComplete += 
                        //    () => GameObject.Find("Messages").GetComponent<MessagesManager>().DeleteQuest(name, tag);
                    }
                }
                else
                {
                    quest.Activate();
                    ClosePanelAfterQuestComplete(quest);
                    
                    //QuestManager.instance.GetQuestForGiveCardByName(name).OnQuestComplete += 
                    //    () => GameObject.Find("Messages").GetComponent<MessagesManager>().DeleteQuest(name, tag);
                }
                LastActiveQuest++;
                return true;
            }
            else
            {
                Debug.Log("A lot of active quests");
                StartCoroutine(WarningPanel());
                return false;
            }
        }
        else
        {
            Debug.Log($"Quest {quest.questName} doesn't availible");
            StartCoroutine(WarningPanel());
            return false;
        }

        return false;
    }

    private List<Quest> subscribedQuests = new List<Quest>();
    private List<Action> subscribedActions = new List<Action>();
    public void ClosePanelAfterQuestComplete(Quest quest)
    {
        Action loc = () => OnClickQuestIconDecline(quest.questName);
        quest.OnQuestComplete += loc;
        subscribedQuests.Add(quest);
        subscribedActions.Add(loc);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < subscribedActions.Count; ++i)
        {
            subscribedQuests[i].OnQuestComplete -= subscribedActions[i];
        }
    }

    IEnumerator WarningPanel()
    {
        Warning.SetActive(true);
        yield return new WaitForSeconds(3f);
        Warning.SetActive(false);
    }

    public void OnClickQuestIconDecline(int n)
    {
        StartCoroutine(OnClickQuestIconDeclineCR(n));
    }

    IEnumerator OnClickQuestIconDeclineCR(int n)
    {
        switch (n)
        {
            case 0:
                anim[0].Play("QuestComplete");
                anim[1].Play("QuestUp");
                anim[2].Play("QuestUp");
                yield return new WaitForSeconds(1f);
                QuestIcon[0].GetComponent<QuestPanel>().ShowInfo(QuestIcon[1].GetComponent<QuestPanel>().targetQuest);
                QuestIcon[1].GetComponent<QuestPanel>().ShowInfo(QuestIcon[2].GetComponent<QuestPanel>().targetQuest);
                QuestIcon[2].GetComponent<QuestPanel>().targetQuest = null;
                //QuestDescription[0].GetComponent<Text>().text = QuestDescription[1].GetComponent<Text>().text;
                //QuestDescription[1].GetComponent<Text>().text = QuestDescription[2].GetComponent<Text>().text;
                //QuestPrice[0].GetComponent<Text>().text = QuestPrice[1].GetComponent<Text>().text;
                //QuestPrice[1].GetComponent<Text>().text = QuestPrice[2].GetComponent<Text>().text;
                break;
            case 1:
                anim[1].Play("QuestComplete");
                anim[2].Play("QuestUp");
                yield return new WaitForSeconds(1f);
                QuestIcon[1].GetComponent<QuestPanel>().ShowInfo(QuestIcon[2].GetComponent<QuestPanel>().targetQuest);
                QuestIcon[2].GetComponent<QuestPanel>().targetQuest = null;
                break;
            case 2:
                anim[2].Play("QuestComplete");
                yield return new WaitForSeconds(1f);
                QuestIcon[2].GetComponent<QuestPanel>().targetQuest = null;
                break;
        }
        anim[0].Play("QuestIdle");
        anim[1].Play("QuestIdle");
        anim[2].Play("QuestIdle");
        if (QuestIcon[2].activeSelf)
        {
            QuestIcon[2].SetActive(false);
        }
        else if (QuestIcon[1].activeSelf)
        {
            QuestIcon[1].SetActive(false);
        }
        else
        {
            QuestIcon[0].SetActive(false);
        }
        LastActiveQuest--;
    }
    
    public void OnClickQuestIconDecline(string name)
    {
        int n;
        if (name == QuestIcon[0].GetComponent<QuestPanel>().targetQuest.questName)
        {
            n = 0;
        }
        else if (name == QuestIcon[1].GetComponent<QuestPanel>().targetQuest.questName)
        {
            n = 1;
        }
        else if (name == QuestIcon[2].GetComponent<QuestPanel>().targetQuest.questName)
        {
            n = 2;
        }
        else { n = 10; }

        StartCoroutine(OnClickQuestIconDeclineCR(n));
    }

    public void OnClickHidePanel(int n)
    {
        if (!isHidden[n])
        {
            anim[n].Play("QuestFade");
            QuestDescription[n].SetActive(false);
            QuestPrice[n].SetActive(false);
            QuestDecline[n].SetActive(false);
            isHidden[n] = true;
        }
    }

    public void OnClickShowPanel(int n)
    {
        if (isHidden[n])
        {
            anim[n].Play("QuestFade0");
            QuestDescription[n].SetActive(true);
            QuestPrice[n].SetActive(true);
            QuestDecline[n].SetActive(true);
            isHidden[n] = false;
        }
    }
}
