using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class TaskBasedManager<T1, T2> : MonoBehaviour 
where T1 : Enum
where T2 : Enum
{
    public UIContainer[] uiInfo;
    [HideInInspector]
    public Manager manager;

    public Users users {
        get { return manager.users; }
    }

    public VirtualEnvironment virtualEnvironment {
        get { return manager.virtualEnvironment; }
    }

    public RealSpace realSpace {
        get { return manager.realSpace; }
    }

    protected FiniteStateMachine<T1, T2> task = new FiniteStateMachine<T1, T2>();

    public FiniteStateMachine<T1, T2> Task {
        get { return task; }
    }

    protected void Start() {
        manager = GetComponent<Manager>();

        GenerateTask();
    }

    protected virtual void GenerateTask() {

    }

    protected void GenerateUI(string name, UIContainer uiInfo) {
        User user = users.GetActiveUser();
        if(uiInfo.attachToUser) {
            uiInfo.parent = user.Face.transform;
        }

        UIManager.Instance.GenerateUI(name, uiInfo);
    }

    protected void EnableUI(string name, bool useLocal = false) {
        User user = users.GetActiveUser();

        UIManager.Instance.ToggleUICanvas(name, true, user, useLocal);
        user.ToggleHandPointer(true); 
    }

    protected void DisableUI(string name, bool useLocal = false, bool disableHandPointer = true) {
        User user = users.GetActiveUser();

        UIManager.Instance.ToggleUICanvas(name, false, user, useLocal); 
        if(disableHandPointer) user.ToggleHandPointer(false);
    }

    protected void AddTaskEvent(T2 input, UIBehaviour behaviour, string name, params string[] childName) {
        User user = users.GetActiveUser();


        UIManager.Instance.AddEvent(behaviour, name, () => task.Processing(input), childName);
    }

    protected void AddTaskEvent(T2 input, Behaviour behaviour, string name) {
        User user = users.GetActiveUser();

        user.AddEvent(behaviour, name, (_) => task.Processing(input));
    }

    public void CallInputAfterSeconds(float time, T2 input) {
        StartCoroutine(_CallInputAfterSeconds(time, input));
    }

    public IEnumerator _CallInputAfterSeconds(float time, T2 input) {
        yield return new WaitForSeconds(time);
        task.Processing(input);
    }
}

