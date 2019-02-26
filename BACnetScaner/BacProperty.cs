using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.BACnet;
using System.Linq;
using System.Text;

namespace BACnetScaner
{
    
    public class BacProperty 
    {
        #region ObjectId
       
        public BacnetObjectId ObjectId { get; set; }
         
        #endregion

        #region PROP_DESCRIPTION 描述
      
        public string PROP_DESCRIPTION { get; set; }
        
        #endregion

        #region PROP_OBJECT_NAME 点名
     
        public string PROP_OBJECT_NAME { get; set; }
       
        #endregion

       
      
        public object PROP_PRESENT_VALUE { get; set; }
        
      
    }
}
