using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UDPUtil;
using BACnetCommonLib;
using System.IO;
using System.Xml;
using System.IO.BACnet;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

using System.Collections;
using System.Text.RegularExpressions;

namespace BACnetTranslator
{
    class Program
    {
        static BacnetClient Bacnet_client;
        static AsyncUdpClient Udp_Client;
        static List<BacDevice> DevicesList;
        //BACnet序号
        static int udpId = 0;

        static String LocalBacIP;
        static int LocalBacPort;

        //util.xml
        //common
        static String RunMode = "formal";// 运行模式,支持formal/debug，缺省为formal
        static String BreakPoint = "false";// 断点模式,是否启用断点存储模式
        static String StorageClass = "CSV";// 断点存储格式,与BreakPoint组合使用
        static String StoragePath = "D:";// 断点存储路径,预留
        static String PointsFrom = "EXCEL";// pointList来源
        static String IntervalType = "interval";// 整点：integral/ 间隔：interval
        static String CollectInterval = "20Second";// 采集周期,以Second/min为单位
        static int SendCount = 10;// 最大上传报文条数
        static String SendInterval = "1min";// 上传周期,以Second/min为单位
        static int HandleDataThread = 1;// 报文解析线程数
        static String BuildingQaurry = "manual";// 下发楼号来源
        static String BuildingControl = null;// 下发指定楼号,与BuildingQaurry组合使用
        static String WaitingTimeControl = "3Second";// 下发等待时间,以Second为单位
        static String NodeMac = "1";// 节点网关
        static String ReportVersion = "addtion";// 报文版本，配置为standard时发report版报文；配置为addtion时发reportaddtion版报文
        //upload
        static String LocalUdpIP;
        static int LocalUdpPort;
        static String RemoteUdpIP;
        static int RemoteUdpPort;
        static String protocol = "bacnetipv2";

        static Dictionary<String, Dictionary<String, List<PointSet>>> CollectorPointSetList = new Dictionary<String, Dictionary<String, List<PointSet>>>();// 存放下发的原始设定记录
        static Dictionary<String, Dictionary<String, Dictionary<String, String>>> CollectorPropertyValues = new Dictionary<String, Dictionary<String, Dictionary<String, String>>>();// 存储collector各属性信息
        static Dictionary<String, List<Collector>> ProtocolCollectorList = new Dictionary<String, List<Collector>>();// 存储各协议对应的采集列表
        static Dictionary<String, Dictionary<String, Point>> Collector_Device_Point = new Dictionary<String, Dictionary<String, Point>>();// 存储各采集设备对应的点位信息
        static Dictionary<String, String> Meter_Protocol = new Dictionary<String, String>();// 存储Meter与采集协议的对应
        static Dictionary<String, String> Meter_CollectorMAC = new Dictionary<String, String>();// 存储Meter与采集设备网关的对应
        static Dictionary<String, String> Meter_SubPath = new Dictionary<String, String>();// 定制，不维护
        static Dictionary<String, String> Meter_CollectorAddress = new Dictionary<String, String>();// 存储Meter与采集设备地址的对应
        static Dictionary<String, String> Meter_Site = new Dictionary<String, String>();// 存储Meter与地址（对接系统）的对应
        static Dictionary<String, String> Meter_Building = new Dictionary<String, String>();// 存储Meter与楼号的对应
        static Dictionary<String, String> Meter_FunctionGroup = new Dictionary<String, String>();// 存储Meter与功能号组的对应
        static Dictionary<String, Point> Meter_Point = new Dictionary<String, Point>();// 存储Meter与点位的对应
        static Dictionary<String, Point> Item_Point = new Dictionary<String, Point>();// 存储Item与点位的对应
        static Dictionary<String, Int32> RecordAddress = new Dictionary<String, Int32>();// 存储数据地址
        // static List<Record> RecordList = new ArrayList<Record>();// 存储数据列表
        // static Dictionary<String, List<Report>> ReportList = new HashMap<String, List<Report>>();// 存储报文列表
        // static Dictionary<String, List<Date>> Round_Static = new HashMap<String, List<Date>>();// 预留
        static Dictionary<String, Collector> Meter_Collector = new Dictionary<String, Collector>();// 存储Meter与采集设备的对应
        //static List<String> CollectorProtocolGroup = new ArrayList<String>();// 存储协议组列表
        static Dictionary<String, String> CollectorMAC_Meter = new Dictionary<String, String>();// 存储Meter与采集设备网关的对应
        static Dictionary<String, String> SimulationDevicesMap = new Dictionary<String, String>();// 存储模拟设备列表
        static Dictionary<String, String> PointTopic = new Dictionary<String, String>();// 存储点位与topic对应，mqtt协议用到
        static Dictionary<String, String> PointQueue = new Dictionary<String, String>();// 存储点位与queue对应，amqp协议队列模式用到
        static Dictionary<String, Dictionary<String, String[]>> PointExchange = new Dictionary<String, Dictionary<String, String[]>>();// 存储点位与exchange对应，amqp协议路由模式用到
        static Dictionary<String, String> exchangeType = new Dictionary<String, String>();// 存储路由类型，amqp协议路由模式用到
        static Dictionary<String, String> pointConvType = new Dictionary<String, String>();// 存储点位解析类型，modbus-tcp协议用到
        static Dictionary<String, String> pointSlave = new Dictionary<String, String>();// 存储点位SlaveID，modbus-tcp协议用到
        static Dictionary<String, String> pointAddress = new Dictionary<String, String>();// 存储点位Address，modbus-tcp协议用到


        static Dictionary<String, Dictionary<String, Dictionary<String, String>>> FunctionPropertyValues = new Dictionary<String, Dictionary<String, Dictionary<String, String>>>();// 存储功能号各属性信息
        static Dictionary<String, Dictionary<String, Dictionary<String, String>>> PointPropertyValues = new Dictionary<String, Dictionary<String, Dictionary<String, String>>>();// 存储Point各属性信息
        static Dictionary<String, Dictionary<String, String>> Function_Old_New = new Dictionary<String, Dictionary<String, String>>();// 存储新旧功能号对应
        static Dictionary<String, Dictionary<String, String>> Function_New_Old = new Dictionary<String, Dictionary<String, String>>();// 存储新旧功能号对应
        static Dictionary<String, Dictionary<String, String>> Function_New_Site = new Dictionary<String, Dictionary<String, String>>();// 存储功能号与地址（对接系统）对应
        static Dictionary<String, Dictionary<String, String>> Function_New_SubSite = new Dictionary<String, Dictionary<String, String>>();// 存储功能号与二级地址（对接系统）对应
        static Dictionary<String, Dictionary<String, String>> Function_Old_Type = new Dictionary<String, Dictionary<String, String>>();// 存储功能号与类型（对接系统）对应
        static Dictionary<String, Dictionary<String, String>> Function_New_Type = new Dictionary<String, Dictionary<String, String>>();// 存储功能号与类型（对接系统）对应
        static Dictionary<String, Dictionary<String, String>> Function_New_Reverse = new Dictionary<String, Dictionary<String, String>>();// 存储功能号与寄存器反转标记（对接系统）对应
        static Dictionary<String, Dictionary<String, String>> Function_Old_CONVType = new Dictionary<String, Dictionary<String, String>>();// 存储功能号与数据解析类型（对接系统）对应
        static Dictionary<String, Dictionary<String, String>> Function_Old_Size = new Dictionary<String, Dictionary<String, String>>();// 存储功能号与寄存器数量（对接系统）对应
        static Dictionary<String, Dictionary<String, String>> Function_Old_Radio = new Dictionary<String, Dictionary<String, String>>();// 存储功能号与数据换算系数对应
        static Dictionary<String, Dictionary<String, String>> DataConvert = new Dictionary<String, Dictionary<String, String>>();// 存储数据转换对应关系
        static Dictionary<String, Dictionary<String, String>> Function_New_DataConvert = new Dictionary<String, Dictionary<String, String>>();// 存储功能号及数据转换的对应关系
       
