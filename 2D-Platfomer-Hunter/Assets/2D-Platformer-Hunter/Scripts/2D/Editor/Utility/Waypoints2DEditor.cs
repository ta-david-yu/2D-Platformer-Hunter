using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DYP
{
    [CustomEditor(typeof(Waypoints2D))]
    public class Waypoints2DEditor : Editor
    {
        const float c_FirstPointRadius = 0.3f;
        const float c_PointRadius = 0.2f;
        const float c_SnapDistance = 0.15f;

        private static GUIStyle ToggleButtonStyleNormal = null;
        private static GUIStyle ToggleButtonStyleToggled = null;


        class SelectionInfo
        {
            public int PointIndex = -1;
            public bool MouseIsOverPoint;
            public bool PointIsSelected;
            public Vector2 PositionAtStartOfDrag;

            public int LineIndex = -1;
            public bool MouseIsOverLine;
        }

        Waypoints2D m_Target;

        SelectionInfo m_SelectionInfo;
        bool m_NeedRepaint = false;
        bool m_IsInEditMode = false;
        Tool m_LastTool = Tool.None;

        private void OnEnable()
        {
            m_Target = target as Waypoints2D;
            m_SelectionInfo = new SelectionInfo();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (ToggleButtonStyleNormal == null)
            {
                ToggleButtonStyleNormal = "Button";
                ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
                ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
            }

            if (GUILayout.Button("Edit Waypoints", (m_IsInEditMode) ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
            {
                m_IsInEditMode = !m_IsInEditMode;
                if (m_IsInEditMode)
                {
                    m_LastTool = Tools.current;
                    Tools.current = Tool.None;
                }
                else
                {
                    Tools.current = m_LastTool;
                }
            }
        }

        private void OnSceneGUI()
        {
            Event evt = Event.current;

            if (evt.type == EventType.Repaint)
            {
                drawSceneWaypoints();
            }
            else if (evt.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            else
            {
                handleSceneInput(evt);

                if (m_NeedRepaint)
                {
                    HandleUtility.Repaint();
                    m_NeedRepaint = false;
                }
            }

            if (Tools.current != Tool.None)
            {
                m_IsInEditMode = false;
                Repaint();
            }
        }

        private void handleSceneInput(Event evt)
        {
            if (!m_IsInEditMode)
                return;

            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(evt.mousePosition).origin;
            mousePosition.z = m_Target.transform.position.z;

            if (evt.type == EventType.MouseDown &&
                evt.button == 0 &&
                evt.modifiers != EventModifiers.Alt)
            {
                handleLeftMouseDown(evt, mousePosition);
            }

            if (evt.type == EventType.MouseUp &&
                evt.button == 0 &&
                evt.modifiers != EventModifiers.Alt)
            {
                handleLeftMouseUp(evt, mousePosition);
            }

            if (evt.type == EventType.MouseDrag &&
                evt.button == 0 &&
                evt.modifiers != EventModifiers.Alt)
            {
                handleLeftMouseDrag(evt, mousePosition);
            }

            if (m_IsInEditMode)
            {
                if (!m_SelectionInfo.PointIsSelected)
                    updateMouseOverInfo(mousePosition);
            }
        }

        private void handleLeftMouseDown(Event evt, Vector2 mousePosition)
        {
            bool delete = evt.modifiers == EventModifiers.Control;

            if (!delete)
            {
                if (!m_SelectionInfo.MouseIsOverPoint)
                {
                    int newPointIndex =
                        (m_SelectionInfo.MouseIsOverLine) ? m_SelectionInfo.LineIndex + 1 : m_Target.Count;

                    Undo.RecordObject(m_Target, "Add New Waypoint");
                    m_Target.Points.Insert(newPointIndex, mousePosition);
                    m_SelectionInfo.PointIndex = newPointIndex;
                }
                m_SelectionInfo.PointIsSelected = true;
                m_SelectionInfo.PositionAtStartOfDrag = m_Target.At(m_SelectionInfo.PointIndex);
                m_NeedRepaint = true;
            }
            else
            {
                if (m_SelectionInfo.MouseIsOverPoint)
                {
                    deletePointUnderMouse();
                }
            }
        }

        private void deletePointUnderMouse()
        {
            Undo.RecordObject(m_Target, "Delete Waypoint");
            m_Target.Points.RemoveAt(m_SelectionInfo.PointIndex);
            m_SelectionInfo.PointIsSelected = false;
            m_SelectionInfo.MouseIsOverPoint = false;
            m_NeedRepaint = true;
        }

        private void handleLeftMouseUp(Event evt, Vector2 mousePosition)
        {
            if (m_SelectionInfo.PointIsSelected)
            {
                bool snap = evt.modifiers == EventModifiers.Shift;

                var targetPosition = mousePosition;
                if (snap)
                {
                    int prevIndex = (m_SelectionInfo.PointIndex - 1) % m_Target.Count;
                    int nextIndex = (m_SelectionInfo.PointIndex + 1) % m_Target.Count;
                    targetPosition = snapPosition(m_Target.At(prevIndex), m_Target.At(nextIndex), mousePosition);
                }

                m_Target.Points[m_SelectionInfo.PointIndex] = m_SelectionInfo.PositionAtStartOfDrag;
                Undo.RecordObject(m_Target, "Move Waypoint");
                m_Target.Points[m_SelectionInfo.PointIndex] = targetPosition;

                m_SelectionInfo.PointIsSelected = false;
                m_SelectionInfo.PointIndex = -1;
                m_NeedRepaint = true;
            }
        }

        private void handleLeftMouseDrag(Event evt, Vector2 mousePosition)
        {
            if (m_SelectionInfo.PointIsSelected)
            {
                bool snap = evt.modifiers == EventModifiers.Shift;

                var targetPosition = mousePosition;
                if (snap)
                {
                    int prevIndex = (m_SelectionInfo.PointIndex - 1) % m_Target.Count;
                    int nextIndex = (m_SelectionInfo.PointIndex + 1) % m_Target.Count;
                    targetPosition = snapPosition(m_Target.At(prevIndex), m_Target.At(nextIndex), mousePosition);
                }

                m_Target.Points[m_SelectionInfo.PointIndex] = targetPosition;
                m_NeedRepaint = true;
            }
        }

        private void updateMouseOverInfo(Vector2 mousePosition)
        {
            int overPointIndex = -1;
            for (int i = 0; i < m_Target.Count; i++)
            {
                if (Vector2.Distance(mousePosition, m_Target.At(i)) < ((i == 0) ? c_FirstPointRadius : c_PointRadius))
                {
                    overPointIndex = i;
                    break;
                }
            }

            if (overPointIndex != m_SelectionInfo.PointIndex)
            {
                m_SelectionInfo.PointIndex = overPointIndex;
                m_SelectionInfo.MouseIsOverPoint = overPointIndex != -1;
                m_NeedRepaint = true;
            }

            if (m_SelectionInfo.MouseIsOverPoint)
            {
                m_SelectionInfo.MouseIsOverLine = false;
                m_SelectionInfo.LineIndex = -1;
            }
            else
            {
                int overLineIndex = -1;
                float closestDistance = c_SnapDistance;
                for (int i = 0; i < m_Target.Count; i++)
                {
                    var currPt = m_Target.At(i);
                    var nextPt = m_Target.At((i + 1) % m_Target.Count);
                    float dstFromMouseToLine =
                        HandleUtility.DistancePointToLineSegment(mousePosition, currPt, nextPt);

                    if (dstFromMouseToLine < closestDistance)
                    {
                        closestDistance = dstFromMouseToLine;
                        overLineIndex = i;
                    }
                }

                if (m_SelectionInfo.LineIndex != overLineIndex)
                {
                    m_SelectionInfo.LineIndex = overLineIndex;
                    m_SelectionInfo.MouseIsOverLine = overLineIndex != -1;
                    m_NeedRepaint = true;
                }
            }
        }

        private void drawSceneWaypoints()
        {
            float waypointHandleSize = (m_IsInEditMode) ? 1.0f : 0.2f;

            var originalColor = Handles.color;
            for (int i = 0; i < m_Target.Points.Count; i++)
            {
                var currPt = m_Target.Points[i];
                var nextPt = ((i + 1) == m_Target.Points.Count) ? m_Target.Points[0] : m_Target.Points[i + 1];

                Handles.color =
                    (i == m_SelectionInfo.PointIndex) ? new Color(1.0f, 0.92f, 0.016f, 1.0f) : new Color(1.0f, 0.92f, 0.016f, 0.15f);
                if (i == 0)
                {
                    Handles.DrawSolidDisc(currPt, -Vector3.forward, c_FirstPointRadius * waypointHandleSize);
                }
                else
                {
                    Handles.DrawSolidDisc(currPt, -Vector3.forward, c_PointRadius * waypointHandleSize);
                }

                Handles.color =
                    (i == m_SelectionInfo.LineIndex) ? new Color(1.0f, 0.92f, 0.016f, 1.0f) : new Color(1.0f, 0.92f, 0.016f, 0.5f);
                if ((i + 1) == m_Target.Points.Count)
                {
                    Handles.DrawDottedLine(currPt, nextPt, 4.0f);
                }
                else
                {
                    Handles.DrawLine(currPt, nextPt);
                }
            }
            Handles.color = originalColor;
        }

        private Vector2 snapPosition(Vector2 prevPt, Vector2 nextPt, Vector2 mousePosition)
        {
            var targetPosition = mousePosition;

            // x axis snap
            float offsetXtoPrevPt = Mathf.Abs(mousePosition.x - prevPt.x);
            float offsetXtoNextPt = Mathf.Abs(mousePosition.x - nextPt.x);

            if (offsetXtoPrevPt < offsetXtoNextPt)
            {
                if (offsetXtoPrevPt < c_SnapDistance)
                {
                    targetPosition.x = prevPt.x;
                }
            }
            else
            {
                if (offsetXtoNextPt < c_SnapDistance)
                {
                    targetPosition.x = nextPt.x;
                }
            }

            // y axis snap
            float offsetYtoPrevPt = Mathf.Abs(mousePosition.y - prevPt.y);
            float offsetYtoNextPt = Mathf.Abs(mousePosition.y - nextPt.y);

            if (offsetYtoPrevPt < offsetYtoNextPt)
            {
                if (offsetYtoPrevPt < c_SnapDistance)
                {
                    targetPosition.y = prevPt.y;
                }
            }
            else
            {
                if (offsetYtoNextPt < c_SnapDistance)
                {
                    targetPosition.y = nextPt.y;
                }
            }
            return targetPosition;
        }
    }
}
