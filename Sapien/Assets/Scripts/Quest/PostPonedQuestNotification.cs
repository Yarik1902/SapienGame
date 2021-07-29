using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostPonedQuestNotification : MonoBehaviour
{
    public void StartTimer(float duration , PhoneManager _phoneManager)
    {
        StartCoroutine(cr_StartTimer(duration , _phoneManager));
    }

    IEnumerator cr_StartTimer(float duration, PhoneManager _phoneManager)
    {
        yield return new WaitForSecondsRealtime(duration);
        _phoneManager.OnNotificationOpener();
        Destroy(this.gameObject);
    }
}
