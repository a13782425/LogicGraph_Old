using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图的事件处理
    /// </summary>
    public sealed class LGEvent
    {
        /// <summary>
        /// 变量变化
        /// value : BaseVariable
        /// </summary>
        public const int VAR_MODDIFY = 1;


        private BaseGraphView _graphView;

        public LGEvent(BaseGraphView graphView)
        {
            this._graphView = graphView;
        }
        private Dictionary<int, List<Action<object>>> eventDic = new Dictionary<int, List<Action<object>>>();

        public void AddEvent(int eventId, Action<object> action)
        {
            if (!eventDic.ContainsKey(eventId))
            {
                eventDic.Add(eventId, new List<Action<object>>());
            }

            eventDic[eventId].Add(action);
        }
        public void DelEvent(int eventId, Action<object> action)
        {
            if (eventDic.ContainsKey(eventId))
            {
                eventDic[eventId].Remove(action);
            }
        }

        public void OnEvent(int eventId, object param = null)
        {
            if (eventDic.ContainsKey(eventId))
            {
                eventDic[eventId].ToList().ForEach(a => a?.Invoke(param));
            }
        }
    }
}
