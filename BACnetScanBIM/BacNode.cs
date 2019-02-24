using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.BACnet;
using System.Linq;
using System.Text;
using System.Windows;

namespace BACnetScaner
{
    public class BacDevice 
    {
        #region Address
      
        public BacnetAddress Address { get; set; }
        
        #endregion

        #region DeviceId
        
        public uint DeviceId { get; set; }
        public List<BacProperty> Properties { get;   set; }

        #endregion


        public BacDevice(BacnetAddress adr, uint device_id)
        {
            this.Address = adr;
            this.DeviceId = device_id;
        }

         
    }
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
