using OmicronMeshColoring.Attributes;
using UnityEditor;
using UnityEngine;

namespace OmicronMeshColoring.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyInPlayModeAttribute))]
    public class ReadOnlyInPlayModeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool wasEnabled = GUI.enabled;

            GUI.enabled = !Application.isPlaying;

            EditorGUI.PropertyField(position, property, label, true);

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}