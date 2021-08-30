using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public class ToolbarView : VisualElement
    {
        public ToolbarView()
        {
            Add(new IMGUIContainer(DrawImGUIToolbar));
        }
		void DrawImGUIButtonList()
		{
            GUILayout.Button("测试Bar", EditorStyles.toolbarButton);

        }
		protected virtual void DrawImGUIToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            DrawImGUIButtonList();

            GUILayout.FlexibleSpace();

            DrawImGUIButtonList();

            GUILayout.EndHorizontal();
        }
    }
}
