using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public Text timerText;
    public bool updateTimer = true;
    private void Awake()
    {
        StartCoroutine(TimerUpdater());
        if (InGameTimer.instance == null)
            Debug.Log($"<color=red><size=16> In game timer not found</size></color>");
    }

    private void Update()
    {
        //Debug.Log(InGameTimer.instance.GetTimeHHMM());
    }

    IEnumerator TimerUpdater()
    {
        while (updateTimer)
        {
            if (InGameTimer.instance != null)
            {
                timerText.text = InGameTimer.instance.GetTimeHHMM();
            }
            yield return new WaitForSecondsRealtime(1f);
        }
    }
}
