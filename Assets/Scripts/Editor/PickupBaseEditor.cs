using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PickupBase), true)]
public class PickupBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PickupBase script = (PickupBase)target;

        if (GUILayout.Button("Generate ID"))
        {
            script.GenerateId();
            EditorUtility.SetDirty(script);
        }
    }
}
