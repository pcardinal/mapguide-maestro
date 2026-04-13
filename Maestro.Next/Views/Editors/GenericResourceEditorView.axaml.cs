// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views.Editors;

public partial class GenericResourceEditorView : UserControl
{
    public GenericResourceEditorView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is GenericResourceEditorViewModel vm && !vm.IsLoaded)
            _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
