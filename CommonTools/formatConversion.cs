using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAPI;
namespace CommonTools
{
    public static class formatConversion
    {
        /// <summary>
        /// HL7解析
        /// </summary>
        /// <param name="equipmentCode"></param>
        /// <returns></returns>
        public static Dictionary<string, string> equipmentCodeToDataCode(string equipmentCode)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            switch (equipmentCode)
            {
                case "149522":
                    result.Add("code", "pr");
                    result.Add("codeName", "PR");
                    break;
                case "149530":
                    result.Add("code", "pr");
                    result.Add("codeName", "PR");
                    break;
                case "147842":
                    result.Add("code", "xinlv");
                    result.Add("codeName", "心率");
                    break;
                case "150456":
                    result.Add("code", "spo2");
                    result.Add("codeName", "Spo2");
                    break;
                case "151578":
                    result.Add("code", "hx");
                    result.Add("codeName", "呼吸");
                    break;
                case "151594":
                    result.Add("code", "hx");
                    result.Add("codeName", "呼吸");
                    break;
                case "150037":
                    result.Add("code", "ycssy");
                    result.Add("codeName", "有创收缩");
                    break;
                case "150038":
                    result.Add("code", "ycszy");
                    result.Add("codeName", "有创舒张");
                    break;
                case "150039":
                    result.Add("code", "ycpjy");
                    result.Add("codeName", "有创平均");
                    break;
                case "150017":
                    result.Add("code", "ycssy");
                    result.Add("codeName", "有创收缩");
                    break;
                case "150018":
                    result.Add("code", "ycszy");
                    result.Add("codeName", "有创舒张");
                    break;
                case "150019":
                    result.Add("code", "ycpjy");
                    result.Add("codeName", "有创平均");
                    break;
                case "150087":
                    result.Add("code", "cvp");
                    result.Add("codeName", "CVP");
                    break;
                case "151708":
                    result.Add("code", "etco2");
                    result.Add("codeName", "ETCO2");
                    break;
                case "150301":
                    result.Add("code", "wcssy");
                    result.Add("codeName", "无创收缩");
                    break;
                case "150302":
                    result.Add("code", "wcszy");
                    result.Add("codeName", "无创舒张");
                    break;
                case "150303":
                    result.Add("code", "wcpjy");
                    result.Add("codeName", "无创平均");
                    break;
                case "120":
                    result.Add("code", "bis");
                    result.Add("codeName", "BIS");
                    break;
                case "1.2.1.150344":
                    result.Add("code", "t1");
                    result.Add("codeName", "T1");
                    break;
                case "1.2.2.150344":
                    result.Add("code", "t2");
                    result.Add("codeName", "T2");
                    break;
            }
            return result;
        }
        /// geteway解析
        /// </summary>
        /// <param name="equipmentCode"></param>
        /// <returns></returns>
        public static Dictionary<string, string> equipmentCodeToDataCode2(string equipmentCode)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            switch (equipmentCode)
            {
                case "150344":
                    result.Add("code", "100");
                    result.Add("codeName", "体温");
                    break;
                case "150276":
                    result.Add("code", "101");
                    result.Add("codeName", "CO");
                    break;
                case "150021":
                    result.Add("code", "102");
                    result.Add("codeName", "NIBP(S)");
                    break;
                case "150022":
                    result.Add("code", "103");
                    result.Add("codeName", "NIBP(D)");
                    break;
                case "150023":
                    result.Add("code", "114");
                    result.Add("codeName", "NIBP(M)");
                    break;
                case "150033":
                    result.Add("code", "102");
                    result.Add("codeName", "NIBP(S)");
                    break;
                case "150034":
                    result.Add("code", "103");
                    result.Add("codeName", "NIBP(D)");
                    break;
                case "150035":
                    result.Add("code", "114");
                    result.Add("codeName", "NIBP(M)");
                    break;
                case "150037":
                    result.Add("code", "135");
                    result.Add("codeName", "有创收缩压");
                    break;
                case "150038":
                    result.Add("code", "136");
                    result.Add("codeName", "有创舒张压");
                    break;
                case "150039":
                    result.Add("code", "150");
                    result.Add("codeName", "平均压");
                    break;
                case "109":
                    result.Add("code", "137");
                    result.Add("codeName", "ScvO2");
                    break;
                case "151578":
                    result.Add("code", "711");
                    result.Add("codeName", "呼吸频率");
                    break;       
                case "150456":
                    result.Add("code", "113");
                    result.Add("codeName", "血氧饱和度");
                    break;
                case "147842":
                    result.Add("code", "115");
                    result.Add("codeName", "心率");
                    break;
               //     result.Add("code", "143");
                //    result.Add("codeName", "血糖");
              //      break;
                case "149530":
                    result.Add("code", "152");
                    result.Add("codeName", "脉搏");
                    break;

                default:
                    break;
            }
            return result;
        }
        public static Dictionary<string, string> equipmentCodeToDataCodeT5(string equipmentCode)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            switch (equipmentCode)
            {

                case "170":
                    result.Add("code", "wcssy");
                    result.Add("codeName", "无创收缩");
                    break;
                case "171":
                    result.Add("code", "wcszy");
                    result.Add("codeName", "无创舒张");
                    break;
                case "172":
                    result.Add("code", "wcpjy");
                    result.Add("codeName", "无创平均");
                    break;

                case "161":
                    result.Add("code", "pr");
                    result.Add("codeName", "PR");
                    break;
                case "101":
                    result.Add("code", "xinlv");
                    result.Add("codeName", "心率");
                    break;
                case "151":
                    result.Add("code", "hx");
                    result.Add("codeName", "呼吸");
                    break;

                case "222":
                    result.Add("code", "hx");
                    result.Add("codeName", "呼吸");
                    break;
                case "160":
                    result.Add("code", "spo2");
                    result.Add("codeName", "Spo2");
                    break;
                case "220":
                    result.Add("code", "etco2");
                    result.Add("codeName", "ETCO2");
                    break;
                case "500":
                    result.Add("code", "ycssy");
                    result.Add("codeName", "有创收缩");
                    break;
                case "502":
                    result.Add("code", "ycszy");
                    result.Add("codeName", "有创舒张");
                    break;
                case "501":
                    result.Add("code", "ycpjy");
                    result.Add("codeName", "有创平均");
                    break;
                case "200":
                    result.Add("code", "t1");
                    result.Add("codeName", "T1");
                    break;
                case "201":
                    result.Add("code", "t2");
                    result.Add("codeName", "T2");
                    break;
                case "506":
                    result.Add("code", "ycssy");
                    result.Add("codeName", "有创收缩");
                    break;
                case "508":
                    result.Add("code", "ycszy");
                    result.Add("codeName", "有创舒张");
                    break;
                case "507":
                    result.Add("code", "ycpjy");
                    result.Add("codeName", "有创平均");
                    break;
                default:
                    break;
            }
            return result;
        }

        public static SysDeviceData respirtaor(string[] data)
        {
            List<SysDeviceData> rep = new List<SysDeviceData>();
            SysDeviceData resp = new SysDeviceData();
            /**resp.ms = data[7].Trim();
            resp.sxplsz = data[11].Trim();
            resp.cqlsz = data[12].Trim();
            resp.yndsz = data[14].Trim();
            resp.peepMaxsz = data[60].Trim();
            resp.peepMinsz = data[61].Trim();
            resp.hxpl = data[68].Trim();
            resp.Ppeak = data[70].Trim();
            resp.Pmean = data[71].Trim();
            resp.yqcsbfb = data[75].Trim();
            resp.xqcql = data[69].Trim();
            resp.peepclz = data[85].Trim();
            resp.dtsyx = data[94].Trim();
            resp.dtzl = data[95].Trim();
            resp.zzcql = data[152].Trim();**/
            return resp;
        }

        public static SysDeviceData draegerResp(string msg)
        {
            string[] data = msg.Replace("  ", " ").Split(' ');
            SysDeviceData resp = new SysDeviceData();
            string peepclz = "";
            string hxpl = "";
            string Ppeak = "";
            string Pmean = "";
            string xqcql = "";
            string yqcsbfb = "";
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Length > 2 && data[i].IndexOf("B5") > 0)
                {

                }
                else if (data[i].Contains("D6"))
                {

                    try
                    {
                        hxpl = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        hxpl = "";
                    }
                }
                else if (data[i].Equals("7D") || data[i].IndexOf("7D") > 0)
                {

                    try
                    {
                        Ppeak = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        Ppeak = "";
                    }
                }
                else if (i == 0)
                {
                    try
                    {
                        Pmean = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        Pmean = "";
                    }
                }
                else if (data[i].Equals("88") || data[i].Length > 2 && data[i].IndexOf("88") > 0)
                {
                    try
                    {
                        string cql = data[i + 1].Substring(0, data[i + 1].IndexOf("B5"));
                        xqcql = (double.Parse(cql) / 1000) + "";
                    }
                    catch (Exception e)
                    {
                        xqcql = "";
                    }
                }
                else if (data[i].Length > 2 && data[i].IndexOf("F0") > 0)
                {

                    try
                    {
                        yqcsbfb = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        yqcsbfb = "";
                    }

                }
                else if (data[i].Length > 2 && data[i].IndexOf("78") > 0)
                {
                    try
                    {
                        peepclz = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        peepclz = "";
                    }
                }
            }
           /** resp.hxpl = hxpl;
            resp.Ppeak = Ppeak;
            resp.Pmean = Pmean;
            resp.xqcql = xqcql;
            resp.yqcsbfb = yqcsbfb;
            resp.peepclz = peepclz;
            resp.ms = "";
            resp.sxplsz = "";
            resp.cqlsz = "";
            resp.yndsz = "";
            resp.peepMaxsz = "";
            resp.peepMinsz = "";
            resp.dtsyx = "";
            resp.dtzl = "";
            resp.zzcql = "";**/
            return resp;
        }

        public static SysDeviceData draegerRespE4(string msg)
        {
            string[] data = msg.Replace("  ", " ").Split(' ');
            SysDeviceData resp = new SysDeviceData();
            string peepclz = "";
            string hxpl = "";
            string Ppeak = "";
            string Pmean = "";
            string xqcql = "";
            string yqcsbfb = "";
            string fztql = "";
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Length > 2 && data[i].IndexOf("D6") > 0)
                {
                    try
                    {
                        hxpl = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        hxpl = "";
                    }
                }
                else if (data[i].Length == 2 && data[i].Equals("7D"))
                {

                    try
                    {
                        Ppeak = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        Ppeak = "";
                    }
                }
                else if (data[i].Length == 2 && data[i].Equals("73"))
                {

                    try
                    {
                        Pmean = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        Pmean = "";
                    }
                }
                else if (data[i].Equals("88") || data[i].Length > 2 && data[i].IndexOf("88") > 0)
                {
                    try
                    {
                        string cql = data[i + 1].Substring(0, data[i + 1].IndexOf("B9"));
                        xqcql = (double.Parse(cql) / 1000) + "";
                        try
                        {
                            fztql = data[i + 2].Substring(0, data[i + 2].IndexOf("7A"));
                        }
                        catch (Exception) { fztql = ""; }

                        if (fztql == "")
                        {
                            try
                            {
                                fztql = data[i + 2].Substring(0, data[i + 2].IndexOf("B7"));
                            }
                            catch (Exception)
                            {
                                fztql = "";
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        xqcql = "";
                    }
                }
                else if (data[i].Length == 2 && data[i].Equals("F0"))
                {

                    try
                    {
                        yqcsbfb = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        yqcsbfb = "";
                    }

                }
                else if (data[i].Length > 2 && data[i].Contains("F0"))
                {

                    try
                    {
                        yqcsbfb = data[i].Replace("F0", "");
                    }
                    catch (Exception e)
                    {
                        yqcsbfb = "";
                    }

                }
                else if (data[i].Length == 2 && data[i].Equals("78"))
                {
                    try
                    {
                        peepclz = data[i + 1];
                    }
                    catch (Exception e)
                    {
                        peepclz = "";
                    }
                }
            }

           /** resp.hxpl = hxpl;
            resp.Ppeak = Ppeak;
            resp.Pmean = Pmean;
            resp.xqcql = xqcql;
            resp.yqcsbfb = yqcsbfb;
            resp.peepclz = peepclz;
            resp.ms = "";
            resp.sxplsz = "";
            resp.cqlsz = "";
            resp.yndsz = "";
            resp.peepMaxsz = "";
            resp.peepMinsz = "";
            resp.dtsyx = "";
            resp.dtzl = "";
            resp.zzcql = "";**/

            return resp;
        }


        public static SysDeviceData respirtaorV(string[] data)
        {
            SysDeviceData resp = new SysDeviceData();
            /**if (data[6].Trim().Equals("PCV"))
            {
                resp.ms = data[6].Trim() + "-" + data[32].Trim();
                resp.yndsz = data[45].Trim();
                resp.sxplsz = data[34].Trim();
                resp.peepclz = data[37].Trim();
            }
            else if (data[6].Trim().Equals("VCV"))
            {
                resp.ms = data[6].Trim() + "-" + data[7].Trim();
                resp.cqlsz = data[11].Trim();
                resp.yndsz = data[21].Trim();
                resp.sxplsz = data[10].Trim();
                resp.peepclz = data[13].Trim();
            }
            else if (data[6].Trim().Equals("NPPV"))
            {
                resp.ms = data[6].Trim() + "-" + data[56].Trim();
                resp.yndsz = data[67].Trim();
                resp.sxplsz = data[58].Trim();
            }
            resp.hxpl = data[83].Trim();
            resp.Ppeak = data[76].Trim();
            resp.Pmean = data[77].Trim();
            resp.yqcsbfb = resp.yndsz;
            try
            {
                resp.xqcql = double.Parse(data[80].Trim()) / 1000 + "";
            }
            catch (Exception e)
            {
                resp.xqcql = "";
            }
            // resp.dtsyx = data[94].Trim();
            //resp.dtzl = data[95].Trim();
            // resp.zzcql = data[152].Trim();**/
            return resp;
        }

    }

}
