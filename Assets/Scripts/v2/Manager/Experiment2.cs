using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Exp2State {
    Initial,
    GotoNext,
    Collecting,
    End
}

public enum Exp2Input {
    ClickButton0,
    ClickButton1,
    ClickButton2,
    ReleaseCoin,
    GenerateCoin,
    CollectionDone,
    EnterRoom,
    End,
    NotEnd
}

public class Experiment2 : TaskBasedManager<Exp2State, Exp2Input>
{
    public float ExperimentTimeDuration;
    public int totalCoin;
    public GameObject coinObjPrefab;
    private GameObject coinObj;

    [HideInInspector]
    public bool isLocomotionDone = false;
    private bool isExperimentDone = false;

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
        AddTaskEvent(Exp2Input.ReleaseCoin, Behaviour.Release, "Coin");
        AddTaskEvent(Exp2Input.EnterRoom, Behaviour.CompletelyEnter, "NextRoom");

        // Define task
        task.AddStateStart(Exp2State.Initial, () => EnableUI("Initial UI"))
        .AddTransition(Exp2State.Initial, Exp2State.GotoNext, Exp2Input.ClickButton0, () => DisableUI("Initial UI"), () => CallExperimentDone(ExperimentTimeDuration))

        .AddStateStart(Exp2State.GotoNext, () => EnableUI("Goto Next UI"))
        .AddTransition(Exp2State.GotoNext, Exp2Input.ClickButton1, () => DisableUI("Goto Next UI"))
        .AddTransition(Exp2State.GotoNext, Exp2State.Collecting, Exp2Input.EnterRoom, GenerateCoin, CheckCollectDone)

        .AddTransition(Exp2State.Collecting, Exp2Input.ReleaseCoin, DestroyCoin, () => CallInputAfterSeconds(2.0f, Exp2Input.GenerateCoin))
        .AddTransition(Exp2State.Collecting, Exp2Input.GenerateCoin, GenerateCoin)
        .AddTransition(Exp2State.Collecting, Exp2Input.CollectionDone, DestroyCoin, RaiseEndCondition)
        .AddTransition(Exp2State.Collecting, Exp2State.End, Exp2Input.End)
        .AddTransition(Exp2State.Collecting, Exp2State.GotoNext, Exp2Input.NotEnd)

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

    public void GenerateCoin() {
        // Debug.Log("GenerateCoin");
        Vector2 localPosition = virtualEnvironment.CurrentRoom.SamplingPosition(0.3f, Space.World);
        // Debug.Log($"globalPosition {localPosition}");
        // Debug.Log(Utility.CastVector2Dto3D(localPosition, 1.5f));
        coinObj = Instantiate(coinObjPrefab, virtualEnvironment.transform);
        coinObj.transform.position = Utility.CastVector2Dto3D(localPosition, 1.2f);
    }

    public void DestroyCoin() {
        AudioSource.PlayClipAtPoint(SoundSetting.Instance.coinCollectSound, coinObj.transform.position);
        Destroy(coinObj);
        collectingCount++;
    }

    public virtual void CheckCollectDone() {
        StartCoroutine(_CheckCollectDone());
    }

    public IEnumerator _CheckCollectDone() {
        yield return new WaitUntil(() => (collectingCount >= totalCoin && isLocomotionDone));
        DestroyCoin();
        collectingCount = 0;

        task.Processing(Exp2Input.CollectionDone);
    }

    public void RaiseEndCondition() {
        if(isExperimentDone) {
            task.Processing(Exp2Input.End);
        }
        else {
            task.Processing(Exp2Input.NotEnd);
        }
    }

}
