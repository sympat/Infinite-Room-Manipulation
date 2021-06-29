using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public struct UserEventArgs
{
    public GameObject target;
    public Behaviour behaviour;
    // public UserTarget target;

    public UserEventArgs(Behaviour behaviour) {
        this.target = null;
        this.behaviour = behaviour;
    }

    public UserEventArgs(Behaviour behaviour, GameObject target) { 
        this.target = target; 
        this.behaviour = behaviour; 
        // this.target = UserTarget.Any;
    }
}

// public enum UserEventType {
//     onView,
//     onEnter,
//     onExit,
//     onStay,
//     onClick,
//     onGrab,
//     onRelease,
//     onRotate,
//     onCompletelyStay,
// }

// public class Behaviour : Enumeration {
//     public static readonly Behaviour Watch = new Behaviour(0, "Watch");
//     public static readonly Behaviour Enter = new Behaviour(1, "Enter");
//     public static readonly Behaviour Exit = new Behaviour(2, "Exit");
//     public static readonly Behaviour Stay = new Behaviour(3, "Stay");
//     public static readonly Behaviour CompletelyEnter = new Behaviour(4, "CompletelyEnter");
//     public static readonly Behaviour CompletelyStay = new Behaviour(5, "CompletelyStay");
//     public static readonly Behaviour Grab = new Behaviour(6, "Grab");
//     public static readonly Behaviour Release = new Behaviour(7, "Release");
//     public static readonly Behaviour Rotate = new Behaviour(8, "Rotate");
//     public static readonly Behaviour Translate = new Behaviour(9, "Translate");
//     public static readonly Behaviour Wait = new Behaviour(10, "Wait");

//     public Behaviour() {}
//     protected Behaviour(int value, string displayName) : base(value, displayName) {}
// }

public enum Behaviour {
    Watch,
    Enter,
    Exit,
    Stay,
    CompletelyEnter,
    CompletelyStay,
    Grab,
    Release,
    Rotate,
    Open,
}

public class UserEvent : UnityEvent<GameObject> {}

public class User : Transform2D
{ 

    // [HideInInspector]
    // private UserUI ui;
    private Camera face;
    private UserBody body;
    private Hand[] hands;

    public Camera Face {
        get { return face; }
    }

    public Hand[] Hands {
        get { return hands; }
    }

    public UserBody Body {
        get { return body; }
    }

    // private Room enteredRoom;
    // private bool isEnterNewRoom;
    // private Dictionary<int, Dictionary<string, UnityEvent>> onEnter;
    // private Dictionary<int, Dictionary<string, UnityEventWithTarget>> onEnter2;
    // private Dictionary<int, Dictionary<string, UnityEvent>> onExit;
    // private Dictionary<int, Dictionary<string, UnityEventWithTarget>> onExit2;
    // private Dictionary<int, Dictionary<string, UnityEventWithTarget>> onStay;
    // private Dictionary<int, Dictionary<string, UnityEvent>> onCompletelyStay;
    // private Dictionary<int, Dictionary<string, UnityEvent>> onView;
    // private Dictionary<int, Dictionary<string, UnityEvent>> onGrab;
    // private Dictionary<int, Dictionary<string, UnityEvent>> onRelease;
    // private UnityEvent onRotate;

    private Dictionary<Behaviour, Dictionary<string, UserEvent>> events;

    public override void Initializing()
    {

        face = GetComponentInChildren<Camera>(); // 현재 object에서 camera component를 찾는다
        // ui = GetComponentInChildren<UserUI>();
        body = GetComponentInChildren<UserBody>();
        hands = GetComponentsInChildren<Hand>();
        // pointer = GetComponentInChildren<CustomLaserPointer>();

        if(body == null) throw new System.Exception("User body(Collider) is required.");
        if(face == null) throw new System.Exception("User face(Camera) is required.");
        if(hands == null) throw new System.Exception("User hand(Hand) is required.");

        // body.gameObject.layer = LayerMask.NameToLayer("Virtual User");

        // onEnter = new Dictionary<int, Dictionary<string, UnityEvent>>();
        // onEnter2 = new Dictionary<int, Dictionary<string, UnityEventWithTarget>>();
        // onExit = new Dictionary<int, Dictionary<string, UnityEvent>>();
        // onExit2 = new Dictionary<int, Dictionary<string, UnityEventWithTarget>>();
        // onView = new Dictionary<int, Dictionary<string, UnityEvent>>();
        // onGrab = new Dictionary<int, Dictionary<string, UnityEvent>>();
        // onRelease = new Dictionary<int, Dictionary<string, UnityEvent>>();
        // onCompletelyStay = new Dictionary<int, Dictionary<string, UnityEvent>>();
        // onRotate = new UnityEvent();

        events = new Dictionary<Behaviour, Dictionary<string, UserEvent>>();

        foreach(Behaviour behave in Enum.GetValues(typeof(Behaviour))) {
            events[behave] = new Dictionary<string, UserEvent>();

            for(int i=0; i<32; i++) {
                string layerName = LayerMask.LayerToName(i);
                if(layerName == "") continue;
                events[behave].Add(layerName, new UserEvent());
            }
        }
    }

