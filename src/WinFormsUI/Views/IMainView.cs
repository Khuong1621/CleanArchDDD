namespace WinFormsUI.Views
{
    public interface IMainView
    {
        string StatusText { get; set; }
        event EventHandler RefreshDataClicked;
    }
}
