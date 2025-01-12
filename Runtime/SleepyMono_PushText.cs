using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SleepyMono_PushText : MonoBehaviour
{


    public string m_text= "A0 a0 B0 b0 C0 c0 A1 a1 B1 b1 C1 c1 D1 d1 E1 e1 F1 f1 A5 B5 C5 D5 E5 F5";

    public float m_timeBetweenPushes = 1.0f;

    public UnityEvent<string> m_onTextPush;
    
    [ContextMenu("Trigger")]
    public void Trigger() { 
    StartCoroutine(TriggerCoroutine());
    }

    public IEnumerator TriggerCoroutine() {

        string [] tokens = m_text.Split(" ");
        foreach (var token in tokens)
        {
            Debug.Log("Pushing: " + token);
            m_onTextPush.Invoke(token);
            yield return new WaitForSeconds(m_timeBetweenPushes);
        }
    }
    
}
