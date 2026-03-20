using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application;
using Infrastructure;
using WinFormsUI.Views;
using WinFormsUI.Presenters;

namespace WinFormsUI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Layers
                services.AddApplication();
                services.AddInfrastructure(context.Configuration);

                // MVP Components
                services.AddSingleton<MainForm>();
                services.AddSingleton<IMainView>(sp => sp.GetRequiredService<MainForm>());
                services.AddTransient<MainPresenter>();
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                // Get the presenter first to ensure it attaches to the view
                var presenter = services.GetRequiredService<MainPresenter>();
                var mainForm = services.GetRequiredService<MainForm>();

                System.Windows.Forms.Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                // Handle or log exception
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
