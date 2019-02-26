using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.BACnet;
using System.Collections.ObjectModel;

namespace BACnetScaner
{
    class Program
    {
        static BacnetClient Bacnet_client;
        static void Main(string[] args)
        {
            Bacnet_client = new BacnetClient(new BacnetIpUdpProtocolTransport(47808));
            Bacnet_client.OnIam += new BacnetClient.IamHandler(handler_OnIam);
            Bacnet_client.Start();
            Bacnet_client.WhoIs();
            Console.Read();
            Console.WriteLine("Begin Scan Device");

            foreach (var device in DevicesList)
            {
                var count = GetDeviceArrayIndexCount(device);
                ScanPointsBatch(device, count);
            }


            foreach (var device in DevicesList)
            {
                System.IO.File.WriteAllText($"{device.DeviceId}.json", Newtonsoft.Json.JsonConvert.SerializeObject(device));
            }



            Console.WriteLine("Begin Scan Properties");
            foreach (var device in DevicesList)
            {
                ScanSubProperties(device);
            }
            foreach (var device in DevicesList)
            {
                System.IO.File.WriteAllText($"{device.DeviceId}pppp.json", Newtonsoft.Json.JsonConvert.SerializeObject(device));
            }
            Console.WriteLine("Scan Finished");

        }
        public static List<BacDevice> DevicesList { get; set; }

        static void handler_OnIam(BacnetClient sender, BacnetAddress adr, uint deviceId, uint maxAPDU,
                                  BacnetSegmentations segmentation, ushort vendorId)
        {

            if (DevicesList == null) DevicesList = new List<BacDevice>();
            lock (DevicesList)
            {
                if (DevicesList.Any(x => x.DeviceId == deviceId)) return;
                int index = 0;
                for (; index < DevicesList.Count; index++)
                {
                    if (DevicesList[index].DeviceId > deviceId) break;
                }

                DevicesList.Insert(index, new BacDevice(adr, deviceId));
                Console.WriteLine(@"Detect Device: " + deviceId);
            }

        }
        static int ScanBatchStep = 50;
        //批量扫点,注意不要太多,超过maxAPDU失败
        public static void ScanPointsBatch(BacDevice device, uint count)
        {
            try
            {
                if (device == null) return;
                var pid = BacnetPropertyIds.PROP_OBJECT_LIST;
                var device_id = device.DeviceId;
                var bobj = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id);
                var adr = device.Address;
                if (adr == null) return;
                device.Properties = new List<BacProperty>();

                List<BacnetPropertyReference> rList = new List<BacnetPropertyReference>();

                for (uint i = 1; i < count; i++)
                {
                    rList.Add(new BacnetPropertyReference((uint)pid, i));
                    if (i % ScanBatchStep == 0 || i == count)//不要超了 MaxAPDU
                    {
                        IList<BacnetReadAccessResult> lstAccessRst;
                        var bRst = Bacnet_client.ReadPropertyMultipleRequest(adr, bobj, rList, out lstAccessRst, GetCurrentInvokeId());
                        if (bRst)
                        {
                            foreach (var aRst in lstAccessRst)
                            {
                                if (aRst.values == null) continue;
                                foreach (var bPValue in aRst.values)
                                {
                                    if (bPValue.value == null) continue;
                                    foreach (var bValue in bPValue.value)
                                    {
                                        var strBValue = "" + bValue.Value;
                                        Console.WriteLine(pid + " , " + strBValue + " , " + bValue.Tag);

                                        var strs = strBValue.Split(':');
                                        if (strs.Length < 2) continue;
                                        var strType = strs[0];
                                        var strObjId = strs[1];
                                        var subNode = new BacProperty();
                                        BacnetObjectTypes otype;
                                        Enum.TryParse(strType, out otype);
                                        if (otype == BacnetObjectTypes.OBJECT_NOTIFICATION_CLASS || otype == BacnetObjectTypes.OBJECT_DEVICE) continue;
                                        subNode.ObjectId = new BacnetObjectId(otype, Convert.ToUInt32(strObjId));
                                        device.Properties.Add(subNode);
                                    }
                                }
                            }
                        }
                        rList.Clear();
                    }
                }
            }
            catch (Exception  )
            {
            }
        }
        static byte InvokeId = 0x00;
        public static byte GetCurrentInvokeId()
        {
            InvokeId = (byte)((InvokeId + 1) % 256);
            return InvokeId;
        }
        //逐个扫点,速度较慢
        public static void ScanPointSingle(BacDevice device, uint count)
        {
            if (device == null) return;
            var pid = BacnetPropertyIds.PROP_OBJECT_LIST;
            var device_id = device.DeviceId;
            var bobj = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id);
            var adr = device.Address;
            if (adr == null) return;
            device.Properties = new List<BacProperty>();

