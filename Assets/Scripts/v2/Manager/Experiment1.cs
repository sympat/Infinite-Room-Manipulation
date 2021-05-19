using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Valve.VR;

public class Experiment1 : Manager
{
    public enum DistanceType {
        Short = 0,
        Middle = 1,
        Long = 2
    }

    public Experiement1State currentState {
        get { return _currentState; }
        set { 
            _currentState = value;
            _currentState.onDefault();
        }
    }

    public GameObject portalPrefab;
    public GameObject centerPortalPrefab;
    // public GameObject turnTargetObjectPrefab;
    public int totalTrial = 1;
    [TextArea]
    public string initialText, watchAroundText, targetText, viewDoorText, turnBehindText, selectionText, goToCenterText, endText;

    private Experiement1State _currentState;
    private Room currentRoom;
    private GameObject targetObj;
    private GameObject turnTargetObj;
    private float[] wallTranslateGain;
    private Vector2[] direction;
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

        wallTranslateGain = new float[5];
        wallTranslateGain[0] = 0.8f;
        wallTranslateGain[1] = 0.9f;
        wallTranslateGain[2] = 1.0f;
        wallTranslateGain[3] = 1.1f;
        wallTranslateGain[4] = 1.2f;

        direction = new Vector2[4];
        direction[0] = Vector2.up;
        direction[1] = Vector2.left;
        direction[2] = Vector2.down;
        direction[3] = Vector2.right;

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
        User user = users.GetActiveUser();
        user.AddEnterEvent("Portal", "Untagged", () => task.Processing("onEnterPortal"));
        user.AddViewEvent("Door", "FacingDoor", () => task.Processing("onViewFacingDoor"));
        user.AddEnterEvent("CenterPortal", "Untagged", () => task.Processing("onEnterCenterPortal"));
        user.AddClickEvent("UI", "OKButton", () => task.Processing("onClickOK"));
        user.AddClickEvent("UI", "YesButton", () => task.Processing("onClickYes"));
        user.AddClickEvent("UI", "NoButton", () => task.Processing("onClickNo"));

        // Define task for experiment 1
        task = new FiniteStateMachine<string, string>("Initial", "Around", "Target", "Door_Step1", "Door_Step2", "Door_Step3", "Behind", "Selection", "Center", "End");
        task.AddStateStart("Initial", () => EnableOKUI(initialText))
        .AddTransition("Initial", "Around", "onClickOK", DisableUIandPointer)

        .AddStateStart("Around", () => WakeAfterSeconds(10.0f))
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
        .AddTransition("Behind", "onClickOK", DisableUIandPointer, () => WakeAfterSeconds(5.0f))
        .AddTransition("Behind", "Selection", "onAfterSeconds")

        .AddStateStart("Selection", () => EnableSelectionUI(selectionText))
        .AddTransition("Selection", "Center", "onClickYes", DisableUIandPointer, () => WriteAnswer(true))
        .AddTransition("Selection", "Center", "onClickNo", DisableUIandPointer, () => WriteAnswer(false))

