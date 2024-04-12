using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_SourcesTreeNode : BinaryTreeNode
    {
        public DC_SourcesTreeNode Left { get; private set; }
        public DC_SourcesTreeNode Right { get; private set; }
        public DC_MultiSource Source { get; private set; }

        public DC_SourcesTreeNode(Vector3 center, Vector3 size, bool isLeaf)
            : base(center, size, isLeaf)
        {

        }

        public override BinaryTreeNode GetLeft()
        {
            return Left;
        }

        public override BinaryTreeNode GetRight()
        {
            return Right;
        }


        public void SetChilds(DC_SourcesTreeNode left, DC_SourcesTreeNode right)
        {
            Left = left;
            Right = right;
        }

        public void AddCullingTarget(ICullingTarget cullingTarget)
        {
            if (Source == null)
                Source = new GameObject("DC_MultiSource").AddComponent<DC_MultiSource>();

            Source.SetCullingTarget(cullingTarget);
        }
    }
}
