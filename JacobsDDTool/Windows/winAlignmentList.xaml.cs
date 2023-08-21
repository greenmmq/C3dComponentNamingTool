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
    public partial class winAlignmentList : Window
    {
        public winAlignmentList(Objects.AlignmentObjectCollection Alignments)
        {
            InitializeComponent();
            DatagridAlignments.DataContext = Alignments;

            //Tooltips for the buttons. Hover over the buttons to see them.
            btnRefresh.ToolTip = "Alignments are refreshed in above table.\nUnsaved changes will be ignored, inorder to save it click Apply first.";
            btnDelete.ToolTip = "Deletes currently selected Alignment.\nSelect only one at a time.";
            btnAutocorrect.ToolTip = "Automatically changes associated layer names in the format:\n<Prefix>_<Parent Alignment Name>\n\b⚠ Caution: Changes made this way will be permanent.\b";
        }

        #region Buttons: OK, Apply, Cancel

        //Save changes and exit.
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(sender, e);
            Close();
        }

        //Exist without saving.
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Save changes.
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            //Implement for Alignment.
            var upAlignmentObjColl = new Objects.AlignmentObjectCollection();
            try
            {
                //Replaces Alignment Names in Civil 3D with new ones.
                foreach (Objects.AlignmentsObject alignmentsObject in DatagridAlignments.ItemsSource)
                {
                    upAlignmentObjColl.UpdateAlignment(alignmentsObject.BaseId, alignmentsObject.Name);
                    //Update associated Layer names as well.
                    upAlignmentObjColl.UpdateAssociatedLayer(alignmentsObject, alignmentsObject.AssociatedLayerObjectName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        //Create a new alignment
        private void btnNewAlignment_Click(object sender, RoutedEventArgs e)
        {
            //Create a method for creating new alignment in AlignmentObjects.
        }

        //Deletes the selected alignment.
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //Select an Alignment to be deleted.
            var AlignmentObjColl = new Objects.AlignmentObjectCollection();
            var AlignmentObj = (Objects.AlignmentsObject)DatagridAlignments.SelectedItem;

            try
            {
                //Did user select any alignment before clicking the Delete button?
                if (AlignmentObj != null)
                {
                    //Create a confirmation dialogue box.
                    var msgDescription = new StringBuilder("Are you sure you want to delete?\n");
                    msgDescription.AppendLine("Alignment Name: " + AlignmentObj.Name);
                    //Ask user to confirm deleting the layer.
                    var msgResult = (MessageBoxResult)MessageBox.Show(msgDescription.ToString(), "Delete Alignment", MessageBoxButton.YesNo);
                    //Confirmed? then, Delete Alignment.
                    try
                    {
                        if (msgResult == MessageBoxResult.Yes)
                            AlignmentObjColl.DeleteAlignment(AlignmentObj.BaseId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else throw new Exception("No alignment was selected.");
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }

            //Refresh DataGrid with updated alignment list
            var alignmentObjColl1 = new Objects.AlignmentObjectCollection();
            DatagridAlignments.ItemsSource = alignmentObjColl1.Refresh(alignmentObjColl1);
        }

        //Run Alignment Names check for BIM Compliance.
        private void btnAlignmentAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var alignmentObjColl1 = new Objects.AlignmentObjectCollection();
            try
            {
                //Selects either 'GG184' or 'IAN184' based on BIM Standard selection.
                var SelectedBIMCode = BIMStandard(sender, e);
                //Notify user to select a BIM Standard
                _ = SelectedBIMCode == null ? throw new Exception("Select a BIM Standard.") : false;

                foreach (Objects.AlignmentsObject alignmentObj1 in DatagridAlignments.Items)
                {
                    //Validate Alignment Name and Associated Layer Name.
                    alignmentObjColl1.CheckAlignmentName(alignmentObj1, SelectedBIMCode);
                    alignmentObjColl1.CheckAssociatedLayerObjectName(alignmentObj1, SelectedBIMCode);
                    alignmentObjColl1.Add(alignmentObj1);
                }
                //Refresh the DataGrid with new AlignmentObjectCollection
                DatagridAlignments.ItemsSource = alignmentObjColl1;
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }
        }

        //Keeps track of currently selected BIM Standard
        private string BIMStandard (object sender, RoutedEventArgs e)
        {
            //Selects either 'GG184' or 'IAN184' based on BIM Standard selection.
            var SelectedBIMCode = (bool)radioGG184.IsChecked ? (string)radioGG184.Content : (bool)radioIAN184.IsChecked ? (string)radioIAN184.Content : null;
            return SelectedBIMCode;
        }

        //Update notes for user.
        private void UpdateLabelNotes(object sender, RoutedEventArgs e)
        {
            //Naming convention notes for the user.
            switch (BIMStandard(sender, e))
            {
                case "GG184":
                    #region GG184 Naming Notes for user
                    //Clear Notes.
                    textblockNotes.Inlines.Clear();
                    //Update Notes for GG184
                    textblockNotes.Inlines.Add(new Run("ℹ ") { Foreground = Brushes.DarkBlue, Background = Brushes.SkyBlue, FontSize = 12 });
                    textblockNotes.Inlines.Add(new Run("GG184 Alignment Name format:") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                    //1. C3D Component
                    textblockNotes.Inlines.Add(new Run(" [") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                    textblockNotes.Inlines.Add(new Run("C3D Component") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
                    textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                    textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                    //2. Section ID
                    textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                    textblockNotes.Inlines.Add(new Run("Section ID") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.SaddleBrown });
                    textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                    textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                    //3. Description
                    textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                    textblockNotes.Inlines.Add(new Run("Description") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.OrangeRed });
                    textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                    textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                    //Unique ID
                    textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                    textblockNotes.Inlines.Add(new Run("Unique ID") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkGreen });
                    textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });

                    //Fields Explaination
                    textblockNotes.Inlines.Add(new Run("\n1. ") { FontWeight = FontWeights.Normal });
                    textblockNotes.Inlines.Add(new Run("C3D Component: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                    textblockNotes.Inlines.Add(new Run("Alignment Name must begin with ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                    textblockNotes.Inlines.Add(new Run("M, ML, XL, XR, ") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Normal });
                    textblockNotes.Inlines.Add(new Run("or ") { FontWeight = FontWeights.Normal });
                    textblockNotes.Inlines.Add(new Run("FL.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Normal });

                    textblockNotes.Inlines.Add(new Run("\n2. ") { FontWeight = FontWeights.Normal });
                    textblockNotes.Inlines.Add(new Run("Section ID: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                    textblockNotes.Inlines.Add(new Run("Section ID appends Location codes, Road types, etc. using ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                    textblockNotes.Inlines.Add(new Run("CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                    textblockNotes.Inlines.Add(new Run("\n3. ") { FontWeight = FontWeights.Normal });
                    textblockNotes.Inlines.Add(new Run("Description: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                    textblockNotes.Inlines.Add(new Run("Give a clear and concise description using a maximum of ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                    textblockNotes.Inlines.Add(new Run("100 CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                    textblockNotes.Inlines.Add(new Run("\n4. ") { FontWeight = FontWeights.Normal });
                    textblockNotes.Inlines.Add(new Run("Unique ID: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                    textblockNotes.Inlines.Add(new Run("An optional 4-digit number to avoid duplication.") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                    textblockNotes.Inlines.Add(new Run(" ####") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });


                    //Associated Layer Name (Prefix)
                    textblockNotes.Inlines.Add(new Run("\n\nℹ ") { Foreground = Brushes.DarkBlue, Background = Brushes.SkyBlue, FontSize = 12 });
                    textblockNotes.Inlines.Add(new Run("Associated Layer Name format:") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                    textblockNotes.Inlines.Add(new Run(" [") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkCyan });
                    textblockNotes.Inlines.Add(new Run("Prefix: ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkCyan });
                    textblockNotes.Inlines.Add(new Run("C-Zz3510-M-AlignHoriz") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkCyan });
                    textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkCyan });
                    //Parent Alignment Name
                    textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                    textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DeepPink });
                    textblockNotes.Inlines.Add(new Run("Parent Alignment Name") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DeepPink });
                    textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DeepPink });

                    //Additional Notes
                    textblockNotes.Inlines.Add(new Run("\n❕ ") { Foreground = Brushes.SkyBlue });
                    textblockNotes.Inlines.Add(new Run("Use of spaces and special characters are prohibited. One may only use Underscores between fields.") { FontStyle = FontStyles.Italic });
                    break;
                #endregion

                case "IAN184":
                    #region IAN184 Naming Notes for user.
                    textblockNotes.Inlines.Clear();
                    //Update Notes for GG184
                    textblockNotes.Inlines.Add(new Run("ℹ ") { Foreground = Brushes.DarkBlue, Background = Brushes.SkyBlue, FontSize = 12 });
                    textblockNotes.Inlines.Add(new Run("IAN184 Alignment Name format:") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                    textblockNotes.Inlines.Add(new Run(" Work in progress...") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
                    break;
                #endregion

                default:
                    break;
            }
        }

        //AutoCorrect Associated Layer Names
        private void btnAutocorrect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var SelectedBIMCode = BIMStandard(sender, e);
                _ = SelectedBIMCode == null ? throw new Exception("Select a BIM Standard.") : false;

                var alignmentObjColl2 = new Objects.AlignmentObjectCollection();
                foreach (Objects.AlignmentsObject alignmentObj in DatagridAlignments.ItemsSource)
                {
                    alignmentObjColl2.AutocorrectLayerName(alignmentObj, SelectedBIMCode);
                    alignmentObjColl2.Add(alignmentObj);
                }
                //Refresh DataGrid with New AlignmentObjectCollection.
                DatagridAlignments.ItemsSource = alignmentObjColl2;
                btnAlignmentAnalysis_Click(sender, e);
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(ex.Message);
            }
        }

        //Refresh entire window.
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var Alignments1 = new Objects.AlignmentObjectCollection();
            Alignments1.GetFromAlignmentsDatabase();
            DatagridAlignments.DataContext = Alignments1;

            //Checks if any BIMStandard is selected
            var SelectedBIMCode = BIMStandard(sender, e);
            //Also run analysis if BIMStandard is selected.
            if (SelectedBIMCode != null)
                btnAlignmentAnalysis_Click(sender, e);
        }
    }
}
