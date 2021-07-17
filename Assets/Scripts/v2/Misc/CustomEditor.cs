using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(OrderManager))]
public class OrderInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        if(GUILayout.Button("Get Order")) {
            OrderManager orderManager = (OrderManager) target;
            // bound.ShowLocomotionOrder();
            orderManager.GetOrder();
        }
    }
}

[CustomEditor(typeof(Bound2D))]
public class BoundInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        if(GUILayout.Button("Apply Size")) {
            Bound2D bound = (Bound2D) target;
            bound.ApplySize();
        }
    }
}

[CustomEditor(typeof(Room))]
public class RoomInspector : BoundInspector {
    float translate;
    int wall;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        translate = EditorGUILayout.FloatField("translate", translate);
        wall = EditorGUILayout.IntSlider("wall", wall, 0, 3);

        if(GUILayout.Button("Test Move wall")) {
            Room room = (Room) target;
            room.MoveEdge(wall, translate);
        }
    }
}

[CustomEditor(typeof(RealSpace))]
public class RealSpaceInspector : BoundInspector {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }
}

[CustomEditor(typeof(Door))]
public class DoorInspector : BoundInspector {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Call Open Door")) {
            Door door = (Door) target;
            door.OpenDoor();
        }
    }
    
}

// [CustomPropertyDrawer(typeof(Ingredient))]
// public class UserStateDrawer : PropertyDrawer {

//     public override VisualElement CreatePropertyGUI(SerializedProperty property)
//     {
//         // Create property container element.
//         var container = new VisualElement();

//         // Create property fields.
//         var amountField = new PropertyField(property.FindPropertyRelative("amount"));
//         var unitField = new PropertyField(property.FindPropertyRelative("unit"));
//         var nameField = new PropertyField(property.FindPropertyRelative("name"), "Fancy Name");

//         // Add fields to the container.
//         container.Add(amountField);
//         container.Add(unitField);
//         container.Add(nameField);

//         return container;
//     }
// }


// [CustomEditor(typeof(VirtualEnvironment))]
// public class VirtualEnvironmentInspector : Editor {

//     float translate;
//     int wall;
//     Object source;

//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();

//         translate = EditorGUILayout.FloatField("translate", translate);
//         wall = EditorGUILayout.IntSlider("wall", wall, 0, 3);
//         // source = EditorGUILayout.ObjectField("Target Room", source, typeof(Room), true);

//         if(GUILayout.Button("initializing"))  {
//             VirtualEnvironment virtualEnvironment = target as VirtualEnvironment;
//             virtualEnvironment.Initializing();
//         }

//         if(GUILayout.Button("Test Move wall")) {
//             VirtualEnvironment virtualEnvironment = target as VirtualEnvironment;
//             virtualEnvironment.MoveWall(virtualEnvironment.CurrentRoom, wall, translate);
//         }
//     }
// }

// [CustomEditor(typeof(MyItem))]
// [CanEditMultipleObjects]
// public class MyItemEditor : Editor {

//     void OnEnable(){
//     }

//     public override void OnInspectorGUI ()
//     {
//         EditorGUILayout.ColorField ("Color Field", Color.white);
//         EditorGUILayout.CurveField ("AnimationCurve Field", AnimationCurve.Linear (0, 3, 5, 5));
//         EditorGUILayout.DelayedDoubleField ("DelayedDouble Field", 500);
//         EditorGUILayout.HelpBox ("The helpbox", MessageType.Info);
//         EditorGUILayout.IntField ("Int Field", 5);
//         EditorGUILayout.Knob (new Vector2 (30f, 30f), 50f, 20f, 80f, "Knob", Color.black, Color.gray, true);
//         EditorGUILayout.LabelField ("Label Field", "my label");
//         EditorGUILayout.LayerField ("Layer Field", 0);
//         EditorGUILayout.ObjectField ("Object Field", null, typeof(Room), true);
//         EditorGUILayout.PasswordField ("Password Field", "mypassword");
//         EditorGUILayout.Separator ();
//         EditorGUILayout.Slider ("Slider", 20f, 10f, 90f);
//         EditorGUILayout.Space ();
//         EditorGUILayout.TagField ("Tag Field", "Player2");
//         EditorGUILayout.TextArea ("this is a text area.");
//         EditorGUILayout.TextField ("Text Field", "this is a text field.");
//         EditorGUILayout.Toggle ("Toggle", true);
//         EditorGUILayout.ToggleLeft ("ToggleLeft", false);
//         EditorGUILayout.Vector2Field ("Vector2  field", new Vector2 (50f, 30f));
//     }
// }