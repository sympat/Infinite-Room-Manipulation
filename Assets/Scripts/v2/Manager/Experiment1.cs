using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Valve.VR;
using System.Text;
using System;

public enum DistanceType {
    Short = 0,
    Middle = 1,
    Long = 2
}

public enum Exp1State {
    Initial,
    Around,
    Target,
    Door_Step1,
    Door_Step2,
    Door_Step3,
    Behind,
    Selection,
    Center,
    End,
}

// AddTaskEvent(UIBehaviour.Click, "Initial UI", "image_1", "button_0");
// AddTaskEvent(UIBehaviour.Click, "Watch Around UI", "image_1", "button_0");
// AddTaskEvent(UIBehaviour.Click, "Selection UI", "image_1", "button_0");
// AddTaskEvent(UIBehaviour.Click, "Selection UI", "image_1", "button_1");
// AddTaskEvent(Behaviour.Enter, "Portal");
// AddTaskEvent(Behaviour.Enter, "CenterPortal");
// AddTaskEvent(Behaviour.Watch, "FacingDoor");
// AddTaskEvent(TaskBehaviour.Wait);

public enum Exp1Input {
    ClickButton1,
    ClickButton2,
    ClickButton3,
    ClickButton4,
    ClickButton5,
    ClickButton6,
    ClickButton7,
    ClickButton8,
    EnterPortal,
    EnterCenterPortal,
    WatchFacingDoor,
    WaitAfterSeconds,
    End,
    NotEnd
}


public class Experiment1 : TaskBasedManager<Exp1State, Exp1Input>
{
    public GameObject portalPrefab;
    public GameObject centerPortalPrefab;
    // public BaseUI[] uiPrefabs;
    public int totalTrial;
    public string experimentID;
    // [TextArea]
    // public string initialText, watchAroundText, targetText, viewDoorText, turnBehindText, selectionText, goToCenterText, endText;

    private Room currentRoom;
    private GameObject targetObj;
    private GameObject centerObj;
    // private float[] wallTranslateGain;
    private Dictionary<DistanceType, List<float>> wallTranslateGain2;
    private Vector2[] direction;
    private float[] wallDirection;
    private int count = 0;
    private float grid;
    private Dictionary<DistanceType, List<List<bool>>> answer; // T - yes, F - no
    private int currentTrial = 0;
    private Vector2 targetPosition;
    private float translate;
    private int facingWall;
    private int gainIndex;
    private DistanceType distType;
    private Queue<DistanceType> distTypeQueue;
    private Dictionary<DistanceType, Queue<int>> gainQueue;

