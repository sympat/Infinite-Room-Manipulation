using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Direction {X, Y};

public class Door : Bound2D
{
    public Room room1, room2;

    [Range(0, 3)]
    public int attachedWall;
    private RoomWrapper source, target;
    private bool isOpened = false;
    private VirtualEnvironment virtualEnvironment;

    public bool IsOpen {
        get { return isOpened; }
    }

    public override void Initializing()
    {
        base.Initializing();

        int wall = -1;
        if(room1 == null) throw new System.Exception("Room1 is required");
        if(room2 == null) wall = GetContactWall(room1);
        else wall = room1.GetContactEdge(room2);

        source = new RoomWrapper(room1, wall, this.Position);
        target = new RoomWrapper(room2, (wall + 2) % 4, this.Position);

        this.gameObject.layer = LayerMask.NameToLayer("Door");
    }

    public void Initializing(VirtualEnvironment parentVE) {
        virtualEnvironment = parentVE;
        Initializing();
    }

    public int GetContactWall(Room room) {
        return attachedWall;
        // Room v = room; // TODO : GetThisRoom

        // if(this.Rotation % 2 == 0) {
        //     if(this.Position.y > v.Position.y)
        //         return 0;
        //     else
        //         return 2;
        // }
        // else {
        //     if(this.Position.x > v.Position.x)
        //         return 3;
        //     else
        //         return 1;
        // }
    }
    
    public Room GetConnectedRoom(Room currentRoom = null) {
        if(currentRoom == null) return target.room;
        return GetConnectedRoomWrapper(currentRoom).room;
    }

    public RoomWrapper GetConnectedRoomWrapper(Room currentRoom)
    {
        if (currentRoom.Equals(source.room)) return target;
        else if (currentRoom.Equals(target.room)) return source;
        else return null;
    }

    public Room GetThisRoom(Room currentRoom = null) {
        if(currentRoom == null) return source.room;
        return GetThisRoomWrapper(currentRoom).room;
    }

    public RoomWrapper GetThisRoomWrapper(Room currentRoom)
    {
        if (currentRoom.Equals(source.room)) return source;
        else if (currentRoom.Equals(target.room)) return target;
        else return null;
    }

    public Vector2 GetDoorSize() {
        GameObject doorFrame = Utility.GetChildWithLayer(this.gameObject, "Door Frame");
        BoxCollider box = doorFrame.GetComponent<BoxCollider>();

        return CastVector3Dto2D(box.size);
    }

    public void OpenDoor()
    {
        Debug.Log($"{this.gameObject} OpenDoor");
        // Debug.Log("OpenDoor");
        // virtualEnvironment.ReadyToMoveNextRoom(this);

        GameObject doorMain = Utility.GetChildWithLayer(this.gameObject, "Door Main");
        doorMain.GetComponent<BoxCollider>().enabled = true;
        BoxCollider box = doorMain.GetComponent<BoxCollider>();
        doorMain.GetComponent<BoxCollider>().enabled = false;

        GameObject grabble = Utility.GetChildWithLayer(doorMain, "Grabble");
        grabble.GetComponent<SphereCollider>().enabled = true;

        List<GameObject> knob = Utility.GetChildrenWithLayer(doorMain, "Knob");
        knob.ForEach(x => x.GetComponent<MeshCollider>().enabled = false);

        if(room1 != null) {
            Vector3 doorMin = room1.transform.InverseTransformPoint(doorMain.transform.TransformPoint(box.center - (box.size / 2)));
            Vector3 doorMax = room1.transform.InverseTransformPoint(doorMain.transform.TransformPoint(box.center + (box.size / 2)));

            room1.GetComponent<MeshRenderer>().material.SetVector("DigonalPos1", new Vector4(doorMin.x, doorMin.y, doorMin.z, 0));
            room1.GetComponent<MeshRenderer>().material.SetVector("DigonalPos2", new Vector4(doorMax.x, doorMax.y, doorMax.z, 0));

            Debug.Log($"room1: {room1.gameObject}, doorMIn: {doorMin}");
            Debug.Log($"room1: {room1.gameObject}, doorMax: {doorMax}");
        }

        if(room2 != null) {
            Vector3 doorMin = room2.transform.InverseTransformPoint(doorMain.transform.TransformPoint(box.center - box.size / 2));
            Vector3 doorMax = room2.transform.InverseTransformPoint(doorMain.transform.TransformPoint(box.center + box.size / 2));

            Debug.Log($"room2: {room2.gameObject}, doorMIn: {doorMin}");
            Debug.Log($"room2: {room2.gameObject}, doorMax: {doorMax}");

            room2.GetComponent<MeshRenderer>().material.SetVector("DigonalPos1", new Vector4(doorMin.x, doorMin.y, doorMin.z, 0));
            room2.GetComponent<MeshRenderer>().material.SetVector("DigonalPos2", new Vector4(doorMax.x, doorMax.y, doorMax.z, 0));
        }

        isOpened = true;

        User user = virtualEnvironment.users.GetActiveUser();
        UserEventArgs caller = new UserEventArgs(Behaviour.Open, this.gameObject);
        user.ProcessingEvent(caller);
    }

