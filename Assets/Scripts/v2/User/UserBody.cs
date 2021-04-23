using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR.Extras;

public class UserBody : Transform2D {

    private static int totalID = 0;
    private int id;
    private Camera userCamera;
    private CustomLaserPointer laserPointer;
    private Room enteredRoom;
    private bool isEnterNewRoom, isExitCurrentRoom;
    private class RoomEvent : UnityEvent<Room> { }
    public struct UserEventArgs
    {
        public Transform target;
        public UserEventArgs(Transform tf) { target = tf; }
    }
    private static RoomEvent onEnterNewRoom = new RoomEvent();
    private static RoomEvent onExitRoom = new RoomEvent();
    private static UnityEvent onEnterTarget = new UnityEvent();
    private static UnityEvent onDetachTurnTarget = new UnityEvent();
    private static List<UnityEvent> onClickButtons = new List<UnityEvent>();

    private float deltaRotation;    
    private Vector2 deltaPosition;
    private Vector2 previousPosition;
    private float previousRotation;
    private Vector2 previousForward;


    public float fov
    {
        get { return userCamera.fieldOfView; }
    }

    public float DeltaRotation {
        get { return deltaRotation; }
    }

    public Vector2 DeltaPosition {
        get { return deltaPosition; }
    }

    public void AddEnterNewRoomEvent(UnityAction<Room> call) {
        onEnterNewRoom.AddListener(call);
    }

    public void AddExitRoomEvent(UnityAction<Room> call) {
        onExitRoom.AddListener(call);
    }

    public void AddReachTargetEvent(UnityAction call) {
        onEnterTarget.AddListener(call);
    }

    public void AddDetachTargetEvent(UnityAction call) {
        onDetachTurnTarget.AddListener(call);
    }

    public void AddClickEvents(UnityAction call) {
        foreach(var eachClickEvent in onClickButtons) {
            eachClickEvent.AddListener(call);
        }
    }

    public void AddClickEvent(UnityAction call, int index) {
        while (onClickButtons.Count <= index) {
            onClickButtons.Add(new UnityEvent());
        }
        onClickButtons[index].AddListener(call);
    }

    private IEnumerator UpdateCurrentState()
    {
        while(true) {
            // TODO: VR body 일때 delta 값이 적게 측정되는 경우가 있음
            deltaPosition = (this.Position - previousPosition) / Time.fixedDeltaTime;
            deltaRotation = Vector2.SignedAngle(previousForward, this.Forward) / Time.fixedDeltaTime;

            previousPosition = this.Position;
            previousForward = this.Forward;

            yield return new WaitForFixedUpdate();
        }
    }

    private void ResetCurrentState()
    {
        deltaPosition = Vector2.zero;
        deltaRotation = 0;
        previousPosition = this.Position;
        previousForward = this.Forward;
    }

    public override void Initializing()
    {
        id = totalID++;

        userCamera = GetComponentInChildren<Camera>(); // 현재 object에서 camera component를 찾는다
        if (userCamera == null)
        {
            userCamera = transform.parent.GetComponentInChildren<Camera>(); // 다른 object에서 camera component를 찾는다
            if (userCamera == null) throw new System.Exception("Can't find user camera");
        }

        laserPointer = transform.parent.GetComponentInChildren<CustomLaserPointer>();
        if(laserPointer != null)  {
            laserPointer.PointerClick += OnClickButton;
        }

        // onExitRoom.AddListener((other) => {Debug.Log("User exited " + other.name);});
        // onEnterNewRoom.AddListener((other) => {Debug.Log("User entered " + other.name);});

        this.gameObject.layer = LayerMask.NameToLayer("Player");

        // ResetCurrentState();
        // StartCoroutine("UpdateCurrentState");
    }

    private void OnEnable() {
        ResetCurrentState();
        StartCoroutine("UpdateCurrentState");
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Room")) {
            Room room = other.GetComponent<Room>();

            if(other.gameObject.tag != "CurrentRoom") {
                enteredRoom = room;
                isEnterNewRoom = true;
            }
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Target")) {
            onEnterTarget.Invoke();
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Room")) {
            Room room = other.GetComponent<Room>();

            onExitRoom.Invoke(room);

            if(room == enteredRoom) {
                enteredRoom = null;
                isEnterNewRoom = false;
            }
            else if(other.gameObject.tag == "CurrentRoom") {
                if(isEnterNewRoom) {
                    onEnterNewRoom.Invoke(enteredRoom);
                }
            }
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Real Space")) {
            Debug.Log("User exited real space");
        }
    }

    public void OnDetachFromHand(UserEventArgs e) {
        if(e.target.gameObject.layer == LayerMask.NameToLayer("TurnTarget"))
            onDetachTurnTarget.Invoke();
    }

    public void OnClickButton(object sender, PointerEventArgs e) { // sender = dispatcher, e = producer
        if(e.target.gameObject.layer == LayerMask.NameToLayer("UI")) {
            if(e.target.gameObject.tag == "OKButton")
                onClickButtons[0].Invoke();
            else if(e.target.gameObject.tag == "OK2Button")
                onClickButtons[1].Invoke();
            else if(e.target.gameObject.tag == "YesButton")
                onClickButtons[2].Invoke();
            else if(e.target.gameObject.tag == "NoButton")
                onClickButtons[3].Invoke();
        }
    }

    public bool IsTargetInUserFov(Vector2 target) // global 좌표계 기준으로 비교
    {
        Vector2 userToTarget = target - this.Position;
        Vector2 userForward = this.Forward;

        float unsignedAngle = Vector2.Angle(userToTarget, userForward);

        if (unsignedAngle - ((this.fov + 50) / 2) < 0.01f)
            return true;
        else
            return false;
    }
}