using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MessagesManager : MonoBehaviour
{
    public GameObject ChatIcon;
    public GameObject ChatPanel;
    public Text QuestText;
    public Text QuestDescription;
    public Vector3 FirstChatPos = new Vector3(0f, 425f, 0f);
    public bool QuestAvailable = false;
    public float Increment = 130f;
    public GameObject chat;
    public GameObject Content;
    public GameObject[] Quests;
    public Button declineButton;
    public int n = 0;
    public int OpenedQuest = 100;

    public PhoneManager _phoneManager;
    public GameObject PostPonedQuestNotificationPrefab;

    public void Awake()
    {
       // MessagesSave.instance.Load(SceneManager.GetActiveScene() , LoadSceneMode.Single);
    }

    public void MakeMessageUnreaded(string questName)
    {
        int idx  = Array.FindIndex(Quests, row =>
        {
            if (row == null)
                return false;
            string[] tokens = row.name.Split(' ');
            tokens = tokens.Skip(1).ToArray();
            return string.Join(" ",tokens) == questName;
        });
        Quests[idx].transform.Find("Unreaded").gameObject.SetActive(true);
    }

    public void MarkMessageRead(GameObject message)
    {
        message.transform.Find("Unreaded").gameObject.SetActive(false);
    }

    public void Postpone(float duration)
    {
        GameObject notify = Instantiate(PostPonedQuestNotificationPrefab);
        notify.GetComponent<PostPonedQuestNotification>().StartTimer(duration , _phoneManager);
        _phoneManager.OnPointerClickMessagesIconClose("Messages");
    }

    public void OnClickChatOpener(GameObject target)
    {
        ChatIcon.SetActive(false);
        ChatPanel.SetActive(true);
        
        string[] tokens = target.name.Split(' ');
        OpenedQuest = Int32.Parse(tokens[0]);
        tokens = tokens.Skip(1).ToArray();
        QuestText.text = string.Join(" ",tokens);
        QuestDescription.text = QuestManager.instance.GetQuestByName(QuestText.text).questDescription;
        ChatPanel.transform.Find("AvatarMask/Avatar").GetComponent<Image>().sprite =
            QuestManager.instance.GetQuestByName(QuestText.text).questGiverAvatar;

        QuestAfterStoryQuest quest = QuestManager.instance.GetQuestAfterStoryQuestByName(string.Join(" ", tokens));
        
        if (quest == null)
        {
            ChatPanel.transform.Find("Buttons1").gameObject.SetActive(false);    
            ChatPanel.transform.Find("Buttons2").gameObject.SetActive(true);    
        }
        else
        {
            ChatPanel.transform.Find("Buttons1").gameObject.SetActive(true);    
            ChatPanel.transform.Find("Buttons2").gameObject.SetActive(false);
        }
        
        MarkMessageRead(target);
    }

    public void LoadScene(int idx)
    {
        SceneManager.LoadScene(idx);
    }
    public void DeleteQuest(string name, string type = "")
    {
        int idx = Array.FindIndex(Quests, row =>
        {
            if (row == null)
                return false;
            string[] tokens = row.name.Split(' ');
            tokens = tokens.Skip(1).ToArray();
            return string.Join(" ", tokens) == name;
        });
        if (idx >= 0)
        {
            ChatIcon.SetActive(true);
            ChatPanel.SetActive(false);
            //GameObject.Find("PhoneButton").GetComponent<QuestPanelManager>().OnClickQuestIconDecline(name);
            Quests[idx].SetActive(false);
            Destroy(Quests[idx]);
            for (int k = idx; k >= 0; k--)
            {
                if (k < Quests.Length && Quests[k] != null)
                {
                    Vector3 CurrentPos = Quests[k].GetComponent<RectTransform>().localPosition;
                    Quests[k].GetComponent<RectTransform>().localPosition = new Vector3(0, CurrentPos.y + Increment, 0);
                }
            }

            OpenedQuest = 100;
        }
    }

    public void OnClickChatDelete()
    {
        string[] tokens = Quests[OpenedQuest].name.Split(' ');
        tokens = tokens.Skip(1).ToArray();
        QuestAfterStoryQuest quest = QuestManager.instance.GetQuestAfterStoryQuestByName(string.Join(" ", tokens));
        if (quest != null)
        {
            quest.AddCardToIgnoreList(QuestManager.instance.card);
            DeleteQuest(quest.questName , Quests[OpenedQuest].tag);
        }
    } 

    public void OnClickChatPostPone()
    {
        Postpone(10);
        Debug.Log("PostPoned");
    }

    public void OnClickChatActivate()
    {
        string[] tokens = Quests[OpenedQuest].name.Split(' ');
        tokens = tokens.Skip(1).ToArray();
        ActivateQuest(string.Join(" " , tokens));
    }

    public void ActivateQuest(string name)
    {
        if (GameObject.Find("PhoneButton").GetComponent<QuestPanelManager>()
            .AddQuestToActiveList(name))
        {
            DeleteQuest(name);
        }
    }
    
    public void AddQuest(string name, string type , bool unreaded = true)
    {
        if (IsQuestAdded(name))
        {
            Debug.Log($"<color=red><b>Quest {name} is added</b></color>");
            return;
        }

        if (QuestManager.instance.GetQuestByName(name) == null)
        {
            Debug.Log($"<color=red><b>Quest {name} doesn't exist</b></color>");
            return;
        }

        for (int k = 0; k <= n; k++)
        {
            if (k < Quests.Length && Quests[k] != null)
            {
                Vector3 CurrentPos = Quests[k].GetComponent<RectTransform>().localPosition;
                //Quests[k].GetComponent<RectTransform>().Translate(0,-Increment,0);
                Quests[k].GetComponent<RectTransform>().localPosition = new Vector3(0, CurrentPos.y - Increment, 0);
            }
        }

        Quests[n] = GameObject.Instantiate(chat, FirstChatPos, Quaternion.identity) as GameObject;
        Quests[n].GetComponent<RectTransform>().SetParent(Content.GetComponent<RectTransform>(), false);
        
        Quests[n].transform.Find("Name").GetComponent<Text>().text = name;
        Quests[n].transform.Find("Type").GetComponent<Text>().text = QuestManager.instance.GetQuestByName(name).questDescription;
        Quests[n].transform.Find("Mask/Avatar").GetComponent<Image>().sprite = QuestManager.instance.GetQuestByName(name).questGiverAvatar;
        
        
        Quests[n].name = n.ToString() + " " + name;
        Quests[n].tag = type;
        FragmentCard.instance.onFragmentCardComplete += (CardInfo card) =>
        {
            DeleteQuest(name, type);
        };
        OnClickChatOpener(Quests[n]);
        if (unreaded)
        {
            MakeMessageUnreaded(name);
        }
        else
        {
            MarkMessageRead(Quests[n]);
        }

        n++;
    }

    public string GetQuestName(GameObject quest)
    {
        if (quest == null)
            return " ";
        return quest.transform.Find("Name").GetComponent<Text>().text;
    }
    
    public bool IsQuestAdded(string name)
    {
        return Array.Exists(Quests, row =>
        {
            if (row == null)
                return false;
            string[] tokens = row.name.Split(' ');
            tokens = tokens.Skip(1).ToArray();
            return string.Join(" ", tokens) == name;
        });
    }

    public int GetUnreadMassages()
    {
        int res = Quests.Count((GameObject lhs) =>
        {
            if (lhs == null)
                return false;
            return lhs.transform.Find("Unreaded").gameObject.activeSelf;
        });
        return res;
    }

    private void OnDestroy()
    {
        //MessagesSave.instance.Save(SceneManager.GetActiveScene());
    }
}
