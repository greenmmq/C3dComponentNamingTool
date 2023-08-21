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
    public partial class winCorridorsList : Window
    {
        public winCorridorsList(Objects.CorridorsObjectCollection corridors)
        {
            InitializeComponent();
            DatagridCorridors.ItemsSource = corridors;

            //Tooltips for the buttons. Hover over the buttons to see them.
            btnRefresh.ToolTip = "Corridors are refreshed in above table.\nUnsaved changes will ignored, inorder to save it click Apply first.";
            btnDelete.ToolTip = "Deletes currently selected Corridor.\nSelect only one at a time.";
            btnAutocorrect.ToolTip = "Automatically changes associated layer names in the format:\n<\bPrefix\b>_<\bParent Corridor Name\b>\n\b⚠ Caution: Changes made this way will be permanent.\b";
        }

        #region Button Apply, OK, Cancel

        //Save Changes and exit.
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(sender, e);
            Close();
        }

        //Exit without saving changes.
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Save changes.
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var upCorridorObjColl = new Objects.CorridorsObjectCollection();
            try
            {
                foreach (Objects.CorridorsObject corridorObj in DatagridCorridors.ItemsSource)
                    upCorridorObjColl.UpdateCorridors(corridorObj.BaseId, corridorObj.Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        //Deletes the selected corridor.
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //Select a Corridor to be deleted.
            var CorridorObjColl = new Objects.CorridorsObjectCollection();
            var CorridorObj = (Objects.CorridorsObject)DatagridCorridors.SelectedItem;

            try
            {
                //Did user select any corridor before clicking the Delete button?
                if (CorridorObj != null)
                {
                    //Create a confirmation dialogue box.
                    var msgDescription = new StringBuilder("Are you sure you want to delete?\n");
                    msgDescription.AppendLine("Corridor Name: " + CorridorObj.Name);
                    //Ask user to confirm deleting the corridor.
                    var msgResult = (MessageBoxResult)MessageBox.Show(msgDescription.ToString(), "Delete Corridor", MessageBoxButton.YesNo);
                    //Confirmed? then, Delete Layer.
                    try
                    {
                        if (msgResult == MessageBoxResult.Yes)
                            CorridorObjColl.DeleteCorridor(CorridorObj.BaseId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else throw new Exception("No corridor was selected.");
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }

            //Refresh DataGrid with updated Corridor list.
            var corridorObjColl1 = new Objects.CorridorsObjectCollection();
            DatagridCorridors.ItemsSource = corridorObjColl1.Refresh(corridorObjColl1);
        }

        //Create a nwe Corridor.
        private void btnNewCorridor_Click(object sender, RoutedEventArgs e)
        {
            //Create a method for creating a new Corridor in CorridorsObject.
        }

        //Check Corridor Names for BIM Compliance.
        private void btnCorridorAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var corridorObjColl1 = new Objects.CorridorsObjectCollection();
            try
            {
                //Selects either 'GG184' or 'IAN184' based on BIM Standard selection.
                var SelectedBIMCode = BIMStandard(sender, e);
                //Notify user to select a BIM Standard
                _ = SelectedBIMCode == null ? throw new Exception("Select a BIM Standard.") : false;

                foreach (Objects.CorridorsObject corridorObj1 in DatagridCorridors.ItemsSource)
                {
                    //Validate Corridor Names and associated Layer Names.
                    corridorObjColl1.CheckCorridorName(corridorObj1, SelectedBIMCode);
                    corridorObjColl1.CheckAssociatedLayerObjectName(corridorObj1, SelectedBIMCode);
                    corridorObjColl1.Add(corridorObj1);
                }
                //Refresh the DataGrid with new CorridorObjectCollection.
                DatagridCorridors.ItemsSource = corridorObjColl1;
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }
        }

        //AutoCorrect Associated Layer Names
        private void btnAutocorrect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var SelectedBIMCode = BIMStandard(sender, e);
                _ = SelectedBIMCode == null ? throw new Exception("Select a BIM Standard.") : false;

                var corridorObjColl1 = new Objects.CorridorsObjectCollection();
                foreach (Objects.CorridorsObject corridorObj in DatagridCorridors.ItemsSource)
                {
                    corridorObjColl1.AutocorrectLayerName(corridorObj, SelectedBIMCode);
                    corridorObjColl1.Add(corridorObj);
                }
                //Refresh DataGrid with New AlignmentObjectCollection.
                DatagridCorridors.ItemsSource = corridorObjColl1;
                btnCorridorAnalysis_Click(sender, e);
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(ex.Message);
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
                textblockNotes.Inlines.Add(new Run("GG184 Corridors Name format:") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                //1. C3D Component
                textblockNotes.Inlines.Add(new Run(" [") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("C3D Component") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
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
                //4. Road type
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateBlue });
                textblockNotes.Inlines.Add(new Run("Road type") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkSlateBlue });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateBlue });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //5. Unique ID
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("Unique ID") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });

                //Fields Explaination
                textblockNotes.Inlines.Add(new Run("\n1. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("C3D Component: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("Corridor Name must begin with ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("C.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Normal });

                textblockNotes.Inlines.Add(new Run("\n2. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Model Name: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("A short and concise description of Model using ") { FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("100 CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n3. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Direction: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("Direction of the route. Whether mainline is to use ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("EB, WB, Both, North, East, West, ") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("or ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("South.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n4. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Road type: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateBlue });
                textblockNotes.Inlines.Add(new Run("Appropriate road type using a maximum of ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("25 CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n5. ") { FontWeight = FontWeights.Normal });
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
            var Corridors1 = new Objects.CorridorsObjectCollection();
            Corridors1.GetFromCorridorsDatabase();
            DatagridCorridors.DataContext = Corridors1;
            //Checks if any BIMStandard is selected
            var SelectedBIMCode = BIMStandard(sender, e);
            //Also run analysis if BIMStandard is selected.
            if (SelectedBIMCode != null)
                btnCorridorAnalysis_Click(sender, e);
        }
    }
}
