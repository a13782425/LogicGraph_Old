using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public enum NodeEnum
{
    //[10000-12000) 公共节点
    /// <summary>
    /// 通用开始节点
    /// </summary>
    Start = 10000,
    /// <summary>
    /// 等待
    /// </summary>
    Delay,
    /// <summary>
    /// 显示通知
    /// </summary>
    Notification,
    /// <summary>
    /// 随机float
    /// </summary>
    RandomFloat,
    /// <summary>
    /// 判断单精度
    /// </summary>
    IfFloat,
    /// <summary>
    /// 判断整型
    /// </summary>
    IfInt,
    /// <summary>
    /// 判断Bool
    /// </summary>
    IfBool,
    /// <summary>
    /// 判断变量和变量
    /// </summary>
    IfVar,
    /// <summary>
    /// 偏移Vector2坐标
    /// </summary>
    ModifyV2,
    /// <summary>
    /// 偏移Vector3坐标
    /// </summary>
    ModifyV3,
    /// <summary>
    /// 输出Float节点
    /// </summary>
    NewFloat,
    /// <summary>
    /// 创建游戏对象
    /// </summary>
    CreateObj,
    /// <summary>
    /// 删除游戏对象
    /// </summary>
    DeleteObj,
    /// <summary>
    /// 设置对象状态显示还是隐藏
    /// </summary>
    SetObjState,
    /// <summary>
    /// 移动游戏对象
    /// </summary>
    MoveObjPos,
    /// <summary>
    /// 查找游戏对象子节点
    /// </summary>
    FindObjChild,
    /// <summary>
    /// 获取相机
    /// </summary>
    GetCamera,
    /// <summary>
    /// 设置Animation动画状态
    /// </summary>
    SetAtnState,
    /// <summary>
    /// 设置Animator动画状态
    /// </summary>
    SetAtrState,
    /// <summary>
    /// 播放特效
    /// </summary>
    PlayEffect,
    /// <summary>
    /// 停止播放特效
    /// </summary>
    StopEffect,
    /// <summary>
    /// 播放音效
    /// </summary>
    PlaySound,
    /// <summary>
    /// 停止音效
    /// </summary>
    StopSound,
    /// <summary>
    /// 是否节点是否执行完
    /// </summary>
    IsComplete,
    /// <summary>
    /// 触发器
    /// </summary>
    Trigger,
    /// <summary>
    /// 获取特效物体
    /// </summary>
    GetEffObj,
    /// <summary>
    /// 获取对象坐标点
    /// </summary>
    GetTfPos,
    /// <summary>
    /// for循环(逻辑迭代)
    /// </summary>
    LogicIterat,

    //[12000-13000) 天气节点
    EnterWeater = 12000,

    WeaterSun = 12100,

    //[13000-14000) 放置AI节点
    /// <summary>
    /// 是否存在放置事件
    /// </summary>
    IdleHasEvent = 13000,
    /// <summary>
    /// 选取放置的一个点
    /// </summary>
    IdlePickPoint,
    /// <summary>
    /// 生成放置事件
    /// </summary>
    IdleGenEvent,
    /// <summary>
    /// 设置船只状态
    /// </summary>
    IdleSetShipState,

    //[14000-15000) 放置事件节点
    /// <summary>
    /// 获取事件对象
    /// </summary>
    IdleEventGetObj = 14000,
    /// <summary>
    /// 获取事件点坐标
    /// </summary>
    IdleEventGetPos,
    /// <summary>
    /// 是否是玩家自己
    /// </summary>
    IsPlayer,
    /// <summary>
    /// 事件是否导航中
    /// </summary>
    NaviState,
    /// <summary>
    /// 播放特效(通过位置)
    /// </summary>
    PlayEffectByPos,

    //[50000-60000) 服务端节点
    /// <summary>
    /// 服务端开始节点
    /// </summary>
    ServerStart = 50000,

    /// <summary>
    /// 并行节点
    /// </summary>
    ServerParallel,

    /// <summary>
    /// 选择节点
    /// </summary>
    ServerSelector,

    /// <summary>
    /// 序列节点
    /// </summary>
    ServerSequence,

    /// <summary>
    /// 循环节点
    /// </summary>
    ServerLoop,

    /// <summary>
    /// 条件循环节点
    /// </summary>
    ServerWhile,

    /// <summary>
    /// CD节点
    /// </summary>
    ServerCD,

    /// <summary>
    /// 行为节点
    /// </summary>
    ServerAction,

    /// <summary>
    /// 成功节点
    /// </summary>
    ServerSuccess,

    /// <summary>
    /// 失败节点
    /// </summary>
    ServerFailure,

    /// <summary>
    /// 延迟节点
    /// </summary>
    ServerDelay,

    /// <summary>
    /// 条件节点
    /// </summary>
    ServerCondition,

}
public enum FieldType
{ 
   Int,
   Float,
   String,
   Vector3,
   Vector2,
   GameObject,
}

