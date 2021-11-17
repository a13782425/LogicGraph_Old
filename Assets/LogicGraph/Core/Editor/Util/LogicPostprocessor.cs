using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
    /// <summary>
    /// 每次资源变化调用
    /// </summary>
    public sealed class LogicPostprocessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.DidReloadScripts(100)]
        static void OnScriptReload()
        {
            LGCacheOp.Refresh();
        }

        /// <summary>
        /// 所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
        /// </summary>
        /// <param name="importedAsset">导入的资源</param>
        /// <param name="deletedAssets">删除的资源</param>
        /// <param name="movedAssets">移动后资源路径</param>
        /// <param name="movedFromAssetPaths">移动前资源路径</param>
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var str in movedFromAssetPaths)
            {
                //移动前资源路径
                string ext = Path.GetExtension(str);
                if (ext == ".asset")
                {
                    LGCacheOp.RemoveLogicGraph(str);
                }
            }
            foreach (var str in movedAssets)
            {
                //移动后资源路径
                string ext = Path.GetExtension(str);
                if (ext == ".asset")
                {
                    LGCacheOp.AddLogicGraph(str);
                }
            }
            foreach (string str in importedAsset)
            {
                string ext = Path.GetExtension(str);
                if (ext == ".asset")
                {
                    LGCacheOp.AddLogicGraph(str);
                }
            }
            foreach (string str in deletedAssets)
            {
                string ext = Path.GetExtension(str);
                if (ext == ".asset")
                {
                    LGCacheOp.RemoveLogicGraph(str);
                }
            }
        }


    }
}
