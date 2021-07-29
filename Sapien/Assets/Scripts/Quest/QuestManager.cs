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
    public List<FriendQuest> friendQuestsList;
    
    public Dictionary<string, bool> completedQuest = new Dictionary<string, bool>();
    public Dictionary<string, bool> deletedQuest = new Dictionary<string, bool>();

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
        
        SceneManager.sceneLoaded += LoadAllFriendQuests;

        LoadAllQuestsForGiveCard(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        LoadAllStoryQuestOnScene(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        LoadAllQuestsAfterStoryQuest(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        LoadAllFriendQuests(SceneManager.GetActiveScene() , LoadSceneMode.Single);
    }

    public void TakeNewFragmentCard(CardInfo newCard)
    {
        OnStoryStarted?.Invoke(newCard);
        storyQuestStage = StoryQuestStage.Started;
        storyQuestCompleted = 0;
        completedQuest.Clear();
        card = newCard;
        
        LoadAllQuestsForGiveCard(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        LoadAllStoryQuestOnScene(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        LoadAllQuestsAfterStoryQuest(SceneManager.GetActiveScene() , LoadSceneMode.Single);
        
        if (questLogic != null)
            StopCoroutine(questLogic);
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
        OnStoryStarted?.Invoke(card);
        currentActiveStoryQuest = 0;
        storyQuestList.Sort((x,y) => x.questOrder.CompareTo(y.questOrder));
        while (currentActiveStoryQuest < storyQuestList.Count && storyQuestCompleted < card.storyQuestCount)
        {
            int i = currentActiveStoryQuest;
            if (!storyQuestList[i].availible && lastQuestOrder + 1 == storyQuestList[i].questOrder)
            {
                storyQuestList[i].OpenQuest();
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
            if (!completedQuest.ContainsKey(quest.questName) && !deletedQuest.ContainsKey(quest.questName))
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
            if (quest.questFromCard.cardID == card.cardID && !completedQuest.ContainsKey(quest.questName) && !deletedQuest.ContainsKey(quest.questName))
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
            if (quest.CanActivateCardOnFragmentCard(card) && !completedQuest.ContainsKey(quest.questName) && !deletedQuest.ContainsKey(quest.questName))
            {
                questAfterStoryQuestList.Add(quest);
            }
        }
        
        Debug.Log($"<size=14>Found  <b><color=blue>{questAfterStoryQuestList.Count}</color></b> after story quests</size>");
    }
    
    public void LoadAllFriendQuests(Scene scene , LoadSceneMode mode)
    {
        friendQuestsList = new List<FriendQuest>();
        FriendQuest[] questListLoc = FindObjectsOfType<FriendQuest>(true);
        foreach (FriendQuest quest in questListLoc)
        {
            if (!completedQuest.ContainsKey(quest.questName) && !deletedQuest.ContainsKey(quest.questName))
            {
                friendQuestsList.Add(quest);
            }
        }
        
        Debug.Log($"<size=14>Found  <b><color=blue>{friendQuestsList.Count}</color></b> friend quests</size>");
    }

    public QuestForGiveCard GetQuestForGiveCardByName(string name_)
    {
        foreach (QuestForGiveCard quest in questForGiveCardList)
        {
            if (quest != null && quest.questName == name_)
            {
                return quest;
            }
        }
        return null;
    }
    
    public StoryQuest GetStoryQuestByName(string name_)
    {
        Debug.Log(name_);
        foreach (StoryQuest quest in storyQuestList)
        {
            if (quest != null && quest.questName == name_)
            {
                return quest;
            }
        }
        return null;
    }
    
    public QuestAfterStoryQuest GetQuestAfterStoryQuestByName(string name_)
    {
        foreach (QuestAfterStoryQuest quest in questAfterStoryQuestList)
        {
            if (quest != null && quest.questName == name_)
            {
                return quest;
            }
        }
        return null;
    }
    public FriendQuest GetFriendQuestByName(string name_)
    {
        foreach (FriendQuest quest in friendQuestsList)
        {
            if (quest != null && quest.questName == name_)
            {
                return quest;
            }
        }
        return null;
    }
    
    
    public Quest GetQuestByName(string name_)
    {
        //Debug.Log(name_);
        Quest result = GetStoryQuestByName(name_);
        
        if (result == null)
            result = GetQuestForGiveCardByName(name_);

        if (result == null)
            result = GetQuestAfterStoryQuestByName(name_);
        
        if (result == null)
            result = GetFriendQuestByName(name_);
        //Debug.Log(result.questName);
        return result;
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

    public void AddToDeletedQuest(Quest quest)
    {
        if (deletedQuest.ContainsKey(quest.questName))
        {
            deletedQuest[quest.questName] = true;
        }
        else
        {
            deletedQuest.Add(quest.questName , true);  
        }
    }

    public void DeleteQuest(Quest quest)
    {
        questAfterStoryQuestList.RemoveAll((QuestAfterStoryQuest lhs) =>
        {
            if (quest.questName == lhs.questName)
                AddToDeletedQuest(lhs);
            return quest.questName == lhs.questName;
        });
        questForGiveCardList.RemoveAll((QuestForGiveCard lhs) =>
        {
            if (quest.questName == lhs.questName)
                AddToDeletedQuest(lhs);
            return quest.questName == lhs.questName;
        });
        storyQuestList.RemoveAll((StoryQuest lhs) =>
        {
            if (quest.questName == lhs.questName)
                AddToDeletedQuest(lhs);
            return quest.questName == lhs.questName;
        });
    }

    public bool IsQuestCompleted(Quest quest)
    {
        bool flag;
        completedQuest.TryGetValue(quest.questName, out flag);
        return flag;
    }
}
