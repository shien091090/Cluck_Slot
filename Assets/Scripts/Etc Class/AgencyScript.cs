//代理呼叫腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgencyScript : MonoBehaviour
{
    //撥放音效
    public void PlaySound(string clipName)
    {
        AudioManagerScript.Instance.PlayAudioClip(clipName);
    }
}
