namespace WinFormsUI;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button btnRefresh;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        btnRefresh = new System.Windows.Forms.Button();
        SuspendLayout();

        //
        // btnRefresh
        //
        btnRefresh.Location = new Point(10, 10);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(150, 40);
        btnRefresh.TabIndex = 0;
        btnRefresh.Text = "Refresh Data";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += new EventHandler(btnRefresh_Click);

        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(btnRefresh);
        Name = "MainForm";
        Text = "Clean Arch WinForms";
        ResumeLayout(false);
    }

    #endregion
}
