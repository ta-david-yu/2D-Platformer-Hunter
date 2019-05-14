using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DYP
{
    [CustomEditor(typeof(Ladder))]
    public class LadderEditor : Editor
    {
        private static GUIStyle ToggleButtonStyleToggled = null;

        private Ladder m_Target;

        private SerializedProperty m_RestrictedArea;
        private SerializedProperty m_HasRestrictedArea;

        private SerializedProperty m_IsRestrictedAreaTopIgnored;

        private SerializedProperty m_TopAreaHeight;
        private SerializedProperty m_BottomAreaHeight;

        private List<Area2D> m_ChildArea;
        private List<BoxCollider2D> m_ChildCollider;

        private void OnEnable()
        {
            m_Target = target as Ladder;

            m_RestrictedArea = serializedObject.FindProperty("m_RestrictedArea");
            m_HasRestrictedArea = serializedObject.FindProperty("m_HasRestrictedArea");
            m_IsRestrictedAreaTopIgnored = serializedObject.FindProperty("m_IsRestrictedAreaTopIgnored");
            m_TopAreaHeight = serializedObject.FindProperty("m_TopAreaHeight");
            m_BottomAreaHeight = serializedObject.FindProperty("m_BottomAreaHeight");

            m_ChildCollider = new List<BoxCollider2D>(m_Target.GetComponentsInChildren<BoxCollider2D>());
        }

        public override void OnInspectorGUI()
        {
            if (ToggleButtonStyleToggled == null)
            {
                ToggleButtonStyleToggled = new GUIStyle("Button");
                ToggleButtonStyleToggled.active.background = ToggleButtonStyleToggled.active.background;
            }

            EditorGUILayout.Space();

            /* top and bottom area settings */
            var bakcgroundStyle = EditorStyles.textArea;
            EditorGUILayout.BeginVertical(bakcgroundStyle);
            {
                drawTopBottomAreaInspector();
            }
            EditorGUILayout.EndVertical();
            /* */

            EditorGUILayout.Space();

            /* restricted area settings */
            EditorGUILayout.BeginVertical(bakcgroundStyle);
            {
                drawRestrictedAreaInspector();
            }
            EditorGUILayout.EndVertical();
            /* */

            serializedObject.ApplyModifiedProperties();
        }

        private void drawTopBottomAreaInspector()
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Top & Bottom Area Settings", style);

            m_TopAreaHeight.floatValue = EditorGUILayout.FloatField("TopArea Height", m_TopAreaHeight.floatValue);
            m_BottomAreaHeight.floatValue = EditorGUILayout.FloatField("BottomArea Height", m_BottomAreaHeight.floatValue);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add Extra Top Area", ToggleButtonStyleToggled))
                {
                    addTopArea();
                }

                if (GUILayout.Button("Add Extra Bottom Area", ToggleButtonStyleToggled))
                {
                    addBottomArea();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void drawRestrictedAreaInspector()
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Restricted Area Settings", style);

            bool newHasRestrictedArea =
                EditorGUILayout.Toggle(new GUIContent("Use Restricted Area"), m_HasRestrictedArea.boolValue);

            if (!newHasRestrictedArea) EditorGUI.BeginDisabledGroup(true);
            {
                if (newHasRestrictedArea)
                {
                    if (m_RestrictedArea.objectReferenceValue == null)
                    {
                        addRestrictedArea();
                    }

                    if (!m_HasRestrictedArea.boolValue)
                    {
                        m_HasRestrictedArea.boolValue = newHasRestrictedArea;
                    }
                }

                m_RestrictedArea.objectReferenceValue =
                    EditorGUILayout.ObjectField("Restricted Area", m_Target.RestrictedArea, typeof(Area2D), true);
            }

            m_IsRestrictedAreaTopIgnored.boolValue =
                EditorGUILayout.Toggle(new GUIContent("Ignore Restricted Area Top"), m_IsRestrictedAreaTopIgnored.boolValue);

            if (!newHasRestrictedArea) EditorGUI.EndDisabledGroup();

            m_HasRestrictedArea.boolValue = newHasRestrictedArea;
        }

        private void OnSceneGUI()
        {
            draw();
        }

        private void draw()
        {
            var oriColor = Handles.color;

            var areaColor = new Color(1.0f, 0.92f, 0.016f, 0.5f);
            var extraAreaColor = Color.white;

            GUIStyle txtStyle = new GUIStyle();
            txtStyle.normal.textColor = Color.white;

            Handles.color = areaColor;

            // draw main area
            var areaBounds = m_Target.AreaBounds;
            Handles.DrawSolidRectangleWithOutline(new Rect(areaBounds.center - areaBounds.extents, areaBounds.extents * 2), areaColor, areaColor);

            // draw top area
            var topAreaBounds = new Bounds(
                new Vector3(areaBounds.center.x, areaBounds.center.y + areaBounds.extents.y - m_Target.TopAreaHeight / 2, 0),
                new Vector3(areaBounds.size.x, m_Target.TopAreaHeight, 100)
            );
            Handles.DrawSolidRectangleWithOutline(new Rect(topAreaBounds.center - topAreaBounds.extents, topAreaBounds.extents * 2), extraAreaColor, extraAreaColor);
            Handles.Label(topAreaBounds.center, "Top", txtStyle);

            // draw bottom area
            var bottomAreaBounds = new Bounds(
                new Vector3(areaBounds.center.x, areaBounds.center.y - areaBounds.extents.y + m_Target.BottomAreaHeight / 2, 0),
                new Vector3(areaBounds.size.x, m_Target.BottomAreaHeight, 100)
            );
            Handles.DrawSolidRectangleWithOutline(new Rect(bottomAreaBounds.center - bottomAreaBounds.extents, bottomAreaBounds.extents * 2), extraAreaColor, extraAreaColor);
            Handles.Label(bottomAreaBounds.center, "Bottom", txtStyle);

            // draw child collder (extra top / bottom area)
            foreach (var col in m_ChildCollider)
            {
                drawArea(col.bounds, Color.yellow);
                Handles.Label(col.bounds.center, col.gameObject.name, txtStyle);
            }

            // draw restricted area if had one
            if (m_Target.HasRestrictedArea && m_Target.RestrictedArea != null)
            {
                drawArea(m_Target.RestrictedArea.Bounds, m_Target.RestrictedArea._debugColor);
                Handles.Label(m_Target.RestrictedArea.Center, "Restricted", txtStyle);
            }

            Handles.color = oriColor;
        }

        private void drawArea(Bounds bounds, Color color)
        {
            Handles.DrawSolidRectangleWithOutline(new Rect(bounds.center - bounds.size / 2, bounds.size), color * 0.6f, color);
        }

        private void addRestrictedArea()
        {
            GameObject areaObj = new GameObject("_restrictedArea");
            areaObj.transform.SetParent(m_Target.transform);
            areaObj.transform.localPosition = Vector3.zero;
            areaObj.transform.localRotation = Quaternion.identity;
            areaObj.transform.localScale = Vector3.one;

            m_RestrictedArea.objectReferenceValue = areaObj.AddComponent<Area2D>();
            (m_RestrictedArea.objectReferenceValue as Area2D)._debugColor = Color.red;

            serializedObject.ApplyModifiedProperties();
        }

        private void addTopArea()
        {
            GameObject areaObj = new GameObject("_extraTopArea");
            areaObj.transform.SetParent(m_Target.transform);
            areaObj.transform.localPosition = Vector3.zero;
            areaObj.transform.localRotation = Quaternion.identity;
            areaObj.transform.localScale = Vector3.one;

            var ladderZone = areaObj.AddComponent<LadderZoneSetter>();
            ladderZone.Zone = LadderZone.Top;

            var col = ladderZone.GetComponent<BoxCollider2D>();
            col.offset = new Vector2(m_Target.Collider.offset.x, m_Target.Collider.offset.y + m_Target.Collider.size.y / 2 + m_Target.TopAreaHeight / 2);
            col.size = new Vector2(m_Target.Collider.size.x * 1.2f, m_Target.TopAreaHeight);

            m_ChildCollider.Add(col);
        }

        private void addBottomArea()
        {
            GameObject areaObj = new GameObject("_extraBottomArea");
            areaObj.transform.SetParent(m_Target.transform);
            areaObj.transform.localPosition = Vector3.zero;
            areaObj.transform.localRotation = Quaternion.identity;
            areaObj.transform.localScale = Vector3.one;

            var ladderZone = areaObj.AddComponent<LadderZoneSetter>();
            ladderZone.Zone = LadderZone.Bottom;

            var col = ladderZone.GetComponent<BoxCollider2D>();
            col.offset = new Vector2(m_Target.Collider.offset.x, m_Target.Collider.offset.y - m_Target.Collider.size.y / 2 - m_Target.TopAreaHeight / 2);
            col.size = new Vector2(m_Target.Collider.size.x * 1.2f, m_Target.TopAreaHeight);

            m_ChildCollider.Add(col);
        }
    }
}
