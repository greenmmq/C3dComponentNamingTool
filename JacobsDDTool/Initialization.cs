using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;


namespace Jacobs.DD
{
    public class Initialization : IExtensionApplication
    {
        #region CommandMethods

        //Writes current layer name on the CommandLine Editor in AutoCAD/C3D
        [CommandMethod("JDDLayerName")] public void cmdAcadLayerName()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var lyrName = new Objects.LayerObjectCollection();

            //Gets CurrentLayerName and casts into string type explicitly.
            ed.WriteMessage("\nCurrent Layer is " + (string)lyrName.GetCurrentLayerName());
        }

        //Opens Jacobs Digital Delivery Toolkit V2.0 Main window.
        [CommandMethod("JDDToolkit")] public void cmdAcadJDDToolkit()
        {
            Windows.winWelcomeScreen owinWelcomeScreen = new Windows.winWelcomeScreen();
            //Application.ShowModalWindow(Application.MainWindow.Handle, owinWelcomeScreen);
            Application.ShowModelessWindow(Application.MainWindow.Handle, owinWelcomeScreen);
        }

        //Creates List of Layers in active document
        [CommandMethod("JDDLayerList")] public void cmdAcadLayerList() 
        {
            //Create new instance of LayerObjectCollection Object and store it in variable lyrs.
            Objects.LayerObjectCollection lyrs = new Objects.LayerObjectCollection();
            //Stores Layers list
            lyrs.GetFromDrawingDatabase();

            //Sends Layers List to GUI Window.
            Windows.winLayerList win_Layers = new Windows.winLayerList(lyrs);

            //Open API Window within AutoCAD.
            //Application.ShowModalWindow(Application.MainWindow.Handle, win_Layers);
            Application.ShowModelessWindow(Application.MainWindow.Handle, win_Layers);
        }

        //Creates list of Alignments in active document
        [CommandMethod("JDDAlignmentList")] public void cmdAcadAlignmentList()
        {
            Objects.AlignmentObjectCollection align = new Objects.AlignmentObjectCollection();
            align.GetFromAlignmentsDatabase();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


            Windows.winAlignmentList win_Alignments = new Windows.winAlignmentList(align);
            //Application.ShowModalWindow(Application.MainWindow.Handle, win_Alignments);
            Application.ShowModelessWindow(Application.MainWindow.Handle, win_Alignments);
        }

        //Creates list of Corridors in active document
        [CommandMethod("JDDCorridorsList")] public void cmdAcadCorridorsList()
        {
            Objects.CorridorsObjectCollection corridorColl = new Objects.CorridorsObjectCollection();
            corridorColl.GetFromCorridorsDatabase();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


            Windows.winCorridorsList win_Corridors = new Windows.winCorridorsList(corridorColl);
            //Application.ShowModalWindow(Application.MainWindow.Handle, win_Corridors);
            Application.ShowModelessWindow(Application.MainWindow.Handle, win_Corridors);
        }

        //Creates list of Surfacecs in active document
        [CommandMethod("JDDSurfaceList")] public void cmdAcadSurfaceList()
        {
            Objects.SurfaceObjectCollection surfaceColl = new Objects.SurfaceObjectCollection();
            surfaceColl.GetFromSurfaceDatabase();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Windows.winSurfaceList win_Surfaces = new Windows.winSurfaceList(surfaceColl);
            //Application.ShowModalWindow(Application.MainWindow.Handle, win_Surfaces);
            Application.ShowModelessWindow(Application.MainWindow.Handle, win_Surfaces);
        }

        //Creates list of Assemblies in active document
        [CommandMethod("JDDAssemblyList")] public void cmdAcadAssemblyList()
        {
            Objects.AssemblyObjectCollection assemblyColl = new Objects.AssemblyObjectCollection();
            assemblyColl.GetFromAssemblyDatabase();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Windows.winAssemblyList win_Assembly = new Windows.winAssemblyList(assemblyColl);
            //Application.ShowModalWindow(Application.MainWindow.Handle, win_Assembly);
            Application.ShowModelessWindow(Application.MainWindow.Handle, win_Assembly);
        }

        //Creates list of Subassemblies in active document
        [CommandMethod("JDDSubAssemblyList")] public void cmdAcadSubAssemblyList()
        {
            Objects.SubAssemblyObjectCollection subAssemblyColl = new Objects.SubAssemblyObjectCollection();
            subAssemblyColl.GetFromSubAssemblyDatabase();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Windows.winSubAssemblyList win_SubAssembly = new Windows.winSubAssemblyList(subAssemblyColl);
            //Application.ShowModalWindow(Application.MainWindow.Handle, win_SubAssembly);
            Application.ShowModelessWindow(Application.MainWindow.Handle, win_SubAssembly);
        }

        #endregion


        #region Initialization

        void IExtensionApplication.Initialize()
        {
            //Message for Script load confirmation.

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            AssemblyName appName = Assembly.GetExecutingAssembly().GetName();
            object[] attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

            ed.WriteMessage(((AssemblyTitleAttribute)attrs[0]).Title + "Ver " +
                appName.Version.Major + "." +
                appName.Version.MajorRevision + "." +
                appName.Version.Minor + "." +
                appName.Version.MinorRevision + " is loaded successfully.");
        }

        void IExtensionApplication.Terminate()
        {
            
        }

        #endregion
    }
}
