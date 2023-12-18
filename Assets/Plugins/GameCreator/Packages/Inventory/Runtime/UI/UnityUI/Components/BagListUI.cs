using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Inventory.UnityUI
{
    [AddComponentMenu("Game Creator/UI/Inventory/Bag List UI")]
    [Icon(RuntimePaths.PACKAGES + "Inventory/Editor/Gizmos/GizmoBagUI.png")]
    
    [Serializable]
    public class BagListUI : TBagUI
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private Item m_FilterByParent;
        [SerializeField] private RectTransform m_Content;
        
        [SerializeField] private bool m_HideEquipped;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override Item FilterByParent
        {
            get => this.m_FilterByParent;
            set
            {
                this.m_FilterByParent = value;
                this.RefreshUI();
            }
        }
        
        public override Type ExpectedBagType => typeof(BagList);
        
        protected override RectTransform Content => this.m_Content;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void RefreshUI()
        {
            if (this.Bag == null) return;
            base.RefreshUI();
            
            List<Cell> cellList = this.Bag.Content.CellList;
            List<int> cellIndices = new List<int>(cellList.Count);
            
            for (int i = cellList.Count - 1; i >= 0; --i)
            {
                RuntimeItem cellItem = cellList[i].RootRuntimeItem;
                if (this.m_HideEquipped && this.Bag.Equipment.IsEquipped(cellItem))
                {
                    continue;
                }
                    
                cellIndices.Add(i);
            }
            
            int numCells = cellIndices.Count;
            int numChildren = this.m_Content.childCount;

            int numCreate = numCells - numChildren;
            int numDelete = numChildren - numCells;

            for (int i = numCreate - 1; i >= 0; --i) this.CreateCell();
            for (int i = numDelete - 1; i >= 0; --i) this.DeleteCell(numCells + i);
            
            for (int i = 0; i < numCells; ++i)
            {
                Transform child = this.m_Content.GetChild(i);
                BagCellUI cellUI = child.Get<BagCellUI>();
                
                int index = cellIndices[i];
                if (cellUI != null) cellUI.RefreshUI(0, index);
            }
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateCell()
        {
            GameObject instance = Instantiate(this.PrefabCell, this.m_Content.transform);
            
            BagCellUI bagCellUI = instance.Get<BagCellUI>();
            if (bagCellUI != null) bagCellUI.OnCreate(this);
        }
        
        private void DeleteCell(int index)
        {
            Transform child = this.m_Content.transform.GetChild(index);
            Destroy(child.gameObject);
        }
    }
}