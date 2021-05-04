using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Experiment1 : Manager
{
    public enum DistanceType {
        Short = 0,
        Middle = 1,
        Long = 2
    }

    // public VirtualEnvironment virtualEnvironment;
    public GameObject targetObjectPrefab;
    public GameObject turnTargetObjectPrefab;
    public UIHandler userUI;
    public CustomLaserPointer userPointer;
    public int totalTrial = 1;
    private Room currentRoom;
    private GameObject targetObj;
    private GameObject turnTargetObj;
    private float[] wallTranslateGain;
    private Vector2[] direction;
    private float grid;
    private Dictionary<DistanceType, List<List<bool>>> answer; // T - yes, F - no

    private int currentTrial = 0;
    private int tempTrial = 0;

    Vector2 targetPosition;
    Vector2 translate;
    int facingWall;
    int gainIndex;
    DistanceType distType;
    // Queue<int> distSample;
    Queue<DistanceType> distTypeQueue;
    // Queue<int>[] gainSample;
    Dictionary<DistanceType, Queue<int>> gainQueue;

    public override void Awake() {
        base.Awake();

        UserBody userBody = user.GetTrackedUserBody();
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

        // PrintResult();

        // gainSample = new Queue<int>[3]; // short, middle, long
        // for(int i=0; i<gainSample.Length; i++)
        //     gainSample[i] = new Queue<int>();

        distTypeQueue = new Queue<DistanceType>();

        gainQueue = new Dictionary<DistanceType, Queue<int>>();
        gainQueue.Add(DistanceType.Short, new Queue<int>());
        gainQueue.Add(DistanceType.Middle, new Queue<int>());
        gainQueue.Add(DistanceType.Long, new Queue<int>());

        targetPosition = Vector2.zero;

        float userInitRotation = Utility.sampleUniform(0, 360);
        userBody.Rotation = userInitRotation;

        userBody.AddClickEvent(GenerateTarget, 0);
        userBody.AddClickEvent(userUI.DisableUI, 0);
        userBody.AddClickEvent(userPointer.HidePointer, 0);

        userBody.AddReachTargetEvent(DestroyTarget);
        userBody.AddReachTargetEvent(GenerateTurnTarget);

        userBody.AddDetachTargetEvent(DestroyTurnTarget);
        userBody.AddDetachTargetEvent(MoveOppositeWall);
        userBody.AddDetachTargetEvent(userPointer.ShowPointer);
        userBody.AddDetachTargetEvent(userUI.PopUPOK2Paragraph);

        userBody.AddClickEvent(userUI.PopUpChoiceParagraph, 1);
        userBody.AddClickEvent(userUI.DisableUI, 1);

        userBody.AddClickEvent(CheckAnswerYes, 2);
        userBody.AddClickEvent(GenerateTarget, 2);
        userBody.AddClickEvent(userUI.DisableUI, 2);
        userBody.AddClickEvent(userPointer.HidePointer, 2);

        userBody.AddClickEvent(CheckAnswerNo, 3);
        userBody.AddClickEvent(GenerateTarget, 3);
        userBody.AddClickEvent(userUI.DisableUI, 3);
        userBody.AddClickEvent(userPointer.HidePointer, 3);

        // userPointer.ShowPointer();
        // userUI.PopUpOkParagraph();

        // virtualEnvironment.MoveWall(currentRoom, 0, 1.0f);
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
                        output += "T";
                    else
                        output += "F";
                }
                output += "\n";
            }

            Debug.Log(output);
        }
    }

    public void CheckEndExperiment() {
        if(gainQueue[DistanceType.Short].Count == 0 
        && gainQueue[DistanceType.Middle].Count == 0 
        && gainQueue[DistanceType.Long].Count == 0) { 
            if(currentTrial == totalTrial) {
                userUI.PopUpEndParagraph();
                PrintResult();
                QuitGame();
            }
            else {
                currentTrial += 1;
                InitializeAppliedGain();
            }
        }

        if(distTypeQueue.Count == 0)
            InitializeDistance();
    }

    public void GenerateTarget() {
        CheckEndExperiment();
        SelectNextTargetPositionAndTranslate();
        Vector3 targetInitPosition = virtualEnvironment.CurrentRoom.DenormalizePosition3D(targetPosition);
        targetObj = Instantiate(targetObjectPrefab, targetInitPosition, Quaternion.identity);
    }

    public void GenerateTurnTarget() {
        // Vector3 turnTargetInitPosition = virtualEnvironment.CurrentRoom.DenormalizePosition3D(targetPosition + direction[facingWall] * 0.4f, 1.4f); // TODO 여기고쳐
        // turnTargetObj = Instantiate(turnTargetObjectPrefab, turnTargetInitPosition, Quaternion.identity);
    }

    public void DestroyTarget() {
        if(targetObj != null) Destroy(targetObj);
    }

    public void DestroyTurnTarget() {
        if(turnTargetObj != null) Destroy(turnTargetObj);
    }

    public void MoveOppositeWall() {
        virtualEnvironment.MoveWall(currentRoom, (facingWall + 2) % 4, translate);
    }

    public void InitializeDistance() {
        distTypeQueue = new Queue<DistanceType>(Utility.sampleWithoutReplacement(3, 0, 3).Select(x => (DistanceType) x)); // IV 1
        if(targetPosition == Vector2.zero && distTypeQueue.Peek() == DistanceType.Middle) {
            distTypeQueue.Enqueue(distTypeQueue.Dequeue());
        }
    }

    public void InitializeAppliedGain() {
        gainQueue[DistanceType.Short] = new Queue<int>(Utility.sampleWithoutReplacement(5, 0, 5)); // IV 2
        gainQueue[DistanceType.Middle] = new Queue<int>(Utility.sampleWithoutReplacement(5, 0, 5)); // IV 2
        gainQueue[DistanceType.Long] = new Queue<int>(Utility.sampleWithoutReplacement(5, 0, 5)); // IV 2
    }

    public void SelectNextTargetPositionAndTranslate() {
        distType = distTypeQueue.Dequeue();

        Debug.Log(distType);

        Vector2 nextTargetPosition;

        do {
            facingWall = Utility.sampleUniform(0, 4);
            nextTargetPosition = CalculateTargetPosition(facingWall, distType);
        } while(targetPosition == nextTargetPosition);

        targetPosition = nextTargetPosition;
        gainIndex = gainQueue[distType].Dequeue();
        translate = CalculateTranslate(facingWall, gainIndex);

        // tempTrial += 1;
        // Debug.Log($"Start {tempTrial} ---------------------");
        // Debug.Log($"Facing Wall {facingWall}");
        // Debug.Log($"Opposite Wall {(facingWall + 2) % 4}");
        // Debug.Log($"Distance from opposite wall {distType}");
        // Debug.Log($"Applied gain {wallTranslateGain[gainIndex]}");
        // Debug.Log($"Next target position {targetPosition}");
        // Debug.Log($"Next translate {translate}");
        // Debug.Log($"End {tempTrial} ---------------------");
    }

    public void CheckAnswerYes() { // 커졌다고 대답
        answer[distType][currentTrial-1][gainIndex] = true;
    }

    public void CheckAnswerNo() { // 작아졌다고 대답
        answer[distType][currentTrial-1][gainIndex] = false;
    }

    public Vector2 CalculateTargetPosition(int facingWall, DistanceType distanceFromBehindWall) {
        Vector2 result = (int)(distanceFromBehindWall - 1) * grid * direction[facingWall];
        return result;
    }

    public Vector2 CalculateTranslate(int facingWall, int gainIndex) {
        Vector2 result = (wallTranslateGain[gainIndex] - 1) * direction[(facingWall + 2) % 4] * currentRoom.Size;
        return result;
    }
}
