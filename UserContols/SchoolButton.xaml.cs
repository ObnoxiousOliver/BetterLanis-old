using BetterLanis.Extensions;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetterLanis.Login.SchoolList
{
    /// <summary>
    /// Interaction logic for SchoolButton.xaml
    /// </summary>
    public partial class SchoolButton : UserControl
    {
        public SchoolButton(string districtName, string schoolName, string schoolLocal, string id)
        {
            InitializeComponent();

            DistrictName = districtName;
            SchoolName = schoolName;
            SchoolLocal = schoolLocal;
            schoolId = id;

            MouseDown += OnSchoolSelected;
        }

        public string DistrictName
        {
            get { return DistrictNameTextBox.Text; }
            set { DistrictNameTextBox.Text = value; }
        }

        public string SchoolName
        {
            get { return SchoolNameTextBox.Text; }
            set { SchoolNameTextBox.Text = value; ButtonGrid.ToolTip = value; }
        }

        public string SchoolLocal
        {
            get { return SchoolLocalTextBox.Text; }
            set { SchoolLocalTextBox.Text = value; }
        }

        public string schoolId;

        protected virtual void OnSchoolSelected(object sender, MouseButtonEventArgs e)
        {
            var eventArgs = new SchoolSelectedEventArgs();
            eventArgs.Id = schoolId;
            eventArgs.District = DistrictName;
            eventArgs.Local = SchoolLocal;
            eventArgs.Name = SchoolName;
            SchoolSelected?.Invoke(this, eventArgs);
        }

        public event EventHandler<SchoolSelectedEventArgs> SchoolSelected;
    }

    
}
public class SchoolSelectedEventArgs : EventArgs
{
    public string Id { get; set; }
    public string District { get; set; }
    public string Local { get; set; }
    public string Name { get; set; }
}