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
    public partial class winSurfaceList : Window
    {
        public winSurfaceList(Objects.SurfaceObjectCollection surface)
        {
            InitializeComponent();
            DatagridSurface.ItemsSource = surface;

            //Tooltips for the buttons. Hover over the buttons to see them.
            btnRefresh.ToolTip = "Surfaces are refreshed in above table.\nUnsaved changes will be ignored, inorder to save it click Apply first.";
            btnDelete.ToolTip = "Deletes currently selected Surface.\nSelect only one at a time.";
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
            try
            {
                var upSurfaceObjColl = new Objects.SurfaceObjectCollection();
                foreach (Objects.SurfaceObject surfaceObj in DatagridSurface.ItemsSource)
                    upSurfaceObjColl.UpdateSurface(surfaceObj.BaseId, surfaceObj.Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        //Delete selected surface.
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //Select a surface to be deleted.
            var SurfaceObjColl = new Objects.SurfaceObjectCollection();
            var SurfaceObj = (Objects.SurfaceObject)DatagridSurface.SelectedItem;

            try
            {
                //Did user select any layer before clicking the Delete button?
                if (SurfaceObj != null)
                {
                    //Create a confirmation dialogue box.
                    var msgDescription = new StringBuilder("Are you sure you want to delete?\n");
                    msgDescription.AppendLine("Surface Name: " + SurfaceObj.Name);
                    //Ask user to confirm deleting the layer.
                    var msgResult = (MessageBoxResult)MessageBox.Show(msgDescription.ToString(), "Delete Surface", MessageBoxButton.YesNo);
                    //Confirmed? then, Delete surface.
                    try
                    {
                        if (msgResult == MessageBoxResult.Yes)
                            SurfaceObjColl.DeleteSurface(SurfaceObj.BaseId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else throw new Exception("No surface was selected.");
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }

            //Refresh DataGrid with updated surface list
            var surfaceObjColl1 = new Objects.SurfaceObjectCollection();
            DatagridSurface.ItemsSource = surfaceObjColl1.Refresh(surfaceObjColl1);
        }

        //Create a new surface.
        private void btnNewSurface_Click(object sender, RoutedEventArgs e)
        {
            //Create a method for creating new surface in SurfaceObject.
        }

        //Run BIM compliance check for Surfaces.
        private void btnSurfaceAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var surfaceObjColl1 = new Objects.SurfaceObjectCollection();
            try
            {
                //Selects either 'GG184' or 'IAN184' based on BIM Standard selection.
                var SelectedBIMCode = (bool)radioGG184.IsChecked ? (string)radioGG184.Content : (bool)radioIAN184.IsChecked ? (string)radioIAN184.Content : null;
                //Notify user to select a BIM Standard
                _ = SelectedBIMCode == null ? throw new Exception("Select a BIM Standard.") : false;

                foreach (Objects.SurfaceObject surfaceObj1 in DatagridSurface.ItemsSource)
                {
                    surfaceObjColl1.CheckSurfaceName(surfaceObj1, SelectedBIMCode);
                    surfaceObjColl1.Add(surfaceObj1);
                }
                //Refresh the DataGrid with new AssemblyObjectCollection
                DatagridSurface.ItemsSource = surfaceObjColl1;
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
                textblockNotes.Inlines.Add(new Run("GG184 Surface Name format:\n") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                //A. Independent Surface
                //1. C3D Component
                textblockNotes.Inlines.Add(new Run("Independent Surface: ") { FontWeight = FontWeights.Bold });
                textblockNotes.Inlines.Add(new Run(" [") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("C3D Component") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //2.Type
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("Type") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //3. Description
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("Description") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //4. Unique ID
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("Unique ID") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });

                //B. Surface associated with Corridor
                //1. C3D Component
                textblockNotes.Inlines.Add(new Run("\nSurface associated with Corridor: ") { FontWeight = FontWeights.Bold });
                textblockNotes.Inlines.Add(new Run(" [") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("C3D Component") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //2.Type
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("Parent Corridor Name") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //3. Description
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("Description") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //4. Unique ID
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("Unique ID") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });

                //Fields Explaination
                textblockNotes.Inlines.Add(new Run("\n1. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("C3D Component: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("Surface Name must begin with ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("S.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Normal });

                textblockNotes.Inlines.Add(new Run("\n2. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Type: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("\n\ta. Independent Surface: Source format of the Surface. E.g. Lidar, Topo, etc.") { FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("\n\tb. Surface associated with Corridor: Append entire parent corridor name.") { FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n3. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Description: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("Give a clear and concise description using a maximum of ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("100 CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n4. ") { FontWeight = FontWeights.Normal });
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
            var Surface1 = new Objects.SurfaceObjectCollection();
            Surface1.GetFromSurfaceDatabase();
            DatagridSurface.DataContext = Surface1;
            //Checks if any BIMStandard is selected
            var SelectedBIMCode = BIMStandard(sender, e);
            //Also run analysis if BIMStandard is selected.
            if (SelectedBIMCode != null)
                btnSurfaceAnalysis_Click(sender, e);
        }
    }
}
