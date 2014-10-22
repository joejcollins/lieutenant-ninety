﻿using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.Text;

namespace SvnAutomation
{
    public class Student
    {
        private string login;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        public Student(string login)
        {
            this.login = login;
        }

        public string Login
        {
            get
            {
                return login;
            }
        }

        public string Sid
        {
            get
            {
                return GetSid("Harper-Adams\\" + login);
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strLogin"></param>
        /// <returns></returns>
        private string GetSid(string strLogin)
        {
            string str = "";
            // Parse the string to check if domain name is present.

            int idx = strLogin.IndexOf('\\');
            if (idx == -1)
            {
                idx = strLogin.IndexOf('@');
            }

            string strDomain;
            string strName;

            if (idx != -1)
            {
                strDomain = strLogin.Substring(0, idx);
                strName = strLogin.Substring(idx + 1);
            }
            else
            {
                strDomain = Environment.MachineName;
                strName = strLogin;
            }
            
            DirectoryEntry obDirEntry = null;
            try
            {
                Int64 iBigVal = 5;
                Byte[] bigArr = BitConverter.GetBytes(iBigVal);
                obDirEntry = new DirectoryEntry("WinNT://" + strDomain + "/" + strName);
                System.DirectoryServices.PropertyCollection coll = obDirEntry.Properties;
                object obVal = coll["objectSid"].Value;
                if (null != obVal)
                {
                    str = this.ConvertByteToStringSid((Byte[])obVal);
                }

            }
            catch (Exception exception)
            {
                str = "";
                Program.traceSource.TraceEvent(TraceEventType.Error, 0, exception.Message);
            }
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sidBytes"></param>
        /// <returns></returns>
        private string ConvertByteToStringSid(Byte[] sidBytes)
        {
            StringBuilder strSid = new StringBuilder();
            strSid.Append("S-");
            try
            {
                // Add SID revision.

                strSid.Append(sidBytes[0].ToString());
                // Next six bytes are SID authority value.

                if (sidBytes[6] != 0 || sidBytes[5] != 0)
                {
                    string strAuth = String.Format
                        ("0x{0:2x}{1:2x}{2:2x}{3:2x}{4:2x}{5:2x}",
                        (Int16)sidBytes[1],
                        (Int16)sidBytes[2],
                        (Int16)sidBytes[3],
                        (Int16)sidBytes[4],
                        (Int16)sidBytes[5],
                        (Int16)sidBytes[6]);
                    strSid.Append("-");
                    strSid.Append(strAuth);
                }
                else
                {
                    Int64 iVal = (Int32)(sidBytes[1]) +
                        (Int32)(sidBytes[2] << 8) +
                        (Int32)(sidBytes[3] << 16) +
                        (Int32)(sidBytes[4] << 24);
                    strSid.Append("-");
                    strSid.Append(iVal.ToString());
                }

                // Get sub authority count...
                int iSubCount = Convert.ToInt32(sidBytes[7]);
                int idxAuth = 0;
                for (int i = 0; i < iSubCount; i++)
                {
                    idxAuth = 8 + i * 4;
                    UInt32 iSubAuth = BitConverter.ToUInt32(sidBytes, idxAuth);
                    strSid.Append("-");
                    strSid.Append(iSubAuth.ToString());
                }
            }
            catch (Exception exception)
            {
                Program.traceSource.TraceEvent(TraceEventType.Error, 0, exception.Message);
                return "";
            }
            return strSid.ToString();
        }
    }
}


