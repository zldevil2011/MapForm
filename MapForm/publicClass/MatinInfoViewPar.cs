using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapForm.publicClass
{
    public class MatinInfoViewPar
    {
        /// <summary>
        /// 经度
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// 全路径及文件名
        /// </summary>
        public string PathName { get; set; }
        /// <summary>
        /// 闪烁标志
        /// </summary>
        public bool IsFlash;
        /// <summary>
        /// 显示的字符串数组
        /// </summary>
        public string[] View { get; set; }
    }

}
