using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;


namespace Jacobs.DD.AbstractObjects
{
    //Abstract class allows inheritance and prevents direct Instantiation.
    //Inheriting ObservableObjects to implement INotifyPropertyChanged.
    public abstract class BaseObjects : ObservableObjects  
    {
        #region Properties of BaseObjects Class

        private string _bName = "";
        public string Name
        {
            get { return _bName; }
            set 
            { 
                _bName = value;
                OnPropertyChanged();
            }
        }

        /// DO NOT IMPLEMENT INotifyProperyChanged WITH BaseId.
        private ObjectId _objId = ObjectId.Null;
        public ObjectId BaseId
        {
            get { return _objId; }
            set { _objId = value; }
        }

        private Boolean _isSelected = false;
        public Boolean IsSelected
        {
            get { return _isSelected; }
            set 
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods of BaseObjects Class
        //Methods of BaseObjects Class.

        #endregion

    }
}
