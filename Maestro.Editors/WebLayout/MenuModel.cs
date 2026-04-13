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

using OSGeo.MapGuide.ObjectModels.WebLayout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Maestro.Editors.WebLayout
{
    internal abstract class ItemBase
    {
        public abstract string Label { get; }

        public abstract Image Icon { get; }

        public abstract IUIItem Item { get; }
    }

    internal abstract class ItemBase<T> : ItemBase where T : IUIItem
    {
        protected ItemBase(T item) { this.Tag = item; }

        public T Tag { get; }

        public override IUIItem Item => this.Tag;
    }

    internal class CommandItem : ItemBase<ICommandItem>
    {
        public CommandItem(ICommandItem item, Image icon) : base(item)
        {
            _icon = icon ?? Properties.Resources.question;
        }

        public override string Label => this.Tag.Command;

        private readonly Image _icon;

        public override Image Icon => _icon;
    }

    internal class SeparatorItem : ItemBase<ISeparatorItem>
    {
        public SeparatorItem(ISeparatorItem sep) : base(sep) { }

        public override string Label => this.Tag.Function.ToString();

        public override Image Icon => Properties.Resources.ui_splitter_horizontal;
    }

    internal class FlyoutItem : ItemBase<IFlyoutItem>
    {
        public FlyoutItem(IFlyoutItem fly) : base(fly) { }

        public override string Label => this.Tag.Label;

        public override Image Icon => Properties.Resources.ui_menu;

        public IEnumerable<IUIItem> SubItem => this.Tag.Items;
    }

    /// <summary>
    /// Replaces the former Aga.Controls.Tree ITreeModel-based MenuTreeModel.
    /// Directly populates a standard WinForms TreeView.
    /// </summary>
    internal class MenuTreeModel
    {
        private readonly IMenu _menu;
        private readonly IWebLayout _wl;
        private TreeView _boundTree;

        public MenuTreeModel(IMenu menu, IWebLayout wl)
        {
            _menu = menu;
            _wl = wl;
        }

        private TreeNode MakeNode(ItemBase item) => new TreeNode(item.Label) { Tag = item };

        private void PopulateFlyoutNode(TreeNode parentNode, IEnumerable<IUIItem> items)
        {
            foreach (var item in items)
            {
                if (item.Function == UIItemFunctionType.Command)
                {
                    var ci = (ICommandItem)item;
                    var cmd = _wl.GetCommandByName(ci.Command);
                    Debug.Assert(cmd != null);
                    parentNode.Nodes.Add(MakeNode(new CommandItem(ci, CommandIconCache.GetStandardCommandIcon(cmd.ImageURL))));
                }
                else if (item.Function == UIItemFunctionType.Flyout)
                {
                    var flyItem = new FlyoutItem((IFlyoutItem)item);
                    var flyNode = MakeNode(flyItem);
                    PopulateFlyoutNode(flyNode, flyItem.SubItem);
                    parentNode.Nodes.Add(flyNode);
                }
                else
                {
                    parentNode.Nodes.Add(MakeNode(new SeparatorItem((ISeparatorItem)item)));
                }
            }
        }

        internal void PopulateTree(TreeView tree)
        {
            _boundTree = tree;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            foreach (var item in _menu.Items)
            {
                if (item.Function == UIItemFunctionType.Command)
                {
                    var ci = (ICommandItem)item;
                    var cmd = _wl.GetCommandByName(ci.Command);
                    Debug.Assert(cmd != null);
                    tree.Nodes.Add(MakeNode(new CommandItem(ci, CommandIconCache.GetStandardCommandIcon(cmd.ImageURL))));
                }
                else if (item.Function == UIItemFunctionType.Flyout)
                {
                    var flyItem = new FlyoutItem((IFlyoutItem)item);
                    var flyNode = MakeNode(flyItem);
                    PopulateFlyoutNode(flyNode, flyItem.SubItem);
                    tree.Nodes.Add(flyNode);
                }
                else
                {
                    tree.Nodes.Add(MakeNode(new SeparatorItem((ISeparatorItem)item)));
                }
            }
            tree.ExpandAll();
            tree.EndUpdate();
        }

        internal void Refresh()
        {
            if (_boundTree != null)
                PopulateTree(_boundTree);
        }
    }
}