    public IEnumerator CallAfterRotation(float degree) {

        Vector2 prevForward = Body.Forward;
        float sumAngle = 0;

        while(true) {

            Vector2 currentForward = Body.Forward;

            // float currentRotating = Vector2.SignedAngle(startForward, currentForward);
            // float currentDirection = (currentRotating == 0) ? 0 : Mathf.Sign(currentRotating);

            // Debug.Log(currentRotating);

            // float prevRotating = Vector2.SignedAngle(startForward, prevForward);
            // float prevDirection = (prevRotating == 0) ? 0 : Mathf.Sign(prevRotating);

            float deltaAngle = Vector2.SignedAngle(prevForward, currentForward);
            sumAngle += deltaAngle;

            // Debug.Log(sumAngle);

            if(degree < 0) {
                if(sumAngle <= degree)
                    break;
            }
            else {
                if(sumAngle >= degree)
                    break;
            }

            // if(prevDirection * currentDirection < 0 && Mathf.Abs(prevRotating) + Mathf.Abs(currentRotating) >= degree) {
            //     break;
            // }

            // if(rotDirection * currentDirection > 0 && Mathf.Abs(currentRotating) >= Mathf.Abs(degree)) {
            //     break;
            // }

            prevForward = currentForward;

            yield return new WaitForFixedUpdate();
        }

        // Debug.Log("Rotation Done!");

        UserEventArgs e = new UserEventArgs(Behaviour.Rotate);
        ProcessingEvent(e);
    }

    public void ToggleHandPointer(bool enabled) {
        foreach(var hand in hands) {
            CustomLaserPointer pointer = hand.GetComponent<CustomLaserPointer>();
            if(pointer != null) pointer.active = enabled;
        }
    }

    public void AddEvent(Behaviour behaviour, string layer, UnityAction<GameObject> call) {
        // Debug.Log($"AddEvent {behaviour} {layer}");
        int layerToInt = LayerMask.NameToLayer(layer);
        if(layerToInt < 0) throw new System.Exception("InValid Layer index");

        events[behaviour][layer].AddListener(call);
    }

    public void RemoveEvent(Behaviour behaviour, string layer, UnityAction<GameObject> call) {
        int layerToInt = LayerMask.NameToLayer(layer);
        if(layerToInt < 0) throw new System.Exception("InValid Layer index");

        events[behaviour][layer].RemoveListener(call);
    }

    public void InvokeEvent(Behaviour behaviour, GameObject target) {
        if(target == null) events[behaviour]["Default"].Invoke(target);
        else events[behaviour][LayerMask.LayerToName(target.layer)].Invoke(target);
    }

    public void ProcessingEvent(UserEventArgs e) {
        // if(e.behaviour == Behaviour.Open) { 
        //     Debug.Log($"ProcessingEvent {e.behaviour} {LayerMask.LayerToName(e.target.layer)}"); 
        // }

        InvokeEvent(e.behaviour, e.target);
    }

    // public int CheckValidLayerAndTag(string layer, string tag) {
    //     int layerToInt = LayerMask.NameToLayer(layer);

    //     if(layerToInt <= -1)
    //         throw new System.Exception("Layer does not exist");

    //     GameObject.FindGameObjectWithTag(tag);

    //     return layerToInt;
    // }

    // public bool CheckValidEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEvent>> targetEvent) {
    //     int layerToInt = CheckValidLayerAndTag(layer, tag);

