using BACnetCommonLib;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.BACnet;
using System.Linq;
using System.Text;
using System.Windows;

namespace BACnetScanBIM
{
    public class BacDevice : DependencyObject
    {
        #region Address
        public static readonly DependencyProperty AddressProperty = DependencyProperty.Register("Address",
            typeof(BacnetAddress), typeof(BacDevice),
            new PropertyMetadata((sender, e) =>
            {
                var vm = sender as BacDevice;
                if (vm == null) return;
            }));
        public BacnetAddress Address
        {
            get { return GetValue(AddressProperty) as BacnetAddress; }
            set { SetValue(AddressProperty, value); }
        }
        #endregion

        #region DeviceId
        public static readonly DependencyProperty DeviceIdProperty = DependencyProperty.Register("DeviceId",
            typeof(uint), typeof(BacDevice),
            new PropertyMetadata((sender, e) =>
            {
                var vm = sender as BacDevice;
                if (vm == null) return;
            }));
        public uint DeviceId
        {
            get { return (uint)GetValue(DeviceIdProperty); }
            set { SetValue(DeviceIdProperty, value); }
        }
        #endregion

        #region Properties
        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties",
            typeof(ObservableCollection<BacProperty>), typeof(BacDevice),
            new PropertyMetadata((sender, e) =>
            {
                var vm = sender as BacDevice;
                if (vm == null) return;
            }));
        public ObservableCollection<BacProperty> Properties
        {
            get { return GetValue(PropertiesProperty) as ObservableCollection<BacProperty>; }
            set { SetValue(PropertiesProperty, value); }
        }
        #endregion

        public BacDevice(BacnetAddress adr, uint device_id)
        {
            this.Address = adr;
            this.DeviceId = device_id;
        }

        public void Save2Excel()
        {
            if (this.Properties == null || this.Properties.Count == 0) return;
            if (!Directory.Exists(Constants.ExcelDir)) Directory.CreateDirectory(Constants.ExcelDir);
            var excelFilePath = Path.Combine(Constants.ExcelDir, this.DeviceId + ".xls");
            if (File.Exists(excelFilePath)) File.Delete(excelFilePath);
            var book = new HSSFWorkbook();
            var sheet = book.CreateSheet();
            sheet.DefaultColumnWidth = 15;
            var rowHeader = sheet.CreateRow(0);
            rowHeader.CreateCell(0).SetCellValue(@"ID");
            rowHeader.CreateCell(1).SetCellValue(@"Name");
            rowHeader.CreateCell(2).SetCellValue(@"RegType");
            rowHeader.CreateCell(3).SetCellValue(@"RegAddress");
            rowHeader.CreateCell(4).SetCellValue(@"DataType");
            rowHeader.CreateCell(5).SetCellValue(@"Description");
            int row = 1;
            foreach (var pro in this.Properties)
            {
                if (pro == null || pro.ObjectId == null) continue;
                var rowContent = sheet.CreateRow(row);
                rowContent.CreateCell(0).SetCellValue(row);
                rowContent.CreateCell(1).SetCellValue(pro.PROP_OBJECT_NAME);
                rowContent.CreateCell(2).SetCellValue(pro.ObjectId.Type + "");
                rowContent.CreateCell(3).SetCellValue(pro.ObjectId.Instance);
                rowContent.CreateCell(5).SetCellValue(pro.PROP_DESCRIPTION);
                row++;
            }
            // 写入到客户端  
            using (var ms = new MemoryStream())
            {
                book.Write(ms);
                File.WriteAllBytes(excelFilePath, ms.ToArray());
                book = null;
            }
        }
    }
    public class BacProperty : DependencyObject
    {
        #region ObjectId
        public static readonly DependencyProperty ObjectIdProperty = DependencyProperty.Register("ObjectId",
            typeof(BacnetObjectId), typeof(BacProperty),
            new PropertyMetadata((sender, e) =>
            {
                var vm = sender as BacProperty;
                if (vm == null) return;
            }));
        public BacnetObjectId ObjectId
        {
            get { return (BacnetObjectId)GetValue(ObjectIdProperty); }
            set { SetValue(ObjectIdProperty, value); }
        }
        #endregion

        #region PROP_DESCRIPTION 描述
        public static readonly DependencyProperty PROP_DESCRIPTIONProperty = DependencyProperty.Register("PROP_DESCRIPTION",
            typeof(string), typeof(BacProperty),
            new PropertyMetadata((sender, e) =>
            {
                var vm = sender as BacProperty;
                if (vm == null) return;
            }));
        public string PROP_DESCRIPTION
        {
            get { return GetValue(PROP_DESCRIPTIONProperty) as string; }
            set { SetValue(PROP_DESCRIPTIONProperty, value); }
        }
        #endregion

        #region PROP_OBJECT_NAME 点名
        public static readonly DependencyProperty PROP_OBJECT_NAMEProperty = DependencyProperty.Register("PROP_OBJECT_NAME",
            typeof(string), typeof(BacProperty),
            new PropertyMetadata((sender, e) =>
            {
                var vm = sender as BacProperty;
                if (vm == null) return;
            }));
        public string PROP_OBJECT_NAME
        {
            get { return GetValue(PROP_OBJECT_NAMEProperty) as string; }
            set { SetValue(PROP_OBJECT_NAMEProperty, value); }
        }
        #endregion

        #region PROP_PRESENT_VALUE 值
        public static readonly DependencyProperty PROP_PRESENT_VALUEProperty = DependencyProperty.Register("PROP_PRESENT_VALUE",
            typeof(object), typeof(BacProperty),
            new PropertyMetadata((sender, e) =>
            {
                var vm = sender as BacProperty;
                if (vm == null) return;
            }));
        public object PROP_PRESENT_VALUE
        {
            get { return GetValue(PROP_PRESENT_VALUEProperty); }
            set { SetValue(PROP_PRESENT_VALUEProperty, value); }
        }
        #endregion
    }
}