            for (uint index = 1; index <= count; index++)
            {
                try
                {
                    var list = ReadScalarValue(adr, bobj, pid, GetCurrentInvokeId(), index);
                    if (list == null) continue;
                    foreach (var bValue in list)
                    {
                        var strBValue = "" + bValue.Value;
                        Console.WriteLine(pid + " , " + strBValue + " , " + bValue.Tag);
                        var strs = strBValue.Split(':');
                        if (strs.Length < 2) continue;
                        var strType = strs[0];
                        var strObjId = strs[1];
                        var subNode = new BacProperty();
                        BacnetObjectTypes otype;
                        Enum.TryParse(strType, out otype);
                        subNode.ObjectId = new BacnetObjectId(otype, Convert.ToUInt32(strObjId));
                        device.Properties.Add(subNode);
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine("Error: " + index + " , " + exp.Message);
                }
            }
        }
        public static void ScanSubProperties(BacDevice device)
        {
            var adr = device.Address;
            if (adr == null) return;
            if (device.Properties == null) return;
            foreach (BacProperty subNode in device.Properties)
            {
                try
                {
                    List<BacnetPropertyReference> rList = new List<BacnetPropertyReference>();
                    rList.Add(new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_DESCRIPTION, uint.MaxValue));
                    rList.Add(new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_REQUIRED, uint.MaxValue));
                    IList<BacnetReadAccessResult> lstAccessRst;
                    var bRst = Bacnet_client.ReadPropertyMultipleRequest(adr, subNode.ObjectId, rList, out lstAccessRst, GetCurrentInvokeId());
                    if (bRst)
                    {
                        foreach (var aRst in lstAccessRst)
                        {
                            if (aRst.values == null) continue;
                            foreach (var bPValue in aRst.values)
                            {
                                if (bPValue.value == null || bPValue.value.Count == 0) continue;
                                var pid = (BacnetPropertyIds)(bPValue.property.propertyIdentifier);
                                var bValue = bPValue.value.First();
                                var strBValue = "" + bValue.Value;
                                Console.WriteLine(pid + " , " + strBValue + " , " + bValue.Tag);
                                switch (pid)
                                {
                                    case BacnetPropertyIds.PROP_DESCRIPTION://描述
                                        {
                                            subNode.PROP_DESCRIPTION = bValue + "";
                                        }
                                        break;
                                    case BacnetPropertyIds.PROP_OBJECT_NAME://点名
                                        {
                                            subNode.PROP_OBJECT_NAME = bValue + "";
                                        }
                                        break;
                                    case BacnetPropertyIds.PROP_PRESENT_VALUE://值
                                        {
                                            subNode.PROP_PRESENT_VALUE = bValue.Value;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine("Error: " + exp.Message);
                }
            }
        }
        //获取子节点个数
        public static uint GetDeviceArrayIndexCount(BacDevice device)
        {
            try
            {
                var adr = device.Address;
                if (adr == null) return 0;
                var list = ReadScalarValue(adr,
                    new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device.DeviceId),
                    BacnetPropertyIds.PROP_OBJECT_LIST, 0, 0);
                var rst = Convert.ToUInt32(list.FirstOrDefault().Value);
                return rst;
            }
            catch
            { }
            return 0;
        }
        static IList<BacnetValue> ReadScalarValue(BacnetAddress adr, BacnetObjectId oid,
            BacnetPropertyIds pid, byte invokeId = 0, uint arrayIndex = uint.MaxValue)
        {
            try
            {
                IList<BacnetValue> NoScalarValue;
                var rst = Bacnet_client.ReadPropertyRequest(adr, oid, pid, out NoScalarValue, invokeId, arrayIndex);
                if (!rst) return null;
                return NoScalarValue;
            }
            catch { }
            return null;
        }
    }
}
