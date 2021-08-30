using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class LGWindow : EditorWindow
    {
        public LGInfoCache LGInfoCache => _lgInfoCache;
        private LGInfoCache _lgInfoCache;
        public LGEditorCache LGEditorCache => _lgEditorCache;
        private LGEditorCache _lgEditorCache;

        [MenuItem("Framework/逻辑图/打开逻辑图")]
        private static void OpenLogic()
        {
            ShowLGPanel(null);
        }

        /// <summary>
        /// 显示逻辑图面板
        /// </summary>
        /// <param name="p"></param>
        public static void ShowLGPanel(LGInfoCache info)
        {
            LGWindow panel = CreateWindow<LGWindow>();
            if (info != null)
            {
                panel._lgInfoCache = info;
                panel._lgEditorCache = LGCacheOp.GetEditorCache(info);
            }
            panel.Show();
            panel.Focus();
        }



        #region 内置函数

        private void OnEnable()
        {
            titleContent = new GUIContent("逻辑图");
            ShowGui();
        }

        /// <summary>
        /// 当rootVisualElement准备完成后调用
        /// </summary>
        private void ShowGui()
        {
            VisualElement root = rootVisualElement;
            var panelView = new LGPanelView(this);
            this.rootVisualElement.Add(panelView);
        }
        private void OnDestroy()
        {
            //this._panelData.Save();
        }
        #endregion
    }

}