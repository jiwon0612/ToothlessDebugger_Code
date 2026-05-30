using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ToothlessDebugger.Editor;
using ToothlessDebugger.RunTime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class CustomDebugWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset _visualTreeAsset = default;
    [SerializeField] private VisualTreeAsset customLogTreeAsset = default;

    
    private Label _clearBtn;
    //private Label _collapseBtn;
    private Label _debugCounterLabel;
    private ScrollView _logContainer;
    private Label _logTextLabel;
    private VisualElement _logMessageContainer;
    private EnumFlagsField _showDebugTypeField;

    // private Label _testBtn;
    // private Label _testBtn2;

    private List<CustomLog> _logList;
    private CustomLog _selectedLog;
        
    private string _rootFolder;
    private int _logCount;
    //public bool IsCollapse => _collapseBtn.ClassListContains("btn_label_selected");

    [MenuItem("Tools/ToothlessDebugger/DebugWindow")]
    public static void ShowExample()
    {
        CustomDebugWindow wnd = GetWindow<CustomDebugWindow>();
        wnd.titleContent = new GUIContent("DebugWindow");
    }

    public void CreateGUI()
    {
        InitRootFolder();
        _logCount = 0;
        
        VisualElement root = rootVisualElement;
        _visualTreeAsset.CloneTree(root);

        _clearBtn = root.Q<Label>("ClearBtn");
        //_collapseBtn = root.Q<Label>("CollapseBtn");
        _debugCounterLabel = root.Q<Label>("DebugCounterText");
        _logContainer = root.Q<ScrollView>("ScrollLogContainer");
        _logTextLabel = root.Q<Label>("LogText");
        _logMessageContainer = root.Q<VisualElement>("LogMessage");
        //_testBtn = root.Q<Label>("TestBtn");
        //_testBtn2 = root.Q<Label>("TestBtn2");
        _showDebugTypeField = root.Q<EnumFlagsField>("ShowDebugType");
        //_showDebugTypeField.Init(DebugType.);

        _clearBtn.RegisterCallback<ClickEvent>(HandleClearBtn);
        //_collapseBtn.RegisterCallback<ClickEvent>(HandleCollapseBtn);
        _logMessageContainer.RegisterCallback<ClickEvent>(HandleContainerClick);
        //_testBtn.RegisterCallback<ClickEvent>(HandleTest);
        //_testBtn2.RegisterCallback<ClickEvent>(HandleTest2);
        _showDebugTypeField.RegisterValueChangedCallback(HandleFilterChanged);

        _logList = new List<CustomLog>();
        
        for (int i = 0; i < CustomDebug.debugData.Count; i++)
        {
            CreateLogWindow(CustomDebug.debugData[i]);
        }

        CustomDebug.OnAddDebug += HandleDebugLog;
        
        //CustomDebug.Log("박시우 바보");
        //CustomDebugEnumGenerator d =  new CustomDebugEnumGenerator();
    }

    private void HandleFilterChanged(ChangeEvent<Enum> evt)
    {
        // _selectedLog = null;
        // _logList.Clear();
        _selectedLog = null;
        _logContainer.Clear();
        _logCount = 0;
        _debugCounterLabel.text = (_logCount).ToString();
        
        for (int i = 0; i < CustomDebug.debugData.Count; i++)
        {
            //if (evt.newValue.)
            CreateLogWindow(CustomDebug.debugData[i]);
        }
    }

    private void HandleContainerClick(ClickEvent evt)
    {
        if (evt.clickCount == 2)
        {
            OpenCodeFile();
        }
    }

    private void OpenCodeFile()
    {
        if (_selectedLog != null)
        {
            var frame = _selectedLog.Data.stackTrace.GetFrame(1);
                
            string filePath = frame.GetFileName();
            int lineNumber = frame.GetFileLineNumber();

            if (!string.IsNullOrEmpty(filePath) && lineNumber > 0)
            {
                //Debug.Log("dd");
                InternalEditorUtility.OpenFileAtLineExternal(filePath.Replace("\\", "/"), lineNumber);
            }
                
            //Debug.Log(_selectedLog._data.stackTrace.ToString());
        }
    }

    private void HandleDebugLog(CustomDebugData log)
    {
        CreateLogWindow(log);
    }

    private void CreateLogWindow(CustomDebugData log)
    {
        // _logList.Clear();
        // _logContainer.Clear();
        
        if (_showDebugTypeField.value.HasFlag(log.debugType))
        {
            VisualElement logWindow = new VisualElement();
            customLogTreeAsset.CloneTree(logWindow);
            CustomLog logUI = new CustomLog(logWindow, log);
            _logContainer.Add(logWindow);
            _logList.Add(logUI);

            logUI.OnSelected += HandleSelected;

            // var d = _logContainer.verticalScroller;
            // d.value += _logList.Count * 100;

            _logContainer.schedule.Execute(() =>
            {
                _logContainer.scrollOffset = new Vector2(0, _logContainer.verticalScroller.highValue);
            }).ExecuteLater(1);

            _debugCounterLabel.text = (++_logCount).ToString();
        }
    }

    private void HandleSelected(CustomLog logWindow, ClickEvent evt)
    {
        if (_selectedLog != null)
            _selectedLog.IsSelected = false;
        
        _selectedLog = logWindow;
        _selectedLog.IsSelected = true;

        StackFrame trace = _selectedLog.Data.stackTrace.GetFrame(1);//null; // _selectedLog._data.stackTrace.GetFrame(1);
        // foreach (var frame in _selectedLog._data.stackTrace.GetFrames())
        // {
        //     var file =  frame.GetFileName();
        //     if (!string.IsNullOrEmpty(file))
        //     {
        //         trace = frame;
        //     }
        // }

        _logTextLabel.text = $"File : {trace.GetFileName()} \nLine : {trace.GetFileLineNumber()} \nType : {logWindow.Data.debugType}";

        if (evt.clickCount == 2)
        {
            OpenCodeFile();
        }
    }

    // private void HandleTest(ClickEvent evt)
    // {
    //     // TemplateContainer logWindow = customLogTreeAsset.Instantiate();
    //     // CustomLog logUI = new CustomLog(logWindow, new CustomDebugData("123", "안녕"));
    //     // _logContainer.Add(logWindow);
    //     // _logList.Add(logUI);
    //     //CustomDebug.Log("Test",DebugType.Entity);
    //     // var d = _logContainer.verticalScroller;
    //     // d.value += _logList.Count * 100;
    //
    // }
    
    // private void HandleTest2(ClickEvent evt)
    // {
    //     //CustomDebug.Log("Test2", DebugType.Test2);
    // }

    private void HandleClearBtn(ClickEvent evt)
    {
        _logContainer.schedule.Execute(() =>
        {
            _logList.Clear();
            _logContainer.Clear();
            CustomDebug.debugData.Clear();
            _logCount = 0;
            _debugCounterLabel.text = (_logCount).ToString();
            _logList = new List<CustomLog>();
            _logTextLabel.text = string.Empty;
        });
    }

    // private void HandleCollapseBtn(ClickEvent evt)
    // {
    //     //_collapseBtn.ToggleInClassList("btn_label_selected");
    // }

    private void InitRootFolder()
    {
        MonoScript monoScript = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(monoScript);
        _rootFolder = Path.GetDirectoryName(Path.GetDirectoryName(scriptPath)).Replace("\\", "/");

        if (_visualTreeAsset == null)
        {
            string path = $"{_rootFolder}/Editor/CustomDebugWindow.uxml";
            _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Debug.Assert(_visualTreeAsset != null, "VisualTreeAsset is null");
        }

        if (customLogTreeAsset == null)
        {
            string path = $"{_rootFolder}/Editor/CustomDebugWindow.uxml";
            customLogTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Debug.Assert(customLogTreeAsset != null, "VisualTreeAsset is null");
        }
    }
}