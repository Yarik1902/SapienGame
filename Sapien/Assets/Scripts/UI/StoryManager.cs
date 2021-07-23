using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{

    public GameObject SeasonPanel;
    public int SeasonNumber;
    public GameObject MainButton;
    public float Increment;
    public Sprite[] SeasonSprite;
    public GameObject[] Season;
    public int MaxSeasonAvailable;

    [Header("Card Info and stats")]
    public Text textWithSlider, reviewText;
    public Image cardImage;
    public Slider sliderTime, sliderQuality, sliderMoney;
    
    public void OnClickSeasonOpener(int season)
    {
        if (season <= MaxSeasonAvailable)
        {
            SeasonPanel.SetActive(true);
            if (SeasonNumber != null && SeasonNumber != 0)
            {
                OnPointerExitSeason("Season" + SeasonNumber);
                OnPointerEnterSeason("Season" + season);
                SeasonNumber = season;
            }
            else
            {
                OnPointerEnterSeason("Season" + season);
                SeasonNumber = season;
            }
        }
    }

    public void OnPointerEnterIncrease(string tag)
    {
        if (tag == "Season" + SeasonNumber)
        {
            
        }
        else if (tag.Contains("Season"))
        {
            if ((int)System.Char.GetNumericValue(tag[6]) <= MaxSeasonAvailable)
            {
                GameObject.Find(tag).GetComponent<RectTransform>().localScale = new Vector3(Increment, Increment, 1f);
            }
        }
        else if (tag.Contains("Day"))
        {
            GameObject.Find(tag).GetComponent<RectTransform>().localScale = new Vector3(Increment, Increment, 1f);
        }
    }

    public void OnPointerEnterDecrease(string tag)
    {
        if (("Season" + SeasonNumber) != tag)
        {
            GameObject.Find(tag).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        }
    }

    public void OnPointerEnterSeason(string tag)
    {
        GameObject.Find(tag).GetComponent<Image>().sprite = SeasonSprite[1];
        GameObject.Find(tag).GetComponent<RectTransform>().localScale = new Vector3(1.125f*Increment, 1.2f*Increment, 1);
    }

    public void OnPointerExitSeason(string tag)
    {
        GameObject.Find(tag).GetComponent<Image>().sprite = SeasonSprite[0];
        GameObject.Find(tag).GetComponent<RectTransform>().localScale = new Vector3(1.1f, 1.1f, 1);
    }

    private int currentCR = 0;
    
    public void ShowCardInfo(FragmentCardPhoneCell cell)
    {
        //Debug.Log(currentCR);
        if (currentCR == 0 && !cell.locked)
        {
            StartCoroutine(CR_ShowCardSprite(cell));
            StartCoroutine(CR_ShowCardReviewText(cell));
            StartCoroutine(CR_ShowCardTextWithSlider(cell));
            StartCoroutine(CR_ShowStats(cell));
        }
    }
    

    IEnumerator CR_ShowCardSprite(FragmentCardPhoneCell cell)
    {
        currentCR++;
        float speed = 2;
        while (cardImage.fillAmount > 0)
        {
            float nextFill = Mathf.Clamp01(cardImage.fillAmount - Time.deltaTime * speed);
            cardImage.fillAmount = nextFill;
            
            yield return new WaitForEndOfFrame();
        }

        cardImage.sprite = cell.cardInfo.cardSprite;
        cardImage.fillClockwise = !cardImage.fillClockwise;
        
        while (cardImage.fillAmount < 1)
        {
            float nextFill = Mathf.Clamp01(cardImage.fillAmount + Time.deltaTime * speed);
            cardImage.fillAmount = nextFill;
            
            yield return new WaitForEndOfFrame();
        }

        cardImage.fillClockwise = !cardImage.fillClockwise;
        Debug.Log("Image End");
        currentCR--;
    }

    IEnumerator CR_ShowCardTextWithSlider(FragmentCardPhoneCell cell)
    {
        currentCR++;
        textWithSlider.text = cell.cardInfo.cardReview;

        Color loc = textWithSlider.color;
        loc.a = 0;
        textWithSlider.color = loc;
        
        float speed = 1;
        while (textWithSlider.color.a < 1)
        {
            Color nextColor = textWithSlider.color;
            nextColor.a = Mathf.Clamp01(nextColor.a + Time.deltaTime * speed);
            textWithSlider.color = nextColor;
            yield return new WaitForEndOfFrame();
        }
        
        Debug.Log("TextSlider End");
        currentCR--;
        yield return null;
    }

    IEnumerator CR_ShowCardReviewText(FragmentCardPhoneCell cell)
    {
        currentCR++;
        int i = reviewText.text.Length - 1;

        reviewText.text = "";

        i = 0;
        
        while (i < cell.cardInfo.cardReview.Length)
        {
            reviewText.text += cell.cardInfo.cardReview[i++];
            yield return new WaitForSeconds(0.04f);
        }

        Debug.Log("Text End");
        currentCR--;
        yield return null;
    }

    IEnumerator CR_ShowStats(FragmentCardPhoneCell cell)
    {
        float duration = 1 , elapsedTime = 0;
        float speed = 5;
        
        Text timeText = sliderTime.GetComponentInChildren<Text>();

        int hh = (int) cell.cardInfo.time / 3600 , mm = ((int)(cell.cardInfo.time / 60)) % 60;
        
        
        timeText.text = (hh == 0) ? mm.ToString()+"m" : hh.ToString() + "h " + mm.ToString() + "m";

        float timeNormilized = Mathf.Min(cell.cardInfo.time / 3600f, 1);
        while (elapsedTime < duration)
        {
            sliderMoney.value = Mathf.Lerp(sliderMoney.value, cell.cardInfo.money, Time.deltaTime * speed);
            sliderQuality.value = Mathf.Lerp(sliderQuality.value, cell.cardInfo.quality, Time.deltaTime * speed);
            sliderTime.value = Mathf.Lerp(sliderTime.value, timeNormilized, Time.deltaTime * speed);

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        sliderMoney.value = cell.cardInfo.money;
        sliderTime.value = timeNormilized;
        sliderQuality.value = cell.cardInfo.quality;
        
        yield return null;
    }
}
