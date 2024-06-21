using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAPI
{
    public class SysDeviceData
    {
        /// <summary>
        /// 
        /// </summary>
        public long dataId { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string dataCode { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string dataName { get; set; }

        /// <summary>
        /// 数值
        /// </summary>
        public string dataValue { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string deptId { get; set; }

        /// <summary>
        /// 整点标识
        /// </summary>
        public string deviceType { get; set; }

        /// <summary>
        /// 整点标识
        /// </summary>
        public string deviceIp { get; set; }

        /// <summary>
        /// 整点标识
        /// </summary>
        public string createTime { get; set; }

        /// <summary>
        /// 整点标识
        /// </summary>
        public string createBy { get; set; }

        /// <summary>
        /// 整点标识
        /// </summary>
        public string updateBy { get; set; }
        /// <summary>
        /// 整点标识
        /// </summary>
        public string updateTime { get; set; }

        /// <summary>
        /// 整点标识
        /// </summary>
        public string remark { get; set; }


    }
}
