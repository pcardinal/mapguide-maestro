// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views.Editors;

public partial class ApplicationDefinitionEditorView : UserControl
{
    public ApplicationDefinitionEditorView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ApplicationDefinitionEditorViewModel vm && !vm.IsLoaded)
            _ = vm.LoadCommand.ExecuteAsync(null);
    }
}
