using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StatefulAI))]
public class StatefulAIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StatefulAI script = (StatefulAI)target;

        if (GUILayout.Button("Generate ID"))
        {
            script.GenerateId();
            EditorUtility.SetDirty(script);
        }
    }
}
