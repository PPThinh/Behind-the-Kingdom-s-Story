using System;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using UnityEditor;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Melee
{
    public class CombosTool : VisualElement
    {
        private const string USS_PATH = EditorPaths.PACKAGES + "Melee/Editor/StyleSheets/CombosTool";

        private const string KEY_LASTS_SELECTION = "gc:melee:inspector-selection:{0}";

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private readonly TwoPaneSplitView m_SplitView;

        // PROPERTIES: ----------------------------------------------------------------------------

        [field: NonSerialized] public SerializedProperty Property { get; }

        public ComboTree Instance
        {
            get
            {
                this.Property.serializedObject.Update();
                return this.Property.managedReferenceValue as ComboTree;
            }
            set
            {
                this.SerializedObject.Update();
                this.Property.managedReferenceValue = value;

                int random = UnityEngine.Random.Range(1, 99999);
                this.Property.FindPropertyRelative("m_Dirty").intValue = random;
                this.SerializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public SerializedObject SerializedObject => this.Property.serializedObject;

        private SerializedProperty PropertyData => 
            this.Property.FindPropertyRelative(ComboTree.NAME_DATA);
        
        private SerializedProperty PropertyDataKeys => 
            this.PropertyData.FindPropertyRelative(ComboTree.NAME_DATA_KEYS);
        
        private SerializedProperty PropertyDataValues => 
            this.PropertyData.FindPropertyRelative(ComboTree.NAME_DATA_VALUES);

        public CombosToolToolbar Toolbar { get; }
        public CombosToolTree Tree { get; }
        public CombosToolInspector Inspector { get; }
        
        private int LastSelection
        {
            get => SessionState.GetInt(this.SelectionKey, ComboTree.NODE_INVALID);
            set => SessionState.SetInt(this.SelectionKey, value);
        }

        private string SelectionKey => string.Format(
            KEY_LASTS_SELECTION,
            this.Property.serializedObject.targetObject.GetInstanceID()
        );

        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public CombosTool(SerializedProperty property)
        {
            this.Property = property;

            StyleSheet[] sheets = StyleSheetUtils.Load(USS_PATH);
            foreach (StyleSheet sheet in sheets) this.styleSheets.Add(sheet);

            this.Toolbar = new CombosToolToolbar(this);
            this.Tree = new CombosToolTree(this);
            this.Inspector = new CombosToolInspector(this);
            
            this.Toolbar.Setup();
            this.Tree.Setup();
            this.Inspector.Setup();
            
            int lastSelection = this.LastSelection;
            if (lastSelection != ComboTree.NODE_INVALID)
            {
                this.Tree.Select(lastSelection);
            }

            this.m_SplitView = new TwoPaneSplitView(
                1, this.Inspector.Slider,
                TwoPaneSplitViewOrientation.Horizontal
            );
            
            this.Inspector.EventState += this.RefreshSplitViews;
            this.Tree.EventSelection += this.RefreshLastSelection;

            this.Add(this.Toolbar);
            this.Add(this.m_SplitView);
            this.m_SplitView.Add(this.Tree);
            this.m_SplitView.Add(this.Inspector);

            this.RegisterCallback<GeometryChangedEvent>(_ => this.RefreshSplitViews());

            this.RefreshSplitViews();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public SerializedProperty FindPropertyForId(int id)
        {
            SerializedProperty keys = this.PropertyDataKeys;
            SerializedProperty values = this.PropertyDataValues;

            int size = keys.arraySize;
            for (int i = 0; i < size; ++i)
            {
                if (keys.GetArrayElementAtIndex(i).intValue != id) continue;
                return values.GetArrayElementAtIndex(i);
            }

            return null;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RefreshSplitViews()
        {
            switch (this.Inspector.State)
            {
                case true: this.m_SplitView.UnCollapse(); break;
                case false: this.m_SplitView.CollapseChild(1); break;
            }
            
            this.m_SplitView.fixedPaneInitialDimension = this.Inspector.Slider;
        }
        
        private void RefreshLastSelection(int nodeId)
        {
            this.LastSelection = nodeId;
        }
    }
}