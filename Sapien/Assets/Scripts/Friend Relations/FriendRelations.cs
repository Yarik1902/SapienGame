using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FriendRelations : MonoBehaviour
{
    public Image[] friendRelationFill;
    public Slider friendRelationSlider;
    public Slider friendRelationBigSlider;
    public CanvasGroup bigCanvasGroup;
    public Text friendStage;
    public Animator anim;
    
    public Color firstColor, secondColor , thirdColor;
    public string[] friendStages;
    

    public GameObject CurrentCharacterGameObject;
    
    private Gradient _gradient;

    private bool showSlider = false;
    private Ray ray;
    private RaycastHit hit;

    

    private void Start()
    {

        _gradient = new Gradient();
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        
        colorKeys[0].color = firstColor;
        colorKeys[0].time = 0.0f;
        
        colorKeys[1].color = secondColor;
        colorKeys[1].time = 0.5f;
        
        colorKeys[2].color = thirdColor;
        colorKeys[2].time = 1.0f;
        
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 0.5f;
        alphaKeys[2].alpha = 1f;
        alphaKeys[2].time = 1.0f;
        
        _gradient.SetKeys(colorKeys,alphaKeys);
        //GetFriendRelationPoints(0);
        OnValueChanged();
        
        QuestManager.instance.OnStoryComplete += Activate;
        this.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == CurrentCharacterGameObject)
            {
               //Debug.Log("Піпався");
                showSlider = true;
            }
            else
            {
                showSlider = false;    
            }
        }
        else
        {
            showSlider = false;
        }
    }

    public void Activate(CardInfo card)
    {
        this.gameObject.SetActive(true);
        StartCoroutine(TextShowing());
    }

    public void OnValueChanged()
    {
        foreach (var image in friendRelationFill)
        {
            image.color = _gradient.Evaluate(friendRelationSlider.value);
        }
    
        friendStage.text = friendStages[Mathf.Max(Mathf.CeilToInt((friendRelationSlider.value * (friendStages.Length - 1)) - 1) , 0)];

        if (Mathf.Approximately(friendRelationSlider.value , 1))
        {
            friendStage.text = friendStages[friendStages.Length - 1];
        }
        
        friendStage.color = _gradient.Evaluate(friendRelationSlider.value);
        
    }

    public void GetFriendRelationPoints(float amount)
    {
        if (friendRelationSlider.value + amount > 1)
        {
            StartCoroutine(SmoothSliderValueChange(1 , true));
        }
        else
        {
            StartCoroutine(SmoothSliderValueChange(friendRelationSlider.value + amount , true));
        }
        anim.SetTrigger("Victory");
    }
    
    public void GetFriendRelationPoints(float amount , bool showOnBigThermometr = true)
    {
        if (friendRelationSlider.value + amount > 1)
        {
            StartCoroutine(SmoothSliderValueChange(1 , showOnBigThermometr));
        }
        else
        {
            StartCoroutine(SmoothSliderValueChange(friendRelationSlider.value + amount , showOnBigThermometr));
        }
        anim.SetTrigger("Victory");
    }

    IEnumerator SmoothSliderValueChange(float targetValue , bool showBigThermometr = false)
    {
        float elapsedTime = 0;
        
        if (showBigThermometr)
        {
            //CanvasGroup canvasGroup = friendRelationBigSlider.gameObject.GetComponent<CanvasGroup>();
            while (elapsedTime < 0.5f)
            {
                float nextA = Mathf.Lerp(bigCanvasGroup.alpha, 1, Time.deltaTime * 10);
                bigCanvasGroup.alpha = nextA;
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } 
            bigCanvasGroup.alpha = 1;
        }
        
        elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            friendRelationSlider.value = Mathf.Lerp(friendRelationSlider.value, targetValue, 8 * Time.deltaTime);
            friendRelationBigSlider.value = friendRelationSlider.value;
            OnValueChanged();
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        elapsedTime = 0;
        if (showBigThermometr)
        {
            //CanvasGroup canvasGroup = friendRelationBigSlider.gameObject.GetComponent<CanvasGroup>();
            while (elapsedTime < 0.5f)
            {
                float nextA = Mathf.Lerp(bigCanvasGroup.alpha, 0, Time.deltaTime * 10);
                bigCanvasGroup.alpha = nextA;
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            bigCanvasGroup.alpha = 0;
        }
    }
    
    IEnumerator TextShowing()
    {
        CanvasGroup canvasGroup = friendRelationSlider.gameObject.GetComponent<CanvasGroup>();
        while (true)
        {
            float nextA = Mathf.Lerp(canvasGroup.alpha, (showSlider ? 1 : 0), Time.deltaTime * 6);
            canvasGroup.alpha = nextA;
            
            yield return new WaitForEndOfFrame();
        }        
    }

    
    
    private void OnDestroy()
    {
        QuestManager.instance.OnStoryComplete -= Activate;
    }
}
