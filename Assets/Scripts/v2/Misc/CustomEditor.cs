using UnityEngine;
using UnityEditor;

// [CustomEditor(typeof(Bound2D))]
// public class BoundInspector : Editor {
//     public override void OnInspectorGUI() {
//         base.OnInspectorGUI();
        
//         if(GUILayout.Button("Resize room")) {
//             Bound2D bound = (Bound2D) target;
//             bound.ApplySize();
//         }
//     }
// }

[CustomEditor(typeof(Room))]
public class RoomInspector : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Resize room")) {
            Bound2D bound = (Bound2D) target;
            bound.ApplySize();
        }
    }
}

// [CustomEditor(typeof(RealSpace))]
// public class RealSpaceInspector : BoundInspector {
//     public override void OnInspectorGUI() {
//         base.OnInspectorGUI();
//     }
// }

