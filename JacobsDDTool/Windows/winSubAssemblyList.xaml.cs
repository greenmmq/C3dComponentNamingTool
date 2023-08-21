using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Jacobs.DD.Windows
{
    public partial class winSubAssemblyList : Window
    {
        public winSubAssemblyList(Objects.SubAssemblyObjectCollection SubAssemblies)
        {
            InitializeComponent();
            DatagridSubAssembly.ItemsSource = SubAssemblies;

            //Tooltips for the buttons. Hover over the buttons to see them.
            btnRefresh.ToolTip = "Subassemblies are refreshed in above table.\nUnsaved changes will ignored, inorder to save it click Apply first.";
            btnDelete.ToolTip = "Deletes currently selected Subassembly.\nSelect only one at a time.";
        }

        #region Button Apply, OK, Cancel

        //Save changes and exit.
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(sender, e);
            Close();
        }

        //Exit without saving
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Save changes.
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var subAssemblyColl = new Objects.SubAssemblyObjectCollection();
            try
            {
                //Send updated Subassembly Names to Civil3D
                foreach (Objects.SubAssemblyObject subAssemblyObj in DatagridSubAssembly.ItemsSource)
                    subAssemblyColl.UpdateSubAssembly(subAssemblyObj.BaseId, subAssemblyObj.Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        //Deletes selected SubAssembly.
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //Select a subassembly to be deleted.
            var SubAssemblyObjColl = new Objects.SubAssemblyObjectCollection();
            var SubAssemblyObj = (Objects.SubAssemblyObject)DatagridSubAssembly.SelectedItem;

            try
            {
                //Did user select any subassembly before clicking the Delete button?
                if (SubAssemblyObjColl != null)
                {
                    //Create a confirmation dialogue box.
                    var msgDescription = new StringBuilder("Are you sure you want to delete?\n");
                    msgDescription.AppendLine("Subassembly Name: " + SubAssemblyObj.Name);
                    //Ask user to confirm deleting the SubAssembly.
                    var msgResult = (MessageBoxResult)MessageBox.Show(msgDescription.ToString(), "Delete Subassembly", MessageBoxButton.YesNo);
                    //Confirmed? then, Delete Layer.
                    try
                    {
                        if (msgResult == MessageBoxResult.Yes)
                            SubAssemblyObjColl.DeleteSubAssembly(SubAssemblyObj.BaseId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else throw new Exception("No Subassembly was selected.");
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }

            //Refresh DataGrid with updated Subassembly list
            var subAssemblyObjColl1 = new Objects.SubAssemblyObjectCollection();
            DatagridSubAssembly.ItemsSource = subAssemblyObjColl1.Refresh(subAssemblyObjColl1);
        }

        //Creates new SubAssembly.
        private void btnNewSubAssembly_Click(object sender, RoutedEventArgs e)
        {
            //Create a method for creating SubAssembly in SubAssemblyObjects.
        }

        //Run SubAssembly names check for BIM Compliance.
        private void btnSubAssemblyAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var subAssemblyObjColl1 = new Objects.SubAssemblyObjectCollection();
            try
            {
                //Selects either 'GG184' or 'IAN184' based on BIM Standard selection.
                var SelectedBIMCode = (bool)radioGG184.IsChecked ? (string)radioGG184.Content : (bool)radioIAN184.IsChecked ? (string)radioIAN184.Content : null;
                //Notify user to select a BIM Standard
                _ = SelectedBIMCode == null ? throw new Exception("Select a BIM Standard.") : false;

                foreach (Objects.SubAssemblyObject subAssemblyObj1 in DatagridSubAssembly.ItemsSource)
                {
                    subAssemblyObjColl1.CheckSubAssemblyName(subAssemblyObj1, SelectedBIMCode);
                    subAssemblyObjColl1.Add(subAssemblyObj1);
                }
                //Refresh the DataGrid with new AssemblyObjectCollection
                DatagridSubAssembly.ItemsSource = subAssemblyObjColl1;
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }
        }

        //Update notes for user.
        private void UpdateLabelNotes(object sender, RoutedEventArgs e)
        {
            //Naming convention notes for the user.
            if (radioGG184.IsChecked == true)
            {
                //Clear Notes.
                textblockNotes.Inlines.Clear();
                //Update Notes for GG184
                textblockNotes.Inlines.Add(new Run("ℹ ") { Foreground = Brushes.DarkBlue, Background = Brushes.SkyBlue, FontSize = 12 });
                textblockNotes.Inlines.Add(new Run("GG184 Subassembly Name format:") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                //1. C3D Component Default name
                textblockNotes.Inlines.Add(new Run(" [") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("C3D Component default name") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //2. Description
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("Description") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //3. Unique ID
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("Unique ID") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });

                //Fields Explaination
                textblockNotes.Inlines.Add(new Run("\n1. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("C3D Component default name: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("Subassembly name must begin with ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("JENG_GG184.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Normal });

                textblockNotes.Inlines.Add(new Run("\n2. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Description: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("Give a clear and concise description using a maximum of ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("100 CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n3. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Unique ID: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("An optional 4-digit number to avoid duplication.") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run(" ####") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                //Additional Notes
                textblockNotes.Inlines.Add(new Run("\n❕ ") { Foreground = Brushes.SkyBlue });
                textblockNotes.Inlines.Add(new Run("Use of spaces and special characters are prohibited. One may only use Underscores between fields.") { FontStyle = FontStyles.Italic });
            }
            else if (radioIAN184.IsChecked == true)
            {
                textblockNotes.Inlines.Clear();
                //Update Notes for GG184
                textblockNotes.Inlines.Add(new Run("ℹ ") { Foreground = Brushes.DarkBlue, Background = Brushes.SkyBlue, FontSize = 12 });
                textblockNotes.Inlines.Add(new Run("IAN184 Alignment Name format:") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                textblockNotes.Inlines.Add(new Run(" Work in progress...") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
            }
        }

        //Keeps track of currently selected BIM Standard
        private string BIMStandard(object sender, RoutedEventArgs e)
        {
            //Selects either 'GG184' or 'IAN184' based on BIM Standard selection.
            var SelectedBIMCode = (bool)radioGG184.IsChecked ? (string)radioGG184.Content : (bool)radioIAN184.IsChecked ? (string)radioIAN184.Content : null;
            return SelectedBIMCode;
        }

        //Refresh entire window.
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var SubAssembly1 = new Objects.SubAssemblyObjectCollection();
            SubAssembly1.GetFromSubAssemblyDatabase();
            DatagridSubAssembly.DataContext = SubAssembly1;
            //Checks if any BIMStandard is selected
            var SelectedBIMCode = BIMStandard(sender, e);
            //Also run analysis if BIMStandard is selected.
            if (SelectedBIMCode != null)
                btnSubAssemblyAnalysis_Click(sender, e);
        }
    }
}
