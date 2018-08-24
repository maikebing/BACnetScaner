using BACnetCommonLib;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.BACnet;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BACnetTranslator
{
    public class BacDevice
    {
        public BacnetAddress Address { get; set; }
        public uint DeviceId { get; set; }
        public ObservableCollection<BacProperty> Properties { get; set; }
        public int ScanIndex { get; set; }  //扫点序号
        public BacDevice(BacnetAddress adr, uint device_id)
        {
            this.Address = adr;
            this.DeviceId = device_id;
        }
        public void LoadPropertiesFromExcel(string file)
        {
            try
            {
                if (!File.Exists(file)) return;
                if (this.Properties == null) this.Properties = new ObservableCollection<BacProperty>();
                using (FileStream fp = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var book = new HSSFWorkbook(fp);
                    for (int sheetNum = 0; sheetNum < book.NumberOfSheets; sheetNum++)
                    {
                        var sheet = book.GetSheetAt(sheetNum);
                        if (sheet.FirstRowNum < 0 || sheet.FirstRowNum >= sheet.LastRowNum) continue;
                        for (var rowNum = sheet.FirstRowNum + 1; rowNum <= sheet.LastRowNum; rowNum++)
                        {
                            var rowContent = sheet.GetRow(rowNum);
                            if (rowContent == null) continue;
                            var pro = BacProperty.FromExcelRow(rowContent);
                            if (pro == null) continue;
                            pro.Device = this;
                            this.Properties.Add(pro);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Error("BacDevice.LoadPropertiesFromExcel: ", exp);
            }
        }
        public void LoadProperties(uint dID, Dictionary<String, Point> c)
        {
            try
            {
               if (this.Properties == null) this.Properties = new ObservableCollection<BacProperty>();
               foreach(var p in c)
               {
                       var pro = BacProperty.getPro(p.Key,p.Value);
                       if (pro == null) continue;
                       pro.Device = this;
                       if (pro.Device.DeviceId == dID)
                       {
                           this.Properties.Add(pro);   
                       }
              }
     
            }
            catch (Exception exp)
            {
                Logger.Error("BacDevice.LoadProperties: ", exp);
            }
        }
        public void LoadPointsFromExcel(string file)
        {
            try
            {
                if (!File.Exists(file)) return;
                excuteDataConvert(file);
                excuteFunctions(file);
                excutePoints(file);

            }
            catch (Exception exp)
            {
                Logger.Error("BacDevice.LoadPropertiesFromExcel: ", exp);
            }
        }

        private void excutePoints(string file)
        {
            using (FileStream fp = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var book = new HSSFWorkbook(fp);
                for (int sheetNum = 0; sheetNum < book.NumberOfSheets; sheetNum++)
                {
                    var sheet = book.GetSheetAt(sheetNum);
                    if ("bacnetipv2".Equals(sheet.SheetName))
                    {
                        if (sheet.FirstRowNum < 0 || sheet.FirstRowNum >= sheet.LastRowNum) continue;
                        for (var rowNum = sheet.FirstRowNum + 1; rowNum <= sheet.LastRowNum; rowNum++)
                        {

                        }
                    }
                }
            }
        }

        private void excuteFunctions(string file)
        {
            using (FileStream fp = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var book = new HSSFWorkbook(fp);
                for (int sheetNum = 0; sheetNum < book.NumberOfSheets; sheetNum++)
                {
                    var sheet = book.GetSheetAt(sheetNum);
                    if ("bacnetipv2".Equals(sheet.SheetName))
                    {
                        if (sheet.FirstRowNum < 0 || sheet.FirstRowNum >= sheet.LastRowNum) continue;
                        for (var rowNum = sheet.FirstRowNum + 1; rowNum <= sheet.LastRowNum; rowNum++)
                        {

                        }
                    }
                }
            }
        }

        private void excuteDataConvert(string file)
        {
            using (FileStream fp = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var book = new HSSFWorkbook(fp);
                for (int sheetNum = 0; sheetNum < book.NumberOfSheets; sheetNum++)
                {
                    var sheet = book.GetSheetAt(sheetNum);
                    if ("bacnetipv2".Equals(sheet.SheetName))
                    {
                        if (sheet.FirstRowNum < 0 || sheet.FirstRowNum >= sheet.LastRowNum) continue;
                        for (var rowNum = sheet.FirstRowNum + 1; rowNum <= sheet.LastRowNum; rowNum++)
                        {

                        }
                    }
                }
            }
        }
        private void readExcel(string file,string sheetName)
        {
            Dictionary<Int32, Dictionary<Int32, String>> result = new Dictionary<Int32, Dictionary<Int32, String>>();
            using (FileStream fp = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var book = new HSSFWorkbook(fp);
                for (int sheetNum = 0; sheetNum < book.NumberOfSheets; sheetNum++)
                {
                    var sheet = book.GetSheetAt(sheetNum);
                    if (sheetName.Equals(sheet.SheetName))
                    {
                        if (sheet.FirstRowNum < 0 || sheet.FirstRowNum >= sheet.LastRowNum) continue;
                        for (var rowNum = sheet.FirstRowNum + 1; rowNum <= sheet.LastRowNum; rowNum++)
                        {
                            var rowContent = sheet.GetRow(rowNum);
                            for (var colNum = rowContent.FirstCellNum + 1; colNum <= rowContent.LastCellNum;colNum++ )
                            {
                                var cellContent = rowContent.GetCell(colNum);
                            }

                        }
                    }
                }
            }
        }

        public class BacProperty
        {
            public BacDevice Device { get; set; }
            public BacnetObjectId ObjectId { get; set; }
            public string building { get; set; }
            public string meter { get; set; }
            public string funcid { get; set; }
            public string PROP_OBJECT_NAME { get; set; }
            public string PROP_PRESENT_VALUE { get; set; }
            public static BacProperty FromExcelRow(IRow row)
            {
                if (row == null) return null;
                try
                {
                    BacProperty rst = new BacProperty();
                    rst.PROP_OBJECT_NAME = row.GetCell(1).ToString();
                    rst.ObjectId = new BacnetObjectId(
                        (BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), row.GetCell(2).ToString()),
                        uint.Parse(row.GetCell(3).ToString()));
                    // rst.PROP_DESCRIPTION = row.GetCell(5).ToString();
                    //rst.PROP_PRESENT_VALUE = row.GetCell(6).ToString();
                    rst.building = row.GetCell(4).ToString();
                    rst.meter = row.GetCell(5).ToString();
                    rst.funcid = row.GetCell(6).ToString();
                    return rst;
                }
                catch (Exception exp)
                {
                    Logger.Error("BacProperty.FromExcelRow", exp);
                    return null;
                }
            }
            public static BacProperty getPro(String key, Point point)
            {
                //Key = "192.168.20.159:2068-1968-analogInput-0"
                try
                {
                    BacProperty rst = new BacProperty();
                    //rst.PROP_OBJECT_NAME = row.GetCell(1).ToString();
                    rst.ObjectId = new BacnetObjectId(getObjectType(Regex.Split((key), "-", RegexOptions.IgnoreCase)[2]),
                        uint.Parse(Regex.Split((key), "-", RegexOptions.IgnoreCase)[3]));
                    // rst.PROP_DESCRIPTION = row.GetCell(5).ToString();
                    //rst.PROP_PRESENT_VALUE = row.GetCell(6).ToString();
                    rst.building = point.buildingSign;
                    rst.meter = point.meterSign;
                    foreach (var funcid in point.functionList)
                    {
                        rst.funcid = Convert.ToString(funcid.functionID);
                        break;
                    } 
                    
                    return rst;
                }
                catch (Exception exp)
                {
                    Logger.Error("BacProperty.getPro", exp);
                    return null;
                }
            }
            private static BacnetObjectTypes getObjectType(String type)
            {
                // TODO Auto-generated method stub
                if ("analogInput".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_ANALOG_INPUT;
                }
                else if ("analogOutput".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_ANALOG_OUTPUT;
                }
                else if ("analogValue".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_ANALOG_VALUE;
                }
                else if ("binaryInput".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_BINARY_INPUT;
                }
                else if ("binaryOutput".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_BINARY_OUTPUT;
                }
                else if ("binaryValue".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_BINARY_VALUE;
                }
                else if ("multiStateInput".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_MULTI_STATE_INPUT;
                }
                else if ("multiStateOutput".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_MULTI_STATE_OUTPUT;
                }
                else if ("multiStateValue".Equals(type))
                {
                    return BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE;
                }
                    return BacnetObjectTypes.OBJECT_ANALOG_INPUT;
            }
        }

    }
}
