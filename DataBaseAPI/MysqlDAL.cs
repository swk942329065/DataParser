using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAPI
{
    public class MysqlDAL
    {
        #region 监护仪
        public void InsertOne(SysDeviceData data)
        {
            MysqlHelper.InsertOne(data);
        }
        #endregion

        #region 设备
        public List<SysDevice> selectDeviceList(){
            return MysqlHelper.selectDeviceList();
        }
        #endregion
    }
}
