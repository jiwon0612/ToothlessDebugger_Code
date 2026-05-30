using System;
using System.Collections.Generic;
using System.IO;
using ToothlessDebugger.Editor;
using ToothlessDebugger.RunTime;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CustomDebugEnumGenerateWindow : EditorWindow
{
    public const string _codeBase =
        @"using System;

namespace ToothlessDebugger.RunTime
{{
    [Flags]
    public enum DebugType
    {{
        Default = 1 << 0,{0}
    }}
}}";
    
    [SerializeField] private VisualTreeAsset _visualTreeAsset = default;
    [SerializeField] private VisualTreeAsset _enumTreeAsset = default;

    private Label _enumNameLabel;
    private ScrollView _enumsScrollView;
    private TextField _generateEnumName;
    private Button _generateButton;

    private Dictionary<string,EnumWindow> _enumWindows;

    private string _rootFolder;
    private string _enumFolder;
    

    [MenuItem("Tools/ToothlessDebugger/DebugEnumGenerator")]
    public static void ShowExample()
    {
        CustomDebugEnumGenerateWindow wnd = GetWindow<CustomDebugEnumGenerateWindow>();
        wnd.titleContent = new GUIContent("CustomDebugEnumGenerateWindow");
    }

    public void CreateGUI()
    {
        _enumWindows = new Dictionary<string, EnumWindow>();
        
        InitRootFolder();
        VisualElement root  = rootVisualElement;
        _visualTreeAsset.CloneTree(root); 
        
        _enumNameLabel = root.Q<Label>("EnumName");
        _enumsScrollView = root.Q<ScrollView>("EnumsScrollView");
        
        _generateEnumName = root.Q<TextField>("GenerateEnumText");
        _generateButton = root.Q<Button>("GenerateBtn");

        foreach (DebugType type in Enum.GetValues(typeof(DebugType)))
        {
            if (type == DebugType.Default) continue;
            CreateEnumWindow(type.ToString());
        }
        
        _generateButton.RegisterCallback<ClickEvent>(HandleGenerateEvent);
    }

    private void CreateEnumWindow(string type)
    {
        VisualElement enumName = new VisualElement();
        _enumTreeAsset.CloneTree(enumName);
        EnumWindow enumWindow = new EnumWindow(enumName,type);
        _enumsScrollView.Add(enumName);
        _enumWindows.Add(enumWindow.enumName,enumWindow);

        enumWindow.OnDelete += HandleDeleteEvent;
    }

    private void HandleDeleteEvent(string enumName)
    {
        if (EditorUtility.DisplayDialog("Delete", "Do you want to delete this enumType?", "Yes", "No") == false)
        {
            return;
        }
        
        if (_enumWindows.Remove(enumName, out EnumWindow enumWindow))
        {
            Debug.Log("성공");
            enumWindow.Delete();
            _enumsScrollView.Remove(enumWindow.root);
            RefreshEnumFile();
        }
    }

    private void HandleGenerateEvent(ClickEvent evt)
    {
        if (string.IsNullOrEmpty(_generateEnumName.value)) return;

        if (_enumWindows.ContainsKey(_generateEnumName.value))
        {
            EditorUtility.DisplayDialog("already",$"{_generateEnumName.value} is Names already","Yes");
            _generateEnumName.value = string.Empty;
            return;
        }
        
        CreateEnumWindow(_generateEnumName.value);
        _generateEnumName.value = string.Empty;
        RefreshEnumFile();
    }

    public void RefreshEnumFile()
    {
        string enumNames = string.Empty;
        int cnt = 1;
        foreach (string enumName in _enumWindows.Keys)
        {
            enumNames += $"{enumName} = 1 << {cnt++},";
        }
        string code = string.Format(_codeBase, enumNames);
        File.WriteAllText(_enumFolder, code);
        AssetDatabase.Refresh();
        CompilationPipeline.RequestScriptCompilation();
    }

    private void InitRootFolder()
    {
        MonoScript monoScript = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(monoScript);
        _rootFolder = Path.GetDirectoryName(Path.GetDirectoryName(scriptPath));

        if (_visualTreeAsset == null)
        {
            string path = $"{_rootFolder}/Editor/CustomDebugEnumGenerateWindow.uxml";
            _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Debug.Assert(_visualTreeAsset != null,"VIsualTreeAsset is null");
        }

        if (_enumTreeAsset == null)
        {
            string path = $"{_rootFolder}/Editor/EnumWindow.uxml";
            _enumTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Debug.Assert(_enumTreeAsset != null,"enumTreeAsset is null");
        }

        if (string.IsNullOrEmpty(_enumFolder))
        {
            _enumFolder = $"{_rootFolder}/RunTime/DebugType.cs";
        }
    }
}
