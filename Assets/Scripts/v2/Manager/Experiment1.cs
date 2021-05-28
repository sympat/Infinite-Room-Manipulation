using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Valve.VR;
using System.Text;
using System;

public class Experiment1 : Manager
{
    public enum DistanceType {
        Short = 0,
        Middle = 1,
        Long = 2
    }

    public GameObject portalPrefab;
    public GameObject centerPortalPrefab;
    public int totalTrial;
    public string experimentID;
    [TextArea]
    public string initialText, watchAroundText, targetText, viewDoorText, turnBehindText, selectionText, goToCenterText, endText;
    private Room currentRoom;
    private GameObject targetObj;
    private GameObject centerObj;
    // private float[] wallTranslateGain;
    private Dictionary<DistanceType, List<float>> wallTranslateGain2;
    private Vector2[] direction;
    private float[] wallDirection;
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

    private FiniteStateMachine<string, string> task;

    public override void Awake() {
        base.Awake();

        currentRoom = virtualEnvironment.CurrentRoom;

        // wallTranslateGain = new float[5];
        // wallTranslateGain[0] = 0.9f;
        // wallTranslateGain[1] = 0.95f;
        // wallTranslateGain[2] = 1.0f;
        // wallTranslateGain[3] = 1.05f;
        // wallTranslateGain[4] = 1.1f;

        wallTranslateGain2 = new Dictionary<DistanceType, List<float>>();
        wallTranslateGain2[DistanceType.Short] = new List<float>();
        wallTranslateGain2[DistanceType.Short].Add(0.9f);
        wallTranslateGain2[DistanceType.Short].Add(0.95f);
        wallTranslateGain2[DistanceType.Short].Add(1.0f);
        wallTranslateGain2[DistanceType.Short].Add(1.05f);
        wallTranslateGain2[DistanceType.Short].Add(1.1f);
        // wallTranslateGain2[DistanceType.Short].Add(0.98f);
        // wallTranslateGain2[DistanceType.Short].Add(0.99f);
        // wallTranslateGain2[DistanceType.Short].Add(1.0f);
        // wallTranslateGain2[DistanceType.Short].Add(1.01f);
        // wallTranslateGain2[DistanceType.Short].Add(1.02f);

        wallTranslateGain2[DistanceType.Middle] = new List<float>();
        wallTranslateGain2[DistanceType.Middle].Add(0.8f);
        wallTranslateGain2[DistanceType.Middle].Add(0.9f);
        wallTranslateGain2[DistanceType.Middle].Add(1.0f);
        wallTranslateGain2[DistanceType.Middle].Add(1.1f);
        wallTranslateGain2[DistanceType.Middle].Add(1.2f);
        // wallTranslateGain2[DistanceType.Middle].Add(0.95f);
        // wallTranslateGain2[DistanceType.Middle].Add(0.975f);
        // wallTranslateGain2[DistanceType.Middle].Add(1.0f);
        // wallTranslateGain2[DistanceType.Middle].Add(1.025f);
        // wallTranslateGain2[DistanceType.Middle].Add(1.05f);

        wallTranslateGain2[DistanceType.Long] = new List<float>();
        wallTranslateGain2[DistanceType.Long].Add(0.8f);
        wallTranslateGain2[DistanceType.Long].Add(0.9f);
        wallTranslateGain2[DistanceType.Long].Add(1.0f);
        wallTranslateGain2[DistanceType.Long].Add(1.1f);
        wallTranslateGain2[DistanceType.Long].Add(1.2f);
        // wallTranslateGain2[DistanceType.Long].Add(0.9f);
        // wallTranslateGain2[DistanceType.Long].Add(0.95f);
        // wallTranslateGain2[DistanceType.Long].Add(1.0f);
        // wallTranslateGain2[DistanceType.Long].Add(1.05f);
        // wallTranslateGain2[DistanceType.Long].Add(1.1f);

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

        // Add User event as input for task
        users.AddEnterEvent("Portal", "Untagged", () => task.Processing("onEnterPortal"));
        users.AddViewEvent("Door", "FacingDoor", () => task.Processing("onViewFacingDoor"));
        users.AddEnterEvent("CenterPortal", "Untagged", () => task.Processing("onEnterCenterPortal"));
        users.AddClickEvent("UI", "OKButton", () => task.Processing("onClickOK"));
        users.AddClickEvent("UI", "YesButton", () => task.Processing("onClickYes"));
        users.AddClickEvent("UI", "NoButton", () => task.Processing("onClickNo"));

        // Define task for experiment 1
        task = new FiniteStateMachine<string, string>("Initial", "Around", "Target", "Door_Step1", "Door_Step2", "Door_Step3", "Behind", "Selection", "Center", "End");
        task.AddStateStart("Initial", () => EnableOKUI(initialText))
        .AddTransition("Initial", "Around", "onClickOK", DisableUIandPointer)

        .AddStateStart("Around", () => EnableOKUI(watchAroundText), PrintCurrentExperiment)
        .AddTransition("Around", "onClickOK", DisableUIandPointer, () => WakeAfterSeconds(7.0f))
        .AddTransition("Around", "Target", "onAfterSeconds")

        .AddStateStart("Target", () => EnableOKUI(targetText), InitializeTarget)
        .AddTransition("Target", "onClickOK", DisableUIandPointer, GenerateTarget)
        .AddTransition("Target", "Door_Step1", "onEnterPortal", DestroyTarget)

        .AddStateStart("Door_Step1", () => EnableOKUI(viewDoorText))
        .AddTransition("Door_Step1", "Door_Step2", "onClickOK", DisableUIandPointer, ColoringFacingDoor)

        .AddTransition("Door_Step2", "Door_Step3", "onViewFacingDoor")

        .AddStateStart("Door_Step3", () => WakeAfterSeconds(3.0f))
        .AddTransition("Door_Step3", "Behind", "onAfterSeconds")

        .AddStateStart("Behind", () => EnableOKUI(turnBehindText), CleanDoors, MoveOppositeWall)
        .AddTransition("Behind", "onClickOK", DisableUIandPointer, () => WakeAfterSeconds(4.0f))
        .AddTransition("Behind", "Selection", "onAfterSeconds")

        .AddStateStart("Selection", () => EnableSelectionUI(selectionText))
        .AddTransition("Selection", "Center", "onClickYes", DisableUIandPointer, () => WriteAnswer(true))
        .AddTransition("Selection", "Center", "onClickNo", DisableUIandPointer, () => WriteAnswer(false))

        .AddStateStart("Center", () => EnableOKUI(goToCenterText))
        .AddTransition("Center", "onClickOK", DisableUIandPointer, GenerateCenterPoint)
        .AddTransition("Center", "onEnterCenterPortal", DestroyCenterPoint, UserCameraFadeOut, () => WakeAfterSeconds(3.0f))
        .AddTransition("Center", "onAfterSeconds", RestoreOriginWall, UserCameraFadeIn, RaiseEndCondition)
        .AddTransition("Center", "End", "onEnd")
        .AddTransition("Center", "Around", "onNotEnd")

        .AddStateStart("End", () => EnableEndUI(endText), PrintResult);

        // Debug for task process
        // task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        // task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        // task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        // task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        task.Begin("Initial");
    }