    //     if(!targetEvent.ContainsKey(layerToInt))
    //         return false;
    //     if(!targetEvent[layerToInt].ContainsKey(tag))
    //         return false;
    //     if(targetEvent[layerToInt][tag] == null)
    //         return false;

    //     return true;
    // }

    // public bool CheckValidEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEventWithTarget>> targetEvent) {
    //     int layerToInt = CheckValidLayerAndTag(layer, tag);

    //     if(!targetEvent.ContainsKey(layerToInt))
    //         return false;
    //     if(!targetEvent[layerToInt].ContainsKey(tag))
    //         return false;
    //     if(targetEvent[layerToInt][tag] == null)
    //         return false;

    //     return true;
    // }


    // public void AddEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEvent>> targetEvent, UnityAction call) {
    //     int layerToInt = CheckValidLayerAndTag(layer, tag);

    //     if(!targetEvent.ContainsKey(layerToInt))
    //         targetEvent.Add(layerToInt, new Dictionary<string, UnityEvent>());

    //     if(!targetEvent[layerToInt].ContainsKey(tag))
    //         targetEvent[layerToInt].Add(tag, new UnityEvent());

    //     targetEvent[layerToInt][tag].AddListener(call);
    // }

    // public void AddEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEventWithTarget>> targetEvent, UnityAction<Transform> call) {
    //     int layerToInt = CheckValidLayerAndTag(layer, tag);

    //     if(!targetEvent.ContainsKey(layerToInt))
    //         targetEvent.Add(layerToInt, new Dictionary<string, UnityEventWithTarget>());

    //     if(!targetEvent[layerToInt].ContainsKey(tag))
    //         targetEvent[layerToInt].Add(tag, new UnityEventWithTarget());

    //     targetEvent[layerToInt][tag].AddListener(call);
    // }

    // public void RemoveEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEvent>> targetEvent, UnityAction call) {
    //     int layerToInt = CheckValidLayerAndTag(layer, tag);

    //     targetEvent[layerToInt][tag].RemoveListener(call);
    // }

    // public void RemoveEvent(string layer, string tag, Dictionary<int, Dictionary<string, UnityEventWithTarget>> targetEvent, UnityAction<Transform> call) {
    //     int layerToInt = CheckValidLayerAndTag(layer, tag);

    //     targetEvent[layerToInt][tag].RemoveListener(call);
    // }

    // public void AddEnterEvent(string layer, string tag, UnityAction call) {
    //     AddEvent(layer, tag, onEnter, call);
    // }

    // public void AddEnterEvent(string layer, string tag, UnityAction<Transform> call) {
    //     AddEvent(layer, tag, onEnter2, call);
    // }

    // public void RemoveEnterEvent(string layer, string tag, UnityAction call) {
    //     RemoveEvent(layer, tag, onEnter, call);
    // }

    // public void RemoveEnterEvent(string layer, string tag, UnityAction<Transform> call) {
    //     RemoveEvent(layer, tag, onEnter2, call);
    // }

    // public void AddExitEvent(string layer, string tag, UnityAction call) {
    //     AddEvent(layer, tag, onExit, call);
    // }

    // public void AddExitEvent(string layer, string tag, UnityAction<Transform> call) {
    //     AddEvent(layer, tag, onExit2, call);
    // }

    // public void RemoveExitEvent(string layer, string tag, UnityAction call) {
    //     RemoveEvent(layer, tag, onExit, call);
    // }

    // public void RemoveExitEvent(string layer, string tag, UnityAction<Transform> call) {
    //     RemoveEvent(layer, tag, onExit2, call);
    // }

    // public void AddStayEvent(string layer, string tag, UnityAction<Transform> call) {
    //     AddEvent(layer, tag, onStay, call);
    // }

    // public void RemoveStayEvent(string layer, string tag, UnityAction<Transform> call) {
    //     RemoveEvent(layer, tag, onStay, call);
    // }

    // public void AddCompletelyStayEvent(string layer, string tag, UnityAction call) {
    //     AddEvent(layer, tag, onCompletelyStay, call);
    // }

    // public void RemoveCompletelyStayEvent(string layer, string tag, UnityAction call) {
    //     RemoveEvent(layer, tag, onCompletelyStay, call);
    // }

    // public void AddViewEvent(string layer, string tag, UnityAction call) {
    //     AddEvent(layer, tag, onView, call);
    // }

