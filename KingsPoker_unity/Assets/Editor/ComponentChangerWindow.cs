using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.Linq;

namespace JecklFramework.Editor
{
    public class ComponentChangerWindow : EditorWindow
    {
        private List<Type> components;
        private SceneAsset curScene;
        private string fromComponent;
        private string targetComponent;
        private Vector2 scrollPosition = Vector2.zero;
        private Component[] objects;
        private Type fromType;
        private Type targetType;
        private bool forceChange = false;
        private bool allowInherits = false;
        private bool includeInactive = false;
        [MenuItem("Jeckl/Tools/Component Changer")]
        private static void OpenWindow()
        {
            ComponentChangerWindow window = GetWindow<ComponentChangerWindow>("Component Changer");
            window.curScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneManager.GetActiveScene().path);
        }

        private void OnEnable()
        {
            components = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(Component))).ToList();
            forceChange = true;
            Undo.undoRedoPerformed -= OnRedo;
            Undo.undoRedoPerformed += OnRedo;
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= OnRedo;
        }

        private void OnRedo()
        {
            forceChange = true;
        }

        private void OnGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.LabelField("Selected Scene");
            EditorGUILayout.ObjectField(curScene, typeof(SceneAsset));
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();
            fromComponent = EditorGUILayout.TextField("From Component", fromComponent);
            targetComponent = EditorGUILayout.TextField("Target Component", targetComponent);
            allowInherits = EditorGUILayout.Toggle("Allow inherits", allowInherits);
            includeInactive = EditorGUILayout.Toggle("Include Inactive", includeInactive);
            bool componentChanged = EditorGUI.EndChangeCheck();
            EditorGUILayout.Space(5f);
            if (componentChanged || forceChange)
            {
                forceChange = false;
                fromType = components.Find(type => type.Name == fromComponent);
               targetType = components.Find(type => type.Name == targetComponent);
                objects = null;
                if (fromType != null && targetType != null)
                {
                    if (fromType == targetType)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.LabelField("Error : fromComponent and targetComponent is same");
                        GUI.enabled = true;
                    }
                    else
                    {
                        if (fromType.IsSubclassOf(targetType) || targetType.IsSubclassOf(fromType))
                        {
                            objects = FindObjectsOfType(fromType, includeInactive) as Component[];
                            if (!allowInherits)
                            {
                                objects = objects.Where(item => item.GetType() == fromType).ToArray();
                            }
                        }
                        else
                        {
                            GUI.enabled = false;
                            EditorGUILayout.LabelField("Error : targetComponent and fromComponent is not an inheritance relationship.");
                            GUI.enabled = true;
                        }
                    }
                }
            }
            if (objects != null)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                for (int i = 0; i < objects.Length; i++)
                {
                    Component item = objects[i];
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(item.gameObject, typeof(GameObject), true);
                    GUI.enabled = true;
                    if (GUILayout.Button("exclude"))
                    {
                        UnityEditor.ArrayUtility.Remove(ref objects, item);
                        i -= 1;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUI.enabled = objects.Length > 0;
                if (GUILayout.Button("Start Change")) // ư
                {
                    forceChange = true;
                    int groupId = Undo.GetCurrentGroup();
                    foreach (Component item in objects)
                    {
                        ComponentUtil.ConvertInheritsComponent(item, targetType);
                    }
                    Undo.CollapseUndoOperations(groupId);
                }
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button("Start Change");
            }
        }
    }
}
