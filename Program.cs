using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

class Program
{
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class MainForm : Form
{
    private CheckedListBox printerListBox;
    private CheckedListBox filamentListBox;
    private CheckedListBox printListBox;

    public MainForm()
    {
        InitializeComponents();
        DisplayProfiles("printer", printerListBox);
        DisplayProfiles("filament", filamentListBox);
        DisplayProfiles("print", printListBox);
    }

    private void InitializeComponents()
    {
        this.Text = "TH3D EZProfiles for PrusaSlicer";
        this.Size = new System.Drawing.Size(800, 500);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        printerListBox = CreateCheckedListBox("Printer Profiles", 10, 10);
        filamentListBox = CreateCheckedListBox("Filament Profiles", 200, 10);
        printListBox = CreateCheckedListBox("Print Profiles", 400, 10);

        Button installButton = new Button();
        installButton.Text = "Install Selected Profiles";
        installButton.Location = new System.Drawing.Point(10, 400);
        installButton.Click += InstallButton_Click;

        this.Controls.Add(installButton);
    }

    private CheckedListBox CreateCheckedListBox(string label, int x, int y)
    {
        Label listLabel = new Label();
        listLabel.Text = label;
        listLabel.Location = new System.Drawing.Point(x, y);

        CheckedListBox checkedListBox = new CheckedListBox();
        checkedListBox.Size = new System.Drawing.Size(180, 300);
        checkedListBox.Location = new System.Drawing.Point(x, y + 20);

        this.Controls.Add(listLabel);
        this.Controls.Add(checkedListBox);

        return checkedListBox;
    }

private void DisplayProfiles(string profileType, CheckedListBox checkedListBox)
{
    string programFolderPath = AppDomain.CurrentDomain.BaseDirectory;
    string profilesFolderPath = Path.Combine(programFolderPath, "profiles");
    string profileFolderPath = Path.Combine(profilesFolderPath, profileType);

    if (Directory.Exists(profileFolderPath))
    {
        string[] profileFiles = Directory.GetFiles(profileFolderPath, "*.ini")
                                          .Select(Path.GetFileNameWithoutExtension)
                                          .ToArray();

        checkedListBox.Items.AddRange(profileFiles.Cast<object>().ToArray());
    }
    else
    {
        checkedListBox.Items.Add($"No {profileType} profiles found.", false);
    }
}

    private void InstallButton_Click(object sender, EventArgs e)
    {
        if (!AnyProfileSelected())
        {
            MessageBox.Show("Please select at least one profile to install.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        InstallProfiles("printer", printerListBox);
        InstallProfiles("filament", filamentListBox);
        InstallProfiles("print", printListBox);

        MessageBox.Show("Profiles installed successfully.");
    }

    private void InstallProfiles(string profileType, CheckedListBox checkedListBox)
    {
        string programFolderPath = AppDomain.CurrentDomain.BaseDirectory;
        string profilesFolderPath = Path.Combine(programFolderPath, "profiles");
        string profileFolderPath = Path.Combine(profilesFolderPath, profileType);

        if (Directory.Exists(profileFolderPath))
        {
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                if (checkedListBox.GetItemChecked(i))
                {
                    string profile = checkedListBox.Items[i].ToString();
                    string profileFilePath = Path.Combine(profileFolderPath, $"{profile}.ini");
                    string destinationFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PrusaSlicer", profileType);

                    if (File.Exists(profileFilePath))
                    {
                        string destinationFilePath = Path.Combine(destinationFolderPath, $"{profile}.ini");
                        File.Copy(profileFilePath, destinationFilePath, true);
                    }
                }
            }
        }
    }

    private bool AnyProfileSelected()
    {
        return printerListBox.CheckedItems.Count > 0 || filamentListBox.CheckedItems.Count > 0 || printListBox.CheckedItems.Count > 0;
    }
}