    protected override void GenerateTask() {
        currentRoom = virtualEnvironment.CurrentRoom;

        wallTranslateGain2 = new Dictionary<DistanceType, List<float>>();
        wallTranslateGain2[DistanceType.Short] = new List<float>();
        wallTranslateGain2[DistanceType.Short].Add(0.9f);
        wallTranslateGain2[DistanceType.Short].Add(0.95f);
        wallTranslateGain2[DistanceType.Short].Add(1.0f);
        wallTranslateGain2[DistanceType.Short].Add(1.05f);
        wallTranslateGain2[DistanceType.Short].Add(1.1f);

        wallTranslateGain2[DistanceType.Middle] = new List<float>();
        wallTranslateGain2[DistanceType.Middle].Add(0.8f);
        wallTranslateGain2[DistanceType.Middle].Add(0.9f);
        wallTranslateGain2[DistanceType.Middle].Add(1.0f);
        wallTranslateGain2[DistanceType.Middle].Add(1.1f);
        wallTranslateGain2[DistanceType.Middle].Add(1.2f);

        wallTranslateGain2[DistanceType.Long] = new List<float>();
        wallTranslateGain2[DistanceType.Long].Add(0.8f);
        wallTranslateGain2[DistanceType.Long].Add(0.9f);
        wallTranslateGain2[DistanceType.Long].Add(1.0f);
        wallTranslateGain2[DistanceType.Long].Add(1.1f);
        wallTranslateGain2[DistanceType.Long].Add(1.2f);

        direction = new Vector2[4];
        direction[0] = Vector2.up;
        direction[1] = Vector2.left;
        direction[2] = Vector2.down;
        direction[3] = Vector2.right;

        wallDirection = new float[4];
        wallDirection[0] = 1;
        wallDirection[1] = -1;
        wallDirection[2] = -1;
        wallDirection[3] = 1;

        grid = 0.5f;

        answer = new Dictionary<DistanceType, List<List<bool>>>();
        answer.Add(DistanceType.Short, new List<List<bool>>());
        answer.Add(DistanceType.Middle, new List<List<bool>>());
        answer.Add(DistanceType.Long, new List<List<bool>>());

        for(int i=0; i<totalTrial; i++) {
            answer[DistanceType.Short].Add(new List<bool>(new bool[5]));
            answer[DistanceType.Middle].Add(new List<bool>(new bool[5]));
            answer[DistanceType.Long].Add(new List<bool>(new bool[5]));
        }

        distTypeQueue = new Queue<DistanceType>();

        gainQueue = new Dictionary<DistanceType, Queue<int>>();
        gainQueue.Add(DistanceType.Short, new Queue<int>());
        gainQueue.Add(DistanceType.Middle, new Queue<int>());
        gainQueue.Add(DistanceType.Long, new Queue<int>());

        targetPosition = Vector2.zero;

        // Generate UI
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("Watch Around UI", uiInfo[1]);
        GenerateUI("Target UI", uiInfo[2]);
        GenerateUI("Watch Door UI", uiInfo[3]);
        GenerateUI("Turn Behind UI", uiInfo[4]);
        GenerateUI("Selection UI", uiInfo[5]);
        GenerateUI("Goto Center UI", uiInfo[6]);
        GenerateUI("End UI", uiInfo[7]);

        // Add User event as input for task
        AddTaskEvent(Exp1Input.ClickButton1, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(Exp1Input.ClickButton2, UIBehaviour.Click, "Watch Around UI", "image_1", "button_0");
        AddTaskEvent(Exp1Input.ClickButton3, UIBehaviour.Click, "Target UI", "image_1", "button_0");
        AddTaskEvent(Exp1Input.ClickButton4, UIBehaviour.Click, "Watch Door UI", "image_1", "button_0");
        AddTaskEvent(Exp1Input.ClickButton5, UIBehaviour.Click, "Turn Behind UI", "image_1", "button_0");
        AddTaskEvent(Exp1Input.ClickButton6, UIBehaviour.Click, "Selection UI", "image_1", "button_0");
        AddTaskEvent(Exp1Input.ClickButton7, UIBehaviour.Click, "Selection UI", "image_1", "button_1");
        AddTaskEvent(Exp1Input.ClickButton8, UIBehaviour.Click, "Goto Center UI", "image_1", "button_0");
        AddTaskEvent(Exp1Input.EnterPortal, Behaviour.Enter, "Portal");
        AddTaskEvent(Exp1Input.EnterCenterPortal, Behaviour.Enter, "CenterPortal");
        AddTaskEvent(Exp1Input.WatchFacingDoor, Behaviour.Watch, "FacingDoor");

        // Define task for experiment 1
        task.AddStateStart(Exp1State.Initial, () => EnableUI("Initial UI"))
        .AddTransition(Exp1State.Initial, Exp1State.Around, Exp1Input.ClickButton1, () => DisableUI("Initial UI"))

        .AddStateStart(Exp1State.Around, () => EnableUI("Watch Around UI"), PrintCurrentExperiment)
        .AddTransition(Exp1State.Around, Exp1Input.ClickButton2, () => DisableUI("Watch Around UI"), () => WaitForSeconds(7.0f))
        .AddTransition(Exp1State.Around, Exp1State.Target, Exp1Input.WaitAfterSeconds)

        .AddStateStart(Exp1State.Target, () => EnableUI("Target UI"), InitializeTarget)
        .AddTransition(Exp1State.Target, Exp1Input.ClickButton3, () => DisableUI("Target UI"), GenerateTarget)
        .AddTransition(Exp1State.Target, Exp1State.Door_Step1, Exp1Input.EnterPortal, DestroyTarget)

        .AddStateStart(Exp1State.Door_Step1, () => EnableUI("Watch Door UI"))
        .AddTransition(Exp1State.Door_Step1, Exp1State.Door_Step2, Exp1Input.ClickButton4, () => DisableUI("Watch Door UI"), ColoringFacingDoor)

        .AddTransition(Exp1State.Door_Step2, Exp1State.Door_Step3, Exp1Input.WatchFacingDoor)

        .AddStateStart(Exp1State.Door_Step3, () => WaitForSeconds(3.0f))
        .AddTransition(Exp1State.Door_Step3, Exp1State.Behind, Exp1Input.WaitAfterSeconds)

        .AddStateStart(Exp1State.Behind, () => EnableUI("Turn Behind UI"), CleanDoors, MoveOppositeWall)
        .AddTransition(Exp1State.Behind, Exp1Input.ClickButton5, () => DisableUI("Turn Behind UI"), () => WaitForSeconds(4.0f))
        .AddTransition(Exp1State.Behind, Exp1State.Selection, Exp1Input.WaitAfterSeconds)

        .AddStateStart(Exp1State.Selection, () => EnableUI("Selection UI"))
        .AddTransition(Exp1State.Selection, Exp1State.Center, Exp1Input.ClickButton6, () => DisableUI("Selection UI"), () => WriteAnswer(true))
        .AddTransition(Exp1State.Selection, Exp1State.Center, Exp1Input.ClickButton7, () => DisableUI("Selection UI"), () => WriteAnswer(false))

        .AddStateStart(Exp1State.Center, () => EnableUI("Goto Center UI"))
        .AddTransition(Exp1State.Center, Exp1Input.ClickButton8, () => DisableUI("Goto Center UI"), GenerateCenterPoint)
        .AddTransition(Exp1State.Center, Exp1Input.EnterCenterPortal, DestroyCenterPoint, UserCameraFadeOut, () => WaitForSeconds(3.0f))
        .AddTransition(Exp1State.Center, Exp1Input.WaitAfterSeconds, RestoreOriginWall, UserCameraFadeIn, RaiseEndCondition)
        .AddTransition(Exp1State.Center, Exp1State.End, Exp1Input.End)
        .AddTransition(Exp1State.Center, Exp1State.Around, Exp1Input.NotEnd)

        .AddStateStart(Exp1State.End, () => EnableUI("End UI"), PrintResult);

        // Debug for task process
        // task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        // task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        // task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        // task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        task.Begin(Exp1State.Initial);
    }

    public void WaitForSeconds(float time) {
        StartCoroutine(CallAfterSeconds(time));
    }

    public IEnumerator CallAfterSeconds(float time) {
        yield return new WaitForSeconds(time);

        task.Processing(Exp1Input.WaitAfterSeconds);
    }

    public void PrintCurrentExperiment() {
        count++;
        Debug.Log($"Event Time: {DateTime.Now.ToString()}\ncurrent count: {count}\ntotal count: {totalTrial * 3 * wallTranslateGain2[DistanceType.Short].Count}");
    }

    public void InitializeTarget() {
        if(AreGainQueuesEmpty())
            InitializeAppliedGain();
        
        if(IsDistanceQueueEmpty())
            InitializeDistance();
    }

    public void UserCameraFadeOut() {
        User user = users.GetActiveUser();

        if(user.Face.GetComponent<SteamVR_Fade>() != null)
            user.Face.GetComponent<CameraFade>().FadeOutVR(1.0f);
        else
            user.Face.GetComponent<CameraFade>().FadeOut();
    }

    public void UserCameraFadeIn() {
        User user = users.GetActiveUser();

        if(user.Face.GetComponent<SteamVR_Fade>() != null)
            user.Face.GetComponent<CameraFade>().FadeInVR(1.0f);
        else
            user.Face.GetComponent<CameraFade>().FadeIn();
    }

    // public void QuitGame()
    // {
    //     // save any game data here
    //     #if UNITY_EDITOR
    //         // Application.Quit() does not work in the editor so
    //         // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
    //         UnityEditor.EditorApplication.isPlaying = false;
    //     #else
    //         Application.Quit();
    //     #endif
    // }

    public void WriteResultInFile(DistanceType distType, int trial, int gainIdx, char character) {
        string directoryPath = "Assets/Resources/Experiment1_Result";
        string fileName = $"answer_{distType}_{experimentID}.txt";
        string filePath = directoryPath + "/" + fileName;
        int gainCount = wallTranslateGain2[DistanceType.Short].Count;

        if(!Directory.Exists(directoryPath)) 
            Directory.CreateDirectory(directoryPath);

        if(!File.Exists(filePath)) {
            List<string> lines = new List<string>();

            for(int i=0; i<totalTrial; i++) {
                string line = null;
                
                for(int j=0; j<gainCount; j++) 
                    line += "U";

                lines.Add(line);
            }

            File.WriteAllLines(filePath, lines);
        }

        string[] inputs = File.ReadAllLines(filePath);

        StringBuilder sb = new StringBuilder(inputs[trial]);

        sb[gainIdx] = character;
        inputs[trial] = sb.ToString();

        File.WriteAllLines(filePath, inputs);
    }

    public void PrintResult() {
        foreach(var key in answer.Keys) { // distance type
            string output = key.ToString() + "\n";

            for(int i=0; i<answer[key].Count; i++) { // trial
                for(int j=0; j<answer[key][i].Count; j++) { // gain
                    if(answer[key][i][j])
                        output += "Y";
                    else
                        output += "N";
                }
                output += "\n";
            }

            Debug.Log(output);
        }
    }

    public bool AreGainQueuesEmpty() {
        if(gainQueue[DistanceType.Short].Count == 0 
        && gainQueue[DistanceType.Middle].Count == 0 
        && gainQueue[DistanceType.Long].Count == 0)
            return true;
        
        return false;
    }

    public bool IsDistanceQueueEmpty() {
        if(distTypeQueue.Count == 0)
            return true;
        else
            return false;
    }

    public bool IsTrialEnded() {
        if(currentTrial == totalTrial)
            return true;
        else
            return false;
    }

    public void RaiseEndCondition() {
        if(AreGainQueuesEmpty() && IsTrialEnded()) {
            task.Processing(Exp1Input.End);
        }
        else {
            task.Processing(Exp1Input.NotEnd);
        }
    }

    public void GenerateTarget() {
        SelectNextTargetPosition();

        Vector2 denormalizedTargetPosition2D = virtualEnvironment.CurrentRoom.DenormalizePosition2D(targetPosition, Space.World);
        Vector3 targetInitPosition = Utility.CastVector2Dto3D(denormalizedTargetPosition2D);
        targetObj = Instantiate(portalPrefab, targetInitPosition, Quaternion.identity);
    }

    public void GenerateCenterPoint() {
        Vector3 targetInitPosition = Vector3.zero;
        centerObj = Instantiate(centerPortalPrefab, targetInitPosition, Quaternion.identity);
    }

    public void DestroyCenterPoint() {
        if(centerObj != null) Destroy(centerObj);
    }

    public void DestroyTarget() {
        if(targetObj != null) Destroy(targetObj);
    }

    public void MoveOppositeWall() {
        SelectWallTranslate();
        virtualEnvironment.MoveWall(currentRoom, (facingWall + 2) % 4, translate);
    }

    public void RestoreOriginWall() {
        virtualEnvironment.MoveWall(currentRoom, (facingWall + 2) % 4, -translate);
    }

    public void InitializeDistance() {
        distTypeQueue = new Queue<DistanceType>(Utility.sampleWithoutReplacement(3, 0, 3).Select(x => (DistanceType) x)); // IV 1
        if(targetPosition == Vector2.zero && distTypeQueue.Peek() == DistanceType.Middle) {
            distTypeQueue.Enqueue(distTypeQueue.Dequeue());
        }
    }

    public void InitializeAppliedGain() {
        currentTrial += 1;

        gainQueue[DistanceType.Short] = new Queue<int>(Utility.sampleWithoutReplacement(5, 0, 5)); // IV 2
        gainQueue[DistanceType.Middle] = new Queue<int>(Utility.sampleWithoutReplacement(5, 0, 5)); // IV 2
        gainQueue[DistanceType.Long] = new Queue<int>(Utility.sampleWithoutReplacement(5, 0, 5)); // IV 2
    }

    public void SelectNextTargetPosition() {
        distType = distTypeQueue.Dequeue();

        Vector2 nextTargetPosition;

        do {
            facingWall = Utility.sampleUniform(0, 4);
            nextTargetPosition = CalculateTargetPosition(facingWall, distType);
        } while(targetPosition == nextTargetPosition);

        targetPosition = nextTargetPosition;
        gainIndex = gainQueue[distType].Dequeue();
    }

    public void ColoringFacingDoor() {
        List<Door> doors = virtualEnvironment.GetConnectedDoors(virtualEnvironment.CurrentRoom);

        foreach(var door in doors) {
            if(door.GetThisRoomWrapper(virtualEnvironment.CurrentRoom).wall == facingWall) {
                door.gameObject.layer = LayerMask.NameToLayer("FacingDoor");
                door.GetComponent<Outline>().enabled = true;
                
            }
        }
    }

    public void CleanDoors() {
        List<Door> doors = virtualEnvironment.GetConnectedDoors(virtualEnvironment.CurrentRoom);

        foreach(var door in doors) {
            door.gameObject.layer = LayerMask.NameToLayer("Door");
            door.GetComponent<Outline>().enabled = false;
        }
    }

    public void SelectWallTranslate() {
        int oppositeWall = (facingWall + 2) % 4;
        Debug.Log($"{oppositeWall} {distType} {gainIndex} {wallTranslateGain2[distType][gainIndex]}");
        User user = users.GetActiveUser();
        
        float[] DistWalltoUser = new float[4];
        // DistWalltoUser[0] = (user.Position.y + 0.4f) - currentRoom.GetEdge2D(0, Space.World).y;
        // DistWalltoUser[1] = (user.Position.x - 0.4f) - currentRoom.GetEdge2D(1, Space.World).x;
        // DistWalltoUser[2] = (user.Position.y - 0.4f) - currentRoom.GetEdge2D(2, Space.World).y;
        // DistWalltoUser[3] = (user.Position.x + 0.4f) - currentRoom.GetEdge2D(3, Space.World).x;
        DistWalltoUser[0] = Mathf.Abs(currentRoom.GetEdge2D(0, Space.World).y - user.Position.y);
        DistWalltoUser[1] = Mathf.Abs(currentRoom.GetEdge2D(1, Space.World).x - user.Position.x);
        DistWalltoUser[2] = Mathf.Abs(currentRoom.GetEdge2D(2, Space.World).y - user.Position.y);
        DistWalltoUser[3] = Mathf.Abs(currentRoom.GetEdge2D(3, Space.World).x - user.Position.x);

        // float[] DistgainApplied = new float[4];
        // DistgainApplied[0] = (direction[0] * (wallTranslateGain[gainIndex] - 1) * currentRoom.Size).y;
        // DistgainApplied[1] = (direction[1] * (wallTranslateGain[gainIndex] - 1) * currentRoom.Size).x;
        // DistgainApplied[2] = (direction[2] * (wallTranslateGain[gainIndex] - 1) * currentRoom.Size).y;
        // DistgainApplied[3] = (direction[3] * (wallTranslateGain[gainIndex] - 1) * currentRoom.Size).x;

        translate = wallDirection[oppositeWall] * (wallTranslateGain2[distType][gainIndex] - 1) * DistWalltoUser[oppositeWall];
        // if(DistWalltoUser[oppositeWall] * DistgainApplied[oppositeWall] > 0) {
        //     translate = Mathf.Sign(DistgainApplied[oppositeWall]) * Mathf.Min(Mathf.Abs(DistWalltoUser[oppositeWall]), Mathf.Abs(DistgainApplied[oppositeWall]));
        // }
        // else {
        //     translate = DistgainApplied[oppositeWall];
        // }
    }

    public void WriteAnswer(bool userAnswer) {
        answer[distType][currentTrial-1][gainIndex] = userAnswer;

        string correctAnswer = null;
        if(gainIndex < 2) {
            correctAnswer = "N";
        }
        else if(gainIndex == 2) {
            correctAnswer = "Not Move";
        }
        else {
            correctAnswer = "Y";
        }

        if(userAnswer) {
            Debug.Log($"correct answer: {correctAnswer}, user answer: Y");
            WriteResultInFile(distType, currentTrial-1, gainIndex, 'Y');

        }
        else {
            Debug.Log($"correct answer: {correctAnswer}, user answer: N");
            WriteResultInFile(distType, currentTrial-1, gainIndex, 'N');
        }
    }

    public Vector2 CalculateTargetPosition(int facingWall, DistanceType distanceFromBehindWall) {
        Vector2 result = (int)(distanceFromBehindWall - 1) * grid * direction[facingWall];
        return result;
    }
}
