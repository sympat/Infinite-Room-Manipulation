using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public struct UserEventArgs
{
    public Transform target;
    public UserEventArgs(Transform tf) { target = tf; }
}

public enum UserEventType {
    onView,
    onEnter,
    onExit,
    onClick,
    onGrab,
    onRelease,
}

public class RoomEvent : UnityEvent<Room> { }

public class User : Transform2D
{

    [HideInInspector]
    public UserUI ui;
    [HideInInspector]
    public Camera _camera;
    [HideInInspector]
    public UserBody body;
    [HideInInspector]
    public CustomLaserPointer pointer;

    private Room enteredRoom;
    private bool isEnterNewRoom;
    // private static RoomEvent onEnterNewRoom = new RoomEvent();
    // private static RoomEvent onExitRoom = new RoomEvent();
    // private static UnityEvent onExitRealSpace = new UnityEvent();
    // private static UnityEvent onEnterTarget = new UnityEvent();
    // private static UnityEvent onDetachTurnTarget = new UnityEvent();
    // private static UnityEvent onViewTarget = new UnityEvent();
    // private static List<UnityEvent> onClickButtons = new List<UnityEvent>();

    private Dictionary<int, Dictionary<string, UnityEvent>> onEnter;
    private Dictionary<int, Dictionary<string, UnityEvent>> onExit;
    private Dictionary<int, Dictionary<string, UnityEvent>> onView;
    private Dictionary<int, Dictionary<string, UnityEvent>> onGrab;
    private Dictionary<int, Dictionary<string, UnityEvent>> onRelease;

    public override void Initializing()
    {
        Debug.Log("Initializing");
        _camera = GetComponentInChildren<Camera>(); // 현재 object에서 camera component를 찾는다
        ui = GetComponentInChildren<UserUI>();
        body = GetComponentInChildren<UserBody>();
        pointer = GetComponentInChildren<CustomLaserPointer>();

        if(body == null) throw new System.Exception("User Collider is required.");
        if(_camera == null) throw new System.Exception("User Camera is required.");
        // if(pointer != null) pointer.PointerClick += OnClick;

        body.gameObject.layer = LayerMask.NameToLayer("Player");

        onEnter = new Dictionary<int, Dictionary<string, UnityEvent>>();
        onExit = new Dictionary<int, Dictionary<string, UnityEvent>>();
        onView = new Dictionary<int, Dictionary<string, UnityEvent>>();
        onGrab = new Dictionary<int, Dictionary<string, UnityEvent>>();
        onRelease = new Dictionary<int, Dictionary<string, UnityEvent>>();
    }

    public int CheckValidLayerAndTag(string layer, string tag) {
        int layerToInt = LayerMask.NameToLayer(layer);

        if(layerToInt <= -1)
            throw new System.Exception("Layer does not exist");

        GameObject.FindGameObjectWithTag(tag);

        return layerToInt;
    }

