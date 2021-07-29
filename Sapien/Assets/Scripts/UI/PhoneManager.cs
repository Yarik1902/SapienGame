using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PhoneManager : MonoBehaviour
{
    [HideInInspector] public PhoneManager instance;
    public GameObject Phone;
    public float Increment;
    public GameObject StoryScreen;
    public Animator anim;
    public GameObject Notification;
    public bool closeable = false;
    public Vector3 StartMousePos;
    public Vector3 EndMousePos;
    public GameObject Messages;
    private bool QuestAvailable = false;
    public string[] QuestType = { "Start of fragment", "Story quest", "Battle", "Wish mission" };
    public bool SecondQuestAvailable = false;
    
    public int ActiveQuests = 0;
    public int CurrentQuestType;

    public bool NotOpened = true;
    public GameObject CameraPanel;

    [HideInInspector] public Coroutine CR_StoryQuest , CR_NotificationShaking;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        MessagesSave.instance.Load(SceneManager.GetActiveScene() , LoadSceneMode.Single);
    }

    public void OnButtonClickPhoneOpener()
    {
        Phone.SetActive(true);

        
        int unread = Messages.GetComponent<MessagesManager>().GetUnreadMassages();

        if (unread > 0)
        {
            Phone.transform.Find("Mail").GetChild(0).gameObject.SetActive(true);
            Phone.transform.Find("Mail").GetChild(1).gameObject.SetActive(true);
            Phone.transform.Find("Mail").GetChild(1).GetComponent<Text>().text = unread.ToString();
        }
        else
        {
            Phone.transform.Find("Mail").GetChild(0).gameObject.SetActive(false);
            Phone.transform.Find("Mail").GetChild(1).gameObject.SetActive(false);
        }
    }

    public void OnButtonClickPhoneCloser()
    {
        StartCoroutine(PhoneCloser());
    }

    IEnumerator PhoneCloser()
    {
        Phone.GetComponent<Animator>().Play("PhoneClosed");
        yield return new WaitForSeconds(1f);
        Phone.SetActive(false);
    }

    public void OnPointerEnterIncrease(string tag)
    {
        GameObject.Find(tag).GetComponent<RectTransform>().localScale = new Vector3(Increment, Increment, 1f);
    }

    public void OnPointerEnterDecrease(string tag)
    {
        GameObject.Find(tag).GetComponent<RectTransform>().localScale = new Vector3(0.9f, 0.9f, 1f);
    }

    public void OnPointerClickStoryIcon(string tag)
    {
        StartCoroutine(PhoneCloser());
        anim.Play("StoryScreen");
        GetComponent<Button>().interactable = false;
    }

    public void OnPointerClickStoryIconClose(string tag)
    {
        anim.Play("StoryScreen0");
        Phone.SetActive(true);
        GetComponent<Button>().interactable = true;
    }

    public void OnPointerClickCardIcon(string tag)
    {
        StartCoroutine(PhoneCloser());
        anim.Play("FragmentCard");
        GetComponent<Button>().interactable = false;
    }

    public void OnPointerClickCardIconClose(string tag)
    {
        anim.Play("FragmentCard0");
        Phone.SetActive(true);
        GetComponent<Button>().interactable = true;
    }

    public void OnPointerClickMessagesIcon(string tag)
    {
        StartCoroutine(PhoneCloser());
        anim.Play("Messages");
        GetComponent<Button>().interactable = false;
    }

    public void OnPointerClickMessagesIconClose(string tag)
    {
        anim.Play("Messages0");
        GetComponent<Button>().interactable = true;
        //GetComponent<QuestPanelManager>().AddQuestToActiveList("System", QuestType[CurrentQuestType]);
    }

    public void OnNotificationOpener()
    {
        Notification.SetActive(true);
        Messages.GetComponent<MessagesManager>().QuestAvailable = QuestAvailable;
        if (QuestManager.instance.storyQuestStage == StoryQuestStage.DontStarted)
        {
            QuestForGiveCard questForGiveCard = FindObjectOfType<QuestForGiveCard>(true);
            if (!questForGiveCard.availible)
            {
                questForGiveCard.OpenQuest();
            }
            if (!Messages.GetComponent<MessagesManager>().IsQuestAdded(questForGiveCard.questName) && !QuestManager.instance.IsQuestCompleted(questForGiveCard))
            {
                Messages.GetComponent<MessagesManager>().AddQuest(questForGiveCard.questName, QuestType[0]);
            }
            if (!questForGiveCard.activated && !QuestManager.instance.IsQuestCompleted(questForGiveCard))
            {
                Messages.GetComponent<MessagesManager>().MakeMessageUnreaded(questForGiveCard.questName);
                UpdateNotificationInfo(questForGiveCard , true , 1);
                CR_NotificationShaking = StartCoroutine(ShowNotification(9999999));
            }
            /*Messages.GetComponent<MessagesManager>().OnClickChatOpener(QuestType[0]);*/

            QuestAvailable = true;
            CurrentQuestType = 0;
            
        }
        else
        {
            if (QuestManager.instance.storyQuestStage == StoryQuestStage.Started)
            {
                StoryQuest storyQuest = QuestManager.instance.GetCurrentStoryQuest();
                if (storyQuest.availible)
                {
                    if (storyQuest.questOrder > 1)
                    {
                        //storyQuest.Activate();
                        GetComponent<QuestPanelManager>().AddQuestToActiveList(storyQuest.questName , QuestType[1]);
                    }
                    else
                    {
                        if (!Messages.GetComponent<MessagesManager>().IsQuestAdded(storyQuest.questName) && !QuestManager.instance.IsQuestCompleted(storyQuest))
                        {
                            Messages.GetComponent<MessagesManager>().AddQuest(storyQuest.questName, QuestType[1]);
                        }

                        if (!storyQuest.activated && !QuestManager.instance.IsQuestCompleted(storyQuest))
                        {
                            Messages.GetComponent<MessagesManager>().MakeMessageUnreaded(storyQuest.questName);
                            UpdateNotificationInfo(storyQuest, true, 1);
                            CR_NotificationShaking = StartCoroutine(ShowNotification(999999));
                        }
                    }

                    /*Messages.GetComponent<MessagesManager>().OnClickChatOpener(QuestType[1]);*/
                    //SecondQuestAvailable = true;
                    CurrentQuestType = 1;
                }
            }
            else if (QuestManager.instance.storyQuestStage == StoryQuestStage.Complete)
            {
                QuestAfterStoryQuest lastQuest = null;
                int questsInMessages = 0;
                foreach (QuestAfterStoryQuest quest in QuestManager.instance.questAfterStoryQuestList)
                {
                    if (!QuestManager.instance.completedQuest.ContainsKey(quest.questName) && quest.CanActivateCardOnFragmentCard(QuestManager.instance.card))
                    {
                        if (!quest.availible)
                        {
                            quest.TryOpen(FragmentCard.instance.cardInfo);
                        }
     
                        if (quest.availible && !quest.activated)
                        {
                            lastQuest = quest;
                            bool isAdded = Messages.GetComponent<MessagesManager>().IsQuestAdded(quest.questName); 
                            if (!isAdded)
                            {
                                Messages.GetComponent<MessagesManager>().AddQuest(quest.questName, QuestType[2]);
                                questsInMessages++;
                            }
                            else
                            {
                                questsInMessages++;
                            }

                            Messages.GetComponent<MessagesManager>().MakeMessageUnreaded(quest.questName);
                            CurrentQuestType = 2;
                            //break;
                        }
                    }
                }

                if (questsInMessages > 0)
                {
                    if (questsInMessages > 1)
                    {
                        UpdateNotificationInfo(lastQuest, false, questsInMessages);
                        Notification.transform.Find("QuestType").GetComponent<Text>().text = "Availible " + questsInMessages.ToString() + " quests";
                    }
                    else
                    {
                        UpdateNotificationInfo(lastQuest, true , questsInMessages);
                    }
                    CR_NotificationShaking = StartCoroutine(ShowNotification(5));
                }
                
            }
        }
        //GetComponent<QuestPanelManager>().QuestAdded = true;
    }

    public void UpdateNotificationInfo(Quest quest, bool showAvatar , int unreadedCount)
    {
        Notification.transform.Find("QuestType").GetComponent<Text>().text = quest.questDescription;
        if (showAvatar)
        {
            Notification.transform.Find("Mask").gameObject.SetActive(true);
            Notification.transform.Find("Mask").GetChild(0).GetComponent<Image>().sprite = quest.questGiverAvatar;
        }
        else
        {
            Notification.transform.Find("Mask").gameObject.SetActive(false);
        }

        Notification.transform.Find("UnreadMessages").GetComponent<Text>().text = unreadedCount.ToString();
    }
    
    IEnumerator ShowNotification(float duration)
    {
        anim.Play("NotificationOpen");
        yield return new WaitForSeconds(duration + 1);
        if (anim.GetCurrentAnimatorStateInfo(anim.GetLayerIndex("Base Layer")).IsName("NotificationShaking"))
        {
            anim.Play("NotificationOpen0");
        }
    }
    
    public void StartStoryQuestChain()
    {
        if (CR_StoryQuest == null)
            StartCoroutine(StoryQuestActivate());
    }
    
    IEnumerator StoryQuestActivate()
    {
        int currentQuestOrder;
        StoryQuest quest = QuestManager.instance.GetCurrentStoryQuest();
        while (QuestManager.instance.storyQuestStage == StoryQuestStage.Started)
        {
            bool canNewIteration = false;
            
            yield return new WaitUntil(() => quest.availible);

            quest.OnQuestComplete += () =>
            {
                quest = QuestManager.instance.GetCurrentStoryQuest();
                Debug.Log(quest.questName);
                canNewIteration = true;
                StartCoroutine(WaitAndAddToActiveList(quest));
            };
            
            yield return new WaitUntil(() => canNewIteration);
        }
    }

    IEnumerator WaitAndAddToActiveList(StoryQuest quest)
    {
        yield return new WaitForSeconds(1.5f);
        if (quest.availible)
            GetComponent<QuestPanelManager>().AddQuestToActiveList(quest.questName , QuestType[1]);
    }
    
    public IEnumerator Notifying()
    {
        yield return new WaitForSeconds(5f);
        if (NotOpened)
        {
            anim.Play("NotificationOpen0");
        }
    }

    public void OnNotificationClick()
    {
        NotOpened = false;
        StopCoroutine(CR_NotificationShaking);
        anim.Play("NotificationOpen0");
        anim.Play("Messages");
        GetComponent<Button>().interactable = false;
        closeable = true;
        if (ActiveQuests < 3)
        {
            ActiveQuests++;
        }
    }

    public void OnNotificationPointerDown()
    {
        StartMousePos = Input.mousePosition;
    }

    public void OnNotificationPointerUp()
    {
        EndMousePos = Input.mousePosition;
        if (EndMousePos.x > StartMousePos.x && closeable)
        {
            GetComponent<QuestPanelManager>().QuestAdded = false;
            anim.Play("NotificationOpen0");
        }
    }

    public void OnClickCameraOpen()
    {
        CameraPanel.SetActive(true);
        anim.Play("CameraOpen");
        GetComponent<Button>().interactable = false;
        StartCoroutine(PhoneCloser());
        CameraPanel.GetComponent<CameraManager>().enabled = true;
    }

    public void OnClickCameraClose()
    {
        anim.Play("CameraOpen0");
        GetComponent<Button>().interactable = true;
        CameraPanel.GetComponent<CameraManager>().enabled = false;
        Camera.main.GetComponent<Transform>().localEulerAngles = new Vector3(0f, 0f, 0f);
    }

    private void OnDestroy()
    {
        MessagesSave.instance.Save(SceneManager.GetActiveScene());
    }
}