    public void CloseDoor()
    {
        Debug.Log($"{this.gameObject} CloseDoor");

        GameObject doorMain = Utility.GetChildWithLayer(this.gameObject, "Door Main");

        if(isOpened) {
            doorMain.transform.localRotation = Quaternion.identity;
            AudioSource.PlayClipAtPoint(SoundSetting.Instance.doorCloseSound, this.transform.position);

            foreach(var drive in doorMain.GetComponentsInChildren<CustomCircularDrive>()) {
                drive.ResetRotation();
            }

            GameObject grabble = Utility.GetChildWithLayer(doorMain, "Grabble");
            grabble.GetComponent<SphereCollider>().enabled = false;

            List<GameObject> knob = Utility.GetChildrenWithLayer(doorMain, "Knob");
            knob.ForEach(x => x.GetComponent<MeshCollider>().enabled = true);

            if(room1 != null && room1.gameObject.layer != LayerMask.NameToLayer("CurrentRoom")) {
                Debug.Log($"Reset DigonalPos room1: {room1}");
                room1.GetComponent<MeshRenderer>().material.SetVector("DigonalPos1", new Vector4(0, 0, 0, 0));
                room1.GetComponent<MeshRenderer>().material.SetVector("DigonalPos2", new Vector4(0, 0, 0, 0));
            }

            if(room2 != null && room2.gameObject.layer != LayerMask.NameToLayer("CurrentRoom")) {
                Debug.Log($"Reset DigonalPos room2: {room2}");

                room2.GetComponent<MeshRenderer>().material.SetVector("DigonalPos1", new Vector4(0, 0, 0, 0));
                room2.GetComponent<MeshRenderer>().material.SetVector("DigonalPos2", new Vector4(0, 0, 0, 0));
            }
        }

        isOpened = false;
    }

    // public void UpdateWall(Room currentRoom, int wall) {
    //     GetThisRoomWrapper(currentRoom).wall = wall;
    //     GetConnectedRoomWrapper(currentRoom).wall = (wall + 2) % 4;
    // }

    // public void UpdateDoorWeight(Room currentRoom, int wall) 
    // {
    //     Room v = GetThisRoomWrapper(currentRoom).room;
    //     float newWeight = 0;

    //     if (wall == 0 || wall == 2)
    //     {
    //         newWeight = 2 * (this.Position.x - v.Min.x) / (v.Max.x - v.Min.x) - 1;
    //     }
    //     else if (wall == 1 || wall == 3)
    //     {
    //         newWeight = 2 * (this.Position.y - v.Min.y) / (v.Max.y - v.Min.y) - 1;
    //     }

    //     GetThisRoomWrapper(currentRoom).weight = newWeight;
    // }

    // public void PlaceDoorAndConnectedRoom(Room currentRoom) {
    //     float doorXPos = 0, doorYPos = 0, roomXPos = 0, roomYPos = 0;

    //     Room v = GetThisRoomWrapper(currentRoom).room;
    //     Room u = GetConnectedRoomWrapper(currentRoom).room;
    //     int vWall = GetThisRoomWrapper(currentRoom).wall;
    //     float vWeight = GetThisRoomWrapper(currentRoom).weight;
    //     float uWeight = GetConnectedRoomWrapper(currentRoom).weight;

    //     if (vWall == 0 || vWall == 2)
    //     {
    //         doorXPos = (vWeight + 1) / 2 * (v.Max.x - v.Min.x) + v.Min.x; // 방 v 기준 문의 x축 위치
    //         doorYPos = (vWall == 0) ? v.Max.y : v.Min.y;

    //         if(u != null) {
    //             roomXPos = doorXPos - ((uWeight * u.Size.x) / 2);
    //             roomYPos = (vWall == 0) ? v.Max.y + u.Extents.y : v.Min.y - u.Extents.y;
    //         }
    //     }
    //     else if (vWall == 1 || vWall == 3)
    //     {
    //         doorXPos = (vWall == 3) ? v.Max.x : v.Min.x;
    //         doorYPos = (vWeight + 1) / 2 * (v.Max.y - v.Min.y) + v.Min.y;

    //         if(u != null) {
    //             roomXPos = (vWall == 3) ? v.Max.x + u.Extents.x : v.Min.x - u.Extents.x;
    //             roomYPos = doorYPos - ((uWeight * u.Size.y) / 2);
    //         }
    //     }

    //     if(u != null) u.Position = new Vector2(roomXPos, roomYPos);
    //     this.Position = new Vector2(doorXPos, doorYPos);
    // }

