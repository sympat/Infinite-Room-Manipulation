using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Exp2State {
    Initial,
    NextRoom,
    Portal,
    Coin,
    // Collecting,
    End
}

public enum Exp2Input {
    ClickButton0,
    ClickButton1,
    ClickButton2,
    EnterPortal,
    CollectCoin,
    GenerateCoin,
    CollectionDone,
    EnterRoom,
    TaskEnd,
    SubTaskEnd,
    SubTaskNotEnd
}

public class Experiment2 : TaskBasedManager<Exp2State, Exp2Input>
{
    public float ExperimentTimeDuration;
    public int totalCoin;
    public GameObject coinObjPrefab;
    public GameObject portalObjPrefab;
    [HideInInspector]
    public bool isLocomotionDone = false;
    
    private GameObject coinObj, portalObj;
    private bool isExperimentDone = false, isSubTaskDone = false;
    private int collectingCount = 0;

    // Start is called before the first frame update
    protected override void GenerateTask()
    {
        // Generate UI
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("Goto Next UI", uiInfo[1]);
        GenerateUI("End UI", uiInfo[2]);

        // Add events as task inputs
        AddTaskEvent(Exp2Input.ClickButton0, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(Exp2Input.ClickButton1, UIBehaviour.Click, "Goto Next UI", "image_1", "button_0");
        AddTaskEvent(Exp2Input.CollectCoin, Behaviour.Release, "Coin");
        AddTaskEvent(Exp2Input.EnterPortal, Behaviour.Enter, "Portal");
        AddTaskEvent(Exp2Input.EnterRoom, Behaviour.CompletelyEnter, "NextRoom");

        // Define task
        task.AddStateStart(Exp2State.Initial, () => EnableUI("Initial UI"))
        .AddTransition(Exp2State.Initial, Exp2State.NextRoom, Exp2Input.ClickButton0, () => DisableUI("Initial UI"), () => CallExperimentDone(ExperimentTimeDuration))

        .AddStateStart(Exp2State.NextRoom, () => EnableUI("Goto Next UI"))
        .AddTransition(Exp2State.NextRoom, Exp2Input.ClickButton1, () => DisableUI("Goto Next UI"))
        .AddTransition(Exp2State.NextRoom, Exp2State.Portal, Exp2Input.EnterRoom, CheckSubTaskDone, () => ToggleDoors(false))

        .AddStateStart(Exp2State.Portal, GeneratePortal)
        .AddTransition(Exp2State.Portal, Exp2State.Coin, Exp2Input.EnterPortal, DestroyPortal)

        .AddStateStart(Exp2State.Coin, GenerateCoin)
        .AddTransition(Exp2State.Coin, Exp2Input.CollectCoin, DestroyCoin, RaiseTaskEnd, RaiseSubTaskEnd)
        .AddTransition(Exp2State.Coin, Exp2State.Portal, Exp2Input.SubTaskNotEnd)
        .AddTransition(Exp2State.Coin, Exp2State.NextRoom, Exp2Input.SubTaskEnd, () => Destroy(coinObj), () => ToggleDoors(true))
        .AddTransition(Exp2State.Coin, Exp2State.End, Exp2Input.TaskEnd)

        .AddStateStart(Exp2State.End, () => EnableUI("End UI"));

        // Debug for task process
        // task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        // task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        // task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        // task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // Start task 
        task.Begin(Exp2State.Initial);
    }

    public void CallExperimentDone(float time) {
        CoroutineManager.Instance.CallWaitForSeconds(time, () => isExperimentDone = true);
    }

    public void GeneratePortal() {
        User user = users.GetActiveUser();
        Vector2 portalPos = user.Body.Position;
        do {
            portalPos = virtualEnvironment.CurrentRoom.SamplingPosition(0.3f, Space.World);
        } while ((portalPos - user.Body.Position).magnitude < 0.7f);

        portalObj = Instantiate(portalObjPrefab, virtualEnvironment.transform);
        portalObj.transform.position = Utility.CastVector2Dto3D(portalPos);
    }

    public void DestroyPortal() {
        Destroy(portalObj);
    }

    public void GenerateCoin() {
        User user = users.GetActiveUser();
        Vector2 coinPos = user.Body.Position;
        do {
            coinPos = virtualEnvironment.CurrentRoom.SamplingPosition(0.3f, Space.World);
        } while ((coinPos - user.Body.Position).magnitude < 0.3f);

        coinObj = Instantiate(coinObjPrefab, virtualEnvironment.transform);
        coinObj.transform.position = Utility.CastVector2Dto3D(coinPos, 1.2f);
    }

    public void ToggleDoors(bool enabled) {
        virtualEnvironment.ToggleConnectedDoors(virtualEnvironment.CurrentRoom, enabled);
    }

    public void DestroyCoin() {
        AudioSource.PlayClipAtPoint(SoundSetting.Instance.coinCollectSound, coinObj.transform.position);
        Destroy(coinObj);
        collectingCount++;
    }

    public virtual void CheckSubTaskDone() {
        isSubTaskDone = false;
        StartCoroutine(_CheckCollectDone());
    }

    public IEnumerator _CheckCollectDone() {
        yield return new WaitUntil(() => (collectingCount >= totalCoin && isLocomotionDone));
        collectingCount = 0;
        isLocomotionDone = false;
        isSubTaskDone = true;
    }

    public void RaiseSubTaskEnd() {
        if(isSubTaskDone) {
            task.Processing(Exp2Input.SubTaskEnd);
        }
        else {
            task.Processing(Exp2Input.SubTaskNotEnd);
        }
    }

    public void RaiseTaskEnd() {
        if(isExperimentDone) {
            task.Processing(Exp2Input.TaskEnd);
        }
    }

}
