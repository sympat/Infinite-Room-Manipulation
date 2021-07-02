using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Exp2State {
    Initial,
    GoToNextRoom,
    EnterNextRoom,
    Portal,
    WatchDoor,
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
    // GenerateCoin,
    WatchColoringDoor,
    EndWatchColoringDoor,
    // CollectionDone,
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
        GenerateUI("Room Task UI", uiInfo[2]);
        GenerateUI("End UI", uiInfo[3]);

        // Add events as task inputs
        AddTaskEvent(Exp2Input.ClickButton0, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(Exp2Input.ClickButton1, UIBehaviour.Click, "Goto Next UI", "image_1", "button_0");
        AddTaskEvent(Exp2Input.ClickButton2, UIBehaviour.Click, "Room Task UI", "image_1", "button_0");
        AddTaskEvent(Exp2Input.CollectCoin, Behaviour.Release, "Coin");
        AddTaskEvent(Exp2Input.EnterPortal, Behaviour.Enter, "Portal");
        AddTaskEvent(Exp2Input.EnterRoom, Behaviour.CompletelyEnter, "NextRoom");
        AddTaskEvent(Exp2Input.WatchColoringDoor, Behaviour.Watch, "FacingDoor");

        // Define task
        task.AddStateStart(Exp2State.Initial, () => EnableUI("Initial UI"))
        .AddTransition(Exp2State.Initial, Exp2State.GoToNextRoom, Exp2Input.ClickButton0, () => DisableUI("Initial UI"), () => CallExperimentDone(ExperimentTimeDuration))

        .AddStateStart(Exp2State.GoToNextRoom, () => EnableUI("Goto Next UI"))
        .AddTransition(Exp2State.GoToNextRoom, Exp2Input.ClickButton1, () => DisableUI("Goto Next UI"))
        .AddTransition(Exp2State.GoToNextRoom, Exp2State.EnterNextRoom, Exp2Input.EnterRoom)

        .AddStateStart(Exp2State.EnterNextRoom, () => EnableUI("Room Task UI"))
        .AddTransition(Exp2State.EnterNextRoom, Exp2State.Portal, Exp2Input.ClickButton2, () => DisableUI("Room Task UI"), CheckSubTaskDone, () => ToggleDoors(false))

        .AddStateStart(Exp2State.Portal, GeneratePortal)
        .AddTransition(Exp2State.Portal, Exp2State.WatchDoor, Exp2Input.EnterPortal, DestroyPortal)
         //.AddTransition(Exp2State.Portal, Exp2State.Coin, Exp2Input.EnterPortal, DestroyPortal)

        .AddStateStart(Exp2State.WatchDoor, ColoringDoor)
        .AddTransition(Exp2State.WatchDoor, Exp2Input.WatchColoringDoor, () => CallInputAfterSeconds(1.0f, Exp2Input.EndWatchColoringDoor))
        .AddTransition(Exp2State.WatchDoor, Exp2State.Coin, Exp2Input.EndWatchColoringDoor, DecoloringDoor)

        .AddStateStart(Exp2State.Coin, GenerateCoin)
        .AddTransition(Exp2State.Coin, Exp2Input.CollectCoin, DestroyCoin, CheckEndCondition)
        .AddTransition(Exp2State.Coin, Exp2State.Portal, Exp2Input.SubTaskNotEnd)
        .AddTransition(Exp2State.Coin, Exp2State.GoToNextRoom, Exp2Input.SubTaskEnd, () => Destroy(coinObj), () => ToggleDoors(true))
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

    private Queue<int> locoQueue;
    public void ColoringDoor() {
        List<Door> doors = virtualEnvironment.GetConnectedDoors(virtualEnvironment.CurrentRoom);

        float prob = Utility.sampleUniform(0f, 1.0f);
        if(prob < 0.5f) {
            task.Processing(Exp2Input.WatchColoringDoor);
            return;
        }

        if(locoQueue == null || locoQueue.Count == 0) locoQueue = new Queue<int>(Utility.sampleWithoutReplacement(doors.Count, 0, doors.Count)); // IV 1
          
        int doorIndex = locoQueue.Dequeue();

        if(doorIndex < 0 || doorIndex >= doors.Count) {
            Debug.Log($"Invalid Door index: {doorIndex}");
            task.Processing(Exp2Input.WatchColoringDoor);
            return;
        }

        // doors[doorIndex].GetComponent<Outline>().enabled = true;
        doors[doorIndex].gameObject.layer = LayerMask.NameToLayer("FacingDoor");
    }

    public void DecoloringDoor() {
        List<Door> doors = virtualEnvironment.GetConnectedDoors(virtualEnvironment.CurrentRoom);

        foreach(var door in doors) {
            door.gameObject.layer = LayerMask.NameToLayer("Door");
            // door.GetComponent<Outline>().enabled = false;
        }
    }


    public void GeneratePortal() {
        CoroutineManager.Instance.CallWaitForSeconds(0.0f, _GeneratePortal);
    }

    public void CallExperimentDone(float time) {
        CoroutineManager.Instance.CallWaitForSeconds(time, () => isExperimentDone = true);
    }

    private void _GeneratePortal() {
        User user = users.GetActiveUser();
        Vector2 portalPos = user.Body.Position;

        portalPos = virtualEnvironment.CurrentRoom.SamplingPosition(0.3f, Space.World);
        do {
            portalPos = virtualEnvironment.CurrentRoom.SamplingPosition(0.3f, Space.World);
            // Debug.Log(portalPos);
        } while ((portalPos - user.Body.Position).magnitude < 0.4f);

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

    // public void RaiseSubTaskEnd() {
    //     if(isSubTaskDone) {
    //         task.Processing(Exp2Input.SubTaskEnd);
    //     }
    //     else {
    //         task.Processing(Exp2Input.SubTaskNotEnd);
    //     }
    // }

    public void CheckEndCondition() {
        if(isSubTaskDone) {
            if(isExperimentDone) 
                task.Processing(Exp2Input.TaskEnd);
            else 
                task.Processing(Exp2Input.SubTaskEnd);
        }
        else {
            task.Processing(Exp2Input.SubTaskNotEnd);
        }
    }

    // public void RaiseTaskEnd() {
    //     if(isSubTaskDone && isExperimentDone) {
    //         task.Processing(Exp2Input.TaskEnd);
    //     }
    // }

}
