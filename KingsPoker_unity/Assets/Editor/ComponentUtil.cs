using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace JecklFramework.Editor
{
    public static class ComponentUtil
    {
        static SerializedObject source;
        [MenuItem("CONTEXT/Component/Serialized Copy")]
        private static void CopySerializedFromBase(MenuCommand command)
        {
            CopySerializedFromBase(command.context);
        }
        public static void CopySerializedFromBase(Object context)
        {
            source = new SerializedObject(context);
        }
        [MenuItem("CONTEXT/Component/Serialized Paste")]
        private static void PasteSerializedFromBase(MenuCommand command)
        {
            PasteSerializedFromBase(command.context);
        }
        public static void PasteSerializedFromBase(Object context)
        {
            SerializedObject dest = new SerializedObject(context);
            SerializedProperty prop_iterator = source.GetIterator();
            //jump into serialized object, this will skip script type so that we dont override the destination component's type
            if (prop_iterator.NextVisible(true))
            {
                while (prop_iterator.NextVisible(true)) //itterate through all serializedProperties
                {
                    //try obtaining the property in destination component
                    SerializedProperty prop_element = dest.FindProperty(prop_iterator.name);

                    //validate that the properties are present in both components, and that they're the same type
                    if (prop_element != null && prop_element.propertyType == prop_iterator.propertyType)
                    {
                        //copy value from source to destination component
                        dest.CopyFromSerializedProperty(prop_iterator);
                    }
                }
            }
            dest.ApplyModifiedProperties();
        }
        public static Component ConvertInheritsComponent(Component component, System.Type type)
        {
            int groupId = Undo.GetCurrentGroup();
            CopySerializedFromBase(component);
            GameObject go = component.gameObject;
            Undo.DestroyObjectImmediate(component);
            Component to = Undo.AddComponent(go, type);
            PasteSerializedFromBase(to);
            Undo.CollapseUndoOperations(groupId);
            return to;
        }
    }
}
