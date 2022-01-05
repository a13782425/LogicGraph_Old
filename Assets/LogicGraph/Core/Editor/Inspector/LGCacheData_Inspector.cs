using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
    [CustomEditor(typeof(LGCacheData))]
    sealed class LGCacheData_Inspector : UnityEditor.Editor
    {
        private LGCacheData _cacheData;

        void OnEnable()
        {
            _cacheData = target as LGCacheData;
        }
        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }
}
