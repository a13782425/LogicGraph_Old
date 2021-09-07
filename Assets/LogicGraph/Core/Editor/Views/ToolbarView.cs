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
    public sealed class ToolbarView : VisualElement
    {
        public event Action onDrawLeft;
        public event Action onDrawRight;
        public ToolbarView()
        {
            Add(new IMGUIContainer(DrawImGUIToolbar));
        }
        private void DrawImGUIToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            onDrawLeft?.Invoke();

            GUILayout.FlexibleSpace();

            onDrawRight?.Invoke();

            GUILayout.EndHorizontal();
        }
    }
}
