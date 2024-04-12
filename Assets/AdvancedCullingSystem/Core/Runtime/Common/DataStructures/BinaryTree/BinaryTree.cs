using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem
{
    public abstract class BinaryTree<TNode, TData>
        where TNode : BinaryTreeNode
    {
        public BinaryTreeNode Root 
        {
            get
            {
                return RootInternal;
            }
        }
        public float NodeSize { get; private set; }
        public int Height { get; private set; }

        protected TNode RootInternal { get; private set; }


        public BinaryTree(float nodeSize)
        {
            NodeSize = Mathf.Max(nodeSize, 0.1f);
        }

        public void Add(TData data)
        {
            if (Root == null)
            {
                RootInternal = CreateNode(GetBounds(data).center, Vector3.one * NodeSize, true);
                Height = 1;
            }

            if (!Includes(RootInternal, data))
                GrowTreeUp(data);

            AddInternal(RootInternal, data, 1);
        }


        private TNode ExpandRoot(TNode root, TData target)
        {
            Bounds rootBounds = root.Bounds;
            Bounds targetBounds = GetBounds(target);

            Vector3 parentCenter = Vector3.zero;
            Vector3 parentSize = Vector3.zero;

            Vector3 childCenter = Vector3.zero;

            bool rootIsLeft = false;

            for (int i = 0; i < 3; i++)
            {
                if (targetBounds.min[i] < rootBounds.min[i])
                {
                    parentSize = rootBounds.size;
                    parentSize[i] *= 2;

                    parentCenter = rootBounds.center;
                    parentCenter[i] -= rootBounds.size[i] / 2;

                    childCenter = rootBounds.center;
                    childCenter[i] -= rootBounds.size[i];

                    break;
                }

                if (targetBounds.max[i] > rootBounds.max[i])
                {
                    parentSize = rootBounds.size;
                    parentSize[i] *= 2;

                    parentCenter = rootBounds.center;
                    parentCenter[i] += rootBounds.size[i] / 2;

                    childCenter = rootBounds.center;
                    childCenter[i] += rootBounds.size[i];

                    rootIsLeft = true;

                    break;
                }
            }

            TNode parent = CreateNode(parentCenter, parentSize, false);
            TNode child = CreateNode(childCenter, rootBounds.size, root.IsLeaf);

            if (rootIsLeft)
                SetChildsToNode(parent, RootInternal, child);
            else
                SetChildsToNode(parent, child, RootInternal);

            return parent;
        }

        protected void GrowTreeUp(TData target)
        {
            if (Includes(RootInternal, target))
                return;

            RootInternal = ExpandRoot(RootInternal, target);
            Height++;

            GrowTreeUp(target);
        }

        protected void GrowTreeDown(TNode node, TData target, int depth)
        {
            if (node.HasChilds)
                throw new Exception("GrowTreeDown::" + depth + " node already has childs");

            Bounds nodeBounds = node.Bounds;
            Vector3 offset;
            Vector3 size;

            if (nodeBounds.size.x > nodeBounds.size.y || nodeBounds.size.x > nodeBounds.size.z)
            {
                offset = new Vector3(nodeBounds.size.x / 4, 0, 0);
                size = new Vector3(nodeBounds.size.x / 2, nodeBounds.size.y, nodeBounds.size.z);
            }
            else if (nodeBounds.size.y > nodeBounds.size.x || nodeBounds.size.y > nodeBounds.size.z)
            {
                offset = new Vector3(0, nodeBounds.size.y / 4, 0);
                size = new Vector3(nodeBounds.size.x, nodeBounds.size.y / 2, nodeBounds.size.z);
            }
            else
            {
                offset = new Vector3(0, 0, nodeBounds.size.z / 4);
                size = new Vector3(nodeBounds.size.x, nodeBounds.size.y, nodeBounds.size.z / 2);
            }

            bool isLeaf = (depth == Height);

            TNode left = CreateNode(nodeBounds.center - offset, size, isLeaf);
            TNode right = CreateNode(nodeBounds.center + offset, size, isLeaf);

            SetChildsToNode(node, left, right);

            if (isLeaf)
                return;

            if (Intersects(left, target))
                GrowTreeDown(left, target, depth + 1);

            if (Intersects(right, target))
                GrowTreeDown(right, target, depth + 1);
        }

        protected bool Intersects(TNode node, TData data)
        {
            return node.Bounds.Intersects(GetBounds(data));
        }

        protected bool Includes(TNode node, TData data)
        {
            return node.Bounds.Contains(GetBounds(data));
        }


        protected virtual void AddInternal(TNode node, TData data, int depth)
        {
            if (node.IsLeaf)
            {
                AddDataToNode(node, data);
                return;
            }

            if (!node.HasChilds)
                GrowTreeDown(node, data, depth + 1);

            TNode left = (TNode)node.GetLeft();
            TNode right = (TNode)node.GetRight();

            if (Intersects(left, data))
                AddInternal(left, data, depth + 1);

            if (Intersects(right, data))
                AddInternal(right, data, depth + 1);
        }

        protected abstract Bounds GetBounds(TData data);

        protected abstract TNode CreateNode(Vector3 center, Vector3 size, bool isLeaf);

        protected abstract void SetChildsToNode(TNode parent, TNode leftChild, TNode rightChild);

        protected abstract void AddDataToNode(TNode node, TData data);
    }
}
