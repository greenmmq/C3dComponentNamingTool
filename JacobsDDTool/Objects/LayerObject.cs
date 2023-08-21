using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

//Manual Namespaces @Jacobs.DD  Folder: AbstractObjects
using Jacobs.DD.AbstractObjects;
using System.Text.RegularExpressions;

namespace Objects
{
    #region Layer Properties
    public class LayerObject : BaseObjects
    {
        //Idedntifies if the layer is frozen.
        private Boolean _IsFrozen = false;
        public Boolean IsFrozen
        {
            get { return _IsFrozen; }
            set
            {
                _IsFrozen = value;
                OnPropertyChanged();
            }
        }

        //Stores result: if Layer name is correct or not as per selected BIM Standard.
        private string _IsValidName = "❌";
        public string IsValidName
        {
            get { return _IsValidName; }
            set { _IsValidName = value; }
        }
    }

    #endregion

    #region Layer Methods

    //ObservableCollection acts like List, but unlike List it also allows GUI to update automatically with TwoWay Binding.
    public class LayerObjectCollection : System.Collections.ObjectModel.ObservableCollection<LayerObject>
    {
        //Writes Active layer name on the editor.
        public string GetCurrentLayerName()
        {
            string curLayerName = ""; //LayerName Placeholder
            Database db = HostApplicationServices.WorkingDatabase;
            
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                //Opening LayerTableRecord to get current layer - "Clayer".
                var lyrTblRec = tr.GetObject(db.Clayer,OpenMode.ForRead) as LayerTableRecord;
                curLayerName = lyrTblRec.Name;
                tr.Commit();
            }
            return curLayerName;
        }

