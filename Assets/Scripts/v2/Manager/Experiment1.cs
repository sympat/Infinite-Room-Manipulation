using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment1 : Manager
{
    public enum DistanceType {
        Short = -1,
        Middle = 0,
        Long = 1
    }

    // public VirtualEnvironment virtualEnvironment;
    public GameObject targetObjectPrefab;
    public GameObject turnTargetObjectPrefab;

    public UIHandler userUI;
    public CustomLaserPointer userPointer;
    private Room currentRoom;
    private GameObject targetObj;
    private GameObject turnTargetObj;
    private float[] wallTranslateGain;
    private Vector2[] direction;
    private float grid;
    private Dictionary<DistanceType, List<List<bool>>> answer; // T - yes, F - no

    private int currentTrial = 0;
    public int totalTrial = 1;
    private int tempTrial = 0;

    Vector2 targetPosition;
    Vector2 translate;
    int facingWall;
    int gainIndex;
    DistanceType distType;
    Queue<int> distSample;
    Queue<int>[] gainSample;

    public override void Start() {
        base.Start();

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

        gainSample = new Queue<int>[3];
        for(int i=0; i<gainSample.Length; i++)
            gainSample[i] = new Queue<int>();

        distSample = new Queue<int>();

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
        userUI.PopUpOkParagraph();
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
        if(gainSample[0].Count == 0 && gainSample[1].Count == 0 && gainSample[2].Count == 0) {
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

        if(distSample.Count == 0)
            InitializeDistance();
    }

    public void GenerateTarget() {
        CheckEndExperiment();
        SelectNextTargetPositionAndTranslate();
        Vector3 targetInitPosition = virtualEnvironment.CurrentRoom.DenormalizePosition3D(targetPosition);
        targetObj = Instantiate(targetObjectPrefab, targetInitPosition, Quaternion.identity);
    }

    public void GenerateTurnTarget() {
        Vector3 turnTargetInitPosition = virtualEnvironment.CurrentRoom.DenormalizePosition3D(targetPosition + direction[facingWall] * 0.4f, 1.4f); 
        turnTargetObj = Instantiate(turnTargetObjectPrefab, turnTargetInitPosition, Quaternion.identity);
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
        distSample = new Queue<int>(Utility.sampleWithoutReplacement(3, -1, 2)); // IV 1
        if(targetPosition == Vector2.zero && distSample.Peek() == 0) {
            distSample.Dequeue();
            distSample.Enqueue(0);
        }
    }

    public void InitializeAppliedGain() {
        for(int i=0; i<gainSample.Length; i++)
            gainSample[i] = new Queue<int>(Utility.sampleWithoutReplacement(5, 0, 5)); // IV 2
    }

    public void SelectNextTargetPositionAndTranslate() {
        distType = (DistanceType) distSample.Dequeue();

        Vector2 nextTargetPosition;

        do {
            facingWall = Utility.sampleUniform(0, 4);
            nextTargetPosition = CalculateTargetPosition(facingWall, distType);
        } while(targetPosition == nextTargetPosition);

        targetPosition = nextTargetPosition;
        gainIndex = gainSample[((int)distType + 1)].Dequeue();
        translate = CalculateTranslate(facingWall, gainIndex);

        tempTrial += 1;
        Debug.Log($"Start {tempTrial} ---------------------");
        Debug.Log($"Facing Wall {facingWall}");
        Debug.Log($"Opposite Wall {(facingWall + 2) % 4}");
        Debug.Log($"Distance from opposite wall {distType}");
        Debug.Log($"Applied gain {wallTranslateGain[gainIndex]}");
        Debug.Log($"Next target position {targetPosition}");
        Debug.Log($"End {tempTrial} ---------------------");
    }

    public void CheckAnswerYes() { // 커졌다고 대답
        answer[distType][currentTrial-1][gainIndex] = true;
    }

    public void CheckAnswerNo() { // 작아졌다고 대답
        answer[distType][currentTrial-1][gainIndex] = false;
    }

    public Vector2 CalculateTargetPosition(int facingWall, DistanceType distanceFromBehindWall) {
        Vector2 result = (int) distanceFromBehindWall * grid * direction[facingWall];
        return result;
    }

    public Vector2 CalculateTranslate(int facingWall, int gainIndex) {
        Vector2 result = (wallTranslateGain[gainIndex] - 1) * direction[(facingWall + 2) % 4] * currentRoom.Size;
        return result;
    }
}
