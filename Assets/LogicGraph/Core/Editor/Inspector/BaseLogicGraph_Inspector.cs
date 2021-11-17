using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
    [CustomEditor(typeof(BaseLogicGraph), true)]
    public sealed class BaseLogicGraph_Inspector : UnityEditor.Editor
    {
        private BaseLogicGraph _logic;

        void OnEnable()
        {
            _logic = target as BaseLogicGraph;
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("打开"))
            {
                LGWindow.ShowLGPanel(LGCacheOp.GetLogicInfo(_logic));
            }
            UnityEditor.EditorGUI.BeginDisabledGroup(false);
            base.OnInspectorGUI();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }
}
