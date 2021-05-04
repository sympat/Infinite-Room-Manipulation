using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCript : MonoBehaviour
{

    VirtualEnvironment virtualEnvironment;

    private void Start() {
        virtualEnvironment = GetComponent<VirtualEnvironment>();
        Room currentRoom = virtualEnvironment.CurrentRoom;

        // virtualEnvironment.MoveWall(currentRoom, 1, 1.0f);

        // virtualEnvironment.MoveWallWithLimit(currentRoom, 1, 0);

        // Debug.Log($"MaxDoor in Y-Align {virtualEnvironment.GetMaxDoorInDirection(currentRoom, Direction.Y)}");
        // Debug.Log($"MinDoor in Y-Align {virtualEnvironment.GetMinDoorInDirection(currentRoom, Direction.Y)}");

        // Debug.Log($"MaxDoor in X-Align {virtualEnvironment.GetMaxDoorInDirection(currentRoom, Direction.X)}");
        // Debug.Log($"MinDoor in X-Align {virtualEnvironment.GetMinDoorInDirection(currentRoom, Direction.X)}");
    }
}
