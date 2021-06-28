using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeleportTaskState {
    Initial,
    Step0,
    Step1,
    End
}

public enum TeleportTaskInput {
    ClickButton0,
    ClickButton1,
    ClickButton2,
    CompleteTeleport,
    EnterPortal,
    CompletePortal,
    EnterRoom,
    CollectCoin
}


public class TeleportTask : TaskBasedManager<TeleportTaskState, TeleportTaskInput>
{
    public GameObject portalObjPrefab;
    private GameObject portalObj;
    private int portalCount = 0;

    protected override void GenerateTask() {
        // Generate UI
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("Teleport UI", uiInfo[1]);
        GenerateUI("Portal UI", uiInfo[2]);
        GenerateUI("End UI", uiInfo[3]);

        // Add task event
        AddTaskEvent(TeleportTaskInput.ClickButton0, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(TeleportTaskInput.ClickButton1, UIBehaviour.Click, "Teleport UI", "image_1", "button_0");
        AddTaskEvent(TeleportTaskInput.ClickButton2, UIBehaviour.Click, "Portal UI", "image_1", "button_0");
        AddTaskEvent(TeleportTaskInput.EnterPortal, Behaviour.Enter, "Portal");

        // Define task for pre-experiment 2
        task.AddStateStart(TeleportTaskState.Initial, () => EnableUI("Initial UI"))
        .AddTransition(TeleportTaskState.Initial, TeleportTaskState.Step0, TeleportTaskInput.ClickButton0, () => DisableUI("Initial UI"))

        .AddStateStart(TeleportTaskState.Step0, () => EnableUI("Teleport UI"))
        .AddTransition(TeleportTaskState.Step0, TeleportTaskInput.ClickButton1, () => DisableUI("Teleport UI"), () => CallInputAfterSeconds(10.0f, TeleportTaskInput.CompleteTeleport))
        .AddTransition(TeleportTaskState.Step0, TeleportTaskState.Step1, TeleportTaskInput.CompleteTeleport)

        .AddStateStart(TeleportTaskState.Step1, () => EnableUI("Portal UI"))
        .AddTransition(TeleportTaskState.Step1, TeleportTaskInput.ClickButton2, () => DisableUI("Portal UI"), CheckPortalDone, GeneratePortal)
        .AddTransition(TeleportTaskState.Step1, TeleportTaskInput.EnterPortal, DestroyPortal, GeneratePortal)
        .AddTransition(TeleportTaskState.Step1, TeleportTaskState.End, TeleportTaskInput.CompletePortal, DestroyPortal)

        .AddStateStart(TeleportTaskState.End, () => EnableUI("End UI"));

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // Start task 
        task.Begin(TeleportTaskState.Initial);

    }

    public virtual void CheckPortalDone() {
        StartCoroutine(_CheckPortalDone());
    }

    public IEnumerator _CheckPortalDone() {
        yield return new WaitUntil(() => (portalCount >= 5));
        
        task.Processing(TeleportTaskInput.CompletePortal);
    }

    public void GeneratePortal() {
        User user = users.GetActiveUser();
        Vector2 portalPos = user.Position;
        do {
            portalPos = virtualEnvironment.CurrentRoom.SamplingPosition(0.3f, Space.World);
        } while ((portalPos - user.Position).magnitude < 0.7f);

        portalObj = Instantiate(portalObjPrefab, virtualEnvironment.transform);
        portalObj.transform.position = Utility.CastVector2Dto3D(portalPos);
    }

    public void DestroyPortal() {
        if(portalObj != null) { 
            Destroy(portalObj);
            portalCount +=1;
        }
    }
}
