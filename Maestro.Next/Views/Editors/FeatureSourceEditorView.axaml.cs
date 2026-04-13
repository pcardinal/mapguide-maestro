// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views.Editors;

public partial class FeatureSourceEditorView : UserControl
{
    public FeatureSourceEditorView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // Auto-load when the editor is opened
        if (DataContext is FeatureSourceEditorViewModel vm && !vm.IsLoaded)
            _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
