using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class AnimationEventCopier : EditorWindow
{
    [Header("Members")]
    private AnimationClip sourceObject;
    private AnimationClip targetObject;

    [MenuItem("Window/Animation Event Copier")]
    static void Init()
    {
        GetWindow(typeof(AnimationEventCopier));
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        sourceObject = EditorGUILayout.ObjectField("Source", sourceObject, typeof(AnimationClip), true) as AnimationClip;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        targetObject = EditorGUILayout.ObjectField("Target", targetObject, typeof(AnimationClip), true) as AnimationClip;
        EditorGUILayout.EndHorizontal();

        if (sourceObject != null && targetObject != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Source Event Count: " + AnimationUtility.GetAnimationEvents(sourceObject).Length + "\n\n" +
                "Target Event Count: " + AnimationUtility.GetAnimationEvents(targetObject).Length, MessageType.Info);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy"))
                CopyData();
            EditorGUILayout.EndHorizontal();
        }
    }

    void CopyData()
    {
        Undo.RegisterCompleteObjectUndo(targetObject, "Undo Generic Copy");

        AnimationClip sourceAnimClip = sourceObject as AnimationClip;
        AnimationClip targetAnimClip = targetObject as AnimationClip;

        if (sourceAnimClip && targetAnimClip)
        {
            AnimationUtility.SetAnimationEvents(targetAnimClip, AnimationUtility.GetAnimationEvents(sourceAnimClip));
        }
    }
}