    public void WakeAfterSeconds(float time) {
        CoroutineManager.Instance.CallWaitForSeconds(time, () => task.Processing("onAfterSeconds"));
    }

    private int count = 0;

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

    public void DisableUIandPointer() {
        User user = users.GetActiveUser();

        user.ui.DisableUI();
        if(user.pointer != null) user.pointer.HidePointer(); 
    }

    public void EnableEndUI(string paragraphText) {
        User user = users.GetActiveUser();

        user.ui.PopUpParagraph(paragraphText); 
    }

    public void EnableOKUI(string paragraphText) {
        User user = users.GetActiveUser();

        user.ui.PopUpParagraph(paragraphText); 
        user.ui.PopUpOkButton();
        if(user.pointer != null) user.pointer.ShowPointer(); 
    }

    public void EnableSelectionUI(string paragraphText) {
        User user = users.GetActiveUser();

        user.ui.PopUpParagraph(paragraphText);

        float prob = UnityEngine.Random.Range(0f, 1.0f);

        if(prob < 0.5f) {
            user.ui.PopUpYes2Button();
            user.ui.PopUpNo2Button();
        }
        else {
            user.ui.PopUpYesButton();
            user.ui.PopUpNoButton();
        }

        if(user.pointer != null) user.pointer.ShowPointer();
    }

    public void UserCameraFadeOut() {
        User user = users.GetActiveUser();

        if(user._camera.GetComponent<SteamVR_Fade>() != null)
            user._camera.GetComponent<CameraFade>().FadeOutVR(1.0f);
        else
            user._camera.GetComponent<CameraFade>().FadeOut();
    }

    public void UserCameraFadeIn() {
        User user = users.GetActiveUser();

        if(user._camera.GetComponent<SteamVR_Fade>() != null)
            user._camera.GetComponent<CameraFade>().FadeInVR(1.0f);
        else
            user._camera.GetComponent<CameraFade>().FadeIn();
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
            task.Processing("onEnd");
        }
        else {
            task.Processing("onNotEnd");
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
                door.gameObject.tag = "FacingDoor";
                door.GetComponent<Outline>().enabled = true;
                
            }
        }
    }

    public void CleanDoors() {
        List<Door> doors = virtualEnvironment.GetConnectedDoors(virtualEnvironment.CurrentRoom);

        foreach(var door in doors) {
            door.gameObject.tag = "Untagged";
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
