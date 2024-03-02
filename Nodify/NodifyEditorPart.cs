using System.Windows;
using System.Windows.Input;

namespace Nodify;

public partial class NodifyEditor
{
    public static readonly DependencyProperty CreatingKnotNodeCommandProperty
        = DependencyProperty.Register(nameof(CreatingKnotNodeCommand), typeof(ICommand), typeof(NodifyEditor));

    public ICommand? CreatingKnotNodeCommand
    {
        get => (ICommand?)GetValue(CreatingKnotNodeCommandProperty);
        set => SetValue(CreatingKnotNodeCommandProperty, value);
    }

    protected void OnCreatingKnotNode(
        object sender,
        MouseButtonEventArgs e)
    {
        if (EditorGestures.CreateConnection.Matches(sender, e))
        {
            if (CreatingKnotNodeCommand?.CanExecute(e) == true)
                CreatingKnotNodeCommand.Execute(e);
        }
    }
}
