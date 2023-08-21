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
    /// <summary>
    /// Interaction logic for winLayerList.xaml
    /// </summary>
    public partial class winAssemblyList : Window
    {
        public winAssemblyList(Objects.AssemblyObjectCollection Assemblies)
        {
            InitializeComponent();
            DatagridAssembly.DataContext = Assemblies;

            //Tooltips for the buttons. Hover over the buttons to see them.
            btnRefresh.ToolTip = "Assemblies are refreshed in above table.\nUnsaved changes will be ignored, inorder to save it click Apply first.";
            btnDelete.ToolTip = "Deletes currently selected Assembly.\nSelect only one at a time.";
        }

        #region Button Apply, OK, Cancel

        //Save changes and exit.
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(sender, e);  
            Close();
        }

        //Exit without saving.
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Save changes.
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var upAssemblyObjColl = new Objects.AssemblyObjectCollection();
            try
            {
                foreach (Objects.AssemblyObject assemblyObj in DatagridAssembly.ItemsSource)
                    upAssemblyObjColl.UpdateAssembly(assemblyObj.BaseId, assemblyObj.Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        //Deletes the selected Assembly.
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Select an assembly to delete.
                var AssemblyObjColl = new Objects.AssemblyObjectCollection();
                var AssemblyObj = (Objects.AssemblyObject)DatagridAssembly.SelectedItem;

                //Did user select any assembly before clickign the Delete button?
                if (AssemblyObjColl != null)
                {
                    //Create a confirmation dialogue box.
                    var msgDescription = new StringBuilder("Are you sure you want to delete?\n");
                    msgDescription.AppendLine("Assembly Name: " + AssemblyObj.Name);
                    //Ask user to confirm deletign the Assembly.
                    var msgResult = (MessageBoxResult)MessageBox.Show(msgDescription.ToString(), "Delete Assembly", MessageBoxButton.YesNo);
                    //Confirmed? then, Delete Assembly.
                    try
                    {
                        if (msgResult == MessageBoxResult.Yes)
                            AssemblyObjColl.DeleteAssembly(AssemblyObj.BaseId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else throw new Exception("No assembly was selected.");
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }

            //Refresh DataGrid with updated Assembly list
            var assemblyObjColl1 = new Objects.AlignmentObjectCollection();
            DatagridAssembly.ItemsSource = assemblyObjColl1.Refresh(assemblyObjColl1);
        }

        //Create a new assembly.
        private void btnNewAssembly_Click(object sender, RoutedEventArgs e)
        {
            //Create a new method for creatign new Assembly in AssemblyObjects.
        }

        //Run Assembly names check for BIM Compliance.
        private void btnAssemblyAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var assemblyObjColl1 = new Objects.AssemblyObjectCollection();
            try
            {
                //Selects either 'GG184' or 'IAN184' based on BIM Standard selection.
                var SelectedBIMCode = (bool)radioGG184.IsChecked ? (string)radioGG184.Content : (bool)radioIAN184.IsChecked ? (string)radioIAN184.Content : null;
                //Notify user to select a BIM Standard
                _ = SelectedBIMCode == null ? throw new Exception("Select a BIM Standard.") : false;

                foreach (Objects.AssemblyObject assemblyObj1 in DatagridAssembly.ItemsSource)
                {
                    assemblyObjColl1.CheckAssemblyName(assemblyObj1, SelectedBIMCode);
                    assemblyObjColl1.Add(assemblyObj1);
                }
                //Refresh the DataGrid with new AssemblyObjectCollection
                DatagridAssembly.ItemsSource = assemblyObjColl1;
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
                textblockNotes.Inlines.Add(new Run("GG184 Assembly Name format:") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                //1. C3D Component
                textblockNotes.Inlines.Add(new Run(" [") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("C3D Component ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //2. Model Name
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("Model Name") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //3. Direction
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("Direction") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //4. Description
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateBlue });
                textblockNotes.Inlines.Add(new Run("Description") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkSlateBlue });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateBlue });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //5. Unique ID
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("Unique ID") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });

                //Fields Explaination
                textblockNotes.Inlines.Add(new Run("\n1. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("C3D Component: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("Assembly Name must begin with ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("A.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Normal });

                textblockNotes.Inlines.Add(new Run("\n2. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Model Name: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("Assembly Model Name must exactly match Corridor Model Name.") { FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n3. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Direction: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("Direction of the route. Whether mainline is to use ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("EB, WB, Both, North, East, West, ") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("or ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("South.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n4. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Description: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateBlue });
                textblockNotes.Inlines.Add(new Run("Give a clear and concise description using a maximum of ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("100 CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n5. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Unique ID: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
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
            var Assembly1 = new Objects.AssemblyObjectCollection();
            Assembly1.GetFromAssemblyDatabase();
            DatagridAssembly.DataContext = Assembly1;
            //Checks if any BIMStandard is selected
            var SelectedBIMCode = BIMStandard(sender, e);
            //Also run analysis if BIMStandard is selected.
            if (SelectedBIMCode != null)
                btnAssemblyAnalysis_Click(sender, e);
        }
    }
}
