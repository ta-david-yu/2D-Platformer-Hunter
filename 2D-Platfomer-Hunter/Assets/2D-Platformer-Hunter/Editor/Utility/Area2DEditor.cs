using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;

namespace DYP
{
    [CustomEditor(typeof(Area2D))]
    public class Area2DEditor : Editor
    {

        enum Anchor
        {
            None = -1,

            TopLeft = 0,
            TopCenter,
            TopRight,

            MiddleLeft,
            MiddleRight,

            BottomLeft,
            BottomCenter,
            BottomRight
        }

        class SelectionInfo
        {
            public Anchor Anchor = Anchor.None;
            public bool MouseIsOverAnchor;
            public bool AnchorIsSelected;
            public Vector2 PositionAtStartOfDrag;
        }

        const float c_HandleWidth = 0.065f;
        private static GUIStyle ToggleButtonStyleNormal = null;
        private static GUIStyle ToggleButtonStyleToggled = null;

        private Area2D m_Target;
        private bool m_IsInEditMode = false;
        SelectionInfo m_SelectionInfo;
        Tool m_LastTool = Tool.None;
        bool m_NeedRepaint;

        private SerializedProperty m_Offset;
        private SerializedProperty m_Extents;
        private SerializedProperty m_Bounds;

        private Vector2 TopLeft
        {
            get { return m_Target.Center + new Vector2(-m_Target.Extents.x, m_Target.Extents.y); }
            set
            {
                var symmPt = BottomRight;

                var newCenter = (symmPt + value) / 2.0f;
                m_Target.Offset = (Vector3)newCenter - m_Target.transform.position;
                m_Target.Extents = new Vector2(Mathf.Abs(symmPt.x - value.x) / 2, Math.Abs(value.y - symmPt.y) / 2);
            }
        }
        private Vector2 TopCenter
        {
            get { return m_Target.Center + new Vector2(0, m_Target.Extents.y); }
            set
            {
                var symmPt = BottomCenter;

                var newCenter = (symmPt + value) / 2.0f;
                m_Target.Offset = (Vector3)newCenter - m_Target.transform.position;
                m_Target.Extents = new Vector2(m_Target.Extents.x, Math.Abs(value.y - symmPt.y) / 2);
            }
        }
        private Vector2 TopRight
        {
            get { return m_Target.Center + new Vector2(m_Target.Extents.x, m_Target.Extents.y); }
            set
            {
                var symmPt = BottomLeft;

                var newCenter = (symmPt + value) / 2.0f;
                m_Target.Offset = (Vector3)newCenter - m_Target.transform.position;
                m_Target.Extents = new Vector2(Mathf.Abs(symmPt.x - value.x) / 2, Math.Abs(value.y - symmPt.y) / 2);
            }
        }
        private Vector2 MiddleLeft
        {
            get { return m_Target.Center + new Vector2(-m_Target.Extents.x, 0); }
            set
            {
                var symmPt = MiddleRight;

                var newCenter = (symmPt + value) / 2.0f;
                m_Target.Offset = (Vector3)newCenter - m_Target.transform.position;
                m_Target.Extents = new Vector2(Mathf.Abs(symmPt.x - value.x) / 2, m_Target.Extents.y);
            }
        }
        private Vector2 MiddleRight
        {
            get { return m_Target.Center + new Vector2(m_Target.Extents.x, 0); }
            set
            {
                var symmPt = MiddleLeft;

                var newCenter = (symmPt + value) / 2.0f;
                m_Target.Offset = (Vector3)newCenter - m_Target.transform.position;
                m_Target.Extents = new Vector2(Mathf.Abs(symmPt.x - value.x) / 2, m_Target.Extents.y);
            }
        }
        private Vector2 BottomLeft
        {
            get { return m_Target.Center + new Vector2(-m_Target.Extents.x, -m_Target.Extents.y); }
            set
            {
                var symmPt = TopRight;

                var newCenter = (symmPt + value) / 2.0f;
                m_Target.Offset = (Vector3)newCenter - m_Target.transform.position;
                m_Target.Extents = new Vector2(Mathf.Abs(symmPt.x - value.x) / 2, Math.Abs(value.y - symmPt.y) / 2);
            }
        }
        private Vector2 BottomCenter
        {
            get { return m_Target.Center + new Vector2(0, -m_Target.Extents.y); }
            set
            {
                var symmPt = TopCenter;

                var newCenter = (symmPt + value) / 2.0f;
                m_Target.Offset = (Vector3)newCenter - m_Target.transform.position;
                m_Target.Extents = new Vector2(m_Target.Extents.x, Math.Abs(value.y - symmPt.y) / 2);
            }
        }
        private Vector2 BottomRight
        {
            get { return m_Target.Center + new Vector2(m_Target.Extents.x, -m_Target.Extents.y); }
            set
            {
                var symmPt = TopLeft;

                var newCenter = (symmPt + value) / 2.0f;
                m_Target.Offset = (Vector3)newCenter - m_Target.transform.position;
                m_Target.Extents = new Vector2(Mathf.Abs(symmPt.x - value.x) / 2, Math.Abs(value.y - symmPt.y) / 2);
            }
        }

