using System;
using UnityEngine.UIElements;

namespace ToothlessDebugger.Editor
{
    public class EnumWindow
    {
        public Action<string> OnDelete;
        public string enumName;
        public VisualElement root;
        
        private Label _name;
        private Label _deleteBtn;
        
        public EnumWindow(VisualElement root,string enumName)
        {
            this.root = root;
            this.enumName = enumName;
            
            _name = this.root.Q<Label>("EnumName");
            _deleteBtn = this.root.Q<Label>("DeleteBtn");
            
            _name.text = this.enumName;
            _deleteBtn.RegisterCallback<ClickEvent>(HandleDeleteClickEvent);
        }

        private void HandleDeleteClickEvent(ClickEvent evt)
        {
            OnDelete?.Invoke(enumName);
            evt.StopPropagation();
        }
        
        public void Delete()
        {
            root.UnregisterCallback<ClickEvent>(HandleDeleteClickEvent);
        }
    }
}