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

        private LGPanelView _view;

        public LGInfoCache LGInfoCache => _lgInfoCache;
        private LGInfoCache _lgInfoCache;
        public LGEditorCache LGEditorCache => _lgEditorCache;
        private LGEditorCache _lgEditorCache;

        private bool _isInit = false;

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
            panel.Show();
            panel.Focus();
            panel.SetLogic(info);
        }


        public void SetLogic(LGInfoCache info)
        {
            if (info != null)
            {
                this._lgInfoCache = info;
                this._lgEditorCache = LGCacheOp.GetEditorCache(info);
                this._lgInfoCache.Graph = LGCacheOp.GetLogicGraph(info);
                if (this._isInit)
                {
                    this._view.ShowLogic();
                }
            }
        }


        #region 内置函数

        private void OnEnable()
        {
            titleContent = new GUIContent("逻辑图");
            ShowGui();
            if (!_isInit)
            {
                _isInit = !_isInit;
            }
            if (this._lgInfoCache != null)
            {
                this._view.ShowLogic();
            }
        }

        /// <summary>
        /// 当rootVisualElement准备完成后调用
        /// </summary>
        private void ShowGui()
        {
            VisualElement root = rootVisualElement;
            _view = new LGPanelView(this);
            this.rootVisualElement.Add(_view);
        }
        private void OnDestroy()
        {
            //this._panelData.Save();
        }
        #endregion
    }

}