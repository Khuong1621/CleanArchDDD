using WinFormsUI.Views;

namespace WinFormsUI.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _view;
        // private readonly IApplicationService _appService; // To be injected

        public MainPresenter(IMainView view)
        {
            _view = view;
            _view.RefreshDataClicked += OnRefreshDataClicked;
        }

        private void OnRefreshDataClicked(object? sender, EventArgs e)
        {
            _view.StatusText = "Data refreshed at " + DateTime.Now.ToLongTimeString();
        }
    }
}
