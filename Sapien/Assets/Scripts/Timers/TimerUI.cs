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
    public Image[] cells;

    public Sprite[] staticNums;
    public Sprite[] dynamicNums;
    
    private int[] numbers = new int[4]; 
    private void Awake()
    {
        StartCoroutine(TimerUpdater());
        if (InGameTimer.instance == null)
            Debug.Log($"<color=red><size=16> In game timer not found</size></color>");
    }

    public void ParseTimeToArray(string time , ref int[] data)
    {
        data[0] = Int32.Parse(time[0].ToString());
        data[1] = Int32.Parse(time[1].ToString());
        data[2] = Int32.Parse(time[3].ToString());
        data[3] = Int32.Parse(time[4].ToString());
    }

    private void Update()
    {
        //Debug.Log(InGameTimer.instance.GetTimeHHMM());
    }

    private void Start()
    {
        
    }

    public void test()
    {
        int[] newTime = new int[4];
        ParseTimeToArray("12:34" , ref newTime);

        StartCoroutine(updateCell(0, newTime[0]));
        StartCoroutine(updateCell(1, newTime[1]));
        StartCoroutine(updateCell(2, newTime[2]));
        StartCoroutine(updateCell(3, newTime[3]));
    }

    IEnumerator TimerUpdater()
    {
        while (updateTimer)
        {
            if (InGameTimer.instance != null)
            {
                //ParseTimeToArray(InGameTimer.instance.GetTimeHHMM() , ref numbers);
                UpdateTimer();
                
                timerText.text = InGameTimer.instance.GetTimeHHMM();
            }
            yield return new WaitForSecondsRealtime(10f);
        }
    }

    public void UpdateTimer()
    {
        int[] newTime = new int[4];
        ParseTimeToArray(InGameTimer.instance.GetTimeHHMM() , ref newTime);

        StartCoroutine(updateCell(0, newTime[0]));
        StartCoroutine(updateCell(1, newTime[1]));
        StartCoroutine(updateCell(2, newTime[2]));
        StartCoroutine(updateCell(3, newTime[3]));
    }

    IEnumerator updateCell(int cellNum, int toNum)
    {
        if (numbers[cellNum] != toNum)
        {
            float elapsedTime = 0;
            cells[cellNum].sprite = dynamicNums[numbers[cellNum]];
            while (elapsedTime < 0.15f)
            {
                Color nextC = cells[cellNum].color;
                nextC.a = Mathf.Lerp(nextC.a, 0.5f, 3 * Time.deltaTime);
                cells[cellNum].color = nextC;
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            elapsedTime = 0;
            cells[cellNum].sprite = dynamicNums[toNum];
            
            while (elapsedTime < 0.15f)
            {
                Color nextC = cells[cellNum].color;
                nextC.a = Mathf.Lerp(nextC.a, 0.5f, 3 * Time.deltaTime);
                cells[cellNum].color = nextC;
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            elapsedTime = 0;
            cells[cellNum].sprite = staticNums[toNum];
            
            while (elapsedTime < 0.15f)
            {
                Color nextC = cells[cellNum].color;
                nextC.a = Mathf.Lerp(nextC.a, 1f, 3 * Time.deltaTime);
                cells[cellNum].color = nextC;
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            Color nextColor = cells[cellNum].color;
            nextColor.a = 1;
            cells[cellNum].color = nextColor;

            numbers[cellNum] = toNum;
        }
    }
    
}
