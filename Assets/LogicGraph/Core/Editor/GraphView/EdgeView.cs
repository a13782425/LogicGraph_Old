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
    public sealed class EdgeView : Edge
    {
        public bool isConnected = false;
        //private BaseGraphView owner => ((input ?? output) as PortView).owner.owner;

        public EdgeView() : base()
        {
            styleSheets.Add(LogicUtils.GetEdgeStyle());
        }

        //public override void OnPortChanged(bool isInput)
        //{
        //	base.OnPortChanged(isInput);
        //	UpdateEdgeSize();
        //}

        //public void UpdateEdgeSize()
        //{
        //	if (input == null && output == null)
        //		return;

        //	PortData inputPortData = (input as PortView)?.portData;
        //	PortData outputPortData = (output as PortView)?.portData;

        //	for (int i = 1; i < 20; i++)
        //		RemoveFromClassList($"edge_{i}");
        //	int maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);
        //	if (maxPortSize > 0)
        //		AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");
        //}

        //protected override void OnCustomStyleResolved(ICustomStyle styles)
        //{
        //	base.OnCustomStyleResolved(styles);

        //	UpdateEdgeControl();
        //}


    }
}
