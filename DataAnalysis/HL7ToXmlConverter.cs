using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace DataAnalysis
{
    /// <summary>  
    /// HL7解析器  
    /// </summary>  
    public class HL7ToXmlConverter
    {
        private XmlDocument _xmlDoc;

        /// <summary>  
        /// 把HL7信息转成XML形式  
        /// 分隔顺序 \n,|,~,^,&  
        /// </summary>  
        /// <param name="sHL7">HL7字符串</param>  
        /// <returns></returns>  
        public string ConvertToXml(string sHL7)
        {
            _xmlDoc = ConvertToXmlObject(sHL7);
            return _xmlDoc.OuterXml;
        }

        public XmlDocument ConvertToXmlObject(string sHL7)
        {
            _xmlDoc = CreateXmlDoc();

            //把HL7分成段  
            string newdata = formatGetNewHl7(sHL7);

            string[] sHL7Lines = newdata.Split('\n');

            //去掉XML的关键字  
            for (int i = 0; i < sHL7Lines.Length; i++)
            {
                sHL7Lines[i] = Regex.Replace(sHL7Lines[i], @"[^ -~]", "");
            }

            for (int i = 0; i < sHL7Lines.Length; i++)
            {
                // 判断是否空行  
                if (sHL7Lines[i] != string.Empty)
                {
                    string sHL7Line = sHL7Lines[i];

                    //通过/r 或/n 回车符分隔  
                    string[] sFields = GetMessgeFields(sHL7Line);

                    // 为段（一行）创建第一级节点  
                    XmlElement el = _xmlDoc.CreateElement(sFields[0]);
                    _xmlDoc.DocumentElement.AppendChild(el);

                    // 循环每一行  
                    for (int a = 0; a < sFields.Length; a++)
                    {
                        // 为字段创建第二级节点  
                        XmlElement fieldEl = _xmlDoc.CreateElement(sFields[0] + "." + a.ToString());

                        //是否包括HL7的连接符  
                        if (sFields[a] != @"^~\&")
                        {
                            //0:如果这一行有任何分隔符

                            //通过~分隔  
                            string[] sComponents = GetRepetitions(sFields[a]);
                            if (sComponents.Length > 1)
                            {//1:如果可以分隔  
                                for (int b = 0; b < sComponents.Length; b++)
                                {
                                    XmlElement componentEl = _xmlDoc.CreateElement(sFields[0] + "." + a.ToString() + "." + b.ToString());

                                    //通过&分隔   
                                    string[] subComponents = GetSubComponents(sComponents[b]);
                                    if (subComponents.Length > 1)
                                    {//2.如果有字组，一般是没有的。。。  
                                        for (int c = 0; c < subComponents.Length; c++)
                                        {
                                            //修改了一个错误  
                                            string[] subComponentRepetitions = GetComponents(subComponents[c]);
                                            if (subComponentRepetitions.Length > 1)
                                            {
                                                for (int d = 0; d < subComponentRepetitions.Length; d++)
                                                {
                                                    XmlElement subComponentRepEl = _xmlDoc.CreateElement(sFields[0] + "." + a.ToString() + "." + b.ToString() + "." + c.ToString() + "." + d.ToString());
                                                    subComponentRepEl.InnerText = subComponentRepetitions[d];
                                                    componentEl.AppendChild(subComponentRepEl);
                                                }
                                            }
                                            else
                                            {
                                                XmlElement subComponentEl = _xmlDoc.CreateElement(sFields[0] + "." + a.ToString() + "." + b.ToString() + "." + c.ToString());
                                                subComponentEl.InnerText = subComponents[c];
                                                componentEl.AppendChild(subComponentEl);
                                            }
                                        }
                                        fieldEl.AppendChild(componentEl);
                                    }
                                    else
                                    {//2.如果没有字组了，一般是没有的。。。  
                                        string[] sRepetitions = GetComponents(sComponents[b]);
                                        if (sRepetitions.Length > 1)
                                        {
                                            XmlElement repetitionEl = null;
                                            for (int c = 0; c < sRepetitions.Length; c++)
                                            {
                                                repetitionEl = _xmlDoc.CreateElement(sFields[0] + "." + a.ToString() + "." + b.ToString() + "." + c.ToString());
                                                repetitionEl.InnerText = sRepetitions[c];
                                                componentEl.AppendChild(repetitionEl);
                                            }
                                            fieldEl.AppendChild(componentEl);
                                            el.AppendChild(fieldEl);
                                        }
                                        else
                                        {
                                            componentEl.InnerText = sComponents[b];
                                            fieldEl.AppendChild(componentEl);
                                            el.AppendChild(fieldEl);
                                        }
                                    }
                                }
                                el.AppendChild(fieldEl);
                            }
                            else
                            {
                                //1:如果不可以分隔，可以直接写节点值了。  
                                fieldEl.InnerText = sFields[a];
                                el.AppendChild(fieldEl);
                            }
                        }
                        else
                        {//0:如果不可以分隔，可以直接写节点值了。  
                            fieldEl.InnerText = sFields[a];
                            el.AppendChild(fieldEl);
                        }
                    }
                }
            }
            return _xmlDoc;
        }

        /// <summary>  
        /// 通过|分隔 字段  
        /// </summary>  
        /// <param name="s"></param>  
        /// <returns></returns>  
        private string[] GetMessgeFields(string s)
        {
            return s.Split('|');
        }

        /// <summary>  
        /// 通过^分隔 组字段  
        /// </summary>  
        /// <param name="s"></param>  
        /// <returns></returns>  
        private string[] GetComponents(string s)
        {
            return s.Split('^');
        }

        /// <summary>  
        /// 通过&分隔 子分组组字段  
        /// </summary>  
        /// <param name="s"></param>  
        /// <returns></returns>  
        private string[] GetSubComponents(string s)
        {
            return s.Split('&');
        }

        /// <summary>  
        /// 通过~分隔 重复  
        /// </summary>  
        /// <param name="s"></param>  
        /// <returns></returns>  
        private string[] GetRepetitions(string s)
        {
            return s.Split('~');
        }

        /// <summary>  
        /// 创建XML对象  
        /// </summary>  
        /// <returns></returns>  
        private XmlDocument CreateXmlDoc()
        {
            XmlDocument output = new XmlDocument();
            XmlElement rootNode = output.CreateElement("HL7Message");
            output.AppendChild(rootNode);
            return output;
        }

        public string GetText(XmlDocument xmlObject, string path)
        {
            XmlNode node = xmlObject.DocumentElement.SelectSingleNode(path);
            if (node != null)
            {
                return node.InnerText;
            }
            else
            {
                return null;
            }
        }

        public string GetText(XmlDocument xmlObject, string path, int index)
        {
            XmlNodeList nodes = xmlObject.DocumentElement.SelectNodes(path);
            if (index <= nodes.Count)
            {
                return nodes[index].InnerText;
            }
            else
            {
                return null;
            }
        }

        public String[] GetTexts(XmlDocument xmlObject, string path)
        {
            XmlNodeList nodes = xmlObject.DocumentElement.SelectNodes(path);
            String[] arr = new String[nodes.Count];
            int index = 0;
            foreach (XmlNode node in nodes)
            {
                arr[index++] = node.InnerText;
            }
            return arr;
        }

        public List<string> formatGetHl7(string sHL7)
        {
            //\u为unicode
            string[] newHL7 = Regex.Split(sHL7, "\u001c");
            List<string> newData = new List<string>();
            foreach (string item in newHL7)
            {
                if (item.Contains("MSH") && item.Contains("PID") && item.Contains("PV1") && item.Contains("OBR") && item.Contains("OBX"))
                {
                    newData.Add(item.Replace("\v", "").Replace("\r", ""));
                }
            }
            return newData;
        }
        public List<string> formatGetHl7T5(string sHL7)
        {
            String msg = "";
            string[] newHL7 = Regex.Split(sHL7, "MSH");
            List<string> newData = new List<string>();
            foreach (string item in newHL7)
            {
                msg = "MSH" + item;
                if (msg.Contains("MSH") && msg.Contains("ORU^R01"))
                {
                    newData.Add(msg.Replace("\v", "").Replace("\r", ""));
                }
            }
            return newData;
        }
        private string formatGetNewHl7(string sHL7)
        {
            string newData = "";
            if (sHL7.IndexOf("PID") > 0)
            {
                sHL7 = sHL7.Insert(sHL7.IndexOf("PID"), "\n");
            }
            if (sHL7.IndexOf("PV1") > 0)
            {
                sHL7 = sHL7.Insert(sHL7.IndexOf("PV1"), "\n");
            }
            if (sHL7.IndexOf("OBR") > 0)
            {
                sHL7 = sHL7.Insert(sHL7.IndexOf("OBR"), "\n");
            }
            string[] bcd = Regex.Split(sHL7, "OBX");
            newData = bcd[0];
            for (int i = 1; i < bcd.Length; i++)
            {
                bcd[i] = "\nOBX" + bcd[i];
                newData += bcd[i];
            }

            return newData;
        }
    }
}