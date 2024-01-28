using System;
using System.Collections.Generic;
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
    private ComboBox brandComboBox;

public MainForm()
{
    InitializeComponents();
    DisplayProfiles("printer", printerListBox);
    DisplayProfiles("filament", filamentListBox);
    DisplayProfilesByBrand("printer", printerListBox, brandComboBox.SelectedItem?.ToString());
    DisplayProfiles("print", printListBox);
    UpdateBrandComboBox();  // This should be called after DisplayProfilesByBrand
    this.Text = "TH3D EZProfiles for PrusaSlicer";
}


    private void InitializeComponents()
    {
        this.Size = new System.Drawing.Size(800, 500);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        printerListBox = CreateCheckedListBox("Printer Profiles", 10, 10);
        filamentListBox = CreateCheckedListBox("Filament Profiles", 200, 10);
        printListBox = CreateCheckedListBox("Print Profiles", 400, 10);

        brandComboBox = new ComboBox();
        brandComboBox.Location = new System.Drawing.Point(200, 400);
        brandComboBox.SelectedIndexChanged += BrandComboBox_SelectedIndexChanged;

        Button installButton = new Button();
        installButton.Text = "Install Selected Profiles";
        installButton.Location = new System.Drawing.Point(10, 400);
        installButton.Click += InstallButton_Click;

        Button closeButton = new Button();
        closeButton.Text = "Close";
        closeButton.Location = new System.Drawing.Point(350, 400);
        closeButton.Click += CloseButton_Click;

        this.Controls.Add(installButton);
        this.Controls.Add(brandComboBox);
        this.Controls.Add(closeButton);
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

    private void DisplayProfilesByBrand(string profileType, CheckedListBox checkedListBox, string brand)
    {
        if (brand != null)
        {
            string programFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            string profilesFolderPath = Path.Combine(programFolderPath, "profiles");
            string profileFolderPath = Path.Combine(profilesFolderPath, profileType, brand);

            if (Directory.Exists(profileFolderPath))
            {
                string[] profileFiles = Directory.GetFiles(profileFolderPath, "*.ini")
                                                  .Select(Path.GetFileNameWithoutExtension)
                                                  .ToArray();

                checkedListBox.Items.Clear();
                checkedListBox.Items.AddRange(profileFiles.Cast<object>().ToArray());
            }
            else
            {
                checkedListBox.Items.Clear();
                checkedListBox.Items.Add($"No {profileType} profiles found for {brand}.", false);
            }
        }
    }

    private void LoadBrands()
    {
        string programFolderPath = AppDomain.CurrentDomain.BaseDirectory;
        string profilesFolderPath = Path.Combine(programFolderPath, "profiles", "printer");

        if (Directory.Exists(profilesFolderPath))
        {
            string[] brandFolders = Directory.GetDirectories(profilesFolderPath)
                                            .Select(Path.GetFileName)
                                            .ToArray();

            brandComboBox.Items.AddRange(brandFolders);
        }
    }

    private void UpdateBrandComboBox()
    {
        LoadBrands();
        brandComboBox.SelectedIndex = 0; // Select the first brand by default
    }

    private void BrandComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        DisplayProfilesByBrand("printer", printerListBox, brandComboBox.SelectedItem?.ToString());
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

    private void CloseButton_Click(object sender, EventArgs e)
    {
        this.Close();
    }
}
