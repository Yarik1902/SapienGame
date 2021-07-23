using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FragmentCardTimer : MonoBehaviour
{
    public static FragmentCardTimer instance;
    public CardInfo card;
    public float secondsWithCurrentFragmentCard;
    private bool cardComplete = false;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(instance.gameObject);
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        LoadCurrentCardTime();   
    }

    private void Update()
    {
        secondsWithCurrentFragmentCard += Time.deltaTime;
    }

    public string GetTimeHHMM()
    {
        TimeSpan t = TimeSpan.FromSeconds(secondsWithCurrentFragmentCard);
        string s = t.ToString(@"hh\:mm");
        return s;
    }

    private void OnDestroy()
    {
        SaveCurrentCardTime();
    }

    public void SaveCurrentCardTime()
    {
        if (!cardComplete)
        {
            string path = Application.dataPath + "/Scripts/CurrentFragmentCardTime.txt";
            string content = ((int) secondsWithCurrentFragmentCard).ToString() + "\n" + card.cardName;
            File.WriteAllText(path, content);
        }
    }

    public void CompleteCard()
    {
        Debug.Log("CardComplete");
        string path = Application.dataPath + "/Scripts/CurrentFragmentCardTime.txt";
        File.WriteAllText(path , "Complete: " + card.cardName);
        cardComplete = true;
    }
    public void LoadCurrentCardTime()
    {
        string path = Application.dataPath + "/Scripts/CurrentFragmentCardTime.txt";
        string[] lines = File.ReadAllLines(path);
        if (lines.Length == 2 && card.cardName == lines[1])
        {
            secondsWithCurrentFragmentCard = Int32.Parse(lines[0]);
        }
        else
        {
            secondsWithCurrentFragmentCard = 0;
            SaveCurrentCardTime();
        }
    }
    
}
