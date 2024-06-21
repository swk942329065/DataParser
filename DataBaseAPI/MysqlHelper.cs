using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAPI
{
    public class MysqlHelper
    {
         public  static string connect = "server=localhost;port=3306;database=medtech-xh-aims;user=root;password=root;SslMode=none;";
        //public static string connect = "server=192.168.76.33;port=3306;database=medtech-hd-aims;user=root;password=today;SslMode=none;allowPublicKeyRetrieval=true;";
        // public static string connect = "server=localhost;port=3306;database=medtech-hd-aims;user=root;password=root;SslMode=none;allowPublicKeyRetrieval=true;";

        /// <summary>插入新数据
        /// </summary>
        /// 
        public static void InsertOne(SysDeviceData data){
            MySqlConnection conn = new MySqlConnection(connect);
            try
            {
                conn.Open();
                string insertData = "INSERT INTO sys_device_data(data_code,data_name,data_value,dept_id,device_type,device_ip,create_time,remark)"+
                    "VALUES ('"+data.dataCode+"','"+data.dataName+"','"+data.dataValue+"','"+data.deptId+"','"+data.deviceType+"','"+data.deviceIp+"','"+data.createTime+"','"+data.remark+"')";
                //Console.WriteLine(insertData);
                MySqlCommand mySqlCommand = new MySqlCommand(insertData, conn);
                int result = mySqlCommand.ExecuteNonQuery();//返回值为影响了几行数据
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
        }


        /// <summary>
        /// 查询设备列表
        /// </summary>
        public static List<SysDevice> selectDeviceList(){
            MySqlConnection conn = new MySqlConnection(connect);
            List<SysDevice> deviceList = new List<SysDevice>();
            try
            {
                conn.Open();
                string queryData = "SELECT d.id,d.dept_id,d.device_num,d.device_name,d.device_type,d.device_ip,"+
                "d.device_mill,d.device_collect,d.device_price,d.create_by,d.create_time,d.update_by,d.update_time,d.STATUS,d.remark FROM sys_device d";
                MySqlCommand mySqlCommand = new MySqlCommand(queryData, conn);
                MySqlDataReader reader = mySqlCommand.ExecuteReader();
                while (reader.Read()){
                    SysDevice device = new SysDevice();
                    device.id = long.Parse(reader[0].ToString());
                    device.deptId = long.Parse(reader[1].ToString());
                    device.deviceNum = reader[2].ToString();
                    device.deviceName = reader[3].ToString();
                    device.deviceType = reader[4].ToString();
                    device.deviceIp = reader[5].ToString();
                    device.deviceMill = reader[6].ToString();
                    device.deviceCollect = reader[7].ToString();
                    device.devicePrice =reader[8].ToString();
                    device.status = reader[13].ToString();
                    device.remark = reader[14].ToString();
                    deviceList.Add(device);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return deviceList;
        }
        /// <summary>
        /// 根据设备ip查询床号
        /// </summary>
        public static string getBedNoByDeviceIp(string ip)
        {
            MySqlConnection conn = new MySqlConnection(connect);
            string bedNo = "";
            try
            {
                conn.Open();
                string queryData = "SELECT b.bed_no from icu_bed_device_config b left join icu_device d ON b.device_id=d.device_id where b.dept_id=103 and  d.device_ip='" + ip + "'";
                MySqlCommand mySqlCommand = new MySqlCommand(queryData, conn);
                MySqlDataReader reader = mySqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    bedNo = reader[0].ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return bedNo;
        }
    }
}
