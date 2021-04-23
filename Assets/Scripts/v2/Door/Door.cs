using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door : Transform2D
{
    public Room room1, room2;
    private static int totalID = 0;
    private int id;
    private RoomWrapper source, target;
    private bool isOpened = false;

    public override void Initializing()
    {
        id = totalID++;
        // Debug.Log("Initialzing door " + this.gameObject.name);

        int wall = -1;
        if(room1 == null) throw new System.Exception("Room1 is required");
        if(room2 == null) wall = GetContactWall(room1);
        else wall = room1.GetContactEdge(room2);

        source = new RoomWrapper(room1, wall, this.Position);
        target = new RoomWrapper(room2, (wall + 2) % 4, this.Position);

        this.gameObject.layer = LayerMask.NameToLayer("Door");
    }

    public int GetContactWall(Room room) {
        if(room.Min.x < this.Position.x && this.Position.x < room.Max.x) {
            if(this.Position.y > room.Position.y)
                return 0;
            else
                return 2;
        }

        if(room.Min.y < this.Position.y && this.Position.y < room.Max.y) {
            if(this.Position.x > room.Position.x)
                return 3;
            else
                return 1;
        }

        throw new System.Exception("Invalid door position");
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

    // public void UpdateDoorPosition(Room currentRoom, int wall) {
    //     switch(wall) {
    //         case 0:
    //             this.Position = new Vector2(this.Position.x, currentRoom.Max.y);
    //             break;
    //         case 1:
    //             this.Position = new Vector2(currentRoom.Min.x, this.Position.y);
    //             break;
    //         case 2:
    //             this.Position = new Vector2(this.Position.x, currentRoom.Min.y);
    //             break;
    //         case 3:
    //             this.Position = new Vector2(currentRoom.Max.x, this.Position.y);
    //             break;
    //         default:
    //             throw new System.Exception("Positions of connected Room are invalid");
    //     }
    // }

    public void OpenDoor()
    {
        Debug.Log("OpenDoor");

        GameObject doorMain = Utility.GetChildWithLayer(this.gameObject, "Door Main");
        doorMain.GetComponent<BoxCollider>().enabled = true;
        Bounds box = doorMain.GetComponent<BoxCollider>().bounds;
        doorMain.GetComponent<BoxCollider>().enabled = false;

        GameObject grabble = Utility.GetChildWithLayer(doorMain, "Grabble");
        grabble.GetComponent<SphereCollider>().enabled = true;

        List<GameObject> knob = Utility.GetChildrenWithLayer(doorMain, "Knob");
        knob.ForEach(x => x.GetComponent<MeshCollider>().enabled = false);

        // Debug.Log(new Vector4(box.min.x - box.center.x, box.min.y - box.center.y, box.min.z - box.center.z, 0));
        // Debug.Log(new Vector4(box.max.x - box.center.x, box.max.y - box.center.y, box.max.z - box.center.z, 0));
        // Debug.Log(new Vector4(box.min.x - room1.Position.x, box.min.y, box.min.z - room1.Position.y, 0));
        // Debug.Log(new Vector4(box.max.x - room1.Position.x, box.max.y, box.max.z - room1.Position.y, 0));

        if(room1 != null) {
            room1.GetComponent<MeshRenderer>().material.SetVector("_DoorMin", new Vector4(box.min.x - room1.Position.x, box.min.y, box.min.z - room1.Position.y, 0));
            room1.GetComponent<MeshRenderer>().material.SetVector("_DoorMax", new Vector4(box.max.x - room1.Position.x, box.max.y, box.max.z - room1.Position.y, 0));
        }

        if(room2 != null) {
            room2.GetComponent<MeshRenderer>().material.SetVector("_DoorMin", new Vector4(box.min.x - room2.Position.x, box.min.y, box.min.z - room2.Position.y, 0));
            room2.GetComponent<MeshRenderer>().material.SetVector("_DoorMax", new Vector4(box.max.x - room2.Position.x, box.max.y, box.max.z - room2.Position.y, 0));
        }

        isOpened = true;
    }

    public void CloseDoor()
    {
        Debug.Log("CloseDoor");

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

            if(room1 != null) {
                room1.GetComponent<MeshRenderer>().material.SetVector("_DoorMin", new Vector4(0, 0, 0, 0));
                room1.GetComponent<MeshRenderer>().material.SetVector("_DoorMax", new Vector4(0, 0, 0, 0));
            }

            if(room2 != null) {
                room2.GetComponent<MeshRenderer>().material.SetVector("_DoorMin", new Vector4(0, 0, 0, 0));
                room2.GetComponent<MeshRenderer>().material.SetVector("_DoorMax", new Vector4(0, 0, 0, 0));
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

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Door objAsRoom = obj as Door;
        if (objAsRoom == null) return false;
        else return Equals(objAsRoom);
    }

    public bool Equals(Door v)
    {
        if (v == null) return false;
        else return (this.id == v.id);
    }

        public override int GetHashCode()
    {
        return this.id;
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

    public void PlaceDoorAndConnectedRoom(Room currentRoom) {
        float doorXPos = 0, doorYPos = 0, roomXPos = 0, roomYPos = 0;

        Room v = GetThisRoomWrapper(currentRoom).room;
        Room u = GetConnectedRoomWrapper(currentRoom).room;
        int vWall = GetThisRoomWrapper(currentRoom).wall;
        float vWeight = GetThisRoomWrapper(currentRoom).weight;
        float uWeight = GetConnectedRoomWrapper(currentRoom).weight;

        if (vWall == 0 || vWall == 2)
        {
            doorXPos = (vWeight + 1) / 2 * (v.Max.x - v.Min.x) + v.Min.x; // 방 v 기준 문의 x축 위치
            doorYPos = (vWall == 0) ? v.Max.y : v.Min.y;
            // doorYPos = doorYPos - doorDist;

            if(u != null) {
                roomXPos = doorXPos - ((uWeight * u.Size.x) / 2);
                roomYPos = (vWall == 0) ? v.Max.y + u.Extents.y : v.Min.y - u.Extents.y;
            }
        }
        else if (vWall == 1 || vWall == 3)
        {
            doorXPos = (vWall == 3) ? v.Max.x : v.Min.x;
            // doorXPos = doorXPos - doorDist;

            doorYPos = (vWeight + 1) / 2 * (v.Max.y - v.Min.y) + v.Min.y;

            if(u != null) {
                roomXPos = (vWall == 3) ? v.Max.x + u.Extents.x : v.Min.x - u.Extents.x;
                roomYPos = doorYPos - ((uWeight * u.Size.y) / 2);
            }
        }

        if(u != null) u.Position = new Vector2(roomXPos, roomYPos);
        this.Position = new Vector2(doorXPos, doorYPos);
    }

    public void UpdateDoorWeight(Vector2 currentDoorPos, Room currentRoom) 
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

    public bool CheckWallDirection()
    {
        if (source.wall % 2 == 0 && target.wall % 2 == 0) // true - y
            return true;
        else if (source.wall % 2 != 0 && target.wall % 2 != 0) // false - x
            return false;
        else
            throw new System.Exception("Invalid wall");
    }

    public void MoveConnectedRoom(Room v, float translate, bool moveAlongWall = true)
    {
        Room u = GetConnectedRoomWrapper(v).room;

        if (CheckWallDirection()) // y축으로 서로 연결된 경우
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
}
