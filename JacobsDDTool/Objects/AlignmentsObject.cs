using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;

//Manual Namespaces @Jacobs.DD
//Folder: AbstractObjects 
using Jacobs.DD.AbstractObjects;
using System.Text.RegularExpressions;

namespace Objects
{
    #region Alignment Properties

    public class AlignmentsObject :BaseObjects
    {
        //Alignment specific properties goes here.

        //Result: Is Alignment name valid?
        private string _IsValidAlignmentName = "❌";
        public string IsValidAlignmentName
        {
            get { return _IsValidAlignmentName; }
            set { _IsValidAlignmentName = value; }
        }

        //LayerId: ObjectId of Layer that is associated with the alignment.
        private ObjectId _AssociatedlayerObjectID = ObjectId.Null;
        public ObjectId AssociatedLayerObjectId
        {
            get { return _AssociatedlayerObjectID; }
            set { _AssociatedlayerObjectID = value; }
        }

        //LayerName: Name of layer Associated with Alignment.
        private string _AssociatedLayerObjectName = "";
        public string AssociatedLayerObjectName
        {
            get { return _AssociatedLayerObjectName; }
            set
            {
                _AssociatedLayerObjectName = value;
                OnPropertyChanged();
            }
        }

        //Result: Is layer name associated with Alignment valid?
        private string _IsValidAssociatedLayerName = "❌";
        public string IsValidAssociatedLayerName
        {
            get { return _IsValidAssociatedLayerName; }
            set { _IsValidAssociatedLayerName = value; }
        }
    }

    #endregion

    #region Alignment Methods

    //AlignmentObjectCollection class hosting all the Alignment specific methods.
    public class AlignmentObjectCollection :System.Collections.ObjectModel.ObservableCollection<AlignmentsObject>
    {
        //Creates a list of Alignments in Active Civil3D Document.
        public void GetFromAlignmentsDatabase()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Database DocDatabase = Doc.Database;
            //Lock Document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                using (Transaction trans = DocDatabase.TransactionManager.StartTransaction())
                {
                    try
                    {
                        var CivilDoc = CivilApplication.ActiveDocument;
                        //Get collection of all alignmentIds.
                        var AlignmentIdColl = CivilDoc.GetAlignmentIds();
                        AlignmentsObject alignmentObj; //Placeholder for Alignment Data.

                        //Read Alignments
                        foreach (ObjectId objId in AlignmentIdColl)
                        {
                            Alignment curAlignment = (Alignment)trans.GetObject(objId, OpenMode.ForRead);
                            if (curAlignment.Name.Contains("|") == true) continue;

                            //Adds each alignments to an ObservableCollection.
                            alignmentObj = new AlignmentsObject();
                            alignmentObj.Name = curAlignment.Name;
                            alignmentObj.BaseId = objId;
                            alignmentObj.AssociatedLayerObjectId = curAlignment.LayerId;
                            alignmentObj.AssociatedLayerObjectName = curAlignment.Layer;
                            this.Add(alignmentObj);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Application.ShowAlertDialog("Unable to access Alignment Table.\nError: " + ex.Message);
                    }
                    trans.Commit();
                }
            }//Document unlocked.
        }

        //Update Alignment names.
        public void UpdateAlignment(ObjectId alignmentId, string newAlignmentName)
        {
            Document Doc = Application.DocumentManager.CurrentDocument;
            try
            {
                //Lock document
                using (DocumentLock DocLock = Doc.LockDocument())
                {
                    using (Transaction trans = alignmentId.Database.TransactionManager.StartTransaction())
                    {
                        //Open Alignment corresponding to given alignmentId, open as (or cast) Alignment.
                        var alignment = (Alignment)trans.GetObject(alignmentId, OpenMode.ForWrite);
                        //Change alignment name
                        alignment.Name = newAlignmentName;
                        trans.Commit();
                    }
                }//Document unlocked.
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Alignment names should be unique.\nError: " + ex.Message);
            }
        }

        //Update the layer name associated with Alignments.
        public void UpdateAssociatedLayer(AlignmentsObject alignmentObj, string newAssociatedLayerName)
        {
            try
            {
                var layerObjColl1 = new Objects.LayerObjectCollection();
                layerObjColl1.UpdateLayer(alignmentObj.AssociatedLayerObjectId, newAssociatedLayerName);
                alignmentObj.AssociatedLayerObjectName = newAssociatedLayerName;
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Unable to update associated layer name.\nError: " + ex.Message);
            }
        }

