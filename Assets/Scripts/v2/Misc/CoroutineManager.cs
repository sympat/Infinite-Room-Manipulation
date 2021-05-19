using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoroutineManager : Singleton<CoroutineManager>
{
    private static bool m_Lock = false;

    public void CallWaitForOneFrame(UnityAction call) {
        StartCoroutine(DoCallWaitForOneFrame(call));
    }

    public void CallWaitForSeconds(float seconds, UnityAction call, bool useLock = false) {
        StartCoroutine(DoCallWaitForSeconds(seconds, call, useLock));
    }

    private IEnumerator DoCallWaitForOneFrame(UnityAction call) {
        yield return null;
        call();
    }

    private IEnumerator DoCallWaitForSeconds(float seconds, UnityAction call, bool useLock = false) {
        yield return new WaitForSeconds(seconds);
        call();
    }
}
