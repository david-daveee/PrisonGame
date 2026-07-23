using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(CharacterAppearance))]
public sealed class CharacterAppearanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        bool changed = EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();

        CharacterAppearance appearance = (CharacterAppearance)target;

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Apply now", GUILayout.Height(30)))
            appearance.ApplyAppearance();

        if (changed)
        {
            appearance.ApplyAppearance();
            MarkDirtyOutsidePlayMode(appearance);
        }
    }

    private static void MarkDirtyOutsidePlayMode(CharacterAppearance appearance)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorUtility.SetDirty(appearance);
        if (appearance.gameObject.scene.IsValid())
            EditorSceneManager.MarkSceneDirty(appearance.gameObject.scene);
    }
}