        //Delete Alignment
        public void DeleteAlignment(ObjectId alignmentId)
        {
            Document Doc = Application.DocumentManager.CurrentDocument;
            try
            {
                using (DocumentLock DocLock = Doc.LockDocument())
                {
                    using (Transaction trans = alignmentId.Database.TransactionManager.StartTransaction())
                    {
                        //Open up Alignment Table forWrite.
                        var alignment1 = (Alignment)trans.GetObject(alignmentId, OpenMode.ForWrite);
                        //Delete the alignment.
                        alignment1.Erase(true);
                        trans.Commit();
                    }
                }//Document unlocked.
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("The selected alignment can't be deleted. \nError: " + ex.Message);
            }
        }

        //Refresh alignment list
        public AlignmentObjectCollection Refresh(AlignmentObjectCollection newAlignmentList)
        {
            //Get latest Alignments list, copy to incoming Collection and return it.
            newAlignmentList.GetFromAlignmentsDatabase();
            return newAlignmentList;
        }

        //Checking whether given Alignment complies with BIM standard.
        public void CheckAlignmentName(AlignmentsObject alignmentObj1, string SelectedBIMStandard)
        {
            //BASIC CHECKS ONLY
            //Run RegEx to validate names.
            var pattern = "";
            switch (SelectedBIMStandard)
            {
                case "GG184":
                    pattern = @"^(?<C3DComponent>(M|ML|XL|XR|FL))_(?<SectionID>[A-Za-z0-9]*?)_(?<Description>[A-Za-z0-9]{1,100})(_?(?<UniqueID>_?[0-9]{4})?)$";
                    break;
                case "IAN184":
                    pattern = @"^[A-Z][A-Za-z]{0,1}-[A-Z][A-Za-z](_[0-9]{2}){1,4}( [A-Z]{3,4}){0,1}-[D|M|T|H|P](-[A-Z][a-z]){0,1}_[A-Za-z0-9_ ]{0,250}$";
                    break;
                default:
                    break;
            }
            Match match = Regex.Match(alignmentObj1.Name, pattern);
            alignmentObj1.IsValidAlignmentName = match.Success ? "✔" : "❌";
        }

        //Checking whether given associated Layer complies with BIM Standard.
        public void CheckAssociatedLayerObjectName(AlignmentsObject alignmentObj2, string SelectedBIMStandard)
        {
            //Run RegEx to validate names.
            var pattern = "";
            switch (SelectedBIMStandard)
            {
                case "GG184":
                    //<Prefix: C-Zz3510-M-AlignHoriz_>-<Parent Alignment Name>
                    pattern = @"^(?<AssociatedLayerPrefix>C-Zz3510-M-AlignHoriz)_" + alignmentObj2.Name + "$";
                    break;
                case "IAN184":
                    pattern = @"^[A-Z][A-Za-z]{0,1}-[A-Z][A-Za-z](_[0-9]{2}){1,4}( [A-Z]{3,4}){0,1}-[D|M|T|H|P](-[A-Z][a-z]){0,1}_[A-Za-z0-9_ ]{0,250}$";
                    break;
                default:
                    break;
            }
            Match match = Regex.Match(alignmentObj2.AssociatedLayerObjectName, pattern);
            //Both parent alignment name and Associated layer names must be valid together.
            alignmentObj2.IsValidAssociatedLayerName = (match.Success && alignmentObj2.IsValidAlignmentName == "✔") ? "✔" : "❌";
        }

        //Autocorrect Associated Layer names
        public void AutocorrectLayerName(AlignmentsObject alignmentObj3, String SelectedBIMStandard)
        {
            var newAssociatedLayerObjName = alignmentObj3.AssociatedLayerObjectName;

            switch (SelectedBIMStandard)
            {
                case "GG184":
                    newAssociatedLayerObjName = "C-Zz3510-M-AlignHoriz_" + alignmentObj3.Name;
                    break;
                case "IAN184":
                    //Add autocorrect functionality for IAN184
                    break;
                default:
                    break;
            }
            UpdateAssociatedLayer(alignmentObj3, newAssociatedLayerObjName);
            CheckAssociatedLayerObjectName(alignmentObj3, SelectedBIMStandard);
        }
    }

    #endregion
}
