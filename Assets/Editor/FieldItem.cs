using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class FieldItem : EditorWindow
{
    [MenuItem("Window/UI Toolkit/FieldItem")]
    public static void ShowExample()
    {
        FieldItem wnd = GetWindow<FieldItem>();
        wnd.titleContent = new GUIContent("FieldItem");
    }

    public void CreateGUI()
    {

        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/FieldItem.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
    }
}