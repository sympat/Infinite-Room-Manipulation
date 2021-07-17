using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VirtualEnvironment : Transform2D
{
    public Room startRoom;
    public bool useCenterStart;
    public bool useFullVisualization;
    private Manager manager;
    private Dictionary<Room, List<Door>> adjList;
    private Users _users;
    private Room currentRoom, candidateRoom;
    private static int totalID = 1;
    private int id;

    public Users users {
        get { return _users; }
    }

    public Room CurrentRoom {
        get {
            return currentRoom;
        }
        set {
            if(currentRoom == value) return;
            if(value == null) return;
            if(currentRoom != null) currentRoom.gameObject.layer = LayerMask.NameToLayer("Room");

            currentRoom = value;
            currentRoom.gameObject.layer = LayerMask.NameToLayer("CurrentRoom");

            if(!useFullVisualization) {
                SwitchAllVisualization(false);
                SwtichRoomsVisualization(currentRoom, true);
            }

            List<Door> connectedDoors = GetConnectedDoors(currentRoom);

            foreach(var door in connectedDoors) 
                door.CloseDoor();

            Debug.Log("Current Room is changed " + currentRoom);
        }
    }

    public override void Initializing() {
        manager = this.transform.parent.GetComponent<Manager>();
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

        foreach(var room in rooms) {
            AddRoom(room);
        }

        foreach(var door in doors) {
            AddDoor(door);
        }

        _users = manager.users;
        User user = _users.GetActiveUser();

        user.AddEvent(Behaviour.CompletelyEnter, "NextRoom", ChangeCurrentRoom);
        user.AddEvent(Behaviour.Open, "Door", InitializeNextRoom);

        CurrentRoom = startRoom;

        if(useCenterStart) _users.Position = CurrentRoom.Position;

        this.gameObject.layer = LayerMask.NameToLayer("VirtualEnvironment");
    }

    public void AddRoom(Room room) {
        if(room == null) return;

        room.Initializing();

        adjList.Add(room, new List<Door>());
    }

    public void AddDoor(Door door) {
        if(door == null) return;

        door.Initializing(this);
        
        Room room1 = door.GetThisRoom();
        Room room2 = door.GetConnectedRoom();

        adjList[room1].Add(door);
        if(room2 != null) adjList[room2].Add(door);
    }

    public void SwitchRoomVisualization(Room room, bool isShow, bool toggleLight) {
        if(GetRoom(room) == null) return;

        room.gameObject.SetActive(isShow);
        room.gameObject.GetComponentInChildren<Light>().enabled = toggleLight;

        // room.ToggleTeleportArea(toggleTeleportArea);

        foreach(var door in GetConnectedDoors(room)) {
            door.gameObject.SetActive(isShow);
        }
    }

    public void SwtichRoomsVisualization(Room room, bool isShow) {
        SwitchRoomVisualization(room, isShow, true);

        List<Room> connectedRooms = GetConnectedRooms(room);

        if(connectedRooms == null) return;

        foreach(var connectedRoom in connectedRooms) {
            SwitchRoomVisualization(connectedRoom, isShow, false);
        }
    }

    public void SwitchAllVisualization(bool isShow) {
        foreach(Transform child in this.transform) {
            // if(child.gameObject.layer == LayerMask.NameToLayer("Player"))
            //     continue;
                
            child.gameObject.SetActive(isShow);
        }
    }

    public void MoveWallWithLimit(Room room, int wall, float translate, Room rootRoom) {
        if (GetRoom(room) == null) return;

        Door maxDoor = null, minDoor = null;
        float translate1 = 0, translate2 = 0, finalTranslate;
        float l, e, max, min, w1, w2, o;

        if(wall % 2 != 0) {
            maxDoor = GetMaxDoorInDirection(room, Direction.Y);
            minDoor = GetMinDoorInDirection(room, Direction.Y);

            if(maxDoor != null && minDoor != null) {
                o = room.Position.x;
                l = room.Size.x;
                e = maxDoor.Extents.x;
                max = room.Max.x;
                min = room.Min.x;
                w1 = maxDoor.GetThisRoomWrapper(room).weight;
                w2 = minDoor.GetThisRoomWrapper(room).weight;

                // Debug.Log(room);
                // Debug.Log($"maxDoor {maxDoor}");
                // Debug.Log($"minDoor {minDoor}");
                // Debug.Log($"l {l}");
                // Debug.Log($"e {e}");
                // Debug.Log($"max {max}");
                // Debug.Log($"min {min}");
                // Debug.Log($"w1 {w1}");
                // Debug.Log($"w2 {w2}");
                // Debug.Log(o + w1/2 * l + e);
                // Debug.Log(maxDoor.Max.x);
                // Debug.Log(o + w2/2 * l - e);
                // Debug.Log(minDoor.Min.x);

                if(wall == 3) {
                    translate1 = (o + w1 / 2 * l + e - max) / ((1 - w1) / 2);
                    translate2 = (o + w2 / 2 * l - e - min) / (- (1 + w2) / 2);
                }
                else {
                    translate1 = (o + w1 / 2 * l + e - max) / (- (1 - w1) / 2);
                    translate2 = (o + w2 / 2 * l - e - min) / ((1 + w2) / 2);
                }

                // Debug.Log($"translate {translate}");
                // Debug.Log($"fit to MaxDoor {translate1}");
                // Debug.Log($"fit to MinDoor {translate2}");

                float shrinkDirection = 0;
                if(wall == 1) shrinkDirection = 1.0f;
                else if(wall == 3) shrinkDirection = -1.0f;

                if(translate * shrinkDirection < 0) {
                     finalTranslate = translate;
                }
                else {
                    if(Mathf.Abs(translate1) < 0.01f) translate1 = 0;
                    if(Mathf.Abs(translate2) < 0.01f) translate2 = 0;

                    float minLimitTranslate = Mathf.Min(Mathf.Abs(translate1), Mathf.Abs(translate2));
                    minLimitTranslate *= shrinkDirection;

                    if(Mathf.Abs(translate) < Mathf.Abs(minLimitTranslate)) {
                        finalTranslate = translate;
                    }
                    else {
                        finalTranslate = minLimitTranslate;
                    }
                }


                // finalTranslate = (Mathf.Abs(translate1) < Mathf.Abs(translate2)) ? translate1 : translate2;
                // if(finalTranslate * translate < 0)
                
                //  finalTranslate = translate; // translate이 translate1,2 와 다른 방향일 경우 그냥 translate을 적용
                // else if(Mathf.Abs(translate) < Mathf.Abs(finalTranslate)) finalTranslate = translate; // 같은 방향

                // Debug.Log($"finalTranslate {finalTranslate}");

            }
            else {
                finalTranslate = translate;
            }
        }
        else {
            maxDoor = GetMaxDoorInDirection(room, Direction.X);
            minDoor = GetMinDoorInDirection(room, Direction.X);

            if(maxDoor != null && minDoor != null) {
                o = room.Position.y;
                l = room.Size.y;
                e = maxDoor.Extents.x;
                max = room.Max.y;
                min = room.Min.y;
                w1 = maxDoor.GetThisRoomWrapper(room).weight;
                w2 = minDoor.GetThisRoomWrapper(room).weight;

                if(wall == 0) {
                    translate1 = (o + w1 / 2 * l + e - max) / ((1 - w1) / 2);
                    translate2 = (o + w2 / 2 * l - e - min) / (- (1 + w2) / 2);
                }
                else {
                    translate1 = (o + w1 / 2 * l + e - max) / (- (1 - w1) / 2);
                    translate2 = (o + w2 / 2 * l - e - min) / ((1 + w2) / 2);
                }

                // finalTranslate = (Mathf.Abs(translate1) < Mathf.Abs(translate2)) ? translate1 : translate2;
                // if(finalTranslate * translate < 0) finalTranslate = translate;
                // else if(Mathf.Abs(translate) < Mathf.Abs(finalTranslate)) finalTranslate = translate;

                float shrinkDirection = 0;
                if(wall == 2) shrinkDirection = 1.0f;
                else if(wall == 0) shrinkDirection = -1.0f;

                if(translate * shrinkDirection < 0) {
                     finalTranslate = translate;
                }
                else {
                    if(Mathf.Abs(translate1) < 0.01f) translate1 = 0;
                    if(Mathf.Abs(translate2) < 0.01f) translate2 = 0;

                    float minLimitTranslate = Mathf.Min(Mathf.Abs(translate1), Mathf.Abs(translate2));
                    minLimitTranslate *= shrinkDirection;

                    if(Mathf.Abs(translate) < Mathf.Abs(minLimitTranslate)) {
                        finalTranslate = translate;
                    }
                    else {
                        finalTranslate = minLimitTranslate;
                    }
                }
            }
            else {
                finalTranslate = translate;
            }
        }

        // Debug.Log($"translate1 {translate1}");
        // Debug.Log($"translate2 {translate2}");
        // Debug.Log($"translate {translate}");
        // Debug.Log($"finalTranslate {finalTranslate}");

        MoveWall(room, wall, finalTranslate, rootRoom);
    }


    public void MoveWall(Room room, int wall, float translate, Room rootRoom = null)
    {
        if (GetRoom(room) == null) return;

        room.MoveEdge(wall, translate); // 현재 방의 wall을 이동시킨다
        PlaceAllDoorAndRoom(room, rootRoom); 
    }

    // public void MoveWall(Room room, int wall, Vector2 translate, Room rootRoom = null) {
    //     if (GetRoom(room) == null) return;

    //     if(wall % 2 == 0)
    //         MoveWall(room, wall, translate.y, rootRoom);
    //     else
    //         MoveWall(room, wall, translate.x, rootRoom);
    // }

    public void MoveDoor(Room room, Door door, float translate, Room rootRoom = null)
    {
        if (GetRoom(room) == null) return;

        door.GetThisRoomWrapper(room).weight = translate;
        PlaceAllDoorAndRoom(room, rootRoom);
    }

    public void PlaceAllDoorAndRoom(Room room, Room rootRoom = null) { // 현재 프로그램은 acyclic graph(tree) 로 가정. rootRoom은 이를 위한 변수. TODO: Cycle을 형성하는 graph에서의 정상 동작하게끔 구현
        if (GetRoom(room) == null) return;

        User user = _users.GetActiveUser();
        Dictionary<Room, bool> visited = new Dictionary<Room, bool>();
        foreach (var kv in adjList)
        {
            visited.Add(kv.Key, false);
        }

        Queue<Room> q = new Queue<Room>();

        Room v = room;
        visited[v] = true;
        q.Enqueue(v);

        if(rootRoom != null)
            visited[rootRoom] = true;

        Room u;
        while (q.Count != 0)
        {
            u = q.Dequeue(); // currentRoom
            foreach (var door in adjList[u])
            {
                Room w = door.GetConnectedRoom(u); // nextRoom

                door.PlaceDoor(u);

                // if(u == currentRoom && user.IsTargetInUserFov(door.Position)) {
                //     door.FixDoor(door.Position, u);
                // }
                // else {
                //     door.PlaceDoor(u);
                // }

                if(w == null) continue;
                else if (!visited[w])
                {
                    door.PlaceConnectedRoom(u);
                    visited[w] = true;
                    q.Enqueue(w);
                }
            }
        }
    }

    public List<Room> GetConnectedRooms(Room v, bool containSelf = false)
    {
        if (GetRoom(v) == null) return null;

        List<Room> result = new List<Room>();
        foreach(var door in adjList[v])
        {
            Room targetRoom = door.GetConnectedRoom(v);
            if(targetRoom != null)
                result.Add(targetRoom);
        }

        if (containSelf)
            result.Add(v);

        return result;
    }

    public List<Door> GetConnectedDoors(Room v)
    {
        if (GetRoom(v) == null) return null;
        return adjList[v];
    }

    // public List<Door> GetDoorsInWall(Room v, int wall) {
    //     List<Door> result = new List<Door>();

    //     foreach(var door in adjList[v]) {
    //         if(door.GetThisRoomWrapper(v).wall == wall)
    //             result.Add(door);
    //     }

    //     return result;
    // }

    // type 축으로 고정되어 있는 문들을 반환
    public List<Door> GetDoorsInDirection(Room v, Direction type) {
        List<Door> result = new List<Door>();

        foreach(var door in adjList[v]) {
            if(door.CheckWallDirection() == type)
                result.Add(door);
        }

        return result;
    }

    // type 방향에 있는 문들 중에서 Max 값을 반환
    public Door GetMaxDoorInDirection(Room v, Direction type) { //TODO : align 된 좌표계에서만 통용
        List<Door> doors = GetDoorsInDirection(v, type);

        float temp = float.MinValue;
        Door result = null;

        if(type == Direction.Y) {
            foreach(var door in doors) {
                if(temp < door.Max.x) {
                    temp = door.Max.x;
                    result = door;
                }
            }
        }
        else if(type == Direction.X) {
            foreach(var door in doors) {
                if(temp < door.Max.y) {
                    temp = door.Max.y;
                    result = door;

                }
            }
        }

        return result;
    }

    // type 방향에 있는 문들 중에서 Min 값을 반환
    public Door GetMinDoorInDirection(Room v, Direction type) { //TODO : align 된 좌표계에서만 통용
        List<Door> doors = GetDoorsInDirection(v, type);

        float temp = float.MaxValue;
        Door result = null;

        if(type == Direction.Y) {
            foreach(var door in doors) {
                if(temp > door.Min.x) {
                    temp = door.Min.x;
                    result = door;
                }
            }
        }
        else if(type == Direction.X) {
            foreach(var door in doors) {
                if(temp > door.Min.y) {
                    temp = door.Min.y;
                    result = door;

                }
            }
        }

        return result;
    }

    public Room GetRoom(int id) {
        foreach (var kv in adjList)
        {
            if (kv.Key.ID == id)
                return kv.Key;
        }

        return null;
    }

    public Room GetRoom(Room v)
    {
        if(v == null) return null;
        return GetRoom(v.ID);
    }

    public Door GetDoor(Room v, Room u) {
        foreach(var door in adjList[v]) {
            if(door.GetConnectedRoom(v) == u)
                return door;
        }

        return null;
    }

    public void InitializeNextRoom(GameObject target) {
        // Debug.Log("InitializeNextRoom");

        Door openedDoor = target.GetComponent<Door>();
        if(openedDoor == null) throw new System.Exception("target object does not have Door Component");

        List<Room> neighborRooms = GetConnectedRooms(CurrentRoom);
        List<Door> connectedDoors = GetConnectedDoors(CurrentRoom);

        foreach(var door in connectedDoors)
            if(door != openedDoor)
                door.CloseDoor();

        foreach(var room in neighborRooms) {
            
            room.gameObject.layer = LayerMask.NameToLayer("Room");
            // room.ToggleTeleportArea(false);
        }
        
        Room nextRoom = openedDoor.GetConnectedRoom(CurrentRoom);
        nextRoom.gameObject.layer = LayerMask.NameToLayer("NextRoom");
        // nextRoom.ToggleTeleportArea(true);
    }

    private void ChangeCurrentRoom(GameObject target) {
        // Debug.Log("ChangeCurrentRoom");

        Room targetRoom = target.GetComponent<Room>();

        if(targetRoom == null) throw new System.Exception("target object does not have Room Component");

        CurrentRoom = targetRoom;
    }

    public void ToggleConnectedDoors(Room v, bool enabled) {
        List<Door> doors = GetConnectedDoors(v);

        foreach(Door door in doors) {
            if(door.transform.childCount != 0)
                door.ToggleDoorInteraction(enabled);
        }
    }
}
