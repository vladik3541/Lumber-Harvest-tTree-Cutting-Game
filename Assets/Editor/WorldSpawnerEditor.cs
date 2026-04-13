using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldSpawner))]
public class WorldSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Малюємо стандартний Inspector
        DrawDefaultInspector();

        var spawner = (WorldSpawner)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("─────────────────────────────", EditorStyles.centeredGreyMiniLabel);

        // Інформація про сітку
        GUI.color = new Color(0.7f, 1f, 0.7f);
        EditorGUILayout.HelpBox(
            $"Всього дерев: {spawner.TotalTrees}  (рядів × стовпців)",
            MessageType.Info
        );
        GUI.color = Color.white;

        EditorGUILayout.Space(5);

        // Кнопка центрування сітки на (0,0,0)
        if (GUILayout.Button("Центрувати сітку на (0, 0, 0)", GUILayout.Height(28)))
        {
            CenterGrid(spawner);
        }

        EditorGUILayout.Space(3);

        // Кнопка "Спавнити у Play Mode" — просто пояснення
        GUI.color = new Color(1f, 0.9f, 0.5f);
        EditorGUILayout.HelpBox(
            "Натисни Play → SpawnInitialWorld() викликається з GameManager.\n" +
            "Або виклич вручну через контекст-меню компонента.",
            MessageType.None
        );
        GUI.color = Color.white;
    }

    private void CenterGrid(WorldSpawner spawner)
    {
        // Через SerializedProperty, щоб Undo працював
        SerializedObject so = new SerializedObject(spawner);

        var rowsProp   = so.FindProperty("rows");
        var colsProp   = so.FindProperty("columns");
        var spacingX   = so.FindProperty("spacingX");
        var spacingZ   = so.FindProperty("spacingZ");
        var startPos   = so.FindProperty("startPosition");

        int r = rowsProp.intValue;
        int c = colsProp.intValue;
        float sx = spacingX.floatValue;
        float sz = spacingZ.floatValue;

        // Зміщуємо startPosition так, щоб центр сітки був у (0,0,0)
        float offsetX = -(c - 1) * sx * 0.5f;
        float offsetZ = -(r - 1) * sz * 0.5f;

        startPos.vector3Value = new Vector3(offsetX, 0f, offsetZ);

        so.ApplyModifiedProperties();
        Debug.Log($"Grid centered. Start = ({offsetX:F1}, 0, {offsetZ:F1})");
    }
}
