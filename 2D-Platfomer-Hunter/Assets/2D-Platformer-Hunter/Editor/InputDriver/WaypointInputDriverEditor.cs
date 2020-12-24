using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DYP
{
    [CustomEditor(typeof(WaypointInputDriver))]
    public class WaypointInputDriverEditor : Editor
    {
        WaypointInputDriver m_Target;

        private void OnEnable()
        {
            m_Target = target as WaypointInputDriver;
        }

        private void OnSceneGUI()
        {
            var oriColor = Handles.color;

            Handles.color = Color.blue;
            Handles.DrawWireCube(m_Target.transform.position, Vector3.one);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            Handles.Label(m_Target.transform.position, (m_Target.IsCyclic) ? "cyclic" : "turning", style);

            Handles.color = oriColor;
        }
    }
}
