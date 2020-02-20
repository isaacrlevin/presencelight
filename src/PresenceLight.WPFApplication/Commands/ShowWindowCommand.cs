using System.Windows;
using System.Windows.Input;

namespace PresenceLight.WPFApplication.Commands
{
    /// <summary>
    /// Shows the main window.
    /// </summary>
    public class ShowWindowCommand : CommandBase<ShowWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Show();
            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            GetTaskbarWindow(parameter).Show();
            return win != null && !win.IsVisible;
        }
    }
}