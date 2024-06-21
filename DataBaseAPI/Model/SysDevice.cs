using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAPI
{
    public class SysDevice
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 科室ID
        /// </summary>
        public long deptId { get; set; }

        /// <summary>
        /// 设备好
        /// </summary>
        public string deviceNum { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string deviceName { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string deviceType { get; set; }

        /// <summary>
        /// 设备ip
        /// </summary>
        public string deviceIp { get; set; }

        /// <summary>
        /// 设备厂家
        /// </summary>
        public string deviceMill { get; set; }

        /// <summary>
        /// 采集状态
        /// </summary>
        public string deviceCollect { get; set; }

        /// <summary>
        /// 设备单价
        /// </summary>
        public String devicePrice { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string status { get; set; }
    }
}
