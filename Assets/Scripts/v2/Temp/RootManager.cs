using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITask<TState, TInput> 
where TState : System.Enum
where TInput : System.Enum
{
    FiniteStateMachine<TState, TInput> task {
        get; set;
    }

    void ConstructTask();
}


public enum VEState {
    Idle,
    GoToNext,
}

public enum VEInput {
    EnterNextRoom,
    OpenDoor,
}

public class VETask : ITask<VEState, VEInput> {

    public UIContainer[] uiInfo;

    public FiniteStateMachine<VEState, VEInput> task {
        get; set;
    }

     public void ConstructTask() {
        task.AddStateStart(VEState.Idle)
        .AddTransition(VEState.Idle, VEState.GoToNext, VEInput.OpenDoor, Func1)
        .AddTransition(VEState.GoToNext, VEInput.OpenDoor, Func1)
        .AddTransition(VEState.GoToNext, VEState.Idle, VEInput.EnterNextRoom, Func2);

        task.Begin(VEState.Idle);
    }

    public void Func1() {}
    public void Func2() {}
}

public class VE : Transform2D {
    public Room startRoom;
    public bool useCenterStart;
    public bool useFullVisualization;

    private VETask temp;

    public EnvironmentManager environmentManager { get; set; }

    private Dictionary<Room, List<Door>> adjList;
    private Room currentRoom;

    private static int totalID = 1;
    private int id;

    public override void Initializing() {
        // manager = this.transform.parent.GetComponent<Manager>();
        id = totalID++;
        adjList = new Dictionary<Room, List<Door>>();

        List<Room> rooms = new List<Room>();
        List<Door> doors = new List<Door>();

        foreach(Transform child in this.transform) {
            Transform2D tf = child.GetComponent<Transform2D>();

            if(tf is Room) {
                rooms.Add(tf as Room);
            }
            else if(tf is Door) {
                doors.Add(tf as Door);
            }
        }

        // foreach(var room in rooms) {
        //     AddRoom(room);
        // }

        // foreach(var door in doors) {
        //     AddDoor(door);
        // }

        // _users = manager.users;

        // User user = _users.GetActiveUser();
        // User user = rootManager.environmentManager.users.GetActiveUser();

        // user.AddEvent(Behaviour.CompletelyEnter, "NextRoom", ChangeCurrentRoom);
        // user.AddEvent(Behaviour.Open, "Door", InitializeNextRoom);

        // CurrentRoom = startRoom;

        // if(useCenterStart) _users.Position = CurrentRoom.Position;

        // this.gameObject.layer = LayerMask.NameToLayer("VirtualEnvironment");
    }
}

public class RootManager : MonoBehaviour
{
    public EnvironmentManager environmentManager;
    public LocomotionSystem locomotion;

    public User user {
        get { return environmentManager.users.GetActiveUser(); }
    }

    public VirtualEnvironment virtualEnvironment {
        get { return environmentManager.virtualEnvironment; }
    }

    public RealSpace realSpace {
        get { return environmentManager.realSpace; }
    }

    private void Awake() {
        environmentManager = GetComponent<EnvironmentManager>();
        locomotion = GetComponent<LocomotionSystem>();

        environmentManager.Initiailizing(this);
        locomotion.Initiailizing(this);
        // UIManager.Instance.Initializing(this);
    }
}

public class SubManager : MonoBehaviour {

    protected RootManager _rootManager;

    public RootManager rootManager {
        get { return _rootManager; }
    }

    public virtual void Initiailizing(RootManager root) {
        _rootManager = root;
    }
}

public class EnvironmentManager : SubManager
{
    protected VirtualEnvironment _virtualEnvironment;
    protected Users _users;
    protected RealSpace _realSpace;

    public VirtualEnvironment virtualEnvironment {
        get { return _virtualEnvironment; }
    }

    public Users users {
        get { return _users; }
    }

    public RealSpace realSpace {
        get { return _realSpace; }
    }

    // Start is called before the first frame update
    public override void Initiailizing(RootManager root)
    {
        base.Initiailizing(root);

        foreach(Transform child in transform) {
            Transform2D tf = child.GetComponent<Transform2D>();

            if(tf is VirtualEnvironment)
                _virtualEnvironment = tf as VirtualEnvironment;
            else if(tf is Users)
                _users = tf as Users;
            else if(tf is RealSpace)
                _realSpace = tf as RealSpace;
        }

        

        foreach(Transform child in transform) {
            Transform2D tf = child.GetComponent<Transform2D>();
            if(tf != null) tf.Initializing();
        }
    }

}

public class LocomotionSystem : SubManager
{
    public override void Initiailizing(RootManager root) {
        base.Initiailizing(root);
    }
}