public class FieldData
{
    public FieldType fieldType;
    public string vName;
    public string cName;
    public string jName;
    public string desc;
    public string tip;


    public FieldData(FieldType _fieldType, string _name, string _jName, string _desc, string _tip)
    {
        fieldType = _fieldType;
        vName = $"_{_name}Port";
        cName = $"{_name}VarNode";
        jName = _jName;
        desc = _desc;
        tip = _tip;
    }
}

public class NodeData
{
    public string className;
    public string pathStr;
    public string luaName;
    public NodeEnum nodeEnum;
    public List<FieldData> inputField = new List<FieldData>();
    public List<FieldData> viewField = new List<FieldData>();
    public List<FieldData> outField = new List<FieldData>();
}

public class NodeCreateTool : EditorWindow
{
    [MenuItem("Window/NodeCreateTool")]
    public static void ShowExample()
    {
        NodeCreateTool wnd = GetWindow<NodeCreateTool>();
        wnd.titleContent = new GUIContent("NodeCreateTool");


    }

    private void OnClick(VisualElement root, int idx)
    {
        root.Q<VisualElement>("view1").SetEnabled(idx == 1);
        root.Q<VisualElement>("view2").SetEnabled(idx == 2);
        root.Q<VisualElement>("view3").SetEnabled(idx == 3);
    }

    List<int> items = new List<int>();

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/NodeCreateTool.uxml");
        VisualElement labelWithStyle = visualTree.CloneTree();
        root.Add(labelWithStyle);

        root.Q<ToolbarToggle>("tog1").RegisterCallback<MouseCaptureEvent>((evt) => { OnClick(root, 1); });
        root.Q<ToolbarToggle>("tog2").RegisterCallback<MouseCaptureEvent>((evt) => { OnClick(root, 2); });
        root.Q<ToolbarToggle>("tog3").RegisterCallback<MouseCaptureEvent>((evt) => { OnClick(root, 3); });


        root.Q<Button>("btn_add1").RegisterCallback<MouseCaptureEvent>((evt) => { 
            items.Add(1);
            var listView2 = root.Q<ListView>("list_input1");
            listView2.Refresh();
        });

        root.Q<Button>("btn_save").RegisterCallback<MouseCaptureEvent>((evt) => {
            CreateNode();
        });

        Func<VisualElement> makeItem = () =>
        {
            var visualTree2 = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/FieldItem.uxml");
            VisualElement labelFromUXML = visualTree2.Instantiate();
            return labelFromUXML;// new Label();
        };
        Action<VisualElement, int> bindItem = (item, i) =>
        {
            item.Q<EnumField>("enum").Init(FieldType.Int);
            item.Q<Button>("btn_remove").RegisterCallback<MouseCaptureEvent>((evt) => {
                items.RemoveAt(i);
                var listView2 = root.Q<ListView>("list_input1");
                listView2.Refresh();
            });
        };