        private void OnEnable()
        {
            m_Target = target as Area2D;
            m_SelectionInfo = new SelectionInfo();

            m_Offset = serializedObject.FindProperty("m_Offset");
            m_Extents = serializedObject.FindProperty("m_Extents");
            m_Bounds = serializedObject.FindProperty("m_Bounds");
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

            if (GUILayout.Button("Edit Area", (m_IsInEditMode) ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
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

            //Vector2 newOffset = EditorGUILayout.Vector2Field("Offset", m_Target.Offset);
            //Vector2 newExtents = EditorGUILayout.Vector2Field("Size", m_Target.Extents * 2f) / 2.0f;

            m_Offset.vector2Value = EditorGUILayout.Vector2Field("Offset", m_Offset.vector2Value);
            m_Extents.vector2Value = EditorGUILayout.Vector2Field("Size", m_Extents.vector2Value * 2.0f) / 2.0f;

            m_Target.Offset = m_Offset.vector2Value;
            m_Target.Extents = m_Extents.vector2Value;
            m_Bounds.serializedObject.ApplyModifiedProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            Event evt = Event.current;

            if (evt.type == EventType.Repaint)
            {
                draw();
            }
            else if (evt.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            else
            {
                handleSceneViewInput(evt);

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

        private void handleSceneViewInput(Event evt)
        {
            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(evt.mousePosition).origin;
            mousePosition.z = m_Target.transform.position.z;

            if (evt.type == EventType.MouseDown &&
                evt.button == 0 &&
                evt.modifiers == EventModifiers.None)
            {
                handleLeftMouseDown(evt, mousePosition);
            }

            if (evt.type == EventType.MouseUp &&
                evt.button == 0 &&
                evt.modifiers == EventModifiers.None)
            {
                handleLeftMouseUp(evt, mousePosition);
            }

            if (evt.type == EventType.MouseDrag &&
                evt.button == 0 &&
                evt.modifiers == EventModifiers.None)
            {
                handleLeftMouseDrag(evt, mousePosition);
            }

            if (m_IsInEditMode)
            {
                if (!m_SelectionInfo.AnchorIsSelected)
                {
                    updateMouseOverInfo(mousePosition);
                }
            }
        }

        private void handleLeftMouseDrag(Event evt, Vector3 mousePosition)
        {
            if (m_SelectionInfo.AnchorIsSelected)
            {
                var targetPosition = mousePosition;

                // limit x axis
                if (m_SelectionInfo.Anchor == Anchor.BottomCenter || m_SelectionInfo.Anchor == Anchor.TopCenter)
                {
                    targetPosition.x = m_SelectionInfo.PositionAtStartOfDrag.x;
                }

                // limit y axis
                if (m_SelectionInfo.Anchor == Anchor.MiddleLeft || m_SelectionInfo.Anchor == Anchor.MiddleRight)
                {
                    targetPosition.y = m_SelectionInfo.PositionAtStartOfDrag.y;
                }

                setAnchorPosition(m_SelectionInfo.Anchor, targetPosition);

                m_NeedRepaint = true;
            }
        }

        private void handleLeftMouseUp(Event evt, Vector3 mousePosition)
        {
            if (m_SelectionInfo.AnchorIsSelected)
            {
                var newPosition = getAnchorPosition(m_SelectionInfo.Anchor);

                setAnchorPosition(m_SelectionInfo.Anchor, m_SelectionInfo.PositionAtStartOfDrag);
                Undo.RegisterCompleteObjectUndo(m_Target, "Move Area Anchor");
                setAnchorPosition(m_SelectionInfo.Anchor, newPosition);

                m_SelectionInfo.AnchorIsSelected = false;
                m_SelectionInfo.Anchor = Anchor.None;
                m_NeedRepaint = true;
            }
        }

        private void handleLeftMouseDown(Event evt, Vector3 mousePosition)
        {
            m_SelectionInfo.AnchorIsSelected = true;
            m_SelectionInfo.PositionAtStartOfDrag = getAnchorPosition(m_SelectionInfo.Anchor);

            m_NeedRepaint = true;
        }

        private void updateMouseOverInfo(Vector2 mousePosition)
        {
            Anchor overAnchor = Anchor.None;

            var point = TopLeft;
            float handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
            if (Vector2.Distance(mousePosition, point) < handleSize) overAnchor = Anchor.TopLeft;

            point = TopCenter;
            handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
            if (Vector2.Distance(mousePosition, point) < handleSize) overAnchor = Anchor.TopCenter;

            point = TopRight;
            handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
            if (Vector2.Distance(mousePosition, point) < handleSize) overAnchor = Anchor.TopRight;

            point = MiddleLeft;
            handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
            if (Vector2.Distance(mousePosition, point) < handleSize) overAnchor = Anchor.MiddleLeft;

            point = MiddleRight;
            handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
            if (Vector2.Distance(mousePosition, point) < handleSize) overAnchor = Anchor.MiddleRight;

            point = BottomLeft;
            handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
            if (Vector2.Distance(mousePosition, point) < handleSize) overAnchor = Anchor.BottomLeft;

            point = BottomCenter;
            handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
            if (Vector2.Distance(mousePosition, point) < handleSize) overAnchor = Anchor.BottomCenter;

            point = BottomRight;
            handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
            if (Vector2.Distance(mousePosition, point) < handleSize) overAnchor = Anchor.BottomRight;

            if (overAnchor != m_SelectionInfo.Anchor)
            {
                m_SelectionInfo.Anchor = overAnchor;
                m_SelectionInfo.MouseIsOverAnchor = overAnchor != Anchor.None;
                m_NeedRepaint = true;
            }
        }

        private void draw()
        {
            drawArea();

            if (m_IsInEditMode)
            {
                var notSelCol = new Color(1.0f, 0.92f, 0.316f);
                var selCol = Color.white;

                var point = TopLeft;
                var col = (m_SelectionInfo.Anchor == Anchor.TopLeft) ? selCol : notSelCol;
                float handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
                Handles.DrawSolidRectangleWithOutline(new Rect(point - Vector2.one * 0.5f * handleSize, Vector2.one * handleSize), col, col);

                point = TopCenter;
                col = (m_SelectionInfo.Anchor == Anchor.TopCenter) ? selCol : notSelCol;
                handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
                Handles.DrawSolidRectangleWithOutline(new Rect(point - Vector2.one * 0.5f * handleSize, Vector2.one * handleSize), col, col);

                point = TopRight;
                col = (m_SelectionInfo.Anchor == Anchor.TopRight) ? selCol : notSelCol;
                handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
                Handles.DrawSolidRectangleWithOutline(new Rect(point - Vector2.one * 0.5f * handleSize, Vector2.one * handleSize), col, col);

                point = MiddleLeft;
                col = (m_SelectionInfo.Anchor == Anchor.MiddleLeft) ? selCol : notSelCol;
                handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
                Handles.DrawSolidRectangleWithOutline(new Rect(point - Vector2.one * 0.5f * handleSize, Vector2.one * handleSize), col, col);

                point = MiddleRight;
                col = (m_SelectionInfo.Anchor == Anchor.MiddleRight) ? selCol : notSelCol;
                handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
                Handles.DrawSolidRectangleWithOutline(new Rect(point - Vector2.one * 0.5f * handleSize, Vector2.one * handleSize), col, col);

                point = BottomLeft;
                col = (m_SelectionInfo.Anchor == Anchor.BottomLeft) ? selCol : notSelCol;
                handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
                Handles.DrawSolidRectangleWithOutline(new Rect(point - Vector2.one * 0.5f * handleSize, Vector2.one * handleSize), col, col);

                point = BottomCenter;
                col = (m_SelectionInfo.Anchor == Anchor.BottomCenter) ? selCol : notSelCol;
                handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
                Handles.DrawSolidRectangleWithOutline(new Rect(point - Vector2.one * 0.5f * handleSize, Vector2.one * handleSize), col, col);

                point = BottomRight;
                col = (m_SelectionInfo.Anchor == Anchor.BottomRight) ? selCol : notSelCol;
                handleSize = c_HandleWidth * HandleUtility.GetHandleSize(new Vector3(point.x, point.y, m_Target.transform.position.z));
                Handles.DrawSolidRectangleWithOutline(new Rect(point - Vector2.one * 0.5f * handleSize, Vector2.one * handleSize), col, col);
            }
        }

        private void drawArea()
        {
            var outlineCol = (m_IsInEditMode) ? Color.yellow : new Color(0, 0, 0, 0);
            Handles.DrawSolidRectangleWithOutline(new Rect(m_Target.Center - m_Target.Extents, m_Target.Extents * 2), m_Target._debugColor, outlineCol);
        }


        private Vector2 getAnchorPosition(Anchor anchor)
        {
            return
                (anchor == Anchor.TopLeft) ? TopLeft :
                (anchor == Anchor.TopCenter) ? TopCenter :
                (anchor == Anchor.TopRight) ? TopRight :

                (anchor == Anchor.MiddleLeft) ? MiddleLeft :
                (anchor == Anchor.MiddleRight) ? MiddleRight :

                (anchor == Anchor.BottomLeft) ? BottomLeft :
                (anchor == Anchor.BottomCenter) ? BottomCenter :
                (anchor == Anchor.BottomRight) ? BottomRight : Vector2.zero;
        }

        private void setAnchorPosition(Anchor anchor, Vector2 pos)
        {
            if (anchor == Anchor.TopLeft) TopLeft = pos;
            if (anchor == Anchor.TopCenter) TopCenter = pos;
            if (anchor == Anchor.TopRight) TopRight = pos;
            if (anchor == Anchor.MiddleLeft) MiddleLeft = pos;
            if (anchor == Anchor.MiddleRight) MiddleRight = pos;
            if (anchor == Anchor.BottomLeft) BottomLeft = pos;
            if (anchor == Anchor.BottomCenter) BottomCenter = pos;
            if (anchor == Anchor.BottomRight) BottomRight = pos;
        }
    }
}
