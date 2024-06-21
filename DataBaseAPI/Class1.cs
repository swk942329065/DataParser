﻿/*
* This file is part of VitalSignsCaptureWave v1.009.
* Copyright (C) 2015-19 John George K., xeonfusion@users.sourceforge.net

VitalSignsCapture is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

VitalSignsCapture is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with VitalSignsCapture.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Net;
using DataBaseAPI;
using System.Net.Sockets;
using System.Configuration;
using log4net;

namespace VSCapture
{
    public sealed class DSerialPort : SerialPort
    {
        private static ILog log = LogManager.GetLogger(typeof(DSerialPort));
        // Main Datex Record variables
        private datex_record_req_type request_ptr = new datex_record_req_type();
        private datex_record_wave_req_type wave_request_ptr = new datex_record_wave_req_type();
        public List<datex_record_type> RecordList = new List<datex_record_type>();
        public List<byte[]> FrameList = new List<byte[]>();
        private int DPortBufSize;
        public byte[] DPort_rxbuf;
        private datex_tx_type DPort_txbuf = new datex_tx_type();
        private datex_wave_tx_type DPort_wave_txbuf = new datex_wave_tx_type();

        private bool m_fstart = true;
        private bool m_storestart = false;
        private bool m_storeend = false;
        private bool m_bitshiftnext = false;
        private List<byte> m_bList = new List<byte>();

        public List<NumericValResult> m_NumericValList = new List<NumericValResult>();
        public List<string> m_NumValHeaders = new List<string>();
        public StringBuilder m_strbuildvalues = new StringBuilder();
        public StringBuilder m_strbuildheaders = new StringBuilder();

        public List<WaveValResult> m_WaveValResultList = new List<WaveValResult>();
        public StringBuilder m_strbuildwavevalues = new StringBuilder();

        public string m_strTimestamp;
        public int m_dataexportset = 1;
        private bool m_transmissionstart = true;

        public string m_DeviceID;
        public string m_jsonposturl;
        MysqlBLL bll = new MysqlBLL();
        public class NumericValResult
        {
            public string Timestamp;
            public string PhysioID;
            public string Value;
            public string DeviceID;
        }

        public class WaveValResult
        {
            public string Timestamp;
            public string PhysioID;
            public short[] Value;
            public string DeviceID;
            public double Unitshift;
        }


        //Create a singleton serialport subclass
        private static volatile DSerialPort DPort = null;

        public static DSerialPort getInstance
        {

            get
            {
                if (DPort == null)
                {
                    lock (typeof(DSerialPort))
                        if (DPort == null)
                        {
                            DPort = new DSerialPort();
                        }

                }
                return DPort;
            }

        }

        public DSerialPort()
        {
            string gePort = ConfigurationManager.AppSettings["gePort"];
            DPort = this;

            DPortBufSize = 4096;
            DPort_rxbuf = new byte[DPortBufSize];

            DPort.PortName = gePort; //default Windows port

            DPort.BaudRate = 19200;
            DPort.Parity = Parity.Even;
            DPort.DataBits = 8;
            DPort.StopBits = StopBits.One;

            DPort.Handshake = Handshake.RequestToSend;

            // Set the read/write timeouts
            DPort.ReadTimeout = 600000;
            DPort.WriteTimeout = 600000;

            //ASCII Encoding in C# is only 7bit so
            DPort.Encoding = Encoding.GetEncoding("ISO-8859-1");

        }

        public void WriteBuffer(byte[] txbuf)
        {
            byte[] framebyte = { DataConstants.CTRLCHAR, (DataConstants.FRAMECHAR & DataConstants.BIT5COMPL), 0 };
            byte[] ctrlbyte = { DataConstants.CTRLCHAR, (DataConstants.CTRLCHAR & DataConstants.BIT5COMPL), 0 };

            byte check_sum = 0x00;
            byte b1 = 0x00;
            byte b2 = 0x00;

            int txbuflen = (txbuf.GetLength(0) + 1);
            //Create write packet buffer
            byte[] temptxbuff = new byte[txbuflen];
            //Clear the buffer
            for (int j = 0; j < txbuflen; j++)
            {
                temptxbuff[j] = 0;
            }

            // Send start frame characters
            temptxbuff[0] = DataConstants.FRAMECHAR;

            int i = 1;

            foreach (byte b in txbuf)
            {
                switch (b)
                {
                    case DataConstants.FRAMECHAR:
                        temptxbuff[i] = framebyte[0];
                        temptxbuff[i + 1] = framebyte[1];
                        i += 2;
                        b1 += framebyte[0];
                        b1 += framebyte[1];
                        check_sum += b1;
                        break;
                    case DataConstants.CTRLCHAR:
                        temptxbuff[i] = ctrlbyte[0];
                        temptxbuff[i + 1] = ctrlbyte[1];
                        i += 2;
                        b2 += ctrlbyte[0];
                        b2 += ctrlbyte[1];
                        check_sum += b2;
                        break;
                    default:
                        temptxbuff[i] = b;
                        i++;
                        check_sum += b;
                        break;
                };
            }

            int buflen = i;
            byte[] finaltxbuff = new byte[buflen + 2];

            for (int j = 0; j < buflen; j++)
            {
                finaltxbuff[j] = temptxbuff[j];
            }

            // Send Checksum
            finaltxbuff[buflen] = check_sum;
            // Send stop frame characters
            finaltxbuff[buflen + 1] = DataConstants.FRAMECHAR;

            try
            {
                DPort.Write(finaltxbuff, 0, buflen + 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening/writing to serial port :: " + ex.Message, "Error!");
            }

        }

        public void ClearReadBuffer()
        {
            //Clear the buffer
            for (int i = 0; i < DPortBufSize; i++)
            {
                DPort_rxbuf[i] = 0;
            }
        }

        public int ReadBuffer()
        {
            int bytesreadtotal = 0;

            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "AS3Rawoutput1.raw");

                int lenread = 0;

                do
                {
                    ClearReadBuffer();
                    lenread = DPort.Read(DPort_rxbuf, 0, DPortBufSize);

                    byte[] copyarray = new byte[lenread];

                    for (int i = 0; i < lenread; i++)
                    {
                        copyarray[i] = DPort_rxbuf[i];
                        CreateFrameListFromByte(copyarray[i]);
                    }

                    ByteArrayToFile(path, copyarray, copyarray.GetLength(0));
                    bytesreadtotal += lenread;
                    if (FrameList.Count > 0)
                    {
                        CreateRecordList();
                        ReadSubRecords();
                        ReadMultipleWaveSubRecords();

                        FrameList.RemoveRange(0, FrameList.Count);
                        RecordList.RemoveRange(0, RecordList.Count);

                    }

                }
                while (DPort.BytesToRead != 0);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening/writing to serial port :: " + ex.Message, "Error!");
            }


            return bytesreadtotal;
        }

        public void CreateFrameListFromByte(byte b)
        {

            if (b == DataConstants.FRAMECHAR && m_fstart)
            {
                m_fstart = false;
                m_storestart = true;
            }
            else if (b == DataConstants.FRAMECHAR && (m_fstart == false))
            {
                m_fstart = true;
                m_storeend = true;
                m_storestart = false;
                if (b != DataConstants.FRAMECHAR) m_bList.Add(b);
            }

            if (m_storestart == true)
            {
                if (b == DataConstants.CTRLCHAR)
                    m_bitshiftnext = true;
                else
                {
                    if (m_bitshiftnext == true)
                    {
                        b |= DataConstants.BIT5;
                        m_bitshiftnext = false;
                        m_bList.Add(b);
                    }
                    else if (b != DataConstants.FRAMECHAR) m_bList.Add(b);
                }

            }
            else if (m_storeend == true)
            {
                int framelen = m_bList.Count();
                if (framelen != 0)
                {
                    byte[] bArray = new byte[framelen];
                    bArray = m_bList.ToArray();
                    //Calculate checksum
                    byte checksum = 0x00;
                    for (int j = 0; j < (framelen - 1); j++)
                    {
                        checksum += bArray[j];
                    }
                    if (checksum == bArray[framelen - 1])
                    {
                        FrameList.Add(bArray);
                    }
                    m_bList.Clear();
                    m_storeend = false;
                }
                else
                {
                    m_storestart = true;
                    m_storeend = false;
                    m_fstart = false;
                }

            }

        }

        public void CreateRecordList()
        {
            //Read record from Framelist();

            int recorddatasize = 0;
            byte[] fullrecord = new byte[1490];

            foreach (byte[] fArray in FrameList)
            {
                datex_record_type record_dtx = new datex_record_type();

                for (int i = 0; i < fullrecord.GetLength(0); i++)
                {
                    fullrecord[i] = 0x00;
                }

                recorddatasize = fArray.GetLength(0);

                for (int n = 0; n < (fArray.GetLength(0)) && recorddatasize < 1490; n++)
                {
                    fullrecord[n] = fArray[n];
                }

                GCHandle handle2 = GCHandle.Alloc(fullrecord, GCHandleType.Pinned);
                Marshal.PtrToStructure(handle2.AddrOfPinnedObject(), record_dtx);

                RecordList.Add(record_dtx);
                handle2.Free();

            }


        }

        public void RequestTransfer(byte Trtype, short Interval, byte DRIlevel)
        {
            //Set Record Header
            request_ptr.hdr.r_len = 49; //size of hdr + phdb type
            request_ptr.hdr.r_dri_level = DRIlevel;
            request_ptr.hdr.r_time = 0;
            request_ptr.hdr.r_maintype = DataConstants.DRI_MT_PHDB;


            request_ptr.hdr.sr_offset1 = 0;
            request_ptr.hdr.sr_type1 = 0; // Physiological data request
            request_ptr.hdr.sr_offset2 = 0;
            request_ptr.hdr.sr_type2 = 0xFF; // Last subrecord

            // Request transmission subrecord
            request_ptr.phdbr.phdb_rcrd_type = Trtype;
            request_ptr.phdbr.tx_interval = Interval;
            if (Interval != 0)
                request_ptr.phdbr.phdb_class_bf =
   DataConstants.DRI_PHDBCL_REQ_BASIC_MASK | DataConstants.DRI_PHDBCL_REQ_EXT1_MASK |
   DataConstants.DRI_PHDBCL_REQ_EXT2_MASK | DataConstants.DRI_PHDBCL_REQ_EXT3_MASK;
            else request_ptr.phdbr.phdb_class_bf = 0x0000;

            //Get pointer to structure in memory
            IntPtr uMemoryValue = IntPtr.Zero;

            try
            {
                uMemoryValue = Marshal.AllocHGlobal(49);
                Marshal.StructureToPtr(request_ptr, uMemoryValue, true);
                Marshal.PtrToStructure(uMemoryValue, DPort_txbuf);
                WriteBuffer(DPort_txbuf.data);
            }
            finally
            {
                if (uMemoryValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(uMemoryValue);
                }
            }

            m_transmissionstart = true;


        }

        public void RequestWaveTransfer(byte TrWavetype, short TrSignaltype, byte DRIlevel)
        {
            //Set Record Header
            wave_request_ptr.hdr.r_len = 72; //size of hdr + wfreq type
            wave_request_ptr.hdr.r_dri_level = DRIlevel;
            wave_request_ptr.hdr.r_time = 0;
            wave_request_ptr.hdr.r_maintype = DataConstants.DRI_MT_WAVE;

            // The packet contains only one subrecord
            // 0 = Waveform data transmission request
            wave_request_ptr.hdr.sr_offset1 = 0;
            wave_request_ptr.hdr.sr_type1 = DataConstants.DRI_WF_CMD;
            wave_request_ptr.hdr.sr_offset2 = 0;
            wave_request_ptr.hdr.sr_type2 = DataConstants.DRI_EOL_SUBR_LIST; // Last subrecord

            // Request transmission subrecord
            //wave_request_ptr.wfreq.req_type = DataConstants.WF_REQ_CONT_START;
            wave_request_ptr.wfreq.req_type = TrSignaltype;
            wave_request_ptr.wfreq.res = 0;
            //wave_request_ptr.wfreq.type[0] = DataConstants.DRI_WF_ECG1;
            wave_request_ptr.wfreq.type[0] = TrWavetype;
            wave_request_ptr.wfreq.type[1] = DataConstants.DRI_EOL_SUBR_LIST;

            //Get pointer to structure in memory
            IntPtr uMemoryValue = IntPtr.Zero;

            try
            {
                uMemoryValue = Marshal.AllocHGlobal(72);
                Marshal.StructureToPtr(wave_request_ptr, uMemoryValue, true);
                Marshal.PtrToStructure(uMemoryValue, DPort_wave_txbuf);
                WriteBuffer(DPort_wave_txbuf.data);
            }
            finally
            {
                if (uMemoryValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(uMemoryValue);
                }
            }


        }

        public void RequestMultipleWaveTransfer(byte[] TrWavetype, short TrSignaltype, byte DRIlevel)
        {
            //Set Record Header
            wave_request_ptr.hdr.r_len = 72; //size of hdr + wfreq type
            wave_request_ptr.hdr.r_dri_level = DRIlevel;
            wave_request_ptr.hdr.r_time = 0;
            wave_request_ptr.hdr.r_maintype = DataConstants.DRI_MT_WAVE;

            // The packet contains only one subrecord
            // 0 = Waveform data transmission request
            wave_request_ptr.hdr.sr_offset1 = 0;
            wave_request_ptr.hdr.sr_type1 = DataConstants.DRI_WF_CMD;
            wave_request_ptr.hdr.sr_offset2 = 0;
            wave_request_ptr.hdr.sr_type2 = DataConstants.DRI_EOL_SUBR_LIST; // Last subrecord

            // Request transmission subrecord
            //wave_request_ptr.wfreq.req_type = DataConstants.WF_REQ_CONT_START;
            wave_request_ptr.wfreq.req_type = TrSignaltype;
            wave_request_ptr.wfreq.res = 0;
            //wave_request_ptr.wfreq.type[0] = DataConstants.DRI_WF_ECG1;

            //wave_request_ptr.wfreq.type[0] = TrWavetype;
            //wave_request_ptr.wfreq.type[1] = DataConstants.DRI_EOL_SUBR_LIST;
            for (int i = 0; i < 8; i++)
            {
                wave_request_ptr.wfreq.type[i] = TrWavetype[i];
                if (i < 7) wave_request_ptr.wfreq.type[i + 1] = DataConstants.DRI_EOL_SUBR_LIST;
            }

            //Get pointer to structure in memory
            IntPtr uMemoryValue = IntPtr.Zero;

            try
            {
                uMemoryValue = Marshal.AllocHGlobal(72);
                Marshal.StructureToPtr(wave_request_ptr, uMemoryValue, true);
                Marshal.PtrToStructure(uMemoryValue, DPort_wave_txbuf);
                WriteBuffer(DPort_wave_txbuf.data);
            }
            finally
            {
                if (uMemoryValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(uMemoryValue);
                }
            }

        }

        public void StopwaveTransfer()
        {
            RequestWaveTransfer(0, DataConstants.WF_REQ_CONT_STOP, DataConstants.DRI_LEVEL_2005);
            RequestWaveTransfer(0, DataConstants.WF_REQ_CONT_STOP, DataConstants.DRI_LEVEL_2003);
            RequestWaveTransfer(0, DataConstants.WF_REQ_CONT_STOP, DataConstants.DRI_LEVEL_2001);

        }

        public void StopTransfer()
        {
            //RequestTransfer(DataConstants.DRI_PH_60S_TREND, 0);
            RequestTransfer(DataConstants.DRI_PH_DISPL, 0, DataConstants.DRI_LEVEL_2005);
            RequestTransfer(DataConstants.DRI_PH_DISPL, 0, DataConstants.DRI_LEVEL_2003);
            RequestTransfer(DataConstants.DRI_PH_DISPL, 0, DataConstants.DRI_LEVEL_2001);

        }

        public void ReadSubRecords()
        {
            foreach (datex_record_type dx_record in RecordList)
            {

                short dxrecordmaintype = dx_record.hdr.r_maintype;

                if (dxrecordmaintype == DataConstants.DRI_MT_PHDB)
                {


                    short[] sroffArray = { dx_record.hdr.sr_offset1, dx_record.hdr.sr_offset2, dx_record.hdr.sr_offset3, dx_record.hdr.sr_offset4, dx_record.hdr.sr_offset5, dx_record.hdr.sr_offset6, dx_record.hdr.sr_offset7, dx_record.hdr.sr_offset8 };
                    byte[] srtypeArray = { dx_record.hdr.sr_type1, dx_record.hdr.sr_type2, dx_record.hdr.sr_type3, dx_record.hdr.sr_type4, dx_record.hdr.sr_type5, dx_record.hdr.sr_type6, dx_record.hdr.sr_type7, dx_record.hdr.sr_type8 };

                    uint unixtime = dx_record.hdr.r_time;
                    dri_phdb phdata_ptr = new dri_phdb();

                    for (int i = 0; i < 8 && (srtypeArray[i] != 0xFF); i++)
                    {
                        //if (srtypeArray[i] == DataConstants.DRI_PH_DISPL && srtypeArray[i] !=0xFF)
                        {
                            int offset = (int)sroffArray[i];

                            byte[] buffer = new byte[270];
                            for (int j = 0; j < 270; j++)
                            {
                                buffer[j] = dx_record.data[4 + j + offset];
                            }

                            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                            switch (i)
                            {
                                case 0:
                                    Marshal.PtrToStructure(handle.AddrOfPinnedObject(), phdata_ptr.basic);
                                    break;
                                case 1:
                                    Marshal.PtrToStructure(handle.AddrOfPinnedObject(), phdata_ptr.ext1);
                                    break;
                                case 2:
                                    Marshal.PtrToStructure(handle.AddrOfPinnedObject(), phdata_ptr.ext2);
                                    break;
                                case 3:
                                    Marshal.PtrToStructure(handle.AddrOfPinnedObject(), phdata_ptr.ext3);
                                    break;
                            }

                            handle.Free();

                        }
                    }

                    // Unix timestamp is seconds past epoch 
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    //dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
                    dtDateTime = dtDateTime.AddSeconds(unixtime);
                    m_strTimestamp = dtDateTime.ToString("G", DateTimeFormatInfo.InvariantInfo);

                    Console.WriteLine();
                    Console.WriteLine("Time:{0}", dtDateTime.ToString());

                    ShowBasicSubRecord(phdata_ptr);
                    ShowExt1and2SubRecord(phdata_ptr);

                    // if (m_dataexportset == 2) ExportNumValListToJSON();
                    SaveNumericValueListRows();

                }
            }


        }

        public void ReadMultipleWaveSubRecords()
        {

            foreach (datex_record_type dx_record in RecordList)
            {

                short dxrecordmaintype = dx_record.hdr.r_maintype;

                if (dxrecordmaintype == DataConstants.DRI_MT_WAVE)
                {

                    short[] sroffArray = { dx_record.hdr.sr_offset1, dx_record.hdr.sr_offset2, dx_record.hdr.sr_offset3, dx_record.hdr.sr_offset4, dx_record.hdr.sr_offset5, dx_record.hdr.sr_offset6, dx_record.hdr.sr_offset7, dx_record.hdr.sr_offset8 };
                    byte[] srtypeArray = { dx_record.hdr.sr_type1, dx_record.hdr.sr_type2, dx_record.hdr.sr_type3, dx_record.hdr.sr_type4, dx_record.hdr.sr_type5, dx_record.hdr.sr_type6, dx_record.hdr.sr_type7, dx_record.hdr.sr_type8 };

                    uint unixtime = dx_record.hdr.r_time;
                    // Unix timestamp is seconds past epoch 
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    //dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
                    dtDateTime = dtDateTime.AddSeconds(unixtime);

                    string dtime = dtDateTime.ToLongTimeString();

                    //Read upto 8 subrecords
                    for (int i = 0; i < 8 && (srtypeArray[i] != DataConstants.DRI_EOL_SUBR_LIST); i++)
                    {


                        int offset = (int)sroffArray[i];
                        int nextoffset = 0;

                        //read subrecord length from header to get nextoffset
                        byte[] srsamplelenbytes = new byte[2];
                        srsamplelenbytes[0] = dx_record.data[offset];
                        srsamplelenbytes[1] = dx_record.data[offset + 1];
                        int srheaderlen = 6;
                        int subrecordlen = srheaderlen + (BitConverter.ToInt16(srsamplelenbytes, 0)) * 2;
                        nextoffset = offset + subrecordlen;

                        int buflen = (nextoffset - offset - 6);

                        byte[] buffer = new byte[buflen];

                        for (int j = 0; j < buflen; j++)
                        {
                            buffer[j] = dx_record.data[6 + j + offset];
                        }

                        WaveValResult WaveVal = new WaveValResult();
                        WaveVal.Timestamp = dtime;
                        WaveVal.DeviceID = m_DeviceID;
                        WaveVal.PhysioID = Enum.GetName(typeof(DataConstants.WavesIDLabels), srtypeArray[i]);
                        WaveVal.Unitshift = GetWaveUnitShift(WaveVal.PhysioID);

                        List<short> WaveValList = new List<short>();

                        //Convert Byte array to 16 bit short values
                        for (int n = 0; n < buffer.Length; n += 2)
                        {

                            short wavedata = BitConverter.ToInt16(buffer, n);
                            WaveValList.Add(wavedata);

                        }

                        WaveVal.Value = new short[WaveValList.Count];
                        short[] wavedataarray = WaveValList.ToArray();
                        Array.Copy(wavedataarray, WaveVal.Value, WaveValList.Count);

                        m_WaveValResultList.Add(WaveVal);
                    }

                }
            }

            ExportWaveToCSV();

        }

        public double GetWaveUnitShift(string physioID)
        {
            double decimalshift = 1;

            if (physioID.Contains("ECG") == true)
                return (decimalshift = 0.01);
            if (physioID.Contains("INVP") == true)
                return (decimalshift = 0.01);
            if (physioID.Contains("PLETH") == true)
                return (decimalshift = 0.01);
            if (physioID.Contains("CO2") == true)
                return (decimalshift = 0.01);
            if (physioID.Contains("O2") == true)
                return (decimalshift = 0.01);
            if (physioID.Contains("RESP") == true)
                return (decimalshift = 0.01);
            if (physioID.Contains("AA") == true)
                return (decimalshift = 0.01);
            if (physioID.Contains("FLOW") == true)
                return (decimalshift = 0.01);
            if (physioID.Contains("AWP") == true)
                return (decimalshift = 0.1);
            if (physioID.Contains("VOL") == true)
                return (decimalshift = -1);
            if (physioID.Contains("EEG") == true)
                return (decimalshift = 1);
            else return decimalshift;

        }

        public void ShowBasicSubRecord(dri_phdb driSR)
        {
            short so1 = driSR.basic.ecg.hr;
            short so2 = driSR.basic.nibp.sys;
            short so3 = driSR.basic.nibp.dia;
            short so4 = driSR.basic.nibp.mean;
            short so5 = driSR.basic.SpO2.SpO2;
            short so6 = driSR.basic.co2.et;

            string s1 = ValidateAddData("ECG_HR", so1, 1, true);

            string s2 = ValidateAddData("NIBP_Systolic", so2, 0.01, true);

            string s3 = ValidateAddData("NIBP_Diastolic", so3, 0.01, true);

            string s4 = ValidateAddData("NIBP_Mean", so4, 0.01, true);

            string s5 = ValidateAddData("SpO2", so5, 0.01, true);

            double et = (so6 * driSR.basic.co2.amb_press);
            string s6 = ValidateAddData("ET_CO2", et, 0.00001, true);

            short so7 = driSR.basic.aa.et;
            short so8 = driSR.basic.aa.fi;
            short so9 = driSR.basic.aa.mac_sum;
            ushort so10 = driSR.basic.aa.hdr.label_info;

            ValidateAddData("AA_ET", so7, 0.01, false);
            ValidateAddData("AA_FI", so8, 0.01, false);
            string s9 = ValidateAddData("AA_MAC_SUM", so9, 0.01, false);

            string s10 = "";

            switch (so10)
            {
                case 0:
                    s10 = "Unknown";
                    break;
                case 1:
                    s10 = "None";
                    break;
                case 2:
                    s10 = "HAL";
                    break;
                case 3:
                    s10 = "ENF";
                    break;
                case 4:
                    s10 = "ISO";
                    break;
                case 5:
                    s10 = "DES";
                    break;
                case 6:
                    s10 = "SEV";
                    break;
            }

            AddDataString("Agent_AA", s10);

            double so11 = driSR.basic.o2.fi;
            double so12 = driSR.basic.n2o.fi;
            double so13 = driSR.basic.n2o.et;
            double so14 = driSR.basic.co2.rr;
            double so15 = driSR.basic.t1.temp;
            double so16 = driSR.basic.t2.temp;

            double so17 = driSR.basic.p1.hr;
            double so18 = driSR.basic.p1.sys;
            double so19 = driSR.basic.p1.dia;
            double so20 = driSR.basic.p1.mean;
            double so21 = driSR.basic.p2.hr;
            double so22 = driSR.basic.p2.sys;
            double so23 = driSR.basic.p2.dia;
            double so24 = driSR.basic.p2.mean;

            double so25 = driSR.basic.flow_vol.ppeak;
            double so26 = driSR.basic.flow_vol.pplat;
            double so27 = driSR.basic.flow_vol.tv_exp;
            double so28 = driSR.basic.flow_vol.tv_insp;
            double so29 = driSR.basic.flow_vol.peep;
            double so30 = driSR.basic.flow_vol.mv_exp;
            double so31 = driSR.basic.flow_vol.compliance;
            double so32 = driSR.basic.flow_vol.rr;


            ValidateAddData("O2_FI", so11, 0.01, false);
            ValidateAddData("N2O_FI", so12, 0.01, false);
            ValidateAddData("N2O_ET", so13, 0.01, false);
            ValidateAddData("CO2_RR", so14, 1, false);
            string s15 = ValidateAddData("T1_Temp", so15, 0.01, false);
            string s16 = ValidateAddData("T2_Temp", so16, 0.01, false);


            ValidateAddData("P1_HR", so17, 1, true);
            string s18 = ValidateAddData("P1_Systolic", so18, 0.01, true);
            string s19 = ValidateAddData("P1_Disatolic", so19, 0.01, true);
            string s20 = ValidateAddData("P1_Mean", so20, 0.01, true);
            ValidateAddData("P2_HR", so21, 1, true);
            string s22 = ValidateAddData("P2_Systolic", so22, 0.01, true);
            string s23 = ValidateAddData("P2_Diastolic", so23, 0.01, true);
            string s24 = ValidateAddData("P2_Mean", so24, 0.01, true);


            ValidateAddData("PPeak", so25, 0.01, true);
            ValidateAddData("PPlat", so26, 0.01, true);
            ValidateAddData("TV_Exp", so27, 0.1, true);
            ValidateAddData("TV_Insp", so28, 0.1, true);
            ValidateAddData("PEEP", so29, 0.01, true);
            ValidateAddData("MV_Exp", so30, 0.01, false);
            ValidateAddData("Compliance", so31, 0.01, true);
            ValidateAddData("RR", so32, 1, true);

            Console.WriteLine("ECG HR {0:d}/min NIBP {1:d}/{2:d}({3:d})mmHg SpO2 {4:d}% ETCO2 {5:d}mmHg", s1, s2, s3, s4, s5, s6);
            Console.WriteLine("IBP1 {0:d}/{1:d}({2:d})mmHg IBP2 {3:d}/{4:d}({5:d})mmHg MAC {6} T1 {7}°C T2 {8}°C", s18, s19, s20, s22, s23, s24, s9, s15, s16);

        }

        public void ShowExt1and2SubRecord(dri_phdb driSR)
        {
            short so1 = driSR.ext1.ecg12.stII;
            short so2 = driSR.ext1.ecg12.stV5;
            short so3 = driSR.ext1.ecg12.stAVL;

            string pathcsv = Path.Combine(Directory.GetCurrentDirectory(), "AS3ExportData.csv");

            string s1 = ValidateAddData("ST_II", so1, 0.01, false);

            string s2 = ValidateAddData("ST_V5", so2, 0.01, false);

            string s3 = ValidateAddData("ST_aVL", so3, 0.01, false);

            short so4 = driSR.ext2.ent.eeg_ent;
            short so5 = driSR.ext2.ent.emg_ent;
            short so6 = driSR.ext2.ent.bsr_ent;
            short so7 = driSR.ext2.eeg_bis.bis;
            short so8 = driSR.ext2.eeg_bis.sr_val;
            short so9 = driSR.ext2.eeg_bis.emg_val;
            short so10 = driSR.ext2.eeg_bis.sqi_val;

            ValidateAddData("EEG_Entropy", so4, 1, true);
            ValidateAddData("EMG_Entropy", so5, 1, true);
            ValidateAddData("BSR_Entropy", so6, 1, true);
            ValidateAddData("BIS", so7, 1, true);
            ValidateAddData("BIS_BSR", so8, 1, true);
            ValidateAddData("BIS_EMG", so9, 1, true);
            ValidateAddData("BIS_SQI", so10, 1, true);

            Console.WriteLine("ST II {0:0.0}mm ST V5 {1:0.0}mm ST aVL {2:0.0}mm", s1, s2, s3);


        }


        public string ValidateAddData(string physio_id, object value, double decimalshift, bool rounddata)
        {
            int num = Convert.ToInt32(value);
            double a = Convert.ToDouble(value, CultureInfo.InvariantCulture) * decimalshift;
            if (rounddata)
            {
                a = Math.Round(a);
            }
            string text = a.ToString();
            if (num < -32001)
            {
                text = "";
            }
            NumericValResult numericValResult = new NumericValResult();
            numericValResult.Timestamp = m_strTimestamp;
            numericValResult.PhysioID = physio_id;
            numericValResult.Value = text;
            numericValResult.DeviceID = m_DeviceID;
            m_NumericValList.Add(numericValResult);
            m_NumValHeaders.Add(numericValResult.PhysioID);
            SysDeviceData m = new SysDeviceData();
            if (physio_id.Equals("ECG_HR"))
            {

                m.dataName = "心率";
                m.dataCode = "xinlv";
                m.dataValue = text;

            }
            else if (physio_id.Equals("NIBP_Systolic"))
            {

                 m.dataName = "无创收缩";
                 m.dataCode = "wcssy";

                m.dataValue = text;

            }
            else if (physio_id.Equals("NIBP_Diastolic"))
            {

                 m.dataCode = "wcszy";
                 m.dataName = "无创舒张";
                m.dataValue = text;

            }
            else if (physio_id.Equals("NIBP_Mean"))
            {

                 m.dataCode = "wcpjy";
                 m.dataName = "无创平均";

                m.dataValue = text;

            }
            else if (physio_id.Equals("SpO2"))
            {

                 m.dataCode = "spo2";
                 m.dataName = "Spo2";

                m.dataValue = text;

            }
            else if (physio_id.Equals("ET_CO2"))
            {

                 m.dataCode = "etco2";
                 m.dataName = "ETCO2";

                m.dataValue = text;

            }
            else if (physio_id.Equals("RR"))
            {

                 m.dataCode = "hx";
                 m.dataName = "呼吸";

                m.dataValue = text;

            }
            else if (physio_id.Equals("P1_Systolic"))
            {

                 m.dataCode = "ycssy";
                 m.dataName = "有创收缩";

                m.dataValue = text;

            }
            else if (physio_id.Equals("P1_Disatolic"))
            {

                 m.dataCode = "ycszy";
                 m.dataName = "有创舒张";

                m.dataValue = text;

            }
            else if (physio_id.Equals("P1_Mean"))
            {

                 m.dataCode = "ycpjy";
                 m.dataName = "有创平均";

                m.dataValue = text;

            }
            else if (physio_id.Equals("T1_Temp"))
            {

                 m.dataCode = "t1";
                 m.dataName = "T1";

                m.dataValue = text;

            }
            else if (physio_id.Equals("T2_Temp"))
            {

                 m.dataCode = "t2";
                 m.dataName = "T2";
                m.dataValue = text;


            }
            else if (physio_id.Equals("BIS"))
            {

                 m.dataCode = "bis";
                 m.dataName = "BIS";
                m.dataValue = text;

            }
            else
            {
                 m.dataCode = physio_id;
                 m.dataName = physio_id;
                m.dataValue = text;
            }
            string geIp = ConfigurationManager.AppSettings["geIp"];
            m.deviceIp = geIp;
            m.createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"); ;
            bll.InsertOne(m);
            Console.Write("需要存入的数据：" +  m.dataName + "-------" + m.dataValue);
            log.Info("需要存入的数据：" + m.dataName + "-------" + m.dataValue);
            return null;
        }
       


        public void AddDataString(string physio_id, string valuestr)
        {
            NumericValResult NumVal = new NumericValResult();

            NumVal.Timestamp = m_strTimestamp;
            NumVal.PhysioID = physio_id;
            NumVal.Value = valuestr;
            NumVal.DeviceID = m_DeviceID;

            m_NumericValList.Add(NumVal);
            m_NumValHeaders.Add(NumVal.PhysioID);

        }

        public string ValidateWaveData(object value, double decimalshift, bool rounddata)
        {
            int val = Convert.ToInt32(value);
            double dval = (Convert.ToDouble(value, CultureInfo.InvariantCulture)) * decimalshift;
            if (rounddata) dval = Math.Round(dval);

            string str = dval.ToString();


            if (val < DataConstants.DATA_INVALID_LIMIT)
            {
                str = "-";
            }

            return str;
        }

        public void ExportWaveToCSV()
        {
            int wavevallistcount = m_WaveValResultList.Count;

            if (wavevallistcount != 0)
            {
                foreach (WaveValResult WavValResult in m_WaveValResultList)
                {
                    string WavValID = string.Format("{0}WaveExport.csv", WavValResult.PhysioID);

                    string pathcsv = Path.Combine(Directory.GetCurrentDirectory(), WavValID);

                    int wavvalarraylength = WavValResult.Value.GetLength(0);

                    double decimalshift = WavValResult.Unitshift;

                    for (int index = 0; index < wavvalarraylength; index++)
                    {
                        short Waveval = WavValResult.Value.ElementAt(index);

                        string Wavevalue = ValidateWaveData(Waveval, decimalshift, false);

                        m_strbuildwavevalues.Append(WavValResult.Timestamp);
                        m_strbuildwavevalues.Append(',');
                        m_strbuildwavevalues.Append(Wavevalue);
                        m_strbuildwavevalues.Append(',');
                        m_strbuildwavevalues.AppendLine();

                    }

                    ExportNumValListToCSVFile(pathcsv, m_strbuildwavevalues);

                    m_strbuildwavevalues.Clear();
                }

                m_WaveValResultList.RemoveRange(0, wavevallistcount);

            }

        }

        public void WriteNumericHeadersList()
        {
            if (m_NumericValList.Count != 0 && m_transmissionstart)
            {
                string pathcsv = Path.Combine(Directory.GetCurrentDirectory(), "AS3DataExport.csv");

                m_strbuildheaders.Append("Time");
                m_strbuildheaders.Append(',');

                foreach (NumericValResult NumValResult in m_NumericValList)
                {
                    m_strbuildheaders.Append(NumValResult.PhysioID);
                    m_strbuildheaders.Append(',');

                }

                m_strbuildheaders.Remove(m_strbuildheaders.Length - 1, 1);
                m_strbuildheaders.Replace(",,", ",");
                m_strbuildheaders.AppendLine();
                ExportNumValListToCSVFile(pathcsv, m_strbuildheaders);

                m_strbuildheaders.Clear();
                m_NumValHeaders.RemoveRange(0, m_NumValHeaders.Count);
                m_transmissionstart = false;

            }
        }


        public void SaveNumericValueListRows()
        {
            if (m_NumericValList.Count != 0)
            {
                WriteNumericHeadersList();
                string pathcsv = Path.Combine(Directory.GetCurrentDirectory(), "AS3DataExport.csv");

                m_strbuildvalues.Append(m_NumericValList.ElementAt(0).Timestamp);
                m_strbuildvalues.Append(',');

                foreach (NumericValResult NumValResult in m_NumericValList)
                {
                    m_strbuildvalues.Append(NumValResult.Value);
                    m_strbuildvalues.Append(',');

                }

                m_strbuildvalues.Remove(m_strbuildvalues.Length - 1, 1);
                m_strbuildvalues.Replace(",,", ",");
                m_strbuildvalues.AppendLine();

                ExportNumValListToCSVFile(pathcsv, m_strbuildvalues);
                m_strbuildvalues.Clear();
                m_NumericValList.RemoveRange(0, m_NumericValList.Count);
            }
        }


        public void ExportNumValListToCSVFile(string _FileName, StringBuilder strbuildNumVal)
        {
            try
            {
                // Open file for reading. 
                using (StreamWriter wrStream = new StreamWriter(_FileName, true, Encoding.UTF8))
                {
                    wrStream.Write(strbuildNumVal);
                    strbuildNumVal.Clear();

                    // close file stream. 
                    wrStream.Close();
                }
            }

            catch (Exception _Exception)
            {
                // Error. 
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

        }

        public bool ByteArrayToFile(string _FileName, byte[] _ByteArray, int nWriteLength)
        {
            try
            {
                // Open file for reading. 
                using (FileStream _FileStream = new FileStream(_FileName, FileMode.Append, FileAccess.Write))
                {
                    // Writes a block of bytes to this stream using data from a byte array
                    _FileStream.Write(_ByteArray, 0, nWriteLength);

                    // close file stream. 
                    _FileStream.Close();
                }

                return true;
            }

            catch (Exception _Exception)
            {
                // Error. 
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }
            // error occured, return false. 
            return false;
        }

        public bool OSIsUnix()
        {
            int platform = (int)Environment.OSVersion.Platform;
            if (platform == 2 || platform == 4 || platform == 6 || platform == 128)
            {
                return true;
            }
            return false;
        }
    }

}