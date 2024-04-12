using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem
{
    public class BinaryTreeDrawer
    {
        public Color Color { get; set; }

        public void DrawTreeGizmos(BinaryTreeNode root)
        {
            if (root == null)
                return;

            Bounds bounds = root.Bounds;

            Gizmos.color = Color;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            DrawTreeGizmos(root.GetLeft());
            DrawTreeGizmos(root.GetRight());
        }
    }
}