        //Creates a list of Layers in Active AutoCAD Drawing.
        public void GetFromDrawingDatabase()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Database dwgDB = Doc.Database;
            //Locking the document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                using (Transaction trans = dwgDB.TransactionManager.StartTransaction())
                {
                    try
                    {
                        //AutoCAD Layers Pallet.
                        LayerTable lyrTbl = (LayerTable)trans.GetObject(dwgDB.LayerTableId, OpenMode.ForRead);
                        //Create Variable to store AutoCAD Layers.
                        LayerTableRecord lyrTblRec;
                        LayerObject lyrObj;

                        //Read AutoCAD Layers
                        foreach (ObjectId lyrId in lyrTbl)
                        {
                            lyrTblRec = trans.GetObject(lyrId, OpenMode.ForRead) as LayerTableRecord;
                            if (lyrTblRec.Name.Contains("|") == true) continue;

                            //Gets Layers and adds to the ObservableCollection LayerObject.
                            lyrObj = new LayerObject();
                            lyrObj.Name = lyrTblRec.Name;
                            lyrObj.BaseId = lyrId;
                            lyrObj.IsFrozen = lyrTblRec.IsFrozen;
                            //Is it active layer?
                            lyrObj.IsSelected = (lyrTblRec.Name == GetCurrentLayerName()) ? true : false;
                            this.Add(lyrObj);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Application.ShowAlertDialog("Unable to access Layer Table.\n" + ex.Message);
                    }
                    trans.Commit(); //End Transation.
                }
            } //Document unlocked.
        }

        //Updates with new layer names. Duplicates not allowed.
        public void UpdateLayer(ObjectId layerObjId, string newName)
        {
            try
            {
                Document Doc = Application.DocumentManager.CurrentDocument;
                //Lock Document
                using (DocumentLock DocLock = Doc.LockDocument())
                {
                    using (Transaction trans = layerObjId.Database.TransactionManager.StartTransaction())
                    {
                        //Open up the layer object as LayerTableRecord
                        LayerTableRecord ltr = (LayerTableRecord)trans.GetObject(layerObjId, OpenMode.ForWrite);
                        //Change the layer name
                        ltr.Name = newName;
                        trans.Commit();
                    }
                } //Document unlocked
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Layer names should be unique. \nError: " + ex.Message);
            }
        }

        //Delete layer
        public void DeleteLayer(ObjectId layerObjId)
        {
            try
            {
                Document Doc = Application.DocumentManager.MdiActiveDocument;
                //Lock Document
                using (DocumentLock DocumentLock = Doc.LockDocument())
                {
                    //Start transaction
                    using (Transaction trans = layerObjId.Database.TransactionManager.StartTransaction())
                    {
                        //Open up Layer Table
                        var layerTableRec = (LayerTableRecord)trans.GetObject(layerObjId, OpenMode.ForWrite);
                        //Delete layer except layers: '0' and 'Defpoints'
                        if (layerTableRec.Name.ToString() == "0" || layerTableRec.Name.ToString() == "Defpoints")
                            throw new System.Exception("Layer '0' and 'Defpoints' can't be deleted.");
                        else
                            layerTableRec.Erase(true);
                        trans.Commit(); 
                    }//End transaction
                }//Document unlocked
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("The selected layer can't be deleted.\nError: " + ex.Message);
            }
        }

        //Refresh List of layers on DataGrid.
        public LayerObjectCollection Refresh(LayerObjectCollection newLayersList)
        {
            //Generates the list of layers in incoming LayerObjectCollection and returns it.
            newLayersList.GetFromDrawingDatabase();
            return newLayersList;
        }

        //Create a new layer.
        public void CreateNewLayer(string newLayerName)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Database db = Doc.Database;
            try
            {
                using (DocumentLock DocLock = Doc.LockDocument())
                {
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        //Access layer table
                        LayerTable layerTable = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                        //Does layer already exist?
                        if (layerTable.Has(newLayerName) == false)
                        {
                            //newLayerName doesn't exist so create new layer.
                            //New Layer is creates as New LayerTableRecord.
                            var newLayerTableRecord = new LayerTableRecord();
                            newLayerTableRecord.Name = newLayerName;
                            //Upgrade LayerTable forWrite to add new layer.
                            layerTable.UpgradeOpen();
                            layerTable.Add(newLayerTableRecord);
                            //If you ever create a new objects and add it into the drawing
                            // then you need to add it to the "transaction" so the transaction
                            // can add it to the list of objects its working with
                            trans.AddNewlyCreatedDBObject(newLayerTableRecord, true);
                        }
                        else
                        {
                            throw new InvalidOperationException("Error: Layer " + newLayerName + " already exists.");
                        }
                        trans.Commit();
                    } //End transaction.
                } //Document unlocked.
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
        }

        //Checking whether given Layer Name complies to BIM standard.
        public void CheckLayerName (LayerObject layerObj, string SelectedBIMStandard)
        {
            //Check if the selected layer is "0" or "Defpoints"
            if (layerObj.Name == "0" || layerObj.Name == "Defpoints")
            {
                layerObj.IsValidName = "✔";
            }
            else
            {
                //Run RegEx to validate names.
                var pattern = "";
                switch (SelectedBIMStandard)
                {
                    case "GG184":
                        pattern = @"^(?<C3DComponent>[A-Z][A-za-z]?)-(?<Classification>[A-Za-z0-9]+)-(?<Presentation>[A-Za-z][A-Za-z0-9]?)-(?<Type>[A-Za-z0-9]+)_?(?<Description>[A-Za-z0-9_]+?)([_](?<UniqueID>[0-9]{4}))?$";
                        break;
                    case "IAN184":
                        pattern = @"^[A-Z][A-Za-z]{0,1}-[A-Z][A-Za-z](_[0-9]{2}){1,4}( [A-Z]{3,4}){0,1}-[D|M|T|H|P](-[A-Z][a-z]){0,1}_[A-Za-z0-9_ ]{0,250}$";
                        break;
                    default:
                        break;
                }
                Match match = Regex.Match(layerObj.Name, pattern);
                layerObj.IsValidName = (match.Success)? "✔" : "❌";
            }
        }
    }

    #endregion
}