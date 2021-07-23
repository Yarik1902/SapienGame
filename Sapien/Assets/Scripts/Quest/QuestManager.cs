using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StoryQuestStage
{
    DontStarted = 0 , Started = 1, Complete = 2
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    public CardInfo card;

    public List<StoryQuest> storyQuestList;
    public List<QuestForGiveCard> questForGiveCardList;
    public List<QuestAfterStoryQuest> questAfterStoryQuestList;
    public Dictionary<string, bool> completedQuest = new Dictionary<string, bool>();

    public StoryQuestStage storyQuestStage = StoryQuestStage.DontStarted;
    
    public event Action<CardInfo> OnStoryStarted;
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
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += LoadAllQuestsForGiveCard;
        
        SceneManager.sceneLoaded += LoadAllQuestsAfterStoryQuest;
        
        SceneManager.sceneLoaded += LoadAllStoryQuestOnScene;

        LoadAllQuestsForGiveCard(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        LoadAllStoryQuestOnScene(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        LoadAllQuestsAfterStoryQuest(SceneManager.GetActiveScene() , LoadSceneMode.Single);
    }

    public void TakeNewFragmentCard(CardInfo newCard)
    {
        OnStoryStarted?.Invoke(newCard);
        storyQuestStage = StoryQuestStage.Started;
        storyQuestCompleted = 0;
        completedQuest.Clear();
        card = newCard;
        
        LoadAllStoryQuestOnScene(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        
        questLogic = StartCoroutine(StartNewStoryQuest());
        FragmentCard.instance.TakeFragmentCard(card);
    }

    private int currentActiveStoryQuest = 0;

    public StoryQuest GetCurrentStoryQuest()
    {
        if (storyQuestStage == StoryQuestStage.Started)
            return storyQuestList[currentActiveStoryQuest];
        return null;
    }

    public IEnumerator StartNewStoryQuest()
    {
        currentActiveStoryQuest = 0;
        storyQuestList.Sort((x,y) => x.questOrder.CompareTo(y.questOrder));
        while (currentActiveStoryQuest < storyQuestList.Count)
        {
            int i = currentActiveStoryQuest;
            if (!storyQuestList[i].availible && lastQuestOrder + 1 == storyQuestList[i].questOrder)
            {
                //storyQuestList[i].OpenQuest();
                storyQuestList[i].OnQuestComplete += (() =>
                {
                    lastQuestOrder = storyQuestList[i].questOrder;
                    ++currentActiveStoryQuest;
                    CompleteQuest(storyQuestList[i].questName, true);
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
        storyQuestStage = StoryQuestStage.Complete;
        FragmentCard.instance.GetEnergy(75);
        OnStoryComplete?.Invoke(card);
    }

    public void LoadAllQuestsForGiveCard(Scene scene , LoadSceneMode mode)
    {
        questForGiveCardList = new List<QuestForGiveCard>();
        QuestForGiveCard[] questListLoc = FindObjectsOfType<QuestForGiveCard>(true);
        foreach (QuestForGiveCard quest in questListLoc)
        {
            if (!completedQuest.ContainsKey(quest.questName))
            {
                questForGiveCardList.Add(quest);
            }
        }
        Debug.Log($"<size=14>Found  <b><color=blue>{questForGiveCardList.Count}</color></b> give card quests</size>");
    }
    
    public void LoadAllStoryQuestOnScene(Scene scene , LoadSceneMode mode)
    {
        storyQuestList = new List<StoryQuest>();
        StoryQuest[] questListLoc = FindObjectsOfType<StoryQuest>(true);
        foreach (StoryQuest quest in questListLoc)
        {
            if (quest.questFromCard.cardID == card.cardID && !completedQuest.ContainsKey(quest.questName))
            {
                storyQuestList.Add(quest);
            }
        }
        
        Debug.Log($"<size=14>Found  <b><color=blue>{storyQuestList.Count}</color></b> story quests</size>");
        
        if (questLogic != null)
            StopCoroutine(questLogic);

        questLogic = StartCoroutine(StartNewStoryQuest());
    }

    public void LoadAllQuestsAfterStoryQuest(Scene scene , LoadSceneMode mode)
    {
        questAfterStoryQuestList = new List<QuestAfterStoryQuest>();
        QuestAfterStoryQuest[] questListLoc = FindObjectsOfType<QuestAfterStoryQuest>(true);
        foreach (QuestAfterStoryQuest quest in questListLoc)
        {
            if (quest.questForCard.cardID == card.cardID && !completedQuest.ContainsKey(quest.questName))
            {
                questAfterStoryQuestList.Add(quest);
            }
        }
        
        Debug.Log($"<size=14>Found  <b><color=blue>{questAfterStoryQuestList.Count}</color></b> after story quests</size>");
    }

    public QuestForGiveCard GetQuestForGiveCardByName(string name)
    {
        foreach (QuestForGiveCard quest in questForGiveCardList)
        {
            if (quest.questName == name)
            {
                return quest;
            }
        }
        return null;
    }
    
    public StoryQuest GetStoryQuestByName(string name)
    {
        foreach (StoryQuest quest in storyQuestList)
        {
            if (quest.questName == name)
            {
                return quest;
            }
        }
        return null;
    }
    
    public QuestAfterStoryQuest GetQuestAfterStoryQuestByName(string name)
    {
        foreach (QuestAfterStoryQuest quest in questAfterStoryQuestList)
        {
            if (quest.questName == name)
            {
                return quest;
            }
        }
        return null;
    }
    
    public void CompleteQuest(string questName , bool storyQuest)
    {
        if (completedQuest.ContainsKey(questName))
        {
            completedQuest[questName] = true;
        }
        else
        {
            completedQuest.Add(questName , true);  
        }
        //completedQuest.Add(questName , true);
        if (storyQuest)
            storyQuestCompleted++;
    }

}
