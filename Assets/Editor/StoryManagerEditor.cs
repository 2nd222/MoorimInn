#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StoryManager))]
public class StoryManagerEditor : Editor
{
    private SerializedProperty storySequenceProperty;

    private void OnEnable()
    {
        storySequenceProperty =
            serializedObject.FindProperty("storySequence");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "storySequence");

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "스토리 재생 순서",
            EditorStyles.boldLabel
        );

        DrawStorySequence();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStorySequence()
    {
        if (storySequenceProperty == null)
            return;

        var storySOProperty = serializedObject.FindProperty("storySO");

        if (storySOProperty == null || storySOProperty.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("StorySO를 먼저 지정해주세요.", MessageType.Warning);
            EditorGUILayout.PropertyField(storySequenceProperty);
            return;
        }

        var storySO = storySOProperty.objectReferenceValue as StorySO;

        if (storySO == null)
            return;

        var sequenceNameProperty = storySequenceProperty.FindPropertyRelative("sequenceName");
        var storyIdsProperty = storySequenceProperty.FindPropertyRelative("storyIds");
        EditorGUILayout.PropertyField(sequenceNameProperty);
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Story 순서", EditorStyles.boldLabel);

        // ============================================================
        // 중복 ID / 존재하지 않는 ID 검사
        // ============================================================

        HashSet<int> usedStoryIds = new();
        List<int> duplicateIds = new();
        List<int> invalidIds = new();

        for (int i = 0; i < storyIdsProperty.arraySize; i++)
        {
            var element = storyIdsProperty.GetArrayElementAtIndex(i);

            int storyId = element.intValue;

            // 아직 Story를 선택하지 않은 항목
            if (storyId < 0)
                continue;

            // 중복 ID 검사
            if (!usedStoryIds.Add(storyId))
            {
                if (!duplicateIds.Contains(storyId))
                    duplicateIds.Add(storyId);
            }

            // StorySO에 실제 Story가 존재하는지 검사
            var story = storySO.stories.Find(x => x.id == storyId);

            if (story == null)
            {
                if (!invalidIds.Contains(storyId))
                    invalidIds.Add(storyId);
            }
        }

        // ============================================================
        // 경고 출력
        // ============================================================

        if (duplicateIds.Count > 0)
        {
            EditorGUILayout.HelpBox(
                "같은 Story ID가 중복되어 있습니다.\n" +
                string.Join(
                    ", ",
                    duplicateIds.Select(id => $"ID {id}")
                ),
                MessageType.Error
            );

            EditorGUILayout.Space(3);
        }

        if (invalidIds.Count > 0)
        {
            EditorGUILayout.HelpBox(
                "StorySO에 존재하지 않는 Story ID가 있습니다.\n" +
                string.Join(
                    ", ",
                    invalidIds.Select(id => $"ID {id}")
                ),
                MessageType.Error
            );

            EditorGUILayout.Space(3);
        }

        // ============================================================
        // Story 목록
        // ============================================================

        for (int i = 0; i < storyIdsProperty.arraySize; i++)
        {
            var element =
                storyIdsProperty.GetArrayElementAtIndex(i);

            int currentId = element.intValue;

            string label = GetStoryLabel(
                storySO,
                currentId
            );

            EditorGUILayout.BeginHorizontal();

            // --------------------------------------------------------
            // 순서 + Story 이름
            // --------------------------------------------------------

            EditorGUILayout.LabelField(
                $"{i + 1}. {label}",
                GUILayout.MinWidth(250)
            );

            // --------------------------------------------------------
            // 위로 이동
            // --------------------------------------------------------

            GUI.enabled = i > 0;

            if (GUILayout.Button(
                    "↑",
                    GUILayout.Width(25)))
            {
                storyIdsProperty.MoveArrayElement(
                    i,
                    i - 1
                );
            }

            // --------------------------------------------------------
            // 아래로 이동
            // --------------------------------------------------------

            GUI.enabled = i < storyIdsProperty.arraySize - 1;

            if (GUILayout.Button("↓", GUILayout.Width(25)))
            {
                storyIdsProperty.MoveArrayElement(
                    i,
                    i + 1
                );
            }

            GUI.enabled = true;

            // --------------------------------------------------------
            // Story 선택
            // --------------------------------------------------------

            if (GUILayout.Button("선택", GUILayout.Width(45)))
            {
                ShowStoryMenu(
                    storySO,
                    element
                );
            }

            // --------------------------------------------------------
            // 삭제
            // --------------------------------------------------------

            if (GUILayout.Button(
                    "X",
                    GUILayout.Width(25)))
            {
                storyIdsProperty.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(5);

        // ============================================================
        // Story 추가
        // ============================================================

        if (GUILayout.Button("+ 스토리 추가"))
        {
            storyIdsProperty.InsertArrayElementAtIndex(
                storyIdsProperty.arraySize
            );

            var newElement =
                storyIdsProperty.GetArrayElementAtIndex(
                    storyIdsProperty.arraySize - 1
                );

            // 아직 Story를 선택하지 않은 상태
            newElement.intValue = -1;
        }
    }

    private string GetStoryLabel(StorySO storySO, int storyId)
    {
        if (storyId < 0)
            return "스토리를 선택해주세요.";

        var story =
            storySO.stories.Find(x => x.id == storyId);

        if (story == null)
            return $"ID {storyId} (찾을 수 없음)";

        return $"{story.storyName} [ID: {story.id}]";
    }

    private void ShowStoryMenu(StorySO storySO, SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();

        // 현재 Sequence에 들어가 있는 모든 ID
        HashSet<int> usedStoryIds = new();

        var storyIdsProperty =
            storySequenceProperty.FindPropertyRelative(
                "storyIds"
            );

        for (int i = 0; i < storyIdsProperty.arraySize; i++)
        {
            int id =
                storyIdsProperty
                    .GetArrayElementAtIndex(i)
                    .intValue;

            if (id >= 0)
                usedStoryIds.Add(id);
        }

        foreach (var story in storySO.stories)
        {
            int storyId = story.id;

            // 현재 선택되어 있는 Story
            bool isCurrent = property.intValue == storyId;

            // 이미 다른 항목에서 사용 중인지
            bool isDuplicate = usedStoryIds.Contains(storyId) && !isCurrent;

            string label = $"{story.storyName} [ID: {story.id}]";

            if (isDuplicate)
            {
                label += " (이미 등록됨)";
            }

            menu.AddItem(
                new GUIContent(label),
                isCurrent,
                () =>
                {
                    property.intValue = storyId;
                    serializedObject.ApplyModifiedProperties();
                }
            );
        }

        menu.ShowAsContext();
    }
}

#endif