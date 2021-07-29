using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MessagesSave : MonoBehaviour
{
    public static MessagesSave instance;
    public MessagesManager messagesManager;
    public string relativeSavePath;
    private string savePath;
    private List<string> questNames = new List<string>();
    private List<string> questTags = new List<string>();
    private List<bool> questUnread = new List<bool>();
    private List<Quest> activeQuests = new List<Quest>();
    private QuestPanel[] questPanels = new QuestPanel[3];

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        //SceneManager.sceneLoaded += GetMessageManager;
        //SceneManager.sceneUnloaded += Save;
        //Save(SceneManager.GetActiveScene());

        savePath = Application.dataPath + "/" + relativeSavePath;
    }


    public void Load(Scene scene , LoadSceneMode mode)
    {
        messagesManager = GameObject.Find("PhoneButton").transform.Find("Messages").GetComponent<MessagesManager>();
        
        for (int i = 0; i < questNames.Count; ++i)
        {
            messagesManager.AddQuest(questNames[i] , questTags[i] , questUnread[i]);
        }
        Debug.Log($"Loaded info from scene {scene.name}. Loaded {questNames.Count} quests");
        for (int i = 0; i < questNames.Count; ++i)
        {
            Debug.Log($"Quest {questNames[i]} with tag {questTags[i]}");
        }

        foreach (Quest quest in activeQuests)
        {
            if (QuestManager.instance.GetQuestByName(quest.questName) != null)
                messagesManager.ActivateQuest(quest.questName);
        }
        
    }
    public void Save(Scene scene)
    {
        questNames = new List<string>();
        questTags = new List<string>();
        activeQuests = new List<Quest>();

        questPanels = GameObject.FindObjectsOfType<QuestPanel>(true);
        
        //questPanels[0] = GameObject.Find("PhoneButton").transform.Find("QuestIcon1").GetComponent<QuestPanel>();
        //questPanels[1] = GameObject.Find("PhoneButton").transform.Find("QuestIcon2").GetComponent<QuestPanel>();
        //questPanels[2] = GameObject.Find("PhoneButton").transform.Find("QuestIcon3").GetComponent<QuestPanel>();
        
        foreach (var panel in questPanels)
        {
            if (panel.targetQuest != null)
                activeQuests.Add(panel.targetQuest);
        }
        
        foreach (GameObject quest in messagesManager.Quests)
        {
            if (quest != null)
            {
                Debug.Log($"Quest {messagesManager.GetQuestName(quest)}");
                questNames.Add(messagesManager.GetQuestName(quest));
                questTags.Add((quest == null ? "" : quest.tag));
                questUnread.Add(quest.transform.Find("Unreaded").gameObject.activeSelf);
            }
        }
        
        
        Debug.Log($"Saved info from scene {scene.name}");
        for (int i = 0; i < questNames.Count; ++i)
        {
            //Debug.Log($"Quest {questNames[i]} with tag {questTags[i]}");
        }
    }
}
