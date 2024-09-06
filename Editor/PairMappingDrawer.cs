using UnityEditor;
using UnityEngine;

namespace Spark2D.Editor {
    [CustomPropertyDrawer(typeof(PairMappingAttribute))]
    public class PairMappingDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var pairMapping = (PairMappingAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);
            var signWidth = 20;
            var itemWidth = (position.width - signWidth) / (string.IsNullOrEmpty(pairMapping.label) ? 2 : 3);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var style = new GUIStyle();
            style.normal.textColor = Color.white;

            var rect = new Rect(position.x, position.y, itemWidth, position.height);
            if (!string.IsNullOrEmpty(pairMapping.label)) {
                EditorGUI.LabelField(rect, new GUIContent(pairMapping.label, property.tooltip), style);
                rect.x += itemWidth;
            }
            EditorGUI.PropertyField(rect, property.FindPropertyRelative(pairMapping.@from), GUIContent.none);
            rect.x += itemWidth;
            rect.width = signWidth;
            style.alignment = TextAnchor.MiddleCenter;
            EditorGUI.LabelField(rect, pairMapping.separator, style);
            rect.x += signWidth;
            rect.width = itemWidth;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative(pairMapping.to), GUIContent.none);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}