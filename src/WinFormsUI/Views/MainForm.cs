using System.ComponentModel;
using WinFormsUI.Views;

namespace WinFormsUI;

public partial class MainForm : Form, IMainView
{
    public MainForm()
    {
        InitializeComponent();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string StatusText
    {
        get => this.Text;
        set => this.Text = value;
    }

    public event EventHandler? RefreshDataClicked;

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        RefreshDataClicked?.Invoke(this, EventArgs.Empty);
    }
}
