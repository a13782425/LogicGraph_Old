using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class LGPanelView : GraphView
    {
        private LGWindow _window;
        public LGPanelView(LGWindow lgWindow)
        {
            _window = lgWindow;
            this.Add(new ToolbarView());
            m_addGridBackGround();
        }



        #region 重写方法

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("创建逻辑图", m_onCreateLogic);
            evt.menu.AppendAction("打开逻辑图", m_onOpenLogic);
        }

        #endregion

        #region 子类重写



        #endregion


        #region 私有方法
        /// <summary>
        /// 创建逻辑图
        /// </summary>
        /// <param name="obj"></param>
        private void m_onCreateLogic(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<CreateLGSearchWindow>();
            menuWindowProvider.onSelectHandler += m_onCreateMenuSelectEntry;
            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }
        /// <summary>
        /// 打开逻辑图
        /// </summary>
        /// <param name="obj"></param>
        private void m_onOpenLogic(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<OpenLGSearchWindow>();
            menuWindowProvider.onSelectHandler += m_onOpenMenuSelectEntry;

            Vector2 screenPos = _window.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }

        private bool m_onCreateMenuSelectEntry(SearchTreeEntry arg1, SearchWindowContext arg2)
        {
            return true;
        }
        private bool m_onOpenMenuSelectEntry(SearchTreeEntry arg1, SearchWindowContext arg2)
        {
            return true;
        }

        #endregion



        private class LGPanelViewGrid : GridBackground { }
        /// <summary>
        /// 添加背景网格
        /// </summary>
        private void m_addGridBackGround()
        {
            //添加网格背景
            GridBackground gridBackground = new LGPanelViewGrid();
            gridBackground.name = "GridBackground";
            Insert(0, gridBackground);
            //设置背景缩放范围
            this.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);

            //扩展大小与父对象相同
            this.StretchToParentSize();
        }
    }
}
