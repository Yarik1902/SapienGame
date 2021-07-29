using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class FragmentCard : MonoBehaviour
{
    [Header("CardInfo")] 
    public CardInfo cardInfo;
    
    [Header("CardUI")]
    public Image cardImageUI;
    public Text cardTextUI;
    public Image energyLine;
    public Image[] energyCells;
    public Text backsideCard;
    public Text finishText;

    [Header("EnergyText")] 
    public Text energyText;
    public RectTransform startPosition, finishPosition;

    [Header("Timer")] 
    public GameObject fragmentCardTimerPrefab;

    [HideInInspector]
    public GameObject currentfragmentCardTimer;
    
    [Header("Dependencies")] 
    public PhoneManager phoneManager;
    
    public Image placeInPhone;

    [HideInInspector]public int currentEnergy = 0;

    private bool lastCoroutineIsEnded = true; 
    private bool cardIsFront = true;
    private bool isMouseOverText = false;
    private bool visible = false;
    private float currentFillAmount = 0;
    
    public static FragmentCard instance;
    
    public static List<CardInfo> allCards = new List<CardInfo>();

    public event Action<CardInfo> onFragmentCardComplete;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (allCards.Count == 0)
            allCards = Resources.LoadAll<CardInfo>("Fragment cards").ToList();
        
        allCards.Sort((CardInfo lhs , CardInfo rhs) =>
        {
            return (lhs.cardID - rhs.cardID);
        });
        
        Debug.Log($"<size=15> <color=green>{allCards.Count}</color> fragment cards founded</size>");
        foreach (CardInfo card in allCards)
        {
            Debug.Log($"<color=grey>{card.cardName}</color>");
        }

        phoneManager = GameObject.Find("PhoneButton").GetComponent<PhoneManager>();
    }

    private void Start()
    {
        StartCoroutine(TextShowing());
        LoadLastCard();
    }

    public void LoadLastCard()
    {
        if (PlayerPrefs.GetInt("FragmentCardID") != -1)
        {
            ActivateCard();
            cardInfo = GetCardByID(PlayerPrefs.GetInt("FragmentCardID"));
            currentEnergy = PlayerPrefs.GetInt("FragmentCardEnergy");
            DisplayCard(cardInfo);
            InitializeTimer();
        }
    }

    public static CardInfo GetCardByID(int id)
    {
        foreach (CardInfo card in allCards)
        {
            if (card.cardID == id)
            {
                return card;
            }
        }

        return null;
    }

    public void TakeFragmentCard(CardInfo newCard)
    {
        ActivateCard();
        cardInfo = newCard;
        
        if (currentfragmentCardTimer != null)
            Destroy(currentfragmentCardTimer);

        InitializeTimer();
        
        currentEnergy = 0;
        StartCoroutine(ShowCells(true));
        
        SaveCardData();
        
        DisplayCard(newCard);
    }

    public void ActivateCard()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        visible = true;
        //this.gameObject.SetActive(true);
    }
    public void DeactivateCard()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        visible = false;
        //this.gameObject.SetActive(false);
    }
    
    public void InitializeTimer()
    {
        currentfragmentCardTimer = Instantiate(fragmentCardTimerPrefab);
        currentfragmentCardTimer.GetComponent<FragmentCardTimer>().card = cardInfo;
    }
    
    public void DisplayCard(CardInfo card)
    {
        cardImageUI.sprite = cardInfo.cardSprite;
        backsideCard.text = cardInfo.cardName;
        GetEnergy(0);
    }

    #region EnegryGet and Display
    public void GetEnergy(int energy)
    {
        if (cardInfo != null)
        {
            if (currentEnergy + energy >= cardInfo.cardEnergy)
            {
                currentEnergy = cardInfo.cardEnergy;
                OnCardComplete();
            }
            else
            {
                currentEnergy += energy;
            }
            
            UpdateCardUI();
        }
    }

    void UpdateCardUI()
    {
        SetEnergyCellsFill(GetEnergyNormalized());
    }
    

    IEnumerator SmoothFillCardEnergy(int cellNumber , float amount, float duration)
    {
        if (cellNumber > 0 )
        {
            if (amount > energyCells[cellNumber].fillAmount)
            {
                while (energyCells[cellNumber - 1].fillAmount < 1)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        if (cellNumber < energyCells.Length - 1)
        {
            if (amount < energyCells[cellNumber].fillAmount)
            {
                while (energyCells[cellNumber + 1].fillAmount > 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        lastCoroutineIsEnded = false;
        
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            energyCells[cellNumber].fillAmount = Mathf.Clamp(energyCells[cellNumber].fillAmount + (amount < energyCells[cellNumber].fillAmount ? -1 : 1) * (Time.deltaTime / duration) 
                ,0 , Mathf.Max(amount , energyCells[cellNumber].fillAmount));
            //energyCells[cellNumber].fillAmount = Mathf.Lerp(energyCells[cellNumber].fillAmount , amount , (2f / duration)  * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        energyCells[cellNumber].fillAmount = amount;

        lastCoroutineIsEnded = true;
    }
    
    void SetEnergyCellsFill(float fill)
    {
        cardTextUI.text = currentEnergy.ToString() + " / " + cardInfo.cardEnergy.ToString();
        float amountAtCell = 1f / energyCells.Length;
        float elapsedFill = 0;

        if (fill > currentFillAmount)
        {
            for (int i = 0; i < energyCells.Length; ++i)
            {
                float fillToCurrentCell = Mathf.Max(Mathf.Min(fill - elapsedFill, amountAtCell), 0);
                elapsedFill += fillToCurrentCell;
                fillToCurrentCell /= amountAtCell;

                float timeForCell = 1f / (energyCells.Length);
                StartCoroutine(SmoothFillCardEnergy(i, fillToCurrentCell, timeForCell));
            }
        }
        else
        {
            for (int i = energyCells.Length - 1; i >= 0; --i)
            {
                float fillToCurrentCell = Mathf.Max(Mathf.Min(fill - elapsedFill, amountAtCell), 0);
                elapsedFill += fillToCurrentCell;
                fillToCurrentCell /= amountAtCell;

                float timeForCell = 1f / (energyCells.Length);
                StartCoroutine(SmoothFillCardEnergy(i, fillToCurrentCell, timeForCell));
            }
        }

        currentFillAmount = fill;
    }
    float GetEnergyNormalized()
    {
        return (float)currentEnergy / cardInfo.cardEnergy;
    }
    #endregion

    #region Card Flip

    private Coroutine flip;
    public void FlipCard()
    {
        if (cardIsFront)
        {
            if (flip != null)
                StopCoroutine(flip);
            flip = StartCoroutine(SmoothFlipCard(0));
        }
        else
        {
            if (flip != null)
                StopCoroutine(flip);
            flip = StartCoroutine(SmoothFlipCard(1));
        }
        cardIsFront = !cardIsFront;
    }

    IEnumerator SmoothFlipCard(float opacity)
    {
        float duration = 1 , elapsedTime = 0;
        while (elapsedTime < duration)
        {
            Color nextColor = cardImageUI.color;
            nextColor.a = Mathf.Lerp(nextColor.a, opacity, Time.deltaTime * 3);
            cardImageUI.color = nextColor;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } 
        Color lastcolor = cardImageUI.color;
        lastcolor.a = opacity;
        cardImageUI.color = lastcolor;
    }

    #endregion


    #region EnergyText Display
    IEnumerator TextShowing()
    {
        RectTransform energyTextRect = energyText.gameObject.GetComponent<RectTransform>();
        float speed = finishPosition.position.y - startPosition.position.y;
        while (true)
        {
            Vector3 nextPos = energyTextRect.position;
            float nextYPos = energyTextRect.position.y + (isMouseOverText? 1 : -1) * speed * Time.deltaTime * 3;
            nextYPos = Mathf.Clamp(nextYPos,  finishPosition.position.y , startPosition.position.y);
            nextPos.y = nextYPos;
            energyTextRect.position = nextPos;
            
            yield return new WaitForEndOfFrame();
        }        
    }

    public void MouseOverText(bool value)
    {
        isMouseOverText = value;
    }
    
    
    #endregion

    #region CardComplete
    
    public void OnCardComplete()
    {
        Debug.Log(cardInfo.cardName + " Card Complete");
        onFragmentCardComplete?.Invoke(cardInfo);
        StartCoroutine(ShowCells(false));
    }
    
    IEnumerator ShowCells(bool flag)
    {
        float targetAlpha = (flag ? 1f : 0f);
        float targetFill = (flag ? 0 : 1f);
        for (int i = 0; i < energyCells.Length; ++i)
        {
            float duration = 0.2f, elapsedTime = 0;
            Image currentCellBG = energyCells[i].transform.parent.GetComponent<Image>() , currentCell = energyCells[i].transform.GetComponent<Image>();
            
            while (elapsedTime < duration)
            {
                Color nextColorBG = currentCellBG.color , nextColor = currentCell.color;
                
                nextColor.a = Mathf.Lerp(nextColor.a, 1f * targetAlpha, Time.deltaTime * 8);
                nextColorBG.a = Mathf.Lerp(nextColorBG.a, 0.8f * targetAlpha, Time.deltaTime * 8);;
                
                currentCell.color = nextColor;
                currentCellBG.color = nextColorBG;
                
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            Color lastcolorBG = currentCellBG.color , lastcolor = currentCell.color;
            lastcolor.a = 1f * targetAlpha;
            lastcolorBG.a = 0.8f * targetAlpha;
            
            currentCell.color = lastcolor;
            currentCellBG.color = lastcolorBG;
        }

        float elapsedTime_ = 0;

        while (elapsedTime_ < 1)
        {
            energyLine.fillAmount = Mathf.Lerp(energyLine.fillAmount, 1 * targetFill, Time.deltaTime * 6);
            elapsedTime_ += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        energyLine.fillAmount = 1 * targetFill;

        elapsedTime_ = 0;
        while (elapsedTime_ < 1)
        {
            Color nextColor = finishText.color;
                
            nextColor.a = Mathf.Lerp(nextColor.a, 1 * targetFill, Time.deltaTime * 6);

            finishText.color = nextColor;

            elapsedTime_ += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion

    #region CardGiveOff

    public void OnCardGiveOff()
    {
        if (GetEnergyNormalized() == 1)
        {
            StartCoroutine(GiveOff());
        }
        else
        {
            GiveOffFailed();
        }
    }

    IEnumerator GiveOff()
    {
        cardInfo.time = currentfragmentCardTimer.GetComponent<FragmentCardTimer>().secondsWithCurrentFragmentCard;
        Debug.Log($"{cardInfo.time} seconds wasted for {cardInfo.name} fragment card");        
        currentfragmentCardTimer.GetComponent<FragmentCardTimer>().CompleteCard();
        
        Destroy(currentfragmentCardTimer);
        phoneManager.OnPointerClickCardIcon("Cards");

        yield return new WaitForSeconds(0.3f);
        
        FragmentCardPhoneCell cardCell = FragmentCardPhoneCell.GetCardCellByID(cardInfo.cardID);
        cardCell.OpenCardFirstly();
        
        cardInfo = null;
        DeactivateCard();
    }
    #endregion

    private void SaveCardData()
    {
        if (cardInfo != null)
            PlayerPrefs.SetInt("FragmentCardID" , cardInfo.cardID);
        else
            PlayerPrefs.SetInt("FragmentCardID" , -1);
        
        PlayerPrefs.SetInt("FragmentCardEnergy" , currentEnergy);
    }
    private void OnDestroy()
    {
        SaveCardData();
    }

    public void GiveOffFailed()
    {
        Debug.Log("GiveOff failed");
    }
}
