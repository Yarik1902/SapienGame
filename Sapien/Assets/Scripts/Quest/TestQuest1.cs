using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestQuest1 : StoryQuest
{
    private void Update()
    {
        if (activated)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                QuestComplete();
            }
        }
    }
}