        var listView = root.Q<ListView>("list_input1");
        listView.makeItem = makeItem;
        listView.bindItem = bindItem;
        listView.itemsSource = items;
        listView.selectionType = SelectionType.Multiple;
        //listView.onItemsChosen += Debug.Log;
        //listView.onSelectionChange += Debug.Log;

    }


    //private static Texture2D scriptIcon = (EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D);
    public static string EDITOR_PATH;

    private void CreateNode()
    {

        Type lgType = typeof(NodeCreateTool);
        string[] guids = AssetDatabase.FindAssets(lgType.Name);
        foreach (var item in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(item);
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (monoScript.GetClass() == lgType)
            {
                EDITOR_PATH = Path.GetDirectoryName(path);
                break;
            }
        }

        NodeData temp = new NodeData();
        temp.className = "ComNode";
        temp.luaName = "comNode";
        temp.pathStr = "系统/通用";
        temp.nodeEnum = NodeEnum.CreateObj;

        FieldData field1 = new FieldData(FieldType.Float, "field1", "f1", "参数1", "提示1");
        FieldData field2 = new FieldData(FieldType.Float, "field2", "f2", "参数2", "提示2");
        FieldData field3 = new FieldData(FieldType.Float, "field3", "f3", "参数3", "提示3");
        temp.inputField.Add(field1);
        temp.viewField.Add(field2);
        temp.outField.Add(field3);

        string codePath = Path.Combine(EDITOR_PATH, "Template/LogicNodeTemplate.cs");
        ChangeCodeTemp(EDITOR_PATH, codePath, temp);

        string tempPath = Path.Combine(EDITOR_PATH, "Template/LogicNodeViewTemplate.cs");
        ChangeViewTemp(EDITOR_PATH,tempPath, temp);
    }

    //string path = Path.Combine(EDITOR_PATH, "Template/LogicNodeTemplate.cs");
    //CreateFromTemplate<DoCreateNodeCodeFile>("NewNode.cs", path);

    //string tempPath = Path.Combine(EDITOR_PATH, "Template/LogicNodeViewTemplate.cs");
    //string fileName = Path.GetFileNameWithoutExtension("NewNode.cs");
    //CreateFromTemplate<DoCreateNodeCodeFile>($"{fileName}View.cs", tempPath);

    //public static void CreateFromTemplate<T>(string initialName, string templatePath) where T : UnityEditor.ProjectWindowCallback.EndNameEditAction
    //{
    //    ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
    //        0,
    //        ScriptableObject.CreateInstance<T>(),
    //        initialName,
    //        null,
    //        templatePath
    //    );
    //}

    //private class DoCreateNodeCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
    //{
    //    public override void Action(int instanceId, string pathName, string resourceFile)
    //    {
    //        string className = Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty);
    //        Object o = CreateScript(className, pathName, resourceFile);
    //        ProjectWindowUtil.ShowCreatedAsset(o);
    //    }
    //}

    //internal static Object CreateScript(string className, string pathName, string templatePath)
    //{
    //    string templateText = string.Empty;

    //    UTF8Encoding encoding = new UTF8Encoding(true, false);
    //    templatePath = templatePath += ".txt";
    //    if (File.Exists(templatePath))
    //    {
    //        /// Read procedures.
    //        StreamReader reader = new StreamReader(templatePath);
    //        templateText = reader.ReadToEnd();
    //        reader.Close();

    //        templateText = templateText.Replace("{CLASS_NAME}", className);

    //        StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
    //        writer.Write(templateText);
    //        writer.Close();

    //        AssetDatabase.ImportAsset(pathName);
    //        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    //    }
    //    else
    //    {
    //        Debug.LogError(string.Format("The template file was not found: {0}", templatePath));
    //        return null;
    //    }
    //}

    #region 生成节点
    private void ChangeCodeTemp(string path, string templatePath, NodeData temp)
    {
        string templateText = string.Empty;
        UTF8Encoding encoding = new UTF8Encoding(true, false);
        templatePath = templatePath += ".txt";
        if (File.Exists(templatePath))
        {
            StreamReader reader = new StreamReader(templatePath);
            templateText = reader.ReadToEnd();
            reader.Close();

            templateText = templateText.Replace("{pos1}", temp.className);
            templateText = templateText.Replace("{pos2}", temp.luaName);
            templateText = templateText.Replace("{pos3}", temp.nodeEnum.ToString());
            templateText = templateText.Replace("{pos4}", GetCodeStr4(temp));
            templateText = templateText.Replace("{pos5}", GetCodeStr5(temp));
            templateText = templateText.Replace("{pos6}", GetCodeStr6(temp));

            CreateOrOpenFile(path, $"{temp.className}.cs", templateText);
        }
    }
    private string GetCodeStr4(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"        [SerializeReference]");
            sb.AppendLine($"         public VariableNode {item.cName};");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"        [SerializeReference]");
            sb.AppendLine($"         public VariableNode {item.cName};");
        }
        return sb.ToString();
    }

    private string GetCodeStr5(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"            if ({item.cName} != null)");
            sb.AppendLine("            {");
            sb.AppendLine($"                json.{item.jName} = {item.cName}.variable.Name;");
            sb.AppendLine("            }");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"            if ({item.cName} != null)");
            sb.AppendLine("            {");
            sb.AppendLine($"                json.{item.jName} = {item.cName}.variable.Name;");
            sb.AppendLine("            }");
        }
        return sb.ToString();
    }

    private string GetCodeStr6(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"            public string {item.jName};");
        }
        foreach (var item in temp.viewField)
        {
            sb.AppendLine($"            public string {item.jName};");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"            public string {item.jName};");
        }
        return sb.ToString();
    }
    #endregion

    #region 生成界面
    private void ChangeViewTemp(string path,string templatePath, NodeData temp)
    {
        string templateText = string.Empty;
        UTF8Encoding encoding = new UTF8Encoding(true, false);
        templatePath = templatePath += ".txt";
        if (File.Exists(templatePath))
        {
            StreamReader reader = new StreamReader(templatePath);
            templateText = reader.ReadToEnd();
            reader.Close();

            templateText = templateText.Replace("{pos1}", temp.className);
            templateText = templateText.Replace("{pos2}", temp.pathStr);
            templateText = templateText.Replace("{pos3}", GetStr3(temp));
            templateText = templateText.Replace("{pos4}", GetStr4(temp));
            templateText = templateText.Replace("{pos5}", GetStr5(temp));
            templateText = templateText.Replace("{pos6}", GetStr6(temp));
            templateText = templateText.Replace("{pos7}", GetStr7(temp));
            templateText = templateText.Replace("{pos8}", GetStr8(temp));

            CreateOrOpenFile(path, $"{temp.className}View.cs", templateText);
        }
    }

    private string GetStr3(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"        PortView {item.vName};");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"        PortView {item.vName};");
        }
        return sb.ToString();
    }

    private string GetStr4(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"            {item.vName} = AddPort(\"{item.desc}\", Direction.Input, Port.Capacity.Single, true);");
            sb.AppendLine($"            {item.vName}.tooltip = \"{ item.tip} \";");
            sb.AppendLine($"            AddUI({item.vName});");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"            {item.vName} = AddPort(\"{item.desc}\", Direction.Output, Port.Capacity.Single, true);");
            sb.AppendLine($"            {item.vName}.tooltip = \"{ item.tip} \";");
            sb.AppendLine($"            AddUI({item.vName});");
        }
        foreach (var item in temp.viewField)
        {
            if (item.fieldType == FieldType.GameObject)
            {
                sb.AppendLine($"            AddUI(new IMGUIContainer(GameObjectField));");
            }
            else
            {
                sb.AppendLine($"            AddUI(GetInputField(\"{item.desc}\", node.{item.cName}, (a) => node.{item.cName} = a));");
            }
        }
        return sb.ToString();
    }

    private string GetStr5(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"            if (accessor == ParamAccessor.Get)");
            sb.AppendLine("             {");
            sb.AppendLine($"                if (varNode.variable.GetValueType() == typeof({item.fieldType.ToString().ToLower()}))");
            sb.AppendLine("                {");
            sb.AppendLine($"                    node.{item.cName} = varNode;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"            if (accessor == ParamAccessor.Set)");
            sb.AppendLine("             {");
            sb.AppendLine($"                if (varNode.variable.GetValueType() == typeof({item.fieldType.ToString().ToLower()}))");
            sb.AppendLine("                {");
            sb.AppendLine($"                    node.{item.cName} = varNode;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        return sb.ToString();
    }

    private string GetStr6(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"            if (accessor == ParamAccessor.Get)");
            sb.AppendLine("             {");
            sb.AppendLine($"                if (varNode.variable.GetValueType() == typeof({item.fieldType.ToString().ToLower()}))");
            sb.AppendLine("                {");
            sb.AppendLine($"                    node.{item.cName} = null;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"            if (accessor == ParamAccessor.Set)");
            sb.AppendLine("             {");
            sb.AppendLine($"                if (varNode.variable.GetValueType() == typeof({item.fieldType.ToString().ToLower()}))");
            sb.AppendLine("                {");
            sb.AppendLine($"                    node.{item.cName} = null;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        return sb.ToString();
    }

    private string GetStr7(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"            if (ownerPort == {item.vName})");
            sb.AppendLine("             {");
            sb.AppendLine($"                if (waitLinkPort.Owner is VariableNodeView varView)");
            sb.AppendLine("                {");
            sb.AppendLine($"                    return (varView.Target as VariableNode).variable.GetValueType() == typeof({item.fieldType.ToString().ToLower()});");
            sb.AppendLine("                }");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"            if (ownerPort == {item.vName})");
            sb.AppendLine("             {");
            sb.AppendLine($"                if (waitLinkPort.Owner is VariableNodeView varView)");
            sb.AppendLine("                {");
            sb.AppendLine($"                    return (varView.Target as VariableNode).variable.GetValueType() == typeof({item.fieldType.ToString().ToLower()});");
            sb.AppendLine("                }");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
        }
        return sb.ToString();
    }

    private string GetStr8(NodeData temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in temp.inputField)
        {
            sb.AppendLine($"            if (node.{item.cName} != null)");
            sb.AppendLine("             {");
            sb.AppendLine($"                var nodeView = graphCache.GetNodeView(node.{item.cName} );");
            sb.AppendLine("                 if (nodeView != null && nodeView.OutPut != null)");
            sb.AppendLine("                {");
            sb.AppendLine($"                    DrawLink({item.vName}, nodeView.OutPut);");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        foreach (var item in temp.outField)
        {
            sb.AppendLine($"            if (node.{item.cName} != null)");
            sb.AppendLine("             {");
            sb.AppendLine($"                var nodeView = graphCache.GetNodeView(node.{item.cName} );");
            sb.AppendLine("                 if (nodeView != null && nodeView.OutPut != null)");
            sb.AppendLine("                {");
            sb.AppendLine($"                    DrawLink(nodeView.Input, {item.vName});");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        return sb.ToString();
    }
    #endregion

    private void CreateOrOpenFile(string path, string name, string info)
    {
        string fullPath = $"{path}/{name}";
        if (!File.Exists(fullPath))
        {
            using (StreamWriter sw = File.CreateText(fullPath))
            {
                sw.WriteLine(info);
                sw.Close();
            }
        }
        else
        {
            using (StreamWriter sw = new StreamWriter(fullPath, false, System.Text.Encoding.UTF8))
            {
                sw.WriteLine(info);
                sw.Close();
            }
        }
        AssetDatabase.Refresh();
    }
}