    // public void RemoveViewEvent(string layer, string tag, UnityAction call) {
    //     RemoveEvent(layer, tag, onView, call);
    // }

    // public void AddClickEvent(string layer, string tag, UnityAction call) {
    //     List<GameObject> buttons = ui.transform.FindObjectsWithTag(tag);

    //     foreach(GameObject myButton in buttons) {
    //         myButton.GetComponent<Button>().onClick.AddListener(call);
    //     }
    // }

    // public void RemoveClickEvent(string layer, string tag, UnityAction call) {
    //     List<GameObject> buttons = ui.transform.FindObjectsWithTag(tag);

    //     foreach(GameObject myButton in buttons) {
    //         myButton.GetComponent<Button>().onClick.RemoveListener(call);
    //     }
    // }

    // public void AddReleaseEvent(string layer, string tag, UnityAction call) {
    //     AddEvent(layer, tag, onRelease, call);
    // }

    // public void RemoveReleaseEvent(string layer, string tag, UnityAction call) {
    //     RemoveEvent(layer, tag, onRelease, call);
    // }

    // public void AddRotateEvent(UnityAction call) {
    //     onRotate.AddListener(call);
    // }

    // public void RemoveRotateEvent(UnityAction call) {
    //     onRotate.RemoveListener(call);
    // }

    // public void CallEvent(UserEventArgs e, UserEventType eventType) {
    //     // Debug.Log($"User CallEvent {eventType} {e.target.gameObject}");

    //     int callerLayer = e.target.gameObject.layer;
    //     string callerTag = e.target.gameObject.tag;
    //     string layerName = LayerMask.LayerToName(callerLayer);

    //     switch(eventType) {
    //         case UserEventType.onView:
    //             if(CheckValidEvent(layerName, callerTag, onView)) onView[callerLayer][callerTag].Invoke();
    //             break;
    //         case UserEventType.onEnter:
    //             if(CheckValidEvent(layerName, callerTag, onEnter)) onEnter[callerLayer][callerTag].Invoke();
    //             if(CheckValidEvent(layerName, callerTag, onEnter2)) onEnter2[callerLayer][callerTag].Invoke(e.target);
    //             break;
    //         case UserEventType.onExit:
    //             if(CheckValidEvent(layerName, callerTag, onExit)) onExit[callerLayer][callerTag].Invoke();
    //             if(CheckValidEvent(layerName, callerTag, onExit2)) onExit2[callerLayer][callerTag].Invoke(e.target);
    //             break;
    //         case UserEventType.onStay:
    //             if(CheckValidEvent(layerName, callerTag, onStay)) onStay[callerLayer][callerTag].Invoke(e.target); 
    //             break;
    //         case UserEventType.onGrab:
    //             if(CheckValidEvent(layerName, callerTag, onGrab)) onGrab[callerLayer][callerTag].Invoke();
    //             break;
    //         case UserEventType.onRelease:
    //             if(CheckValidEvent(layerName, callerTag, onRelease)) onRelease[callerLayer][callerTag].Invoke();
    //             break;
    //         // case UserEventType.onClick:
    //         //     if(ui.transform.FindObjectWithTag(callerTag) != null) ui.transform.FindObjectWithTag(callerTag).GetComponent<Button>().onClick.Invoke();
    //         //     break;
    //         case UserEventType.onCompletelyStay:
    //             if(CheckValidEvent(layerName, callerTag, onCompletelyStay)) onCompletelyStay[callerLayer][callerTag].Invoke();
    //             break;
    //         // case UserEventType.onRotate:
    //         //     onRotate.Invoke();
    //         //     break;
    //         default:
    //             throw new System.Exception("Call invalid user event type");
    //     }
    // }

    public bool IsTargetInUserFov(Vector2 target, float bound = 0) // global 좌표계 기준으로 비교
    {
        Vector2 userToTarget = target - this.body.Position;
        Vector2 userForward = this.body.Forward;

        float unsignedAngle = Vector2.Angle(userToTarget, userForward);

        if (unsignedAngle - ((face.fieldOfView + bound)) < 0.01f)
            return true;
        else
            return false;
    }

    public bool IsTargetInUserFov(Vector2 start, Vector2 end, float bound = 0) {
        Vector2 userToStart = start - this.body.Position;
        Vector2 userToEnd = end - this.body.Position;
        Vector2 userForward = this.body.Forward;
        
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
