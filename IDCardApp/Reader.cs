using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IDCardApp
{
    /// <summary>
    /// 精伦idr210机器读卡接口
    /// </summary>
    public class Reader
    {
        #region dll引用
        [DllImport("Sdtapi.dll")]
        private static extern int InitComm(int iPort);
        [DllImport("Sdtapi.dll")]
        private static extern int Authenticate();
        [DllImport("Sdtapi.dll")]
        private static extern int ReadIINSNDN(StringBuilder pMsg);

        /// <summary>
        /// 读取身份证中基本信息，包括文字信息与图像信息。文字信息以字符串格式输出。照片信息被解码后存为文件photo.bmp，存于当前目录
        /// </summary>
        /// <param name="Name">姓名 调用时分配内存，字节数不小31</param>
        /// <param name="Gender">性别信息（男或者女）需要在调用时分配内存，字节数不小3 </param>
        /// <param name="Folk">民族信息。需要在调用时分配内存，字节数不小10。</param>
        /// <param name="BirthDay">出生日期信息。需要在调用时分配内存，字节数不小9，前四位为出生年，第5位到第6位是出生月，后两位是出生日，格式为：CCYYMMDD</param>
        /// <param name="Code">身份证号码信息。需要在调用时分配内存，字节数不小19</param>
        /// <param name="Address">地址信息。需要在调用时分配内存，字节数不小71。</param>
        /// <param name="Agency">签证机关信息。需要在调用时分配内存，字节数不小31。</param>
        /// <param name="ExpireStart">有效期起始日期信息。需要在调用时分配内存，字节数不小9, 格式为：CCYYMMDD。</param>
        /// <param name="ExpireEnd">有效期截至日期信息。需要在调用时分配内存，字节数不小9，格式为：CCYYMMDD，有效期为长期的表示为汉字“长期”。</param>
        [DllImport("Sdtapi.dll")]
        private static extern int ReadBaseInfos(
            StringBuilder Name, StringBuilder Gender, StringBuilder Folk,
            StringBuilder BirthDay, StringBuilder Code, StringBuilder Address,
            StringBuilder Agency, StringBuilder ExpireStart, StringBuilder ExpireEnd);

        [DllImport("Sdtapi.dll")]
        private static extern int CloseComm();
        [DllImport("Sdtapi.dll")]
        private static extern int ReadBaseMsg(byte[] pMsg, ref int len);
        [DllImport("Sdtapi.dll")]
        private static extern int ReadBaseMsgW(byte[] pMsg, ref int len);

        [DllImport("Sdtapi.dll")]
        private static extern int Routon_IC_HL_ReadCardSN(StringBuilder SN);

        [DllImport("Sdtapi.dll")]
        private static extern int HID_BeepLED(bool BeepON, bool LEDON, int duration);

        [DllImport("Sdtapi.dll")]
        private static extern int Routon_IC_HL_WriteCard(int SID, int BID, int KeyType, byte[] Key, byte[] data);

        [DllImport("Sdtapi.dll")]
        private static extern int Routon_IC_FindCard();
        #endregion

        private static Reader instance = null;
        private static object lockObj = new object();
        public static Reader Singleton
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        instance = new Reader();
                    }
                }
                return instance;
            }
        }

        private int ConnectPort = -1;

        public bool IsConnecting
        {
            get; private set;
        }

        public bool Connect()
        {
            int[] ports = { 1001, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0x10 };
            int result = 1000;
            try
            {
                //如果 第一次初始化端口
                if (ConnectPort == -1)
                {
                    foreach (int p in ports)
                    {
                        result = InitComm(p);
                        if (result == 1)
                        {
                            ConnectPort = p;
                            break;
                        }
                    }
                }
                else
                {
                    result = InitComm(ConnectPort);
                }
            }
            catch (Exception ex)
            {                
                //LogHelp.LogError("idr210连接模块", ex);
            }
            IsConnecting = result == 1;
            return IsConnecting;
        }

        public void DisConnect()
        {
            CloseComm();
            IsConnecting = false;
        }

        private string ConvertDate(StringBuilder date)
        {
            var str = date.ToString();
            if (str.Length == 0 || str == "长期") return string.Empty;
            return string.Format("{0}-{1}-{2}",
                str.Substring(0, 4), str.Substring(4, 2), str.Substring(6, 2));
        }

        public Entity.UserInfo ReadIdentityCard()
        {
            if (Authenticate() != 1) return null;

            var pmsg = new StringBuilder(16);
            if (ReadIINSNDN(pmsg) != 1) return null;

            var name = new StringBuilder(31);
            var gender = new StringBuilder(3);
            var folk = new StringBuilder(10);
            var birth = new StringBuilder(9);
            var identityNum = new StringBuilder(19);
            var address = new StringBuilder(71);
            var agency = new StringBuilder(31);
            var validBegin = new StringBuilder(9);
            var validEnd = new StringBuilder(9);

            var readResult = ReadBaseInfos(name, gender, folk, birth, identityNum, address, agency, validBegin, validEnd);
            if (readResult == 1)
            {
                return new Entity.UserInfo()
                {
                    Name = name.ToString(),
                    Gender = gender.ToString(),
                    Nationality = folk.ToString(),
                    Birth = ConvertDate(birth),
                    IdentityNumber = identityNum.ToString(),
                    Address = address.ToString(),
                    IssueOffice = agency.ToString(),
                    ValidBegin = ConvertDate(validBegin),
                    ValidEnd = ConvertDate(validEnd)
                };
                
            }
            return null;
        }

        public string ReadICCardNO()
        {
            var sn = new StringBuilder(16);
            if(Routon_IC_HL_ReadCardSN(sn) == 1)
            {
                return sn.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 写IC卡信息
        /// </summary>
        /// <param name="sector">扇区号，0-15之间（对M1S50卡）</param>
        /// <param name="block">块号，0-3之间,正常之写123，0块不写</param>
        /// <param name="keyType">蜜钥类型，两种：0x60 keyA，0x61 keyB，本程序只用keyA</param>
        /// <param name="key">密钥</param>
        /// <param name="data">准备写入的数据内容，字节数为16</param>
        /// <returns>0：未找到卡 1：写卡成功 3其他异常</returns>
        public int WriteCard(int sector, int block, int keyType, byte[] key, byte[] data)
        {
            int result = 0;
            try
            {
                result = Routon_IC_FindCard();
                switch (result)
                {
                    case 0://未找到卡
                        throw new Exception("未找到卡");
                    //break;
                    case 1://M1-S50卡
                        break;
                    case 3://M1-S70卡
                        break;
                    case 2://CPU卡
                        break;
                    default:
                        throw new Exception("未知错误！");
                }
                if (result != 0 && result <= 3)
                {
                    result = Routon_IC_HL_WriteCard(sector, block, keyType, key, data);
                    if (result == 3)
                        throw new Exception("密钥或卡类型错误！");
                }
                else
                    throw new Exception("未找到卡");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public int WriteCard(int sector, string hexStr)
        {
            var keyType = 0x60;
            var key = HexStr2Bytes("4DC3C5BDFB6A");

            int block= 0;
            int result = 0;
            for(int i=0; i<hexStr.Length; i+=32)
            {
                var sub = hexStr.Substring(i, Math.Min(hexStr.Length - i, 32));
                var subData = HexStr2Bytes(sub);
                result = WriteCard(sector, block++, keyType, key, subData);
            }
            return result;
        }

        public byte[] HexStr2Bytes(string hexStr)
        {
            if (hexStr.Length % 2 != 0)
                hexStr += " ";
            var bytes = new byte[hexStr.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}