    public bool CheckValidEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEvent>> targetEvent) {
        int layerToInt = CheckValidLayerAndTag(layer, tag);

        if(!targetEvent.ContainsKey(layerToInt))
            return false;
        if(!targetEvent[layerToInt].ContainsKey(tag))
            return false;
        if(targetEvent[layerToInt][tag] == null)
            return false;

        return true;
    }

    public void AddEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEvent>> targetEvent, UnityAction call) {
        int layerToInt = CheckValidLayerAndTag(layer, tag);

        if(!targetEvent.ContainsKey(layerToInt))
            targetEvent.Add(layerToInt, new Dictionary<string, UnityEvent>());
        if(!targetEvent[layerToInt].ContainsKey(tag))
            targetEvent[layerToInt].Add(tag, new UnityEvent());

        targetEvent[layerToInt][tag].AddListener(call);
    }

    public void RemoveEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEvent>> targetEvent, UnityAction call) {
        int layerToInt = CheckValidLayerAndTag(layer, tag);

        targetEvent[layerToInt][tag].RemoveListener(call);
    }

    public void AddEnterEvent(string layer, string tag, UnityAction call) {
        AddEvent(layer, tag, onEnter, call);
    }

    public void RemoveEnterEvent(string layer, string tag, UnityAction call) {
        RemoveEvent(layer, tag, onEnter, call);
    }

    public void AddExitEvent(string layer, string tag, UnityAction call) {
        AddEvent(layer, tag, onExit, call);
    }

    public void RemoveExitEvent(string layer, string tag, UnityAction call) {
        RemoveEvent(layer, tag, onExit, call);
    }

    public void AddViewEvent(string layer, string tag, UnityAction call) {
        AddEvent(layer, tag, onView, call);
    }

    public void RemoveViewEvent(string layer, string tag, UnityAction call) {
        RemoveEvent(layer, tag, onView, call);
    }

    public void AddClickEvent(string layer, string tag, UnityAction call) {
        ui.transform.FindObjectWithTag(tag).GetComponent<Button>().onClick.AddListener(call);
    }

    public void RemoveClickEvent(string layer, string tag, UnityAction call) {
        ui.transform.FindObjectWithTag(tag).GetComponent<Button>().onClick.RemoveListener(call);
    }



    // public void AddEnterEvent(string name, UnityAction call) {
    //     if(!onEnter.ContainsKey(name)) return;
    //     onEnter[name].AddListener(call);
    // }

    // public void RemoveEnterEvent(string name, UnityAction call) {
    //     if(!onEnter.ContainsKey(name)) return;
    //     onEnter[name].RemoveListener(call);
    // }

    // public void AddViewEvent(string name, UnityAction call) {
    //     if(!onView.ContainsKey(name)) return;
    //     onView[name].AddListener(call);
    // }

    // public void RemoveViewEvent(string name, UnityAction call) {
    //     if(!onView.ContainsKey(name)) return;
    //     onView[name].RemoveListener(call);
    // }

    // public void AddExitEvent(string name, UnityAction call) {
    //     if(!onExit.ContainsKey(name)) return;
    //     onExit[name].AddListener(call);
    // }

    // public void RemoveExitEvent(string name, UnityAction call) {
    //     if(!onExit.ContainsKey(name)) return;
    //     onExit[name].RemoveListener(call);
    // } 

    // public void AddDetachEvent(string name, UnityAction call) {
    //     if(!onDetach.ContainsKey(name)) return;
    //     onDetach[name].AddListener(call);
    // }

    // public void RemoveDetachEvent(string name, UnityAction call) {
    //     if(!onDetach.ContainsKey(name)) return;
    //     onDetach[name].RemoveListener(call);
    // }

    // public void AddViewTargetEvent(UnityAction call) {
    //     onViewTarget.AddListener(call);
    // }

    // public void RemoveViewTargetEvent(UnityAction call) {
    //     onViewTarget.RemoveListener(call);
    // }

    // public void AddEnterNewRoomEvent(UnityAction<Room> call) {
    //     onEnterNewRoom.AddListener(call);
    // }

    // public void RemoveEnterNewRoomEvent(UnityAction<Room> call) {
    //     onEnterNewRoom.RemoveListener(call);
    // }

    // public void AddExitRoomEvent(UnityAction<Room> call) {
    //     onExitRoom.AddListener(call);
    // }

    // public void RemoveExitRoomEvent(UnityAction<Room> call) {
    //     onExitRoom.RemoveListener(call);
    // }

    // public void AddExitRealSpaceEvent(UnityAction call) {
    //     onExitRealSpace.AddListener(call);
    // }

    // public void RemoveExitRealSpaceEvent(UnityAction call) {
    //     onExitRealSpace.RemoveListener(call);
    // }

    // public void AddEnterTargetEvent(UnityAction call) {
    //     onEnterTarget.AddListener(call);
    // }

    // public void RemoveEnterTargetEvent(UnityAction call) {
    //     onEnterTarget.RemoveListener(call);
    // }

    // public void AddDetachTargetEvent(UnityAction call) {
    //     onDetachTurnTarget.AddListener(call);
    // }

    // public void RemoveDetachEvent(UnityAction call) {
    //     onDetachTurnTarget.RemoveListener(call);
    // }

    // public void AddClickEvents(UnityAction call) {
    //     ui.buttonOK.GetComponent<Button>().onClick.AddListener(call);
    //     ui.buttonYes.GetComponent<Button>().onClick.AddListener(call);
    //     ui.buttonNo.GetComponent<Button>().onClick.AddListener(call);
    // }

    // public void AddClickOkEvent(UnityAction call) {
    //     ui.buttonOK.GetComponent<Button>().onClick.AddListener(call);
    // }

    // public void RemoveClickOkEvent(UnityAction call) {
    //     ui.buttonOK.GetComponent<Button>().onClick.RemoveListener(call);
    // }

    // public void AddClickYesEvent(UnityAction call) {
    //     ui.buttonYes.GetComponent<Button>().onClick.AddListener(call);
    // }

    // public void RemoveClickYesEvent(UnityAction call) {
    //     ui.buttonYes.GetComponent<Button>().onClick.RemoveListener(call);
    // }

    // public void AddClickNoEvent(UnityAction call) {
    //     ui.buttonNo.GetComponent<Button>().onClick.AddListener(call);
    // }

    // public void RemoveClickNoEvent(UnityAction call) {
    //     ui.buttonNo.GetComponent<Button>().onClick.RemoveListener(call);
    // }

    public void CallEvent(UserEventArgs e, UserEventType eventType) {
        int callerLayer = e.target.gameObject.layer;
        string callerTag = e.target.gameObject.tag;
        string layerName = LayerMask.LayerToName(callerLayer);

        switch(eventType) {
            case UserEventType.onView:
                if(CheckValidEvent(layerName, callerTag, onView)) onView[callerLayer][callerTag].Invoke();
                break;
            case UserEventType.onEnter:
                if(CheckValidEvent(layerName, callerTag, onEnter)) onEnter[callerLayer][callerTag].Invoke();
                break;
            case UserEventType.onExit:
                if(CheckValidEvent(layerName, callerTag, onExit)) onExit[callerLayer][callerTag].Invoke();
                break;
            case UserEventType.onGrab:
                if(CheckValidEvent(layerName, callerTag, onGrab)) onGrab[callerLayer][callerTag].Invoke();
                break;
            case UserEventType.onRelease:
                if(CheckValidEvent(layerName, callerTag, onRelease)) onRelease[callerLayer][callerTag].Invoke();
                break;
            case UserEventType.onClick:
                if(ui.transform.FindObjectWithTag(callerTag) != null) ui.transform.FindObjectWithTag(callerTag).GetComponent<Button>().onClick.Invoke();
                break;
            default:
                throw new System.Exception("Call invalid user event type");
        }
    }

    // public void OnView(UserEventArgs e) {

    //     int callerLayer = e.target.gameObject.layer;
    //     string callerTag = e.target.gameObject.tag;

    //     onView[callerLayer][callerTag].Invoke();

    //     // Debug.Log($"OnView {e.target}");

    //     // if(e.target.gameObject.layer == LayerMask.NameToLayer("Door")) {
    //     //     if(e.target.gameObject.tag == "FacingDoor") {
    //     //     Debug.Log($"Door!");

    //     //         onViewTarget.Invoke();
    //     //     }
    //     // }
    // }

    // public void OnEnter(UserEventArgs e) {

    //     int callerLayer = e.target.gameObject.layer;
    //     string callerTag = e.target.gameObject.tag;

    //     onView[callerLayer][callerTag].Invoke();

    //     // Debug.Log($"OnEnter {e.target}");

    //     // if(e.target.gameObject.layer == LayerMask.NameToLayer("Room")) {
    //     //     Room room = e.target.GetComponent<Room>();

    //     //     if(e.target.gameObject.tag != "CurrentRoom") {
    //     //         enteredRoom = room;
    //     //         isEnterNewRoom = true;
    //     //     }
    //     // }
    //     // else if(e.target.gameObject.layer == LayerMask.NameToLayer("Target")) {
    //     //     onEnterTarget.Invoke();
    //     // }
    // }

    // public void OnExit(UserEventArgs e) {
    //     // Debug.Log($"OnExit {e.target}");

    //     if(e.target.gameObject.layer == LayerMask.NameToLayer("Room")) {
    //         Room room = e.target.GetComponent<Room>();

    //         onExitRoom.Invoke(room);

    //         if(e.target.gameObject.tag == "CurrentRoom") {
    //             if(isEnterNewRoom) {
    //                 onEnterNewRoom.Invoke(enteredRoom);
    //             }
    //         }
    //         else {
    //             if(room == enteredRoom) {
    //                 enteredRoom = null;
    //                 isEnterNewRoom = false;
    //             }
    //         }
    //     }
    //     else if(e.target.gameObject.layer == LayerMask.NameToLayer("Real Space")) {
    //         // Debug.Log("User exited real space");
    //         onExitRealSpace.Invoke();
    //     }
    // }
    
    // public void OnDetach(UserEventArgs e) {
    //     if(e.target.gameObject.layer == LayerMask.NameToLayer("TurnTarget"))
    //         onDetachTurnTarget.Invoke();
    // }

    // public void OnClick(object sender, PointerEventArgs e) { // sender = dispatcher, e = producer
    //     Debug.Log("OnClick");
    //     if(e.target.gameObject.layer == LayerMask.NameToLayer("UI")) {
    //         if(e.target.gameObject.tag == "Button") {
    //             Button button = e.target.GetComponent<Button>();
    //             button.onClick.Invoke();
    //         }
    //     }
    // }

    public bool IsTargetInUserFov(Vector2 target, float bound = 0) // global 좌표계 기준으로 비교
    {
        Vector2 userToTarget = target - this.Position;
        Vector2 userForward = this.Forward;

        float unsignedAngle = Vector2.Angle(userToTarget, userForward);

        if (unsignedAngle - ((_camera.fieldOfView + bound)) < 0.01f)
            return true;
        else
            return false;
    }

    public bool IsTargetInUserFov(Vector2 start, Vector2 end, float bound = 0) {
        Vector2 userToStart = start - this.Position;
        Vector2 userToEnd = end - this.Position;
        Vector2 userForward = this.Forward;
        
        float angleUserToStart = Vector2.SignedAngle(userForward, userToStart);
        float angleUserToEnd = Vector2.SignedAngle(userForward, userToEnd);
        float angleStartToEnd = Vector2.Angle(userToStart, userToEnd);

        if(angleUserToStart * angleUserToEnd < 0 && Mathf.Abs(angleUserToStart) + Mathf.Abs(angleUserToEnd) < angleStartToEnd + 0.01f) {
            return true;
        }
        else if(IsTargetInUserFov(start, bound) || IsTargetInUserFov(end, bound)) {
            return true;
        }
        else {
            return false;
        }
    }

}
