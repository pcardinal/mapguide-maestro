#region Disclaimer / License

// Copyright (C) 2010, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using Maestro.Base.Services;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Commands;
using OSGeo.MapGuide.ObjectModels;
using OSGeo.MapGuide.ObjectModels.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Maestro.Base.UI
{
    public interface ISiteExplorerNode
    { 
        public string ConnectionName { get; }

        public string Name { get; }

        public Image Icon { get; }
    }


    /// <summary>
    /// Models an object in the repository
    /// </summary>
    public class RepositoryItem : ISiteExplorerNode
    {
        private Dictionary<string, RepositoryItem> _children;

        private bool _notify = false;

        internal RepositoryItem(string connectionName, IRepositoryItem item)
        {
            _name = string.Empty;
            _children = new Dictionary<string, RepositoryItem>();

            this.ConnectionName = connectionName;
            this.CreatedDate = item.CreatedDate;
            this.ModifiedDate = item.ModifiedDate;
            this.Owner = item.Owner;
            this.ResourceId = item.ResourceId;
            this.ResourceType = item.ResourceType;
            this.Name = item.Name; //set name last because update logic requires other properties be set already

            if (this.IsRoot)
            {
                this.Icon = Properties.Resources.server;
            }
            else
            {
                //TODO: Should probably centralize this in ResourceIconCache
                switch (item.ResourceType)
                {
                    case "DrawingSource":
                        this.Icon = Properties.Resources.blueprints;
                        break;

                    case "FeatureSource":
                        this.Icon = Properties.Resources.database_share;
                        break;

                    case "Folder":
                        this.Icon = Properties.Resources.folder_horizontal;
                        break;

                    case "LayerDefinition":
                        this.Icon = Properties.Resources.layer;
                        break;

                    case "MapDefinition":
                        this.Icon = Properties.Resources.map;
                        break;

                    case "WebLayout":
                        this.Icon = Properties.Resources.application_browser;
                        break;

                    case "ApplicationDefinition":
                        this.Icon = Properties.Resources.applications_stack;
                        break;

                    case "SymbolLibrary":
                        this.Icon = Properties.Resources.images_stack;
                        break;

                    case "PrintLayout":
                        this.Icon = Properties.Resources.printer;
                        break;

                    case "SymbolDefinition":
                        this.Icon = Properties.Resources.marker;
                        break;

                    case "WatermarkDefinition":
                        this.Icon = Properties.Resources.water;
                        break;

                    case "TileSetDefinition":
                        this.Icon = Properties.Resources.grid;
                        break;

                    default:
                        this.Icon = Properties.Resources.document;
                        break;
                }
            }

            _notify = true;
        }

        /// <summary>
        /// Gets whether the specified child item (name) exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name) => _children.ContainsKey(name);

        /// <summary>
        /// Gets the child RepositoryItem by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RepositoryItem this[string name] => _children[name];

        internal RepositoryTreeModel Model
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent of this item
        /// </summary>
        public RepositoryItem Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the children of this item
        /// </summary>
        public IEnumerable<RepositoryItem> Children => _children.Values;

        /// <summary>
        /// Gets the qualified name of this item
        /// </summary>
        public string NameQualified
        {
            get
            {
                if (this.ResourceType == ResourceTypes.Folder.ToString())
                    return this.Name;
                else
                    return $"{this.Name}.{this.ResourceType}"; //NOXLATE
            }
        }

        internal void AddChildWithoutNotification(RepositoryItem item)
        {
            item.Parent = this;
            if (!_children.ContainsKey(item.NameQualified))
                _children.Add(item.NameQualified, item);
        }

        internal void RemoveChildWithoutNotification(RepositoryItem item)
        {
            if (_children.ContainsKey(item.NameQualified) && item.Parent == this)
            {
                if (_children.Remove(item.NameQualified))
                {
                    item.Parent = null;
                }
            }
        }

        /// <summary>
        /// Adds the specified item as a child item
        /// </summary>
        /// <param name="item"></param>
        public void AddChild(RepositoryItem item)
        {
            item.Parent = this;
            _children.Add(item.NameQualified, item);
            NotifyStructureChanged(this);
        }

        /// <summary>
        /// Removes the child item
        /// </summary>
        /// <param name="item"></param>
        public void RemoveChild(RepositoryItem item)
        {
            if (_children.ContainsKey(item.NameQualified) && item.Parent == this)
            {
                if (_children.Remove(item.NameQualified))
                {
                    item.Parent = null;
                    NotifyStructureChanged(this);
                }
            }
        }

        private void NotifyStructureChanged(RepositoryItem repositoryItem)
        {
            if (!_notify) return;
            FindModel()?.InvalidateSubTree(repositoryItem);
        }

        /// <summary>
        /// Gets whether this item is a root node
        /// </summary>
        public bool IsRoot
        {
            get { return this.ResourceId == StringConstants.RootIdentifier; }
        }

        /// <summary>
        /// Gets the resource id
        /// </summary>
        public string ResourceId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the name of the associated connection
        /// </summary>
        public string ConnectionName
        {
            get;
            internal set;
        }

        private string _name;

        /// <summary>
        /// Gets the name of this resource
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                var oldq = this.NameQualified;
                _name = value;
                if (!oldq.Equals(this.NameQualified))
                {
                    if (IsRoot)
                        return;

                    string parentid = ResourceIdentifier.GetParentFolder(this.ResourceId);
                    this.ResourceId = parentid + this.NameQualified + ((IsFolder) ? "/" : string.Empty); //NOXLATE
                    NotifyNodesChanged();
                }
            }
        }

        /// <summary>
        /// Finds the first attached model
        /// </summary>
        /// <returns></returns>
        private RepositoryTreeModel FindModel()
        {
            RepositoryItem item = this;
            while (item != null)
            {
                if (item.Model != null)
                    return item.Model;
                item = item.Parent;
            }
            return null;
        }

        private void NotifyNodesChanged()
        {
            if (!_notify) return;
            FindModel()?.InvalidateItem(this);
        }

        /// <summary>
        /// Gets the resource type
        /// </summary>
        public string ResourceType
        {
            get;
            internal set;
        }

        private string _owner;

        /// <summary>
        /// Gets or sets the resource owner
        /// </summary>
        public string Owner
        {
            get { return _owner; }
            internal set
            {
                _owner = value;
                NotifyNodesChanged();
            }
        }

        /// <summary>
        /// Gets the date this resource was created
        /// </summary>
        public DateTime CreatedDate
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the modified date of this resource
        /// </summary>
        public DateTime ModifiedDate
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets whether this item is a folder
        /// </summary>
        public bool IsFolder
        {
            get { return this.ResourceId.EndsWith("/"); } //NOXLATE
        }

        /// <summary>
        /// Gets the icon for this item
        /// </summary>
        public Image Icon
        {
            get;
            internal set;
        }

        internal void ClearChildrenWithoutNotification()
        {
            _children.Clear();
        }

        private bool _open = false;
        private bool _dirty = false;
        //private bool _clipboarded = false;

        internal bool IsOpen
        {
            get { return _open; }
            set
            {
                _open = value;
                _dirty = !value;
                this.NotifyNodesChanged();
            }
        }

        internal bool IsDirty
        {
            get { return _dirty; }
            set
            {
                _dirty = value;
                _open = !value;
                this.NotifyNodesChanged();
            }
        }

        /// <summary>
        /// Defines valid clipboard actions for Site Explorer
        /// </summary>
        public enum ClipboardAction
        {
            /// <summary>
            /// The node was copied
            /// </summary>
            Copy,

            /// <summary>
            /// The node was cut
            /// </summary>
            Cut,

            /// <summary>
            /// No clipboard action applied
            /// </summary>
            None
        }

        private ClipboardAction _action = ClipboardAction.None;

        /// <summary>
        /// Gets the clipboard state of this item
        /// </summary>
        public ClipboardAction ClipboardState
        {
            get { return _action; }
            set
            {
                _action = value;
                this.NotifyNodesChanged();
            }
        }

        internal void Reset()
        {
            _action = ClipboardAction.None;
            _dirty = false;
            _open = false;
            this.NotifyNodesChanged();
        }
    }

    /// <summary>
    /// Provides tooltips for resources in the Site Explorer
    /// </summary>
    public class RepositoryItemToolTipProvider
    {
        /// <summary>
        /// Gets the tooltip for the given node tag
        /// </summary>
        public string GetToolTip(object tag)
        {
            if (tag is RepositoryItem item && !item.IsRoot)
            {
                return string.Format(Strings.SITE_EXPLORER_TOOLTIP_TEMPLATE, Environment.NewLine, item.Name, item.ResourceType, item.CreatedDate, item.ModifiedDate, item.Owner);
            }
            else if (tag is WfsLayerRepositoryItem wfsl)
            {
                return String.Format(Strings.WfsLayerTooltip, Environment.NewLine, wfsl.LayerName, wfsl.Name, wfsl.Abstract, wfsl.Crs);
            }
            else if (tag is WmsLayerRepositoryItem wmsl)
            {
                return String.Format(Strings.WmsLayerTooltip, Environment.NewLine, wmsl.LayerName, wmsl.Name, wmsl.Abstract, wmsl.Crs, wmsl.BBOX);
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// Defines the repository model for the treeview
    /// </summary>
    public class RepositoryTreeModel
    {
        private System.Windows.Forms.TreeView _tree;

        private ServerConnectionManager _connManager;
        private OpenResourceManager _openResMgr;
        private ClipboardService _clip;

        // Lazy-load placeholder
        internal const string LoadingTag = "__loading__";

        internal RepositoryTreeModel(ServerConnectionManager connManager, System.Windows.Forms.TreeView tree, OpenResourceManager openResMgr, ClipboardService clip)
        {
            _connManager = connManager;
            _tree = tree;
            _openResMgr = openResMgr;
            _clip = clip;
        }

        private System.Collections.IEnumerable GetSorted(string connectionName, ResourceList list)
        {
            //Sort them before returning them
            SortedList<string, RepositoryItem> folders = new SortedList<string, RepositoryItem>();
            SortedList<string, RepositoryItem> docs = new SortedList<string, RepositoryItem>();
            foreach (var item in list.Children)
            {
                var it = new RepositoryItem(connectionName, item);
                it.Model = this;
                if (it.IsFolder)
                    folders.Add(it.ResourceId, it);
                else
                    docs.Add(it.ResourceId, it);
            }
            foreach (var folder in folders.Values)
            {
                yield return folder;
            }
            foreach (var doc in docs.Values)
            {
                yield return doc;
            }
        }

        /// <summary>
        /// Restores node ui state from before refresh
        /// </summary>
        /// <param name="item"></param>
        private void ApplyCurrentItemState(RepositoryItem item)
        {
            var conn = _connManager.GetConnection(item.ConnectionName);
            if (_openResMgr.IsOpen(item.ResourceId, conn))
            {
                item.IsOpen = true;
                var ed = _openResMgr.GetOpenEditor(item.ResourceId, conn);
                if (ed.IsDirty)
                    item.IsDirty = true;
            }
            item.ClipboardState = _clip.GetClipboardState(item.ResourceId);
        }

        private readonly Dictionary<string, RepositoryItem> _rootNodes = new Dictionary<string, RepositoryItem>();

        /// <summary>
        /// Gets the child nodes in the given tree path
        private bool IsLeafItem(object item)
        {
            if (item is RepositoryItem ri) return !ri.IsFolder;
            if (item is WfsLayerRepositoryItem) return true;
            if (item is WmsLayerRepositoryItem) return true;
            return false;
        }

        private System.Windows.Forms.TreeNode MakeNode(object item)
        {
            string text = item is RepositoryItem ri ? ri.Name
                        : item is WfsRootRepositoryItem wfsr ? wfsr.Name
                        : item is WmsRootRepositoryItem wmsr ? wmsr.Name
                        : item is WfsLayerRepositoryItem wfsl ? wfsl.LayerName
                        : item is WmsLayerRepositoryItem wmsl ? wmsl.LayerName
                        : item.ToString();
            var node = new System.Windows.Forms.TreeNode(text) { Tag = item };
            if (!IsLeafItem(item))
                node.Nodes.Add(new System.Windows.Forms.TreeNode(LoadingTag)); // lazy placeholder
            return node;
        }

        /// <summary>
        /// Populates the tree with root connection nodes.
        /// </summary>
        internal void PopulateTree(System.Windows.Forms.TreeView tree)
        {
            _tree = tree;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            _rootNodes.Clear();
            foreach (var connName in _connManager.GetConnectionNames())
            {
                var conn = _connManager.GetConnection(connName);
                var list = conn.ResourceService.GetRepositoryResources(StringConstants.RootIdentifier, 0); //NOXLATE
                if (list.Items.Count != 1) throw new InvalidOperationException();
                var connNode = new RepositoryItem(connName, (IRepositoryItem)list.Items[0]);
                connNode.Name = connName;
                connNode.Model = this;
                _rootNodes[connName] = connNode;
                tree.Nodes.Add(MakeNode(connNode));
            }
            tree.EndUpdate();
        }

        /// <summary>
        /// Called from BeforeExpand to lazily load children of a node.
        /// </summary>
        internal void LoadChildren(System.Windows.Forms.TreeNode treeNode)
        {
            // Only load if placeholder is present
            if (treeNode.Nodes.Count != 1 || treeNode.Nodes[0].Text != LoadingTag)
                return;
            treeNode.Nodes.Clear();
            var item = treeNode.Tag;
            if (item is RepositoryItem node && node.IsFolder)
            {
                string connName = GetParentConnectionName(node);
                var conn = _connManager.GetConnection(connName);
                node.ClearChildrenWithoutNotification();
                var list = conn.ResourceService.GetRepositoryResources(node.ResourceId, string.Empty, 1, false); //NOXLATE
                foreach (RepositoryItem child in GetSorted(connName, list))
                {
                    node.AddChildWithoutNotification(child);
                    ApplyCurrentItemState(child);
                    treeNode.Nodes.Add(MakeNode(child));
                }
                if (node.ResourceId == StringConstants.RootIdentifier)
                {
                    if (conn.Capabilities.SupportedCommands.Contains((int)CommandType.GetWfsCapabilities))
                        treeNode.Nodes.Add(MakeNode(new WfsRootRepositoryItem(connName)));
                    if (conn.Capabilities.SupportedCommands.Contains((int)CommandType.GetWmsCapabilities))
                        treeNode.Nodes.Add(MakeNode(new WmsRootRepositoryItem(connName)));
                }
            }
            else if (item is WfsRootRepositoryItem wfsr)
            {
                if (!wfsr.IsLoaded)
                {
                    var conn = _connManager.GetConnection(wfsr.ConnectionName);
                    var cmd = conn.CreateCommand((int)CommandType.GetWfsCapabilities) as IGetWfsCapabilities;
                    wfsr.Load(cmd);
                }
                foreach (var layer in wfsr.Layers)
                    treeNode.Nodes.Add(MakeNode(layer));
            }
            else if (item is WmsRootRepositoryItem wmsr)
            {
                if (!wmsr.IsLoaded)
                {
                    var conn = _connManager.GetConnection(wmsr.ConnectionName);
                    var cmd = conn.CreateCommand((int)CommandType.GetWmsCapabilities) as IGetWmsCapabilities;
                    wmsr.Load(cmd);
                }
                foreach (var layer in wmsr.Layers)
                    treeNode.Nodes.Add(MakeNode(layer));
            }
        }

        internal static string GetParentConnectionName(RepositoryItem item)
        {
            if (!string.IsNullOrEmpty(item.ConnectionName))
                return item.ConnectionName;
            var current = item.Parent;
            if (current != null)
            {
                while (current.Parent != null)
                    current = current.Parent;
                Debug.Assert(!string.IsNullOrEmpty(current.ConnectionName));
                return current.ConnectionName;
            }
            Debug.Assert(!string.IsNullOrEmpty(item.ConnectionName));
            return item.ConnectionName;
        }

        /// <summary>
        /// Refreshes the currently selected node's subtree.
        /// </summary>
        public void Refresh()
        {
            if (_tree == null) return;
            var sel = _tree.SelectedNode;
            if (sel != null)
                InvalidateSubTree(sel.Tag as RepositoryItem);
            else
                PopulateTree(_tree);
        }

        internal void FullRefresh()
        {
            if (_tree != null)
                PopulateTree(_tree);
        }

        /// <summary>
        /// Updates the text of the tree node for the given item.
        /// </summary>
        internal void InvalidateItem(RepositoryItem item)
        {
            if (_tree == null || item == null) return;
            var node = FindTreeNodeByItem(_tree.Nodes, item);
            if (node != null)
            {
                node.Text = item.Name;
                _tree.Invalidate();
            }
        }

        /// <summary>
        /// Reloads children of the tree node for the given item.
        /// </summary>
        internal void InvalidateSubTree(RepositoryItem item)
        {
            if (_tree == null) return;
            if (item == null) { PopulateTree(_tree); return; }
            var node = FindTreeNodeByItem(_tree.Nodes, item);
            if (node != null)
            {
                node.Nodes.Clear();
                if (!IsLeafItem(item))
                    node.Nodes.Add(new System.Windows.Forms.TreeNode(LoadingTag));
            }
        }

        private static System.Windows.Forms.TreeNode FindTreeNodeByItem(System.Windows.Forms.TreeNodeCollection nodes, object item)
        {
            foreach (System.Windows.Forms.TreeNode node in nodes)
            {
                if (node.Tag == item) return node;
                var found = FindTreeNodeByItem(node.Nodes, item);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Finds a TreeNode by connection name and resource ID.
        /// </summary>
        internal System.Windows.Forms.TreeNode FindTreeNode(string connectionName, string resourceId)
        {
            if (_tree == null) return null;
            if (!_rootNodes.ContainsKey(connectionName)) return null;
            var rootItem = _rootNodes[connectionName];
            var rootNode = FindTreeNodeByItem(_tree.Nodes, rootItem);
            if (rootNode == null) return null;
            if (StringConstants.RootIdentifier.Equals(resourceId)) return rootNode;
            return FindTreeNodeByResourceId(rootNode.Nodes, resourceId);
        }

        private static System.Windows.Forms.TreeNode FindTreeNodeByResourceId(System.Windows.Forms.TreeNodeCollection nodes, string resourceId)
        {
            foreach (System.Windows.Forms.TreeNode node in nodes)
            {
                if (node.Tag is RepositoryItem ri && ri.ResourceId == resourceId)
                    return node;
                var found = FindTreeNodeByResourceId(node.Nodes, resourceId);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Gets whether the specified tree path is a path to a leaf node (kept for compatibility)
        /// </summary>
        [System.Obsolete("Use IsLeafItem instead")]
        public bool IsLeaf(object item) => IsLeafItem(item);

        internal void RaiseNodesChanged(RepositoryItem item) => InvalidateItem(item);

        internal void RaiseStructureChanged(RepositoryItem item) => InvalidateSubTree(item);
    }
}
