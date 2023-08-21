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
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace Jacobs.DD.Windows
{
    
    public partial class winLayerList : Window
    {
        public winLayerList(Objects.LayerObjectCollection Layers)
        {
            InitializeComponent();
            DatagridLayers.DataContext = Layers;

            //Tooltips for the buttons. Hover over the buttons to see them.
            btnRefresh.ToolTip = "Layers are refreshed in above table.\nUnsaved changes will be ignored, inorder to save it click Apply first.";
            btnDelete.ToolTip = "Deletes currently selected Layer.\nSelect only one at a time.";
            btnNewLayer.ToolTip = "Opens Layer Name Tool from Jacobs DD Tools Menu in the Ribbon.\n❕ It takes a few seconds to load, please wait while it loads!";
        }

        #region Button Apply, OK, Cancel

        //Save changes and exit.
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //Press Apply button.
            btnApply_Click(sender, e);
            Close();
        }

        //Exit window without saving changes.
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Save Changes to Civil 3D.
        private void btnApply_Click(object sender, RoutedEventArgs e) 
        {
            var upLyrsObjColl = new Objects.LayerObjectCollection();
            try
            {
                //Sends updated layer names to Civil 3D.
                foreach (Objects.LayerObject layersObj in DatagridLayers.ItemsSource)
                    upLyrsObjColl.UpdateLayer(layersObj.BaseId, layersObj.Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        //Deletes the selected layer
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //Select a Layer to be deleted.
            var LayerObjColl = new Objects.LayerObjectCollection();
            var LayerObj = (Objects.LayerObject)DatagridLayers.SelectedItem;

            try
            {
                //Did user select any layer before clicking the Delete button?
                if (LayerObj != null)
                {
                    //Create a confirmation dialogue box.
                    var msgDescription = new StringBuilder("Are you sure you want to delete?\n");
                    msgDescription.AppendLine("Layer Name: " + LayerObj.Name);
                    //Ask user to confirm deleting the layer.
                    var msgResult = (MessageBoxResult)MessageBox.Show(msgDescription.ToString(), "Delete Layer", MessageBoxButton.YesNo);
                    //Confirmed? then, Delete Layer.
                    try
                    {
                        if (msgResult == MessageBoxResult.Yes)
                            LayerObjColl.DeleteLayer(LayerObj.BaseId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else throw new Exception("No layer was selected.");
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\nError: " + ex.Message);
            }

            //Refresh DataGrid with updated layer list
            var layerObjColl1 = new Objects.LayerObjectCollection();
            //Call Refresh Method and pass new instance of LayerObjectCollection into it.
            DatagridLayers.ItemsSource = layerObjColl1.Refresh(layerObjColl1);
        }

        //Create a new layer
        private void btnNewLayer_Click(object sender, RoutedEventArgs e)
        {
            //Open up Jon Dempsy's LayerNameTool Pallette in Civil3D. CommandMethod: LAYERNAMETOOL
            Document Doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            //Keep a space at the end of Command which will simulate pressing Enter/Space Key on the Keyboard while executing following line of code.
            Doc.SendStringToExecute("LAYERNAMETOOL ", Activate(), false, true);
            
            ////Create a new window to get layer name as input.
            //var oSecondaryWinNewLayerName = new Windows.SecondaryWindows.SecondaryWinNewLayerName();
            //Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(oSecondaryWinNewLayerName);

            ////Refresh DataGrid
            //var layerObjColl2 = new Objects.LayerObjectCollection();
            //DatagridLayers.ItemsSource = layerObjColl2.Refresh(layerObjColl2);
        }

        //Run LayerName checks
        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var layerObjColl3 = new Objects.LayerObjectCollection();
            try
            {
                //Selects either 'GG184' or 'IAN184' based on BIM Standard selection.
                var SelectedBIMCode = (bool)radioGG184.IsChecked ? (string)radioGG184.Content : (bool)radioIAN184.IsChecked ? (string)radioIAN184.Content : null;
                //Notify user to select a BIM Standard
                _ = SelectedBIMCode == null ? throw new Exception("Select a BIM Standard.") : false;

                foreach (Objects.LayerObject layerObject1 in DatagridLayers.ItemsSource)
                {
                    //Validate Layer Name and add to new LayerObjectCollection so that this new collection can be used to refresh the DataGrid.
                    layerObjColl3.CheckLayerName(layerObject1, SelectedBIMCode);
                    layerObjColl3.Add(layerObject1);
                }
                //Refresh the DataGrid - The DataGrid will not be refreshed if user doesn't select a BIM Standard.
                DatagridLayers.ItemsSource = layerObjColl3;
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation!\nError: " + ex.Message);
            }
        }

        //Update notes for user
        private void UpdateLabelNotes(object sender, RoutedEventArgs e)
        {
            //Naming convention notes for the user.
            if (radioGG184.IsChecked == true)
            {
                //Clear Notes.
                textblockNotes.Inlines.Clear();
                //Update Notes for GG184
                textblockNotes.Inlines.Add(new Run("ℹ ") { Foreground = Brushes.DarkBlue, Background = Brushes.SkyBlue, FontSize = 12 });
                textblockNotes.Inlines.Add(new Run("GG184 Layer Name format:") { FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = 12 });
                //1. C3D Component
                textblockNotes.Inlines.Add(new Run(" [") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("C3D Component ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("-") { FontWeight = FontWeights.Bold });
                //2. Classification
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("Classification ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("-") { FontWeight = FontWeights.Bold });
                //3. Presentation
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("Presentation ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("-") { FontWeight = FontWeights.Bold });
                //4. Type (Optional)
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("Type") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //5. Description
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.CornflowerBlue });
                textblockNotes.Inlines.Add(new Run("Description") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.CornflowerBlue });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.CornflowerBlue });
                textblockNotes.Inlines.Add(new Run("_") { FontWeight = FontWeights.Bold });
                //6. Unique ID
                textblockNotes.Inlines.Add(new Run("[") { FontWeight = FontWeights.Bold, Foreground = Brushes.PaleVioletRed });
                textblockNotes.Inlines.Add(new Run("Unique ID") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic, Foreground = Brushes.PaleVioletRed });
                textblockNotes.Inlines.Add(new Run("]") { FontWeight = FontWeights.Bold, Foreground = Brushes.PaleVioletRed });

                //Fields Explaination
                textblockNotes.Inlines.Add(new Run("\n1. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("C3D Component: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.BlueViolet });
                textblockNotes.Inlines.Add(new Run("Based on its origin it can have a maximum of two Alphabetic characters. E.g. ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("C") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Normal });
                textblockNotes.Inlines.Add(new Run(" - represents layer is associated to corridor.") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n2. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Classification: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown });
                textblockNotes.Inlines.Add(new Run("Dynamic classification obtained from Uniclass 2015 without underscores. E.g. ") { FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("Zz3510") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n3. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Presentation: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.OrangeRed });
                textblockNotes.Inlines.Add(new Run("Maximum of two alphanumeric characters. E.g. ") { FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("M2, M3") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n4. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Type: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen });
                textblockNotes.Inlines.Add(new Run("A short description using ") { FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("E.g. AlignHoriz, AlignVert, etc.") { FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n5. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Description: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.CornflowerBlue });
                textblockNotes.Inlines.Add(new Run("Additional optional short description using maximum of ") { FontWeight = FontWeights.Normal, FontStyle = FontStyles.Italic });
                textblockNotes.Inlines.Add(new Run("100 CamelCase alphanumeric characters.") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });

                textblockNotes.Inlines.Add(new Run("\n6. ") { FontWeight = FontWeights.Normal });
                textblockNotes.Inlines.Add(new Run("Unique ID: ") { FontWeight = FontWeights.Bold, Foreground = Brushes.PaleVioletRed });
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
            var LayerObjColl1 = new Objects.LayerObjectCollection();
            LayerObjColl1.GetFromDrawingDatabase();
            DatagridLayers.DataContext = LayerObjColl1;

            //Checks if any BIMStandard is selected
            var SelectedBIMCode = BIMStandard(sender, e);
            //Also run analysis if BIMStandard is selected.
            if (SelectedBIMCode != null)
                btnAnalysis_Click(sender, e);
        }
    }
}
