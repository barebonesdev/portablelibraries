using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class AVLTree<E> where E : IComparable<E>
    {
        private Node _root;
        public int Count { get; private set; }
        public int Capacity { get; private set; }
        public bool AllowDuplicates { get; private set; }

        /// <summary>
        /// NOT FINISHED
        /// </summary>
        /// <param name="allowDuplicates"></param>
        public AVLTree(bool allowDuplicates = false) : this(int.MaxValue, allowDuplicates)
        {

        }

        /// <summary>
        /// NOT FINISHED. Creates an AVLTree that will remove least used items once exceeding capactiy.
        /// </summary>
        /// <param name="capacity"></param>
        public AVLTree(int capacity, bool allowDuplicates = false)
        {
            Capacity = capacity;
        }

        public void Add(E data)
        {
            if (_root == null)
            {
                _root = new Node(data);
                Count++;
                return;
            }
        }

        private class Node
        {
            public E Data { get; set; }

            public AVLTree<E> Left { get; set; }
            public AVLTree<E> Right { get; set; }

            public int ChildrenLeft { get; set; }
            public int ChildrenRight { get; set; }

            public Node(E data)
            {
                Data = data;
            }
        }
    }
}
