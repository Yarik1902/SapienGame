using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestQuest2 : StoryQuest
{

    public void LoadScene(int idx)
    {
        SceneManager.LoadScene(idx);
    }

}
