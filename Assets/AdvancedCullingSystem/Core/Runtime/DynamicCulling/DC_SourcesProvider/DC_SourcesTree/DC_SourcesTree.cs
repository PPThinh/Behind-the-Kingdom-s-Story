using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_SourcesTree : BinaryTree<DC_SourcesTreeNode, ICullingTarget>, IDC_SourcesProvider
    {
        private DC_Source _lastModifiedSource;


        public DC_SourcesTree(float nodeSize) 
            : base(nodeSize)
        {

        }

        public DC_Source GetSource(ICullingTarget cullingTarget)
        {
            _lastModifiedSource = null;

            Add(cullingTarget);

            return _lastModifiedSource;
        }


        protected override void AddInternal(DC_SourcesTreeNode node, ICullingTarget data, int depth)
        {
            if (node.IsLeaf)
            {
                AddDataToNode(node, data);
                return;
            }

            if (!node.HasChilds)
                GrowTreeDown(node, data, depth + 1);

            if (Intersects(node.Left, data))
                AddInternal(node.Left, data, depth + 1);
            else
                AddInternal(node.Right, data, depth + 1);

        }


        protected override Bounds GetBounds(ICullingTarget data)
        {
            return data.Bounds;
        }

        protected override DC_SourcesTreeNode CreateNode(Vector3 center, Vector3 size, bool isLeaf)
        {
            return new DC_SourcesTreeNode(center, size, isLeaf);
        }

        protected override void SetChildsToNode(DC_SourcesTreeNode parent, DC_SourcesTreeNode leftChild, DC_SourcesTreeNode rightChild)
        {
            parent.SetChilds(leftChild, rightChild);
        }

        protected override void AddDataToNode(DC_SourcesTreeNode node, ICullingTarget data)
        {
            node.AddCullingTarget(data);
            _lastModifiedSource = node.Source;
        }
    }
}
