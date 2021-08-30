using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Logic.Editor
{
    /// <summary>
    /// 每次资源变化调用
    /// </summary>
    public sealed class LogicPostprocessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.DidReloadScripts()]
        static void OnScriptReload()
        {
            LGCacheOp.Refresh();
        }
    }
}
