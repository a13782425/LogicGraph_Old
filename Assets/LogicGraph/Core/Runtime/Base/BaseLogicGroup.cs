#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Logic
{
    /// <summary>
    /// 分组缓存
    /// </summary>
    [Serializable]
    public sealed class BaseLogicGroup
    {
        public string Title = "New Group";
        public Color Color = new Color(0, 0, 0, 0.3f);
        public Vector2 Pos;
        public Vector2 Size;
        public List<string> Nodes = new List<string>();
    }

}

#endif