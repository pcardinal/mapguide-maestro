// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views.Editors;

public partial class XmlEditorView : UserControl
{
    public XmlEditorView() => InitializeComponent();

    protected override async void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is XmlEditorViewModel vm && !vm.IsLoaded)
            await vm.LoadCommand.ExecuteAsync(null);
    }
}
