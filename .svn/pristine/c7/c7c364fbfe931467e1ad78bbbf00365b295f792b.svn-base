using BACnetCommonLib;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO.BACnet;
using System.Linq;
using System.Text;

namespace BACnetTranslator
{
    public class BacProperty
    {
        public BacnetObjectId ObjectId { get; set; }
        public string PROP_DESCRIPTION { get; set; }    //描述
        public string PROP_OBJECT_NAME { get; set; }    //点名
        //public object PROP_PRESENT_VALUE { get; set; }  //值
        public static BacProperty FromExcelRow(IRow row)
        {
            if (row == null) return null;
            try
            {
                BacProperty rst = new BacProperty();
                rst.PROP_OBJECT_NAME = row.GetCell(1)?.ToString();
                rst.ObjectId = new BacnetObjectId(
                    (BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), row.GetCell(2)?.ToString()),
                    uint.Parse(row.GetCell(3)?.ToString()));
                rst.PROP_DESCRIPTION = row.GetCell(5)?.ToString();
                return rst;
            }
            catch (Exception exp)
            {
                Logger.Error("BacProperty.FromExcelRow", exp);
                return null;
            }
        }
    }
}
