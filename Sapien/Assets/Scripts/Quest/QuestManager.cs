using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    public CardInfo card;

    public List<StoryQuest> questList;
    public Dictionary<string, bool> completedQuest = new Dictionary<string, bool>();

    
    public event Action<CardInfo> OnStoryComplete;

    private int storyQuestCompleted = 0 , lastQuestOrder = 0; 

    private Coroutine questLogic;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += LoadAllQuestOnScene;
    }

    public void TakeNewFragmentCard(CardInfo newCard)
    {
        storyQuestCompleted = 0;
        completedQuest.Clear();
        card = newCard;
        
        LoadAllQuestOnScene(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        
        questLogic = StartCoroutine(StartNewQuest());
        FragmentCard.instance.TakeFragmentCard(card);
        
    }

    public IEnumerator StartNewQuest()
    {
        questList.Sort((x,y) => x.questOrder.CompareTo(y.questOrder));
        int i = 0;
        while (i < questList.Count)
        {
            if (!questList[i].availible && lastQuestOrder + 1 == questList[i].questOrder)
            {
                questList[i].OpenQuest();
                questList[i].OnQuestComplete += (() =>
                {
                    CompleteQuest(questList[i].questName, true);
                    lastQuestOrder = questList[i].questOrder;
                    ++i;
                });
            }
            yield return new WaitForEndOfFrame();
        }

        if (storyQuestCompleted >= card.storyQuestCount)
        {
            StoryComplete();
        }
    }

    public void StoryComplete()
    {
        Debug.Log($"Story quest for card <b>{card.cardName} <color=green>complete</color></b>. Get <color=yellow>75</color> energy");
        FragmentCard.instance.GetEnergy(75);
        OnStoryComplete?.Invoke(card);
    }

    public void LoadAllQuestOnScene(Scene scene , LoadSceneMode mode)
    {
        questList = new List<StoryQuest>();
        StoryQuest[] questListLoc = FindObjectsOfType<StoryQuest>(true);
        foreach (StoryQuest quest in questListLoc)
        {
            if (quest.questFromCard.cardID == card.cardID && !completedQuest.TryGetValue(quest.questName , out bool value))
            {
                questList.Add(quest);
            }
        }
        
        Debug.Log($"<size=14>Found  <b><color=blue>{questList.Count}</color></b> quests</size>");
        
        if (questLogic != null)
            StopCoroutine(questLogic);
        
        questLogic = StartCoroutine(StartNewQuest());
    }
    
    public void CompleteQuest(string questName , bool storyQuest)
    {
        completedQuest.Add(questName , true);
        if (storyQuest)
            storyQuestCompleted++;
    }
    
}
