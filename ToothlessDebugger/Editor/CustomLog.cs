using System;
using ToothlessDebugger.RunTime;
using UnityEngine;
using UnityEngine.UIElements;

namespace ToothlessDebugger.Editor
{
    public class CustomLog
    {
        public Action<CustomLog, ClickEvent> OnSelected;
        
        private Label _text;
        private Label _counter;
        private VisualElement _root;
        private VisualElement _container;

        public CustomDebugData Data;

        public bool IsSelected
        {
            get => _container.ClassListContains("log_selected");
            set => _container.EnableInClassList("log_selected", value);
        }

        public CustomLog(VisualElement root, CustomDebugData data)
        {
            Data = data;
            _root = root;

            _container = _root.Q("Container");
            _text = _root.Q<Label>("Text");
            _counter = _root.Q<Label>("Count");
            
            _counter.style.display = DisplayStyle.None;
            _text.text = $"{Data.time} : {Data.message}";
            
            _root.RegisterCallback<ClickEvent>(HandleClickEvent);
            
        }

        private void HandleClickEvent(ClickEvent evt)
        {
            OnSelected?.Invoke(this,evt);
            evt.StopPropagation();
        }
        
        
    }
}