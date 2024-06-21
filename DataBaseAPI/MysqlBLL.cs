using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAPI
{
    public class MysqlBLL
    {
        #region 设备数据
        public void InsertOne(SysDeviceData data)
        {
            MysqlDAL dal = new MysqlDAL();
            dal.InsertOne(data);
        }
        #endregion


        #region 设备表
        public List<SysDevice> selectDeviceList()
        {
            MysqlDAL dal = new MysqlDAL();
            return dal.selectDeviceList();
        }
        #endregion
    }
}
