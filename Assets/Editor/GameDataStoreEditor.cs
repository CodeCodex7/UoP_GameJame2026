using AI.Goap.UnitAI.Behaviors;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameDataStore))]
public class GameDataStoreEditor : Editor
{
    private int woodAmount = 500;
    private int metalAmount = 500;
    private int mushroomAmount = 500;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Resources", EditorStyles.boldLabel);

        woodAmount = EditorGUILayout.IntField("Wood", woodAmount);
        metalAmount = EditorGUILayout.IntField("Metal", metalAmount);
        mushroomAmount = EditorGUILayout.IntField("Mushrooms", mushroomAmount);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Give Resources"))
            {
                var gameDataStore = (GameDataStore)target;
                var addedAmount = gameDataStore.AddStoredResources(woodAmount, metalAmount, mushroomAmount);

                Debug.Log($"Added {addedAmount} total resources through GameDataStore.");
                EditorUtility.SetDirty(gameDataStore);
            }

            if (GUILayout.Button("Give 999 Of Each"))
            {
                var gameDataStore = (GameDataStore)target;
                var addedAmount = gameDataStore.AddStoredResources(999, 999, 999);

                Debug.Log($"Added {addedAmount} total resources through GameDataStore.");
                EditorUtility.SetDirty(gameDataStore);
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode before giving resources, so storage buildings are registered with GameDataStore.", MessageType.Info);
        }
    }
}
