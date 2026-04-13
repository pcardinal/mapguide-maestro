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

using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.ObjectModels.MapDefinition;
using OSGeo.MapGuide.ObjectModels.TileSetDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Maestro.Editors.MapDefinition
{
    internal abstract class TreeItem<T>
    {
        protected TreeItem(string text, Image icon, T item)
        {
            this.Text = text;
            this.Icon = icon;
            this.Tag = item;
        }
        public Image Icon { get; set; }
        public string Text { get; set; }
        public T Tag { get; set; }
    }

    internal class ScaleItem : TreeItem<IList<double>>
    {
        public ScaleItem(string name, IList<double> range)
            : base(name, Properties.Resources.magnifier, range) { }
    }

    internal class LayerItem : TreeItem<IMapLayer>
    {
        public LayerItem(IMapLayer layer)
            : base(layer.Name, Properties.Resources.layer, layer)
        {
            layer.PropertyChanged += WeakEventHandler.Wrap<PropertyChangedEventHandler>(OnPropertyChanged, (eh) => layer.PropertyChanged -= eh);
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Tag.Name))
                this.Text = this.Tag.Name;
        }
    }

    internal class GroupItem : TreeItem<IMapLayerGroup>
    {
        public GroupItem(IMapLayerGroup grp)
            : base(grp.Name, Properties.Resources.folder_horizontal, grp)
        {
            grp.PropertyChanged += WeakEventHandler.Wrap<PropertyChangedEventHandler>(OnPropertyChanged, (eh) => grp.PropertyChanged -= eh);
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Tag.Name))
                this.Text = this.Tag.Name;
        }
    }

    internal class BaseLayerItem : TreeItem<IBaseMapLayer>
    {
        public BaseLayerItem(IBaseMapLayer layer, IBaseMapGroup parent)
            : base(layer.Name, Properties.Resources.layer, layer)
        {
            layer.PropertyChanged += WeakEventHandler.Wrap<PropertyChangedEventHandler>(OnPropertyChanged, (eh) => layer.PropertyChanged -= eh);
            this.Parent = parent;
        }
        public IBaseMapGroup Parent { get; set; }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Tag.Name))
                this.Text = this.Tag.Name;
        }
    }

    internal class BaseLayerGroupItem : TreeItem<IBaseMapGroup>
    {
        public BaseLayerGroupItem(IBaseMapGroup group)
            : base(group.Name, Properties.Resources.folder_horizontal, group)
        {
            group.PropertyChanged += WeakEventHandler.Wrap<PropertyChangedEventHandler>(OnPropertyChanged, (eh) => group.PropertyChanged -= eh);
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Tag.Name))
                this.Text = this.Tag.Name;
        }
    }

    /// <summary>
    /// Base class for tree models that directly populate a WinForms TreeView.
    /// Replaces the former Aga.Controls.Tree ITreeModel pattern.
    /// </summary>
    internal abstract class TreeModelBase
    {
        protected TreeView _boundTree;

        internal virtual void Invalidate()
        {
            if (_boundTree != null)
                PopulateTree(_boundTree);
        }

        // Overload kept for call-site compatibility (path argument ignored for standard TreeView)
        internal void Invalidate(object ignoredPath) => Invalidate();

        internal abstract void PopulateTree(TreeView tree);
    }

    internal class DrawOrderLayerModel : TreeModelBase
    {
        private IMapDefinition _map;

        public DrawOrderLayerModel(IMapDefinition map) { _map = map; }

        internal override void PopulateTree(TreeView tree)
        {
            _boundTree = tree;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            foreach (var layer in _map.MapLayer)
            {
                var item = new LayerItem(layer);
                tree.Nodes.Add(new TreeNode(item.Text) { Tag = item });
            }
            tree.EndUpdate();
        }
    }

    internal class GroupedLayerModel : TreeModelBase
    {
        private IMapDefinition _map;

        public GroupedLayerModel(IMapDefinition map) { _map = map; }

        internal override void PopulateTree(TreeView tree)
        {
            _boundTree = tree;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            foreach (var layer in _map.GetLayersWithoutGroups())
            {
                var item = new LayerItem(layer);
                tree.Nodes.Add(new TreeNode(item.Text) { Tag = item });
            }
            foreach (var group in _map.MapLayerGroup.Where(g => string.IsNullOrEmpty(g.Group)))
            {
                var grpItem = new GroupItem(group);
                var grpNode = new TreeNode(grpItem.Text) { Tag = grpItem };
                PopulateGroupNode(grpNode, group.Name);
                tree.Nodes.Add(grpNode);
            }
            tree.ExpandAll();
            tree.EndUpdate();
        }

        private void PopulateGroupNode(TreeNode parentNode, string groupName)
        {
            foreach (var layer in _map.GetLayersForGroup(groupName))
            {
                var item = new LayerItem(layer);
                parentNode.Nodes.Add(new TreeNode(item.Text) { Tag = item });
            }
            foreach (var subGroup in _map.MapLayerGroup.Where(g => g.Group == groupName))
            {
                var grpItem = new GroupItem(subGroup);
                var grpNode = new TreeNode(grpItem.Text) { Tag = grpItem };
                PopulateGroupNode(grpNode, subGroup.Name);
                parentNode.Nodes.Add(grpNode);
            }
        }
    }

    internal class TiledLayerModel : TreeModelBase
    {
        private ITileSetAbstract _tileSet;

        public TiledLayerModel(ITileSetAbstract tileSet) { _tileSet = tileSet; }

        internal void Invalidate(ITileSetAbstract tileSet)
        {
            _tileSet = tileSet;
            base.Invalidate();
        }

        internal override void PopulateTree(TreeView tree)
        {
            _boundTree = tree;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            if (_tileSet != null)
            {
                if (_tileSet.SupportsCustomFiniteDisplayScalesUnconditionally)
                {
                    var si = new ScaleItem(Strings.FiniteDisplayScales, new List<double>(_tileSet.FiniteDisplayScale));
                    tree.Nodes.Add(new TreeNode(si.Text) { Tag = si });
                }
                foreach (var grp in _tileSet.BaseMapLayerGroups)
                {
                    var grpItem = new BaseLayerGroupItem(grp);
                    var grpNode = new TreeNode(grpItem.Text) { Tag = grpItem };
                    foreach (var layer in _tileSet.GetLayersForGroup(grp.Name))
                    {
                        var layerItem = new BaseLayerItem(layer, grp);
                        grpNode.Nodes.Add(new TreeNode(layerItem.Text) { Tag = layerItem });
                    }
                    tree.Nodes.Add(grpNode);
                }
            }
            tree.ExpandAll();
            tree.EndUpdate();
        }
    }
}