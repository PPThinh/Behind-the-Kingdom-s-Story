using UnityEngine;

namespace NGS.AdvancedCullingSystem
{
    public abstract class BinaryTreeNode
    {
        public Vector3 Center { get; private set; }
        public Vector3 Size { get; private set; }
        public Bounds Bounds { get; private set; }
        public bool IsLeaf { get; private set; }
        public bool HasChilds
        {
            get
            {
                return GetLeft() != null;
            }
        }

        public BinaryTreeNode(Vector3 center, Vector3 size, bool isLeaf)
        {
            Center = center;
            Size = size;
            Bounds = new Bounds(center, size);
            IsLeaf = isLeaf;
        }

        public abstract BinaryTreeNode GetLeft();

        public abstract BinaryTreeNode GetRight();
    }
}
