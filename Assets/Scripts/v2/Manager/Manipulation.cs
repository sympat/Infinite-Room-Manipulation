using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class Manipulation : Experiment2
{
    private float DT = 1.2f;

    public override void Awake() {
        base.Awake();
 
        // users.AddEnterNewRoomEvent(SwitchEnable);
        // users.AddEnterNewRoomEvent(OvertManipulate);
    }

    public bool[] CheckWallVisibleToUser(Room currentRoom, User user) {
        bool[] isVisibleToUser = new bool[4];
        for (int i = 0; i < 4; i++)
            isVisibleToUser[i] = false;

        // 벽이 사용자 시야에 있는지를 판단
        for (int i = 0; i < 4; i++)
        {
            Vector2 vertex1 = currentRoom.GetVertex2D(Utility.mod(i, 4), Space.World);
            Vector2 vertex2 = currentRoom.GetVertex2D(Utility.mod(i + 1, 4), Space.World);

            if(user.IsTargetInUserFov(vertex1, vertex2)) {
                isVisibleToUser[Utility.mod(i, 4)] = true;
            }
        }

        return isVisibleToUser;
    }

    public bool[] CheckWallCenterVisibleToUser(Room currentRoom, User user) {
        bool[] isCenterVisibleToUser = new bool[4];
        for (int i = 0; i < 4; i++)
            isCenterVisibleToUser[i] = false;

                // 벽이 사용자 시야에 있는지를 판단
        for (int i = 0; i < 4; i++)
        {
            Vector2 wallCenter = currentRoom.GetEdge2D(i, Space.World);

            if(user.IsTargetInUserFov(wallCenter)) {
                isCenterVisibleToUser[Utility.mod(i, 4)] = true;
            }
        }

        return isCenterVisibleToUser;
    }

    public Tuple<Vector2, Vector2> GetScaleTranlslate(Room currentRoom, Bound2D realSpace) // v is currentRoom
    {
        Room v = currentRoom;
        Vector2 Scale = new Vector2(v.initSize.x / v.Size.x, v.initSize.y / v.Size.y);
        Vector2 Translate = realSpace.Position - v.Position;

        return new Tuple<Vector2, Vector2>(Scale, Translate);
    }

    private bool[] isWallMoveDone = new bool[4];

    public void Restore(VirtualEnvironment virtualEnvironment, Room currentRoom, User user, Vector2 scale, Vector2 translate)
    {
        float[] DistWalltoDest = new float[4];
        DistWalltoDest[0] = (scale.y - 1) * currentRoom.Size.y / 2 + translate.y;
        DistWalltoDest[1] = (1 - scale.x) * currentRoom.Size.x / 2 + translate.x;
        DistWalltoDest[2] = (1 - scale.y) * currentRoom.Size.y / 2 + translate.y;
        DistWalltoDest[3] = (scale.x - 1) * currentRoom.Size.x / 2 + translate.x;

        float[] DistWalltoUser = new float[4];
        DistWalltoUser[0] = (user.Position.y + 0.4f) - currentRoom.GetEdge2D(0, Space.World).y;
        DistWalltoUser[1] = (user.Position.x - 0.4f) - currentRoom.GetEdge2D(1, Space.World).x;
        DistWalltoUser[2] = (user.Position.y - 0.4f) - currentRoom.GetEdge2D(2, Space.World).y;
        DistWalltoUser[3] = (user.Position.x + 0.4f) - currentRoom.GetEdge2D(3, Space.World).x;

        float[] DistgainApplied = new float[4];
        DistgainApplied[0] = Mathf.Sign(DistWalltoDest[0]) * (DT - 1) * currentRoom.Size.y;
        DistgainApplied[1] = Mathf.Sign(DistWalltoDest[1]) * (DT - 1) * currentRoom.Size.x;
        DistgainApplied[2] = Mathf.Sign(DistWalltoDest[2]) * (DT - 1) * currentRoom.Size.y;
        DistgainApplied[3] = Mathf.Sign(DistWalltoDest[3]) * (DT - 1) * currentRoom.Size.x;

        bool[] isVisible = CheckWallVisibleToUser(currentRoom, user);
        bool[] isCenterVisible = CheckWallCenterVisibleToUser(currentRoom, user);

        for (int i = 0; i < 4; i++)
        {
            if(!isVisible[i] && !isWallMoveDone[i]) {

                float finalTranslate = 0;
                if(DistWalltoDest[i] * DistWalltoUser[i] < 0) { 
                    finalTranslate = Mathf.Sign(DistWalltoDest[i]) * Mathf.Min(Mathf.Abs(DistgainApplied[i]), Mathf.Abs(DistWalltoDest[i]));
                }
                else {
                    finalTranslate = Mathf.Sign(DistWalltoDest[i]) * Mathf.Min(Mathf.Abs(DistgainApplied[i]), Mathf.Abs(DistWalltoDest[i]), Mathf.Abs(DistWalltoUser[i]));
                }

                virtualEnvironment.MoveWall(currentRoom, i, finalTranslate);
                isWallMoveDone[i] = true;
            }
            else if(isCenterVisible[i] && isWallMoveDone[i]) {
                isWallMoveDone[i] = false;
            }
        }
    }

    public void Reduce(VirtualEnvironment virtualEnvironment, Room targetRoom, Room currentRoom, Bound2D realSpace)
    {        
        float xMinDist = realSpace.Min.x - targetRoom.Min.x;
        float xMaxDist = realSpace.Max.x - targetRoom.Max.x;
        float yMinDist = realSpace.Min.y - targetRoom.Min.y;
        float yMaxDist = realSpace.Max.y - targetRoom.Max.y;

        if (xMinDist > 0) // 1벽
        {
            virtualEnvironment.MoveWallWithLimit(targetRoom, 1, xMinDist, currentRoom);
        }
        if (xMaxDist < 0) // 3벽
        {
            virtualEnvironment.MoveWallWithLimit(targetRoom, 3, xMaxDist, currentRoom);
        }
        if (yMinDist > 0) // 2벽
        {
            virtualEnvironment.MoveWallWithLimit(targetRoom, 2, yMinDist, currentRoom);
        }
        if (yMaxDist < 0) // 0벽
        {
            virtualEnvironment.MoveWallWithLimit(targetRoom, 0, yMaxDist, currentRoom);
        }
    }

    public void OvertManipulate(Room targetRoom) {
        float xMinDist = realSpace.Min.x - targetRoom.Min.x;
        float xMaxDist = realSpace.Max.x - targetRoom.Max.x;
        float yMinDist = realSpace.Min.y - targetRoom.Min.y;
        float yMaxDist = realSpace.Max.y - targetRoom.Max.y;

        if (xMinDist > 0) // 1벽
        {
            virtualEnvironment.MoveWall(targetRoom, 1, xMinDist);
            virtualEnvironment.MoveWall(targetRoom, 3, xMinDist);
        }
        if (xMaxDist < 0) // 3벽
        {
            virtualEnvironment.MoveWall(targetRoom, 3, xMaxDist);
            virtualEnvironment.MoveWall(targetRoom, 1, xMaxDist);
        }
        if (yMinDist > 0) // 2벽
        {
            virtualEnvironment.MoveWall(targetRoom, 2, yMinDist);
            virtualEnvironment.MoveWall(targetRoom, 0, yMinDist);
        }
        if (yMaxDist < 0) // 0벽
        {
            virtualEnvironment.MoveWall(targetRoom, 0, yMaxDist);
            virtualEnvironment.MoveWall(targetRoom, 2, yMaxDist);
        }
    }

    bool NeedAdjust(VirtualEnvironment virtualEnvironment, Room currentRoom)
    {
        List<Door> connectedDoors = virtualEnvironment.GetConnectedDoors(currentRoom);

        foreach(var door in connectedDoors) {
            if(Mathf.Abs(door.GetThisRoomWrapper(currentRoom).weight - door.GetThisRoomWrapper(currentRoom).originWeight) > 0.01f)
                return true;
        }

        return false;
    }

    void Adjust(VirtualEnvironment virtualEnvironment, Room currentRoom, User user)
    {
        List<Door> connectedDoors = virtualEnvironment.GetConnectedDoors(currentRoom);

        foreach(var door in connectedDoors) {
            if(!user.IsTargetInUserFov(door.Position))
                virtualEnvironment.MoveDoor(currentRoom, door, door.GetThisRoomWrapper(currentRoom).originWeight);
        }
    }

    public void SwitchEnable(Room notUsed = null) {
        this.enabled = !this.enabled;
    }

    private void FixedUpdate() 
    {
        User user = users.GetActiveUser();
        Room currentRoom = virtualEnvironment.CurrentRoom;

        // 알고리즘 시작
        Tuple<Vector2, Vector2> st = GetScaleTranlslate(currentRoom, realSpace); 
        Vector2 scale = st.Item1, translate = st.Item2;

        if ((scale - Vector2.one).magnitude > 0.01f || (translate - Vector2.zero).magnitude > 0.01f) // 복원 연산
        {
            Debug.Log("Restore");
            Restore(virtualEnvironment, currentRoom, user, scale, translate);
        }
        else if(NeedAdjust(virtualEnvironment, currentRoom)) // 조정 연산
        {
            Debug.Log("Adjust");
            Adjust(virtualEnvironment, currentRoom, user);
        }
        else // 축소 연산
        {
            Debug.Log("Reduce");
            List<Room> neighborRooms = virtualEnvironment.GetConnectedRooms(currentRoom);
            foreach (var room in neighborRooms)
            {
                if(room == null) continue;
                if (!room.IsInside(realSpace))
                    Reduce(virtualEnvironment, room, currentRoom, realSpace);
            }

            SwitchEnable();
        }
        // 알고리즘 끝
    }
}