    public void PlaceDoor(Room currentRoom) {
        float doorXPos = 0, doorYPos = 0;

        Room v = GetThisRoomWrapper(currentRoom).room;
        Room u = GetConnectedRoomWrapper(currentRoom).room;
        int vWall = GetThisRoomWrapper(currentRoom).wall;
        float vWeight = GetThisRoomWrapper(currentRoom).weight;
        float uWeight = GetConnectedRoomWrapper(currentRoom).weight;

        if (vWall == 0 || vWall == 2)
        {
            doorXPos = (vWeight + 1) / 2 * (v.Max.x - v.Min.x) + v.Min.x; // 방 v 기준 문의 x축 위치
            doorYPos = (vWall == 0) ? v.Max.y : v.Min.y;
        }
        else if (vWall == 1 || vWall == 3)
        {
            doorXPos = (vWall == 3) ? v.Max.x : v.Min.x;
            doorYPos = (vWeight + 1) / 2 * (v.Max.y - v.Min.y) + v.Min.y;
        }

        this.Position = new Vector2(doorXPos, doorYPos);
    }

    public void PlaceConnectedRoom(Room currentRoom) {
        float doorXPos = this.Position.x, doorYPos = this.Position.y;
        float roomXPos = 0, roomYPos = 0;

        Room v = GetThisRoomWrapper(currentRoom).room;
        Room u = GetConnectedRoomWrapper(currentRoom).room;
        int vWall = GetThisRoomWrapper(currentRoom).wall;
        float vWeight = GetThisRoomWrapper(currentRoom).weight;
        float uWeight = GetConnectedRoomWrapper(currentRoom).weight;

        if(u == null) return;

        if (vWall == 0 || vWall == 2)
        {
            roomXPos = doorXPos - ((uWeight * u.Size.x) / 2);
            roomYPos = (vWall == 0) ? v.Max.y + u.Extents.y : v.Min.y - u.Extents.y;
        }
        else if (vWall == 1 || vWall == 3)
        {
            roomXPos = (vWall == 3) ? v.Max.x + u.Extents.x : v.Min.x - u.Extents.x;
            roomYPos = doorYPos - ((uWeight * u.Size.y) / 2);
        }

        u.Position = new Vector2(roomXPos, roomYPos);
    }

    public void FixDoor(Vector2 currentDoorPos, Room currentRoom) 
    {
        Room v = GetThisRoomWrapper(currentRoom).room;
        int vWall = GetThisRoomWrapper(currentRoom).wall;
        float newWeight = 0;

        if (vWall == 0 || vWall == 2)
        {
            newWeight = 2 * (currentDoorPos.x - v.Min.x) / (v.Max.x - v.Min.x) - 1;

        }
        else if (vWall == 1 || vWall == 3)
        {
            newWeight = 2 * (currentDoorPos.y - v.Min.y) / (v.Max.y - v.Min.y) - 1;
        }

        GetThisRoomWrapper(currentRoom).weight = newWeight;
    }

    public Direction CheckWallDirection()
    {
        if (source.wall % 2 == 0 && target.wall % 2 == 0) // true - y
            return Direction.Y;
        else if (source.wall % 2 != 0 && target.wall % 2 != 0) // false - x
            return Direction.X;
        else
            throw new System.Exception("Invalid wall");
    }

    public void MoveConnectedRoom(Room v, float translate, bool moveAlongWall = true)
    {
        Room u = GetConnectedRoomWrapper(v).room;

        if (CheckWallDirection() == Direction.Y) // y축으로 서로 연결된 경우
        {
            if (moveAlongWall)
            {
                u.transform.Translate(Vector2.right * translate); // 연결된 방을 x축으로 이동시킨다
                this.transform.Translate(Vector2.right * translate); // 문을 x축으로 이동시킨다
            }
            else
            {
                u.transform.Translate(Vector2.up * translate); // 연결된 방을 y축으로 이동시킨다
                this.transform.Translate(Vector2.up * translate); // 문을 y축으로 이동시킨다
            }
        }
        else // x축으로 서로 연결된 경우
        {
            if (moveAlongWall)
            {
                u.transform.Translate(Vector2.up * translate); // 연결된 방을 y축으로 이동시킨다
                this.transform.Translate(Vector2.up * translate); // 문을 y축으로 이동시킨다
            }
            else
            {
                u.transform.Translate(Vector2.right * translate); // 연결된 방을 x축으로 이동시킨다
                this.transform.Translate(Vector2.right * translate); // 문을 x축으로 이동시킨다
            }
        }
    }


    public override string ToString()
    {
        string result = "";
        result += string.Format("ID: {0}", id);
        result += string.Format(", ObjName: {0}", this.gameObject.name);
        result += "\n" + source;
        result += "\n" + target;

        return result;
    }

}
