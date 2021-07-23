using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InGameTimer : MonoBehaviour
{
    public static InGameTimer instance;
    public float secondsTodayInGame;
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
        LoadTodayTime();   
    }

    private void Update()
    {
        secondsTodayInGame += Time.deltaTime;
    }

    public string GetTimeHHMM()
    {
        TimeSpan t = TimeSpan.FromSeconds(secondsTodayInGame);
        string s = t.ToString(@"hh\:mm");
        return s;
    }

    private void OnDestroy()
    {
        SaveTodayTime();
    }

    public void SaveTodayTime()
    {
        string path = Application.dataPath + "/Scripts/TimeAndDate.txt";
        string content = ((int)secondsTodayInGame).ToString() + "\n" + System.DateTime.UtcNow.ToString("MM-dd-yyyy");
        File.WriteAllText(path , content);
    }

    public void LoadTodayTime()
    {
        string path = Application.dataPath + "/Scripts/TimeAndDate.txt";
        string[] lines = File.ReadAllLines(path);
        if (lines.Length == 2 && lines[1] == System.DateTime.UtcNow.ToString("MM-dd-yyyy"))
        {
            secondsTodayInGame = Int32.Parse(lines[0]);
        }
        else
        {
            secondsTodayInGame = 0;
            SaveTodayTime();
            
        }
    }
    
}