        .AddStateStart("Center", () => EnableOKUI(goToCenterText))
        .AddTransition("Center", "onClickOK", DisableUIandPointer, GenerateCenterPoint)
        .AddTransition("Center", "onEnterCenterPortal", DestroyCenterPoint, UserCameraFadeOut, () => WakeAfterSeconds(3.0f))
        .AddTransition("Center", "onAfterSeconds", RestoreOriginWall, UserCameraFadeIn, RaiseEndCondition)
        .AddTransition("Center", "End", "onEnd")
        .AddTransition("Center", "Around", "onNotEnd");

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        task.Begin("Initial");
    }

    public void WakeAfterSeconds(float time) {
        CoroutineManager.Instance.CallWaitForSeconds(time, () => task.Processing("onAfterSeconds"));
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
        user.ui.PopUpYesButton();
        user.ui.PopUpNoButton();
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

    public void QuitGame()
    {
        // save any game data here
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
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
        Debug.Log("GenerateTarget");

        SelectNextTargetPosition();

        Vector2 denormalizedTargetPosition2D = virtualEnvironment.CurrentRoom.DenormalizePosition2D(targetPosition, Space.World);
        Vector3 targetInitPosition = Utility.CastVector2Dto3D(denormalizedTargetPosition2D);
        targetObj = Instantiate(portalPrefab, targetInitPosition, Quaternion.identity);
    }

    private GameObject centerObj;

    public void GenerateCenterPoint() {
        Debug.Log("GenerateCenterPoint");

        Vector3 targetInitPosition = Vector3.zero;
        centerObj = Instantiate(centerPortalPrefab, targetInitPosition, Quaternion.identity);
    }

    public void DestroyCenterPoint() {
        Debug.Log("DestroyCenterPoint");

        if(centerObj != null) Destroy(centerObj);
    }

    public void DestroyTarget() {
        Debug.Log("DestroyTarget");

        if(targetObj != null) Destroy(targetObj);
    }

    public void MoveOppositeWall() {
        Debug.Log("MoveOppositeWall");

        SelectWallTranslate();
        virtualEnvironment.MoveWall(currentRoom, (facingWall + 2) % 4, translate);
    }

    public void RestoreOriginWall() {
        Debug.Log("RestoreOriginWall");
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
        Debug.Log("ColoringFacingDoor");
        List<Door> doors = virtualEnvironment.GetConnectedDoors(virtualEnvironment.CurrentRoom);

        foreach(var door in doors) {
            if(door.GetThisRoomWrapper(virtualEnvironment.CurrentRoom).wall == facingWall) {
                door.gameObject.tag = "FacingDoor";
                door.GetComponent<Outline>().enabled = true;
                
            }
        }
    }

    public void CleanDoors() {
        Debug.Log("CleanDoors");

        List<Door> doors = virtualEnvironment.GetConnectedDoors(virtualEnvironment.CurrentRoom);

        foreach(var door in doors) {
            door.gameObject.tag = "Untagged";
            door.GetComponent<Outline>().enabled = false;
        }
    }

    public void SelectWallTranslate() {
        User user = users.GetActiveUser();
        
        float[] DistWalltoUser = new float[4];
        DistWalltoUser[0] = (user.Position.y + 0.4f) - currentRoom.GetEdge2D(0, Space.World).y;
        DistWalltoUser[1] = (user.Position.x - 0.4f) - currentRoom.GetEdge2D(1, Space.World).x;
        DistWalltoUser[2] = (user.Position.y - 0.4f) - currentRoom.GetEdge2D(2, Space.World).y;
        DistWalltoUser[3] = (user.Position.x + 0.4f) - currentRoom.GetEdge2D(3, Space.World).x;

        float[] DistgainApplied = new float[4];
        DistgainApplied[0] = (direction[0] * (wallTranslateGain[gainIndex] - 1) * currentRoom.Size).y;
        DistgainApplied[1] = (direction[1] * (wallTranslateGain[gainIndex] - 1) * currentRoom.Size).x;
        DistgainApplied[2] = (direction[2] * (wallTranslateGain[gainIndex] - 1) * currentRoom.Size).y;
        DistgainApplied[3] = (direction[3] * (wallTranslateGain[gainIndex] - 1) * currentRoom.Size).x;

        int oppositeWall = (facingWall + 2) % 4;

        if(DistWalltoUser[oppositeWall] * DistgainApplied[oppositeWall] > 0) {
            translate = Mathf.Sign(DistgainApplied[oppositeWall]) * Mathf.Min(Mathf.Abs(DistWalltoUser[oppositeWall]), Mathf.Abs(DistgainApplied[oppositeWall]));
        }
        else {
            translate = DistgainApplied[oppositeWall];
        }
    }

    public void WriteAnswer(bool userAnswer) {
        answer[distType][currentTrial-1][gainIndex] = userAnswer;
    }

    public Vector2 CalculateTargetPosition(int facingWall, DistanceType distanceFromBehindWall) {
        Vector2 result = (int)(distanceFromBehindWall - 1) * grid * direction[facingWall];
        return result;
    }
}
