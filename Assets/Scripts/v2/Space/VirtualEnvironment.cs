using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualEnvironment : Transform2D
{
    public User user;
    public Room startRoom;
    public bool useCenterStart;
    public bool useFullVisualization;
    private Dictionary<Room, List<Door>> adjList;
    private Room currentRoom;
    private static int totalID = 1;
    private int id;

    public UserBody userBody {
        get {
            return user.GetTrackedUserBody();
        }
    }

    public Room CurrentRoom {
        get {
            return currentRoom;
        }
        set {
            if(currentRoom == value) return;
            if(currentRoom != null) {
                currentRoom.gameObject.tag = "Untagged";
                GetDoor(currentRoom, value).CloseDoor();
            }
            currentRoom = value;
            currentRoom.gameObject.tag = "CurrentRoom";
            
            if(!useFullVisualization) {
                SwitchAllVisualization(false);
                SwtichRoomsVisualization(currentRoom, true);
            }

            Debug.Log("Current Room is changed " + currentRoom);
        }
    }

    public override void Initializing() {
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

        // userBody.Initializing();
        // AddUser(startUser);
        userBody.AddEnterNewRoomEvent(ChangeCurrentRoom);

        CurrentRoom = startRoom;

        if(useCenterStart) userBody.Position = CurrentRoom.Position;

        this.gameObject.layer = LayerMask.NameToLayer("VirtualEnvironment");
        this.gameObject.tag = "VirtualEnvironment";
    }

    public void AddRoom(Room room) {
        if(room == null) return;

        room.Initializing();

        adjList.Add(room, new List<Door>());
    }

    public void AddDoor(Door door) {
        if(door == null) return;

        door.Initializing();
        
        Room room1 = door.GetThisRoom();
        Room room2 = door.GetConnectedRoom();

        adjList[room1].Add(door);
        if(room2 != null) adjList[room2].Add(door);
    }

    // public void AddUser(User user) {
    //     if(user == null) return;

    //     this.user = user;
    //     userBody.AddEnterRoomEvent(ChangeCurrentRoom);
    // }

    public void SwitchRoomVisualization(Room room, bool isShow) {
        if(GetRoom(room) == null) return;

        room.gameObject.SetActive(isShow);

        foreach(var door in GetConnectedDoors(room)) {
            door.gameObject.SetActive(isShow);
        }
    }

    public void SwtichRoomsVisualization(Room room, bool isShow) {
        SwitchRoomVisualization(room, isShow);

        List<Room> connectedRooms = GetConnectedRooms(room);

        if(connectedRooms == null) return;

        foreach(var connectedRoom in connectedRooms) {
            SwitchRoomVisualization(connectedRoom, isShow);
        }
    }

    public void SwitchAllVisualization(bool isShow) {
        foreach(Transform child in this.transform) {
            if(child.gameObject.layer == LayerMask.NameToLayer("Player"))
                continue;
                
            child.gameObject.SetActive(isShow);
        }
    }

    public void MoveWall(Room room, int wall, float translate, Room rootRoom = null)
    {
        if (GetRoom(room) == null) return;

        room.MoveEdge(wall, translate); // 현재 방의 wall을 이동시킨다
        PlaceAllDoorAndRoom(room, rootRoom); 
    }

    public void MoveWall(Room room, int wall, Vector2 translate, Room rootRoom = null) {
        if (GetRoom(room) == null) return;

        if(wall % 2 == 0)
            MoveWall(room, wall, translate.y, rootRoom);
        else
            MoveWall(room, wall, translate.x, rootRoom);
    }

    public void MoveDoor(Room room, Door door, float translate, Room rootRoom = null)
    {
        if (GetRoom(room) == null) return;

        door.GetThisRoomWrapper(room).weight = translate;
        PlaceAllDoorAndRoom(room, rootRoom);
    }

    public void PlaceAllDoorAndRoom(Room room, Room rootRoom = null) { // 현재 프로그램은 acyclic graph(tree) 로 가정. rootRoom은 이를 위한 변수. TODO: Cycle을 형성하는 graph에서의 정상 동작하게끔 구현
        if (GetRoom(room) == null) return;

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

                door.PlaceDoorAndConnectedRoom(u);

                if(w == null) continue;
                else if (!visited[w])
                {
                    visited[w] = true;
                    
                    // if(u == currentRoom && user.IsTargetInUserFov(door.Position)) {
                    //     door.UpdateDoorWeight(door.Position, u);
                    // }
                    // else {
                    //     door.PlaceDoorAndConnectedRoom(u);
                    // }

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
            result.Add(door.GetConnectedRoom(v));
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

    private void ChangeCurrentRoom(Room targetRoom) {
        CurrentRoom = targetRoom;
    }

    public void CloseDoors() {

    }
}