        static List<Record> RecordList = new List<Record>();// 存储数据列表
        static int GetCurrentUdpId()
        {
            udpId++;
            if (udpId >= 65536) udpId = 1;
            return udpId;
        }
        //BACnet序号
        static byte invokeId = 0;
        static byte GetCurrentInvokeId()
        {
            invokeId++;
            if (invokeId >= byte.MaxValue) invokeId = 1;
            return invokeId;
        }
        //心跳
        static int HeartBeatIndex = 0;
        static int GetHeartBeatIndex()
        {
            HeartBeatIndex++;
            if (HeartBeatIndex == int.MaxValue)
                HeartBeatIndex = 0;
            return HeartBeatIndex;
        }
        static void Main(string[] args)
        {
            Logger.Log("Begin Translate BACnet Datas");
            Init();
        }
        static void Init()
        {

            try

            {
                Settings.InitSettings();

                LoadSettingFromXML(Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "util.xml"));    
                LoadSettingFromExcel(Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "setting.xls"));          
                LoadPointsFromExcel(Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "pointlist.xls"), protocol);

                InitBacnet_client();
                InitUdp_Client();
                //等待扫描，尽量多等一会
                Thread.Sleep(30000);
                Task.Factory.StartNew(BeginScanBac);
                Task.Factory.StartNew(BeginHeartBeat);
            }
            catch (Exception exp)
            {
                Logger.Error("Init: ", exp);
            }
        }

        private static void LoadPointsFromExcel(string file, string protocol)
        {
            Dictionary<Int32, Dictionary<Int32, String>> sheetData = readExcel(Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "pointlist.xls"), protocol);
            excuteDataConvert(sheetData);
            excuteFunctions(sheetData);
            excutePoints(sheetData);
        }

        private static void excutePoints(Dictionary<int, Dictionary<int, string>> sheetData)
        {
           	int begin = 65535;
			int sbegin = 65535;
			int end = 65535;
			CollectorPropertyValues.Add(protocol, new Dictionary<String, Dictionary<String, String>>());
			CollectorPointSetList.Add(protocol, new Dictionary<String, List<PointSet>>());
			Dictionary<String, List<PointSet>> ps = new Dictionary<String, List<PointSet>>();
			List<Collector> CollectorList = new List<Collector>();
			Dictionary<String, Int32> collectorPty = new Dictionary<String, Int32>();
			Dictionary<Int32, String> collectorPtyValue = new Dictionary<Int32, String>();
			Dictionary<String, Int32> pointPty = new Dictionary<String, Int32>();
			Dictionary<Int32, String> pointPtyValue = new Dictionary<Int32, String>();
			Collector group = new Collector();
			String collectorName = "";
			for (int i = 0; i <= sheetData.Count; i++) {
                if (!sheetData.ContainsKey(i))
                {
                    continue;
                }
				Dictionary<Int32, String> rowData = sheetData[i];
				if (rowData.ContainsKey(0)&&"Collector".Equals(rowData[0])) {
					// Constant.info("Collector begin: " + i);
					begin = i;
					sbegin = 65535;
					for (int j = 1; j < rowData.Count; j++) {
                        if (collectorPty.ContainsKey(rowData[j]))
                        {
                            collectorPty[rowData[j]]=j;
                        }
                        else {
                            collectorPty.Add(rowData[j], j);
                        }
                        if (collectorPtyValue.ContainsKey(j))
                        {
                            collectorPtyValue[j]=rowData[j];
                        }
                        else
                        {
                            collectorPtyValue.Add(j, rowData[j]);
                        }					
					}
                }
                else if (rowData.ContainsKey(0) && "Point".Equals(rowData[0]))
                {
					// Constant.info("Point begin: " + i);
					sbegin = i;
					for (int j = 1; j < rowData.Count; j++) {
                        if (pointPty.ContainsKey(rowData[j]))
                        {
                            pointPty[rowData[j]]= j;
                        }
                        else 
                        {
                            pointPty.Add(rowData[j], j);
                        }
                        if (pointPtyValue.ContainsKey(j))
                        {
                            pointPtyValue[j]=rowData[j];
                        }
                        else 
                        {
                            pointPtyValue.Add(j, rowData[j]);
                        }						
					}
                }
                else if (rowData.ContainsKey(0) && (rowData[0].Length > 0) && !("Point".Equals(rowData[0])))
                {
					// Constant.info("Collector end: " + i);
					end = i;
					begin = 65535;
					sbegin = 65535;
					end = 65535;

					collectorPty = new Dictionary<String, Int32>();
					collectorPtyValue = new Dictionary<Int32, String>();
					pointPty = new Dictionary<String, Int32>();
					pointPtyValue = new Dictionary<Int32, String>();
					group = new Collector();
				} else if (i > sbegin && i < end) {
                    if (rowData.ContainsKey(1) && rowData[1].Length > 0)
                    {
						// Constant.info("Point add: " + i);
						Dictionary<String, Point> deviceList = new Dictionary<String, Point>();

						Point point = new Point();
						Meter_Protocol.Add(rowData[pointPty["MeterSign"]], protocol);
						Meter_CollectorMAC.Add(rowData[pointPty["MeterSign"]], group.MAC);
						Meter_Collector.Add(rowData[pointPty["MeterSign"]], group);
                        //CollectorMAC_Meter[group.MAC] = rowData[pointPty["MeterSign"]];
						//CollectorMAC_Meter.Add(group.MAC, rowData[pointPty["MeterSign"]]);
						Meter_Site.Add(rowData[pointPty["MeterSign"]],
								!pointPty.ContainsKey("Site") ? "0" : rowData[pointPty["Site"]]);
						Meter_SubPath.Add(rowData[pointPty["MeterSign"]],
								!pointPty.ContainsKey("SubPath") ? "null" : rowData[pointPty["SubPath"]]);
						Meter_Building.Add(rowData[pointPty["MeterSign"]],
								rowData[pointPty["BuildingSign"]]);
						Meter_FunctionGroup.Add(rowData[pointPty["MeterSign"]],
								rowData[pointPty["Functions_Group"]]);

						String deviceSite = "";                       
						if (pointPty.ContainsKey("MAC") && rowData[pointPty["MAC"]] != null
								&& rowData[pointPty["MAC"]] != null
								&& rowData[pointPty["MAC"]].ToString().Length > 0) {
							deviceSite = rowData[pointPty["MAC"]].ToString();
							Meter_CollectorMAC.Add(rowData[pointPty["MeterSign"]],
									rowData[pointPty["MAC"]].ToString());
							//CollectorMAC_Meter.Add(rowData[pointPty["MAC"]].ToString(),rowData[pointPty["MeterSign"]]);

						} else {
							deviceSite = (!pointPty.ContainsKey("BusNo") ? "0" : rowData[pointPty["BusNo"]])
									+ "-" + (!pointPty.ContainsKey("Site") ? "0" : rowData[pointPty["Site"]]);
							deviceSite = (!pointPty.ContainsKey("BusNo") || !pointPty.ContainsKey("Site"))
									? rowData[pointPty["MeterSign"]]: deviceSite;
						}
						point.Item = !pointPty.ContainsKey("Item") ? "null" : rowData[pointPty["Item"]];
						point.buildingSign = rowData[pointPty["BuildingSign"]];
						point.meterSign = rowData[pointPty["MeterSign"]];
						point.deviceId = !pointPty.ContainsKey("DeviceId") ? "null"
								: rowData[pointPty["DeviceId"]];

						point.type = !pointPty.ContainsKey("Type") ? "null" : rowData[pointPty["Type"]];
						point.instanceNumber = !pointPty.ContainsKey("InstanceNumber") ? "null"
								: rowData[pointPty["InstanceNumber"]];

						foreach  (String function in Function_New_Old[rowData[pointPty["Functions_Group"]]].Keys) 
                        {
							Function f = new Function();
							f.functionID = int.Parse(function);
							point.functionList.Add(f);

						}
						Meter_Point.Add(point.meterSign, point);
						//Item_Point.Add(point.Item, point);
                        
						deviceSite = group.IP + ":" + group.Port + "-" + point.deviceId + "-" + point.type + "-"+ point.instanceNumber;
						
						deviceList.Add(deviceSite, point);

						if (Collector_Device_Point.ContainsKey(group.IP))
                        {						
                            foreach (KeyValuePair<string, Point> kv in deviceList)
                            {
                                Collector_Device_Point[group.IP].Add(kv.Key,kv.Value);
                            }
						} else {
							Collector_Device_Point.Add(group.IP, deviceList);
						}
						PointPropertyValues[collectorName].Add(point.buildingSign + "-" + point.meterSign,
								new Dictionary<String, String>());
						for (int j = 1; j < rowData.Count; j++) {
							PointPropertyValues[collectorName]
									[point.buildingSign + "-" + point.meterSign]
									.Add(pointPtyValue[j], rowData[j]);
						}
					}
				} else if (i > begin && i < end) {
                    if (rowData.ContainsKey(1) && rowData[1].Length > 0)
                    {
						// Constant.info("Collector add: " + i);
						Collector collector = new Collector();
						collector.IP = !collectorPty.ContainsKey("IP") ? "null" : rowData[(collectorPty[("IP")])];
						collector.Port = int.Parse(
								!collectorPty.ContainsKey("Port") ? "0" : rowData[(collectorPty[("Port")])]);
						collector.User = !collectorPty.ContainsKey("User") ? "null"
								: rowData[(collectorPty[("User")])];
						collector.Password = !collectorPty.ContainsKey("Password") ? "null"
								: rowData[(collectorPty[("Password")])];
						collector.ProgId = !collectorPty.ContainsKey("ProgId") ? "null"
								: rowData[(collectorPty[("ProgId")])];
						collector.BroadcastAddress = !collectorPty.ContainsKey("BroadcastAddress") ? "null"
								: rowData[(collectorPty[("BroadcastAddress")])];
						collector.MAC = !collectorPty.ContainsKey("MAC") ? "null"
								: rowData[(collectorPty[("MAC")])];
						collector.PROTOCOL = !collectorPty.ContainsKey("PROTOCOL") ? "UDP"
								: rowData[(collectorPty[("PROTOCOL")])];
						collector.Mode = !collectorPty.ContainsKey("Mode") ? "mina"
								: rowData[(collectorPty[("Mode")])];
						collector.Max_size = int.Parse(!collectorPty.ContainsKey("Max_size") ? "1000"
								: rowData[(collectorPty[("Max_size")])]);
						collector.SqlContent = !collectorPty.ContainsKey("SqlContent") ? "null"
								: rowData[(collectorPty[("SqlContent")])];
						group = collector;
						if (collectorPty.ContainsKey("IP")) {
							collectorName = rowData[(collectorPty[("IP")])];
						} else if (collectorPty.ContainsKey("url")) {
							collectorName = rowData[(collectorPty[("url")])];
						} else if (collectorPty.ContainsKey("MAC")) {
							collectorName = rowData[(collectorPty[("MAC")])];
						}

						// collectorName = collectorPty.containsKey("IP") ?
						// rowData.get(collectorPty.get("IP"))
						// : rowData.get(collectorPty.get("url"));

						collector.Name = collectorName;
						CollectorList.Add(collector);
                        if (CollectorPropertyValues[(protocol)].ContainsKey(collectorName))
                        {
                            CollectorPropertyValues[(protocol)][collectorName]=new Dictionary<String, String>();
                        }
                        else 
                        {
                            CollectorPropertyValues[(protocol)].Add(collectorName, new Dictionary<String, String>());
                        }
                        if (PointPropertyValues.ContainsKey(collectorName))
                        {
                            PointPropertyValues[collectorName]=new Dictionary<String, Dictionary<String, String>>();
                        }
                        else
                        {
                            PointPropertyValues.Add(collectorName, new Dictionary<String, Dictionary<String, String>>());
                        }
                        if (ps.ContainsKey(collector.IP))
                        {
                            ps[collector.IP]= new List<PointSet>();
                        }else{
                            ps.Add(collector.IP, new List<PointSet>());
                        }
						
						for (int j = 1; j < rowData.Count; j++) {
							CollectorPropertyValues[(protocol)][(collectorName)]
									.Add(collectorPtyValue[j], rowData[j]);
						}
					}
				}
			}
            CollectorPointSetList[protocol]= ps;
            ProtocolCollectorList[protocol]= CollectorList;

			//CollectorPointSetList.Add(protocol, ps);
			//ProtocolCollectorList.Add(protocol, CollectorList);
        }

        private static void excuteFunctions(Dictionary<int, Dictionary<int, string>> sheetData)
        {
            int begin = 65535;
            int sbegin = 65535;
            int end = 65535;
            Dictionary<String, Int32> functionPty = new Dictionary<String, Int32>();
            Dictionary<Int32, String> functionPtyValue = new Dictionary<Int32, String>();
            String group = "";

            for (int i = 0; i <= sheetData.Count; i++)
            {
                if (!sheetData.ContainsKey(i))
                {
                    continue;
                }
                Dictionary<Int32, String> rowData = sheetData[i];
                if (rowData.ContainsKey(0) && "Functions".Equals(rowData[0]))
                {
                    begin = i;
                    sbegin = 65535;
                    // Logger.Log("Functions begin: " + i);
                }
                else if (rowData.ContainsKey(0) && "Function".Equals(rowData[0]))
                {
                    sbegin = i;
                    // Logger.Log("Function begin: " + i);
                    for (int j = 1; j < rowData.Count; j++)
                    {
                        functionPty.Add(rowData[j], j);
                        functionPtyValue.Add(j, rowData[j]);
                    }
                }
                else if (rowData.ContainsKey(0) && (rowData[0].Length > 0) && !("Function".Equals(rowData[0])))
                {
                    end = i;
                    begin = 65535;
                    sbegin = 65535;
                    end = 65535;
                    functionPty = new Dictionary<String, Int32>();
                    functionPtyValue = new Dictionary<Int32, String>();
                    group = "";
                    // Logger.Log("Functions end: " + i);
                }
                else if (i > sbegin && i < end)
                {
                    if (rowData.ContainsKey(1) && rowData[1].Length > 0)
                    {
                        // Logger.Log("Function add: " + i);
                        String function_new = rowData[functionPty["new"]];
                        String function_old = rowData[functionPty["old"]];
                        // function
                        String function = function_new;
                        // DataConvert_Group
                        String dataConvert_Group = !functionPty.ContainsKey("DataConvert_Group") ? "null"
                                : rowData[functionPty["DataConvert_Group"]];

                        Function_Old_New[group].Add(function_old, function_new);
                        Function_New_Old[group].Add(function_new, function_old);

                        
                        if (!DataConvert.ContainsKey(dataConvert_Group) )
                        {
                            Function_New_DataConvert.Add(function_new,null);
                        }else
                        {
                            Function_New_DataConvert.Add(function_new, DataConvert[dataConvert_Group]);
                        }

                        Function_New_Site[group].Add(function_new, !functionPty.ContainsKey("Site") ? "0"
                                : rowData[functionPty["Site"]]);
                        Function_New_SubSite[group].Add(function_new, !functionPty.ContainsKey("subSite")
                                ? "0" : rowData[functionPty["subSite"]]);
                        Function_Old_Type[group].Add(function_old,
                                !functionPty.ContainsKey("type") ? "double" : rowData[functionPty["type"]]);
                        Function_New_Type[group].Add(function_new,
                                !functionPty.ContainsKey("type") ? "double" : rowData[functionPty["type"]]);
                        Function_New_Reverse[group].Add(function_new,
                                !functionPty.ContainsKey("reverse") ? "false" : rowData[functionPty["reverse"]]);
                        Function_Old_CONVType[group].Add(function_old,
                                !functionPty.ContainsKey("convtype") ? "0" : rowData[functionPty["convtype"]]);
                        Function_Old_Size[group].Add(function_old,
                                !functionPty.ContainsKey("size") ? "1" : rowData[functionPty["size"]]);
                        Function_Old_Radio[group].Add(function_old,
                                !functionPty.ContainsKey("radio") ? "1" : rowData[functionPty["radio"]]);
                        FunctionPropertyValues[group].Add(function, new Dictionary<String, String>());
                        for (int j = 1; j < rowData.Count; j++)
                        {
                            FunctionPropertyValues[group][function].Add(functionPtyValue[j],
                                    rowData[j]);
                        }

                    }
                }
                else if (i > begin && i < end)
                {
                    if (rowData.ContainsKey(1) && rowData[1].Length > 0)
                    {
                        // Logger.Log("Functions add: " + i);
                        group = rowData[1];
                        Function_Old_New.Add(group, new Dictionary<String, String>());
                        Function_New_Old.Add(group, new Dictionary<String, String>());
                        Function_New_Site.Add(group, new Dictionary<String, String>());
                        Function_New_SubSite.Add(group, new Dictionary<String, String>());
                        Function_Old_Type.Add(group, new Dictionary<String, String>());
                        Function_New_Type.Add(group, new Dictionary<String, String>());
                        Function_New_Reverse.Add(group, new Dictionary<String, String>());
                        Function_Old_CONVType.Add(group, new Dictionary<String, String>());
                        Function_Old_Size.Add(group, new Dictionary<String, String>());
                        Function_Old_Radio.Add(group, new Dictionary<String, String>());
                        FunctionPropertyValues.Add(group, new Dictionary<String, Dictionary<String, String>>());
                    }
                }
            }
        }

        private static void excuteDataConvert(Dictionary<Int32, Dictionary<Int32, String>> sheetData)
        {
            int begin = 65535;
            int sbegin = 65535;
            int end = 65535;
            String group = "";

            for (int i = 0; i <= sheetData.Count; i++)
            {
                if (!sheetData.ContainsKey(i))
                {
                    continue;
                }
                Dictionary<Int32, String> rowData = sheetData[i];
                if (!rowData.ContainsKey(0))
                {
                    continue;
                }
                if ("DataConvert".Equals(rowData[0]))
                {
                    begin = i;
                    sbegin = 65535;
                    // Logger.Log("DataConvert begin: " + i);
                }
                else if ("Convert".Equals(rowData[0]))
                {
                    sbegin = i;
                    // Logger.Log("Convert begin: " + i);
                }
                else if ((rowData[0].Length > 0) && !("Convert".Equals(rowData[0])))
                {
                    begin = 65535;
                    sbegin = 65535;
                    end = 65535;
                    group = "";
                    // Logger.Log("DataConvert end: " + i);
                }
                else if (i > sbegin && i < end)
                {
                    if (!rowData.ContainsKey(1))
                    {
                        continue;
                    }
                    if (rowData[1].Length > 0)
                    {
                        DataConvert[group].Add(rowData[1], rowData[2]);
                        // Logger.Log("Convert add: " + i);
                    }
                }
                else if (i > begin && i < end)
                {
                    if (!rowData.ContainsKey(1))
                    {
                        continue;
                    }
                    if (rowData[1].Length > 0)
                    {
                        DataConvert.Add(rowData[1], new Dictionary<String, String>());
                        group = rowData[1];
                        // Logger.Log("DataConvert add: " + i);
                    }
                }


            }
        }


        private static Dictionary<Int32, Dictionary<Int32, String>> readExcel(string file, string sheetName)
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
                        Logger.Log("Received:" + sheet.SheetName);
                        if (sheet.FirstRowNum < 0 || sheet.FirstRowNum >= sheet.LastRowNum) continue;
                        for (var rowNum = sheet.FirstRowNum ; rowNum <= sheet.LastRowNum; rowNum++)
                        {
                            try 
                            {
                                var rowContent = sheet.GetRow(rowNum);
                                result.Add(rowNum, new Dictionary<Int32, String>());
                                if (rowContent.FirstCellNum < 0 || rowContent.FirstCellNum >= rowContent.LastCellNum) continue;
                                for (var colNum = rowContent.FirstCellNum; colNum <= rowContent.LastCellNum; colNum++)
                                {
                                    try
                                    {
                                        DataFormatter formatter = new DataFormatter();
                                        String cellContent = formatter.FormatCellValue(sheet.GetRow(rowNum).GetCell(colNum));    
                                        //var cellContent = rowContent.GetCell(colNum);                                       
                                        result[rowNum].Add(colNum, cellContent);
                                        Logger.Log("(" + rowNum + "," + colNum + ")=" + cellContent);
                                    }
                                    catch (Exception exp)
                                    {
                                    }

                                }
                            }
                            catch (Exception exp)
                            {
                            }
                     
                        }
                    }
                }
            }
            return result;
        }

        private static void LoadSettingFromXML(string file)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(file);
            XmlNodeList nodeList;
            //RunMode
            nodeList = xml.SelectNodes("/root/RunMode");
            foreach (XmlNode item in nodeList)
            {
                RunMode = item.InnerXml;
            }
            //BreakPoint
            nodeList = xml.SelectNodes("/root/BreakPoint");
            foreach (XmlNode item in nodeList)
            {
                BreakPoint = item.InnerXml;
            }
            //StorageClass
            nodeList = xml.SelectNodes("/root/StorageClass");
            foreach (XmlNode item in nodeList)
            {
                StorageClass = item.InnerXml;
            }
            //StoragePath
            nodeList = xml.SelectNodes("/root/StoragePath");
            foreach (XmlNode item in nodeList)
            {
                StoragePath = item.InnerXml;
            }
            //PointsFrom
            nodeList = xml.SelectNodes("/root/PointsFrom");
            foreach (XmlNode item in nodeList)
            {
                PointsFrom = item.InnerXml;
            }
            //IntervalType
            nodeList = xml.SelectNodes("/root/IntervalType");
            foreach (XmlNode item in nodeList)
            {
                IntervalType = item.InnerXml;
            }
            //CollectInterval
            nodeList = xml.SelectNodes("/root/CollectInterval");
            foreach (XmlNode item in nodeList)
            {
                CollectInterval = item.InnerXml;
            }
            //SendCount
            nodeList = xml.SelectNodes("/root/SendCount");
            foreach (XmlNode item in nodeList)
            {
                SendCount =int.Parse(item.InnerXml) ;
            }
            //SendInterval
            nodeList = xml.SelectNodes("/root/SendInterval");
            foreach (XmlNode item in nodeList)
            {
                SendInterval =item.InnerXml ;
            }
            //HandleDataThread
            nodeList = xml.SelectNodes("/root/HandleDataThread");
            foreach (XmlNode item in nodeList)
            {
                HandleDataThread =int.Parse(item.InnerXml) ;
            }
            //BuildingQaurry
            nodeList = xml.SelectNodes("/root/BuildingQaurry");
            foreach (XmlNode item in nodeList)
            {
                BuildingQaurry =item.InnerXml ;
            }
            //BuildingControl
            nodeList = xml.SelectNodes("/root/BuildingControl");
            foreach (XmlNode item in nodeList)
            {
                BuildingControl =item.InnerXml ;
            }
            //WaitingTimeControl
            nodeList = xml.SelectNodes("/root/WaitingTimeControl");
            foreach (XmlNode item in nodeList)
            {
                WaitingTimeControl =item.InnerXml ;
            }
            //NodeMac
            nodeList = xml.SelectNodes("/root/NodeMac");
            foreach (XmlNode item in nodeList)
            {
                NodeMac =item.InnerXml ;
            }
            //ReportVersion
            nodeList = xml.SelectNodes("/root/ReportVersion");
            foreach (XmlNode item in nodeList)
            {
                ReportVersion =item.InnerXml ;
            }
            //UploadList
            nodeList = xml.SelectNodes("/root/UploadList/Upload");
            foreach (XmlNode item in nodeList)
            {
                XmlNodeList nodes ;
                nodes = item.SelectNodes("IP");
                foreach (XmlNode n in nodes)
                {
                    LocalUdpIP = n.InnerXml;
                }
                nodes = item.SelectNodes("Port");
                foreach (XmlNode n in nodes)
                {
                    LocalUdpPort =int.Parse(n.InnerXml) ;
                }
                nodes = item.SelectNodes("ServerIP");
                foreach (XmlNode n in nodes)
                {
                    RemoteUdpIP =n.InnerXml ;
                }
                nodes = item.SelectNodes("ServerPort");
                foreach (XmlNode n in nodes)
                {
                    RemoteUdpPort =int.Parse(n.InnerXml) ;
                }
            }
        } 

        static void BeginHeartBeat()
        {
            var heartBeatInterval = Settings.HeartBeatInterval;
            while (true)
            {
                StringBuilder sb = new StringBuilder();
                Thread.Sleep(heartBeatInterval);
                foreach(string building in Meter_Building.Values){
                    sb.AppendLine(SendUdpConn(building, GetCurrentUdpId()));
                    break;
                }
                Thread.Sleep(1000);
                sb = new StringBuilder();
                 foreach(string building in Meter_Building.Values){
                    sb.AppendLine(SendUdpHeart(building, GetCurrentUdpId()));
                    break;
                }
               
                
                Logger.Log(" ^ Heart Beat ^ " + GetHeartBeatIndex());
            }
        }
        static void InitBacnet_client()
        {
            try
            {
               // Bacnet_client = new BacnetClient(new BacnetIpUdpProtocolTransport(Settings.LocalBacPort, false));
                Bacnet_client = new BacnetClient(new BacnetIpUdpProtocolTransport(LocalBacPort, false, false, 1472, LocalBacIP));
                Bacnet_client.OnIam -= new BacnetClient.IamHandler(handler_OnIam);
                Bacnet_client.OnIam += new BacnetClient.IamHandler(handler_OnIam);
                Bacnet_client.Start();
                Bacnet_client.WhoIs();
            }
            catch (Exception exp)
            {
                Logger.Error("InitBacnet_client: ", exp);
            }
        }
        static void InitUdp_Client()
        {
            try
            {
                //本机节点
                // var LocalUdpPort = Settings.LocalUdpPort;
                IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(LocalUdpIP), LocalUdpPort);
                // 远程节点
                //var RemoteUdpIP = IPAddress.Parse(Settings.RemoteUdpIP);
                //var RemoteUdpPort = Settings.RemoteUdpPort;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(RemoteUdpIP), RemoteUdpPort);
                Udp_Client = new AsyncUdpClient(localEP, remoteEP, false, new NullLogger());
                //Receive
                new Thread(new ThreadStart(() =>
                {
                    while (true)
                    {
                        var str = Udp_Client.PopReceive();
                        if (!string.IsNullOrEmpty(str))
                        {
                            StringBuilder sb = new StringBuilder();
                            Logger.Log("Receive UdpCallback:" + str);
                            string[] rList = Regex.Split(System.Convert.ToString(str), ";", RegexOptions.IgnoreCase);
                            if ("pointcount".Equals(rList[2]))
                            {
                                sb = pointcount(rList);
                            }
                            else if ("pointlist".Equals(rList[2]))
                            {
                                sb = pointlist(rList);
                            }
                            else if ("senddownread".Equals(rList[2]))
                            {
                                sb = senddownread(rList);
                            }
                            else if ("senddownset".Equals(rList[2]))
                            {
                                sb = senddownset(rList);
                                
                                //BacnetObjectId oid = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 0);
                                //BacnetAddress adr;
                                //BacnetPropertyIds propertyId = BacnetPropertyIds.PROP_PRESENT_VALUE;
                                //BacnetValue[] NoScalarValue = {new BacnetValue(Convert.ToSingle(1))};
                               
                                //byte invokeId = 0;
                                //foreach (BacDevice device in DevicesList)
                                //{
                                //    bool result = Bacnet_client.WritePropertyRequest(device.Address, oid, propertyId, NoScalarValue, invokeId);
                                //}                             
                            }
                            else if ("reportack".Equals(rList[2]) || "reportaddtionack".Equals(rList[2]))
                            {
                            }
                            else if ("connectack".Equals(rList[2]) || "heartack".Equals(rList[2]))
                            {
                            }
                            else
                            {
                            }
                            if(sb.Length>0){
                                Udp_Client.PushSend(sb.ToString());
                            }
                           
                        }
                        Thread.Sleep(100);
                    }
                })).Start();
                Udp_Client.Start();
            }
            catch (Exception exp)
            {
                Logger.Error("InitUdp_Client: ", exp);
            }
        }

        private static StringBuilder senddownset(string[] rList)
        {
            //1101070037;1;senddownset;;123;1001;11;3.1
            StringBuilder sb = new StringBuilder();
            string buildingSign = rList[0];
            string meterSign = rList[5];
            string funcID = rList[6];
            string dataSet = rList[7];

            Point point = Meter_Point[meterSign];
            bool unanimously = false;
            if (!RecordAddress.ContainsKey(buildingSign + "-"+ meterSign + "-" + funcID))
            {
                return null;
            }

            int address = RecordAddress[buildingSign + "-"+ meterSign + "-" + funcID];
            if (double.Parse(RecordList[address].data).Equals(double.Parse(dataSet)))
            {
                unanimously = true;
            }        
            {
                if (unanimously)
                {
                    Logger.Log("Unanimously!" + " Buildingsign:"
                                    + buildingSign + ", Meter:"
                                    + meterSign + ", FunctionID:"
                                    + funcID + ", Data:"
                                    + RecordList[address].receivetime + " "
                                    + RecordList[address].data + ", DataSet:" + dataSet);
                    sb.AppendWithSplit(rList[0]);
                    sb.AppendWithSplit(rList[1]);
                    sb.AppendWithSplit("senddownsetack");
                    sb.AppendWithSplit((DateTime.Now.ToString(@"yyyyMMddHHmmss")));
                    sb.AppendWithSplit(rList[4]);
                    sb.AppendWithSplit(rList[5]);
                    sb.AppendWithSplit(rList[6]);
                    sb.AppendWithSplit("success");
                    //1101070037;1;senddownsetack;20010203040506;123;1001;11;success
                }
                else
                {
                    Collector c = Meter_Collector[meterSign];
                    if (c != null)
                    {
                                bool result = false;
                                BacnetObjectId oid = new BacnetObjectId(getObjectType(point.type), uint.Parse(point.instanceNumber));
                                //BacnetAddress adr;
                                BacnetPropertyIds propertyId = BacnetPropertyIds.PROP_PRESENT_VALUE;
                                BacnetValue[] NoScalarValue = {new BacnetValue(Convert.ToSingle(double.Parse(dataSet)))};
                               
                                byte invokeId = 0;
                                foreach (BacDevice device in DevicesList)
                                {
                                    if (Convert.ToString(device.Address).Equals(c.IP + ":" + c.Port))
                                    {
                                        if (uint.Parse(Convert.ToString(device.DeviceId)).Equals(uint.Parse(point.deviceId)))
                                        {
                                            result = Bacnet_client.WritePropertyRequest(device.Address, oid, propertyId, NoScalarValue, invokeId);
                                            break;
                                        }
                                    }
                                }
                                if (result)
                                {
                                    sb.AppendWithSplit(rList[0]);
                                    sb.AppendWithSplit(rList[1]);
                                    sb.AppendWithSplit("senddownsetack");
                                    sb.AppendWithSplit((DateTime.Now.ToString(@"yyyyMMddHHmmss")));
                                    sb.AppendWithSplit(rList[4]);
                                    sb.AppendWithSplit(rList[5]);
                                    sb.AppendWithSplit(rList[6]);
                                    sb.AppendWithSplit("success");
                                }else
                                {
                                    sb.AppendWithSplit(rList[0]);
                                    sb.AppendWithSplit(rList[1]);
                                    sb.AppendWithSplit("senddownsetack");
                                    sb.AppendWithSplit((DateTime.Now.ToString(@"yyyyMMddHHmmss")));
                                    sb.AppendWithSplit(rList[4]);
                                    sb.AppendWithSplit(rList[5]);
                                    sb.AppendWithSplit(rList[6]);
                                    sb.AppendWithSplit("fail:error");
                                }
                    }
                }  
                }
            return sb;
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
        private static StringBuilder senddownread(string[] list)
        {
            // TODO Auto-generated method stub
            StringBuilder sb = new StringBuilder();
            Record record = new Record();
            record = getCacheData(list);
            sb.Append(list[0]).Append(";").Append(list[1]).Append(";").Append("senddownreadack").Append(";")
                    .Append(record.receivetime).Append(";").Append(list[4]).Append(";")
                    .Append(record.meterSign).Append(";").Append(record.funcID).Append(";").Append(record.data).Append(";");
            return sb;
        }
        private static Record getCacheData(String[] list) {
		// TODO Auto-generated method stub
		Record record = new Record();
		if (RecordAddress.ContainsKey(list[0] + "-" + list[5] + "-" + list[6]) ) {
			int address = RecordAddress[(list[0] + "-" + list[5] + "-" + list[6])];
			record = RecordList[(address)];
			// Constant.info("get pointread by address !");
		} else {
			foreach (Record r in RecordList) {
                if (list[0].Equals(r.buildingSign) && list[5].Equals(r.meterSign)
                        && int.Parse(list[6]).Equals(r.funcID))
                {
					record = r;
					// Constant.info("get pointread by for circle !");
					break;
				}
			}
		}
		return record;
	}
        private static StringBuilder pointlist(String[] list) {
		// TODO Auto-generated method stub
		int no = 0;
		int count = 0;
		int from = int.Parse(list[5]) * (int.Parse(list[7]) - 1);
		int to =  int.Parse(list[5]) * ( int.Parse(list[7]));
		StringBuilder sb = new StringBuilder();
		sb.Append(list[0]).Append(";").Append(list[1]).Append(";").Append("pointlistack").Append(";;").Append(list[4])
				.Append(";").Append(list[5]).Append(";").Append(list[6]).Append(";").Append(list[7]).Append(";");
		if (to > getCount(list[0])) {
			count = getCount(list[0]) - from;
		} else {
			count = to - from;
		}
		sb.Append(count).Append(";");
		foreach (var deviceList in Collector_Device_Point) {
			foreach (var device in deviceList.Value) {
				if (list[0].Equals(device.Value.buildingSign)) {
					foreach (Function function in device.Value.functionList) {
						no++;
						if (no < from) {
							continue;
						}
						if (no > to) {
							continue;
						}
						sb.Append(device.Value.meterSign).Append(";").Append(function.functionID).Append(";");
					}
				}
			}
		}
		return sb;
	}

        private static StringBuilder pointcount(String[] list)
        {
            // TODO Auto-generated method stub
            StringBuilder sb = new StringBuilder();
            sb.Append(list[0]).Append(";").Append(list[1]).Append(";").Append("pointcountack").Append(";;").Append(list[4])
                    .Append(";").Append(getCount(list[0])).Append(";");
            return sb;
        }

        private static int getCount(String buildingSign)
        {
		// TODO Auto-generated method stub
		int count = 0;
		foreach (var deviceList in Collector_Device_Point) {
			foreach (var device in deviceList.Value) {
				if (buildingSign.Equals(device.Value.buildingSign)) {
					foreach (Function function in device.Value.functionList) {
						count++;
					}
				}
			}
		}
		return count;
	}
        //发现设备
        static bool IsDetecteDeviceFinished = false;
        static void handler_OnIam(BacnetClient sender, BacnetAddress adr, uint deviceId, uint maxAPDU,
                                    BacnetSegmentations segmentation, ushort vendorId)
        {
            try
            {
                if (IsDetecteDeviceFinished) return;
                if (DevicesList == null) DevicesList = new List<BacDevice>();
                lock (DevicesList)
                {
                    if (DevicesList.Any(x => x.DeviceId == deviceId)) return;
                    var device = new BacDevice(adr, deviceId);
                    string ip = Regex.Split(System.Convert.ToString(device.Address), ":", RegexOptions.IgnoreCase)[0];
                    device.LoadProperties(device.DeviceId, Collector_Device_Point[Convert.ToString(ip)]);
                    //device.LoadPropertiesFromExcel(Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\" + Constants.AimExcelDir, "" + deviceId + ".xls"));
                    DevicesList.Add(device);
                    Logger.Log(@"Detect Device: " + deviceId);
                }
            }
            catch (Exception exp)
            {
                Logger.Error("handler_OnIam", exp);
            }
        }
         static void LoadSettingFromExcel(string file)
        {
            try
            {
                if (!File.Exists(file)) return;
         
                using (FileStream fp = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var book = new HSSFWorkbook(fp);

                        var sheet = book.GetSheetAt(0);
                            var rowContent = sheet.GetRow(1);
                             LocalBacIP=rowContent.GetCell(0).ToString();
                             LocalBacPort= Convert.ToInt32(rowContent.GetCell(1).ToString());
                             LocalUdpIP  =rowContent.GetCell(2).ToString();
                             LocalUdpPort=Convert.ToInt32(rowContent.GetCell(3).ToString());
                             RemoteUdpIP= rowContent.GetCell(4).ToString();
                             RemoteUdpPort = Convert.ToInt32(rowContent.GetCell(5).ToString());
                                                     
                }
            }
            catch (Exception exp)
            {
                Logger.Error("BacDevice.LoadPropertiesFromExcel: ", exp);
            }
        }
        //扫点
        static void BeginScanBac()
        {
            try
            {
                Logger.Log("BeginScanBac");
                InitBacnet_client();
                Thread.Sleep(Settings.WaitTime);
                IsDetecteDeviceFinished = true;
                if (DevicesList == null) return;
                DevicesList = DevicesList.Where(x => x != null).
                    OrderBy(x => x.DeviceId).ToList();
                while (true)
                {
                    foreach (var device in DevicesList)
                    {
                        ScanBac(device);

                    }
                    try
                    {
                        Thread.Sleep(1000 * 30);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
            catch (Exception exp)
            {
                Logger.Error("BeginScanBac", exp);
            }
        }
        //扫点轮次
        static int ScanIndex = 0;
        static void ScanBac(BacDevice device)
        {
            if (device == null || device.Address == null) return;
            StringBuilder sb = new StringBuilder();
            try
            {
                Logger.Log(" @ " + device.Address + ":" + device.DeviceId + " Scan " 
                    //+ device.ScanIndex
                    );

                var adr = device.Address;
                var deviceid = device.DeviceId;
                List<BacnetPropertyReference> rList = new List<BacnetPropertyReference>();
                rList.Add(new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_PRESENT_VALUE, uint.MaxValue));

                var group = device.Properties.Partition(Settings.BacSplitSize);
                foreach (var subGroup in group)
                {                   
                    List<BacnetReadAccessSpecification> properties =
                        subGroup.Select(pro => new BacnetReadAccessSpecification(pro.ObjectId, rList)).ToList();
                    IList<BacnetReadAccessResult> lstAccessRst;
                    var bRst = Bacnet_client.ReadPropertyMultipleRequest(adr, properties, out lstAccessRst, GetCurrentInvokeId());
                    
                    if (!bRst) continue;
                    if (lstAccessRst == null || lstAccessRst.Count == 0) continue;
                    //sb.AppendLine("=== properties.Count:" + properties.Count + " ,lstAccessRst.Count: " + lstAccessRst.Count);
                    foreach (var aRst in lstAccessRst)
                    {
                        var bPValue = aRst.values.First();
                        if (bPValue.value == null || bPValue.value.Count == 0) continue;
                        var bValue = bPValue.value.First();
                        var strBValue = "" + bValue.Value;
                        Logger.Log(" @ " + device.Address + "-" + device.DeviceId + "-" + getType(aRst.objectIdentifier.type) + "-" + aRst.objectIdentifier.instance + " " + strBValue);
 
                        //string addr = System.Convert.ToString(device.Address);
                        //string[] sArray = Regex.Split(addr, ":", RegexOptions.IgnoreCase);
                        string ip= Regex.Split(System.Convert.ToString(device.Address), ":", RegexOptions.IgnoreCase)[0];
                        Point point= Collector_Device_Point[ip][device.Address + "-" + device.DeviceId + "-" + getType(aRst.objectIdentifier.type) + "-" + aRst.objectIdentifier.instance];
                        var pro = device.Properties.FirstOrDefault(x => x.ObjectId == aRst.objectIdentifier);
                        if (pro == null) continue;
                       ////值没变,忽略
                       // if (string.Equals(strBValue, pro.PROP_PRESENT_VALUE))
                       // {
                            //sb.AppendLine($"值没变,忽略 {pro.PROP_DESCRIPTION} : { strBValue}");
                       //     continue;
                       // }
                        var preValue = pro.PROP_PRESENT_VALUE;
                        pro.PROP_PRESENT_VALUE = strBValue;
                       
                        //因为霍尼韦尔的误报,消防报警后面跟的故障报警暂时不报(临时)
                        if (string.Equals(strBValue, "3", StringComparison.CurrentCultureIgnoreCase) &&
                           string.Equals(preValue, "2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //sb.AppendLine("因为霍尼韦尔的误报,消防报警后面跟的故障报警暂时不报(临时)");
                            continue;
                        }

                        //if (!Settings.IgnoreFirstValue || device.ScanIndex != 0)//初始值也转发
                       // {
                            //sb.AppendLine(SendUdpMessage(pro, GetCurrentUdpId()));
                        if(preValue!=null&&preValue.Length>0)
                        {
                        sb.AppendLine(SendUdpMessage(point, preValue, GetCurrentUdpId()));
                        }

                        //}
                        //sb.AppendLine("" + pro.PROP_OBJECT_NAME + " , " + strBValue + " ; ");
                    }
                }

            }
            catch (Exception exp)
            {
                Logger.Error("ScanBac Error: {device.Address},{device.DeviceId}", exp);
            }
            finally
            {
                Logger.Log(sb.ToString());
                //扫描完一轮
                ScanIndex++;
                if (ScanIndex == int.MaxValue) ScanIndex = 0;
                //device.ScanIndex = ScanIndex;
            }
        }

        static String getType(BacnetObjectTypes objectType)
        {
            if (objectType.Equals(BacnetObjectTypes.OBJECT_ANALOG_INPUT))
            {
                return "analogInput";
            }
            else if (objectType.Equals(BacnetObjectTypes.OBJECT_ANALOG_OUTPUT))
            {
                return "analogOutput";
            }
            else if (objectType.Equals(BacnetObjectTypes.OBJECT_ANALOG_VALUE))
            {
                return "analogValue";
            }
            else if (objectType.Equals(BacnetObjectTypes.OBJECT_BINARY_INPUT))
            {
                return "binaryInput";
            }
            else if (objectType.Equals(BacnetObjectTypes.OBJECT_BINARY_OUTPUT))
            {
                return "binaryOutput";
            }
            else if (objectType.Equals(BacnetObjectTypes.OBJECT_BINARY_VALUE))
            {
                return "binaryValue";
            }
            else if (objectType.Equals(BacnetObjectTypes.OBJECT_MULTI_STATE_INPUT))
            {
                return "multiStateInput";
            }
            else if (objectType.Equals(BacnetObjectTypes.OBJECT_MULTI_STATE_OUTPUT))
            {
                return "multiStateOutput";
            }
            else if (objectType.Equals(BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE))
            {
                return "multiStateValue";
            }

            return null;
        }
        //上传UDP
        static string SendUdpMessage(BACnetTranslator.BacDevice.BacProperty pro, int udpid = 0)
        {
            try
            {
                if (Udp_Client == null || string.IsNullOrEmpty(Settings.BuildingSign) || pro == null) return string.Empty;
                StringBuilder sb = new StringBuilder();
                sb.AppendWithSplit(pro.building);
                sb.AppendWithSplit(Settings.Gateway);
                sb.AppendWithSplit(Settings.SendType);
                sb.AppendWithSplit(DateTime.Now.ToString(@"yyyyMMddHHmmss"));
                sb.AppendWithSplit(udpid == 0 ? GetCurrentUdpId() : udpid);
                sb.AppendWithSplit(pro.meter);
                sb.AppendWithSplit(1);
                sb.AppendWithSplit(pro.funcid);
                sb.Append(pro.PROP_PRESENT_VALUE);

                var str = sb.ToString();
               // for (int i = 0; i < 5; i++)
               // {
                    Udp_Client.PushSend(str);
                    Thread.Sleep(100);
               // }
                return "SendUdpMessage: " + str;
            }
            catch (Exception exp)
            {
                Logger.Error("SendUdpMessage", exp);
                return string.Empty;
            }
        }

        private static void handleData(string buildingSign, string meterSign, long funcID, string receivetime, string data)
        {
            // 已有数据更新
            if (RecordAddress.ContainsKey(buildingSign + "-" + meterSign + "-" + funcID) && RecordAddress[(buildingSign + "-" + meterSign + "-" + funcID)] >0)
            {
                int address = RecordAddress[(buildingSign + "-" + meterSign + "-" + funcID)];
                try
                {
                    RecordList[(address)].receivetime = (receivetime);
                    RecordList[(address)].data = data;
                    Logger.Log((address + 1) + "/" + RecordList.Count + " UPDATE "
                    + RecordList[(address)].buildingSign + "."
                    + RecordList[(address)].meterSign + "." + RecordList[(address)].funcID
                    + ":" + receivetime + " "
                    + RecordList[(address)].data);

                }
                catch (Exception e)
                {

                }
            }
            else
            {
                // 新增数据并记录地址
                try
                {
                    Record record = new Record();
                    record.buildingSign = buildingSign;
                    record.meterSign = meterSign;
                    record.funcID = System.Convert.ToString(funcID);
                    record.receivetime = (receivetime);
                    record.data = data;
                    RecordAddress.Add(buildingSign + "-" + meterSign + "-" + funcID, RecordList.Count());
                    RecordList.Add(record);
                    Logger.Log(RecordList.Count + "/" + RecordList.Count + " ADD "
                            + record.buildingSign + "." + record.meterSign + "." + record.funcID + ":"
                            + (record.receivetime) + " " + record.data);
                }
                catch (Exception e)
                {
                    // TODO Auto-generated catch block

                }
            }
        }
        static string SendUdpMessage(Point point,string value, int udpid = 0)
        {
            try
            {
                if (Udp_Client == null || string.IsNullOrEmpty(Settings.BuildingSign) || point == null) return string.Empty;
                StringBuilder sb = new StringBuilder();
                string buildingSign=point.buildingSign;
                string meterSign =point.meterSign;
                string receivetime = DateTime.Now.ToString(@"yyyyMMddHHmmss");
                sb.AppendWithSplit(buildingSign);
                sb.AppendWithSplit(Settings.Gateway);
                sb.AppendWithSplit(Settings.SendType);
                sb.AppendWithSplit(receivetime);
                sb.AppendWithSplit(udpid == 0 ? GetCurrentUdpId() : udpid);
                sb.AppendWithSplit(meterSign);
                sb.AppendWithSplit(point.functionList.Count);
               
                foreach (var funcid in point.functionList)
                {                   
                    sb.AppendWithSplit(funcid.functionID);
                    sb.Append(value);
                    handleData(buildingSign, meterSign, funcid.functionID, receivetime, value);
                }
               
                var str = sb.ToString();
                // for (int i = 0; i < 5; i++)
                // {
                Udp_Client.PushSend(str);
                Thread.Sleep(100);
                // }
                return "SendUdpMessage: " + str;
            }
            catch (Exception exp)
            {
                Logger.Error("SendUdpMessage", exp);
                return string.Empty;
            }
        }
        static string SendUdpHeart(string buildingSign, int udpid = 0)
        {
            try
            {
                if (Udp_Client == null) return string.Empty;
                StringBuilder sb = new StringBuilder();
                //1101070037;1;heart
               
                sb.AppendWithSplit(buildingSign);
                sb.AppendWithSplit(Settings.Gateway);
                sb.AppendWithSplit("heart");


                var str = sb.ToString();
  
                Udp_Client.PushSend(str);
                Thread.Sleep(100);
                return "SendUdpHeart: " + str;
            }
            catch (Exception exp)
            {
                Logger.Error("SendUdpHeart", exp);
                return string.Empty;
            }
        }
        static string SendUdpConn(string buildingSign, int udpid = 0)
        {
            try
            {
                if (Udp_Client == null) return string.Empty;
                StringBuilder sb = new StringBuilder();
                //1101070037;1;connect

                sb.AppendWithSplit(buildingSign);
                sb.AppendWithSplit(Settings.Gateway);
                sb.AppendWithSplit("connect");


                var str = sb.ToString();

                Udp_Client.PushSend(str);
                Thread.Sleep(100);
                return "SendUdpConn: " + str;
            }
            catch (Exception exp)
            {
                Logger.Error("SendUdpConn", exp);
                return string.Empty;
            }
        }
    }
    public class NullLogger : ILogger
    {
        public void Debug(string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Fatal(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Warn(string message)
        {
        }
    }

    static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> items,
                                                    int numOfParts)
        {
            int i = 0;
            return items.GroupBy(x => i++ % numOfParts);
        }
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> items,
                                                       int partitionSize)
        {
            int i = 0;
            return items.GroupBy(x => i++ / partitionSize).ToArray();
        }
    }
    public class Collector
    {
        public String Name;
        public String IP;
        public int Port;
        public String User;
        public String Password;
        public String ProgId;
        public String BroadcastAddress;
        public String MAC;
        public String PROTOCOL;
        public String Mode;
        public int Max_size;
        public String SqlContent;
    }
   
    public class Point
    {
        public String MAC;
        public String Item;
        public String buildingSign;
        public String meterSign;
        public String deviceId;
        public String type;
        public String instanceNumber;

        public HashSet<Function> functionList = new HashSet<Function>();
    }
    public class Function
    {
        public long functionID;
        public Dictionary<String, String> DataConvert;
    }
    public class PointSet
    {
        public String uploadName;
        public String[] content;
        public String mac;
        public int ID;
        public double sendTime = 0;
        public double recTime = 0;
        public String status;
        public String buildingSign;
        public String buildingSignNew;
        public String meterSign;
        public int funcID;
        public String collectorMAC;
        public String cmd;
        public double dataSet;

    }
    public class Record
    {
        public String buildingSign;
        public String meterSign;
        public String funcID;
        public String receivetime;
        public String data;
        public String addtion;

    }
}
