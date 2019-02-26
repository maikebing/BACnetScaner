using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.BACnet;
using System.Linq;
using System.Text;

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
 
}
