using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class FragmentCardPhoneCell : MonoBehaviour
{
    public CardInfo cardInfo;
    public GameObject particle;

    public bool locked = true;
    private static List<FragmentCardPhoneCell> cardPhoneCells = new List<FragmentCardPhoneCell>(100);
    private void Awake()
    {
        AddCardToArray();
    }

    private void OnEnable()
    {
        OpenCard();
    }

    private void AddCardToArray()
    {
        while (cardPhoneCells.Count <= cardInfo.cardID + 1)
        {
            cardPhoneCells.Add(null);
        }
        cardPhoneCells[cardInfo.cardID] = this;
    }

    public static FragmentCardPhoneCell GetCardCellByID(int id)
    {
        if (id < cardPhoneCells.Count)
            return cardPhoneCells[id];
        return null;
    }
    
    public void OpenCard()
    {
        if (!locked)
            GetComponent<Image>().sprite = cardInfo.cardSprite;
    }

    public void OpenCardFirstly()
    {
        locked = false;
        StartCoroutine(MakeCardOpaque());
        
    }

    IEnumerator MakeCardOpaque()
    {
        Image cardImage = gameObject.GetComponent<Image>();
        cardImage.sprite = cardInfo.cardSprite;
        Color loc = cardImage.color;
        loc.a = 0;
        cardImage.color = loc;
        
        LayerMask mask = LayerMask.NameToLayer("UI");
        
        
        yield return new WaitForSeconds(0.7f);

        GameObject instantiated = Instantiate(particle, this.transform);
        foreach (Transform trans in instantiated.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = mask;
        }
            
        float speed = 1;
        while (cardImage.color.a < 1)
        {
            Color nextColor = cardImage.color;
            nextColor.a = Mathf.Clamp01(nextColor.a + Time.deltaTime * speed);
            cardImage.color = nextColor;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(8f);
        
        Destroy(instantiated);
    }
}
