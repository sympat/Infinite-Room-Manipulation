using UnityEngine;

public class RoomWrapper {
    public Room room;
    public float weight;
    public float originWeight;
    public int wall;

    public override string ToString()
    {
        string result = "";
        result += string.Format("Weight: {0}", weight);
        result += string.Format("\nWall: {0}", wall);
        result += "\n" + room;

        return result;
    }

    public RoomWrapper(Room room) {
        this.room = room;
    }

    public RoomWrapper(Room room, int wall, Vector2 doorPosition) {
        this.room = room;
        this.wall = wall;

        if(this.room != null) {
            if (this.wall == 0 || this.wall == 2)
            {
                this.weight = 2 * (doorPosition.x - this.room.Min.x) / (this.room.Max.x - this.room.Min.x) - 1;
            }
            else if (this.wall == 1 || this.wall == 3)
            {
                this.weight = 2 * (doorPosition.y - this.room.Min.y) / (this.room.Max.y - this.room.Min.y) - 1;
            }
            
            this.originWeight = this.weight;
        }
    }
}