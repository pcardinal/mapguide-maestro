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
using OSGeo.MapGuide.ObjectModels.WebLayout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Maestro.Editors.WebLayout
{
    [ToolboxItem(false)]
    internal partial class MenuEditorCtrl : EditorBase
    {
        public MenuEditorCtrl()
        {
            InitializeComponent();
        }

        private MenuTreeModel _model;

        // Replaces the former ITreeModel Model property (TreeViewAdv-specific).
        // Setting this now calls PopulateTree on the embedded standard TreeView.
        public MenuTreeModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                if (_model != null)
                    _model.PopulateTree(trvMenuItems);
            }
        }

        public override void Bind(IEditorService service) => service.RegisterCustomNotifier(this);

        protected override void UnsubscribeEventHandlers()
        {
            _wl.CommandSet.CustomCommandAdded -= OnCustomCommandAdded;
            _wl.CommandSet.CustomCommandRemoved -= OnCustomCommandRemoved;
            base.UnsubscribeEventHandlers();
        }

        private IEditorService _edsvc;
        private IWebLayout _wl;
        private IMenu _rootMenu;

        public void Bind(IEditorService service, IWebLayout wl, IMenu menu)
        {
            _edsvc = service;
            Bind(_edsvc);
            _wl = wl;
            _wl.CommandSet.CustomCommandAdded += OnCustomCommandAdded;
            _wl.CommandSet.CustomCommandRemoved += OnCustomCommandRemoved;
            _rootMenu = menu;
            this.Model = _model = new MenuTreeModel(menu, wl);
            InitBuiltinCommandMenu();
            InitCustomCommandMenu();
        }

        public void AddCommand(ICommand cmd)
        {
            var item = _wl.CreateCommandItem(cmd.Name);
            _rootMenu.AddItem(item);
            RefreshModel();
        }

        private void OnCustomCommandRemoved(object sender, CommandEventArgs args)
        {
            RemoveCustomCommandEntry(mnuCustom, args.Command);
            RefreshModel();
        }

        private void OnCustomCommandAdded(object sender, CommandEventArgs args) => AddCustomCommandEntry(mnuCustom, args.Command);

        private void InitCustomCommandMenu()
        {
            foreach (var cmd in _wl.GetCustomCommands())
                AddCustomCommandEntry(mnuCustom, cmd);
        }

        private void InitBuiltinCommandMenu()
        {
            foreach (BuiltInCommandType type in Enum.GetValues(typeof(BuiltInCommandType)))
            {
                ToolStripMenuItem mi = new ToolStripMenuItem(type.ToString(), CommandIconCache.GetStandardCommandIcon(type), new EventHandler(OnAddBuiltInCommand));
                mi.Tag = type;
                mnuBuiltin.DropDown.Items.Add(mi);
            }
            if (_wl.ResourceVersion >= new Version(2, 4, 0))
            {
                ToolStripMenuItem mi = new ToolStripMenuItem(BasicCommandActionType.MapTip.ToString(), null, new EventHandler(OnAddBuiltInCommand));
                mi.Tag = BasicCommandActionType.MapTip;
                mnuBuiltin.DropDown.Items.Add(mi);
            }
        }

        private void RemoveCustomCommandEntry(ToolStripMenuItem tsi, ICommand cmd)
        {
            ToolStripItem find = null;
            foreach (ToolStripItem ti in tsi.DropDown.Items)
            {
                if (ti.Tag == cmd) { find = ti; break; }
            }
            if (find != null)
            {
                tsi.DropDown.Items.Remove(find);
                if (_customCommandListeners.ContainsKey(find))
                {
                    var handler = _customCommandListeners[find];
                    cmd.PropertyChanged -= handler;
                    _customCommandListeners.Remove(find);
                }
            }
        }

        private Dictionary<ToolStripItem, PropertyChangedEventHandler> _customCommandListeners = new Dictionary<ToolStripItem, PropertyChangedEventHandler>();

        private void AddCustomCommandEntry(ToolStripMenuItem tsi, ICommand cmd)
        {
            var icon = CommandIconCache.GetStandardCommandIcon(cmd.ImageURL) ?? Properties.Resources.question;
            ToolStripMenuItem mi = new ToolStripMenuItem(cmd.Name, icon, new EventHandler(OnAddCustomCommand));
            PropertyChangedEventHandler handler = (sender, e) => { if (e.PropertyName == "Name") mi.Text = cmd.Name; };
            _customCommandListeners[mi] = handler;
            cmd.PropertyChanged += WeakEventHandler.Wrap(handler, (eh) => cmd.PropertyChanged -= eh);
            mi.Tag = cmd;
            tsi.DropDown.Items.Add(mi);
        }

        private void OnAddBuiltInCommand(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripItem;
            if (tsi?.Tag != null)
            {
                int cmdAction = Convert.ToInt32(tsi.Tag);
                var cmd = _wl.CommandSet.Commands.OfType<IBasicCommand>().FirstOrDefault(c => cmdAction == (int)c.Action);
                if (cmd != null)
                {
                    var ci = _wl.CreateCommandItem(cmd.Name);
                    var fly = trvMenuItems.SelectedNode?.Tag as FlyoutItem;
                    if (fly != null)
                        fly.Tag.AddItem(ci);
                    else
                        _rootMenu.AddItem(ci);
                    RefreshModel();
                }
            }
        }

        private void OnAddCustomCommand(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripItem;
            if (tsi?.Tag != null)
            {
                var cmd = (ICommand)tsi.Tag;
                var ci = _wl.CreateCommandItem(cmd.Name);
                PropertyChangedEventHandler handler = (s, evt) =>
                {
                    if (evt.PropertyName == "Name") { ci.Command = cmd.Name; trvMenuItems.Refresh(); }
                };
                cmd.PropertyChanged += WeakEventHandler.Wrap(handler, (eh) => cmd.PropertyChanged -= eh);
                var fly = trvMenuItems.SelectedNode?.Tag as FlyoutItem;
                if (fly != null)
                    fly.Tag.AddItem(ci);
                else
                    _rootMenu.AddItem(ci);
                RefreshModel();
            }
        }

        private void addSeparator_Click(object sender, EventArgs e)
        {
            var sep = _wl.CreateSeparator();
            var fly = trvMenuItems.SelectedNode?.Tag as FlyoutItem;
            if (fly != null)
                fly.Tag.AddItem(sep);
            else
                _rootMenu.AddItem(sep);
            RefreshModel();
        }

        private void RefreshModel()
        {
            _model.Refresh();
            OnResourceChanged();
        }

        private void addFlyout_Click(object sender, EventArgs e)
        {
            var fly = _wl.CreateFlyout(Strings.NewFlyout, Strings.NewFlyout, Strings.NewFlyout, null, null);
            var parentFly = trvMenuItems.SelectedNode?.Tag as FlyoutItem;
            if (parentFly != null)
                parentFly.Tag.AddItem(fly);
            else
                _rootMenu.AddItem(fly);
            RefreshModel();
        }

        private void trvMenuItems_AfterSelect(object sender, TreeViewEventArgs e) => EvaluateCommandState();

        private void EvaluateCommandState()
        {
            btnDelete.Enabled = btnMoveDown.Enabled = btnMoveUp.Enabled = (trvMenuItems.SelectedNode != null);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (trvMenuItems.SelectedNode != null)
            {
                var it = trvMenuItems.SelectedNode.Tag as ItemBase;
                if (it != null)
                {
                    var menu = it.Item.Parent ?? _rootMenu;
                    menu.RemoveItem(it.Item);
                    RefreshModel();
                    EvaluateCommandState();
                }
            }
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            var it = trvMenuItems.SelectedNode?.Tag as ItemBase;
            if (it != null)
            {
                var parent = it.Item.Parent ?? _rootMenu;
                if (parent.MoveUp(it.Item))
                {
                    RefreshModel();
                    RestoreItemSelection(it);
                    EvaluateCommandState();
                }
            }
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            var it = trvMenuItems.SelectedNode?.Tag as ItemBase;
            if (it != null)
            {
                var parent = it.Item.Parent ?? _rootMenu;
                if (parent.MoveDown(it.Item))
                {
                    RefreshModel();
                    RestoreItemSelection(it);
                    EvaluateCommandState();
                }
            }
        }

        private void RestoreItemSelection(ItemBase item)
        {
            var found = FindNodeByItem(trvMenuItems.Nodes, item.Item);
            if (found != null)
                trvMenuItems.SelectedNode = found;
        }

        private static TreeNode FindNodeByItem(TreeNodeCollection nodes, IUIItem target)
        {
            foreach (TreeNode node in nodes)
            {
                var it = node.Tag as ItemBase;
                if (it != null && it.Item == target)
                    return node;
                var found = FindNodeByItem(node.Nodes, target);
                if (found != null) return found;
            }
            return null;
        }

        private void trvMenuItems_ItemDrag(object sender, ItemDragEventArgs e)
            => trvMenuItems.DoDragDrop(e.Item, DragDropEffects.All);

        private void trvMenuItems_DragDrop(object sender, DragEventArgs e)
        {
            var dragNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (dragNode == null) return;

            var item = ((ItemBase)dragNode.Tag).Item;
            if (item.Parent != null)
                item.Parent.RemoveItem(item);
            else
                _rootMenu.RemoveItem(item);

            var dropNode = trvMenuItems.GetNodeAt(trvMenuItems.PointToClient(new Point(e.X, e.Y)));
            if (dropNode != null)
            {
                var dropItem = ((ItemBase)dropNode.Tag).Item;
                var menu = dropItem as IMenu;
                if (menu != null)
                {
                    if (MessageBox.Show(Strings.QuestionAddItemToFlyout, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        menu.AddItem(item);
                    else
                    {
                        var pm = dropItem.Parent ?? _rootMenu;
                        pm.Insert(item, pm.GetIndex(dropItem));
                    }
                }
                else
                {
                    var pm = dropItem.Parent ?? _rootMenu;
                    pm.Insert(item, pm.GetIndex(dropItem));
                }
            }
            else
            {
                _rootMenu.AddItem(item);
            }
            RefreshModel();
        }

        private void trvMenuItems_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetData(typeof(TreeNode)) != null ? DragDropEffects.Move : DragDropEffects.None;
        }
    }
}