﻿using IoTClient.Core;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace IoTClient.Clients.ModBus
{
    /// <summary>
    /// ModBusRtu协议客户端
    /// </summary>
    public class ModBusTcpClient : SocketBase
    {
        private IPEndPoint ipAndPoint;
        public ModBusTcpClient(IPEndPoint ipAndPoint)
        {
            this.ipAndPoint = ipAndPoint;
        }

        public ModBusTcpClient(string ip, int port)
        {
            this.ipAndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        protected override bool Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.ReceiveTimeout = 1500;
                socket.SendTimeout = 1500;
                //连接
                socket.Connect(ipAndPoint);
                return true;
            }
            catch (Exception)
            {
                if (socket.Connected) socket?.Shutdown(SocketShutdown.Both);
                socket?.Close();
                return false;
            }
        }

        /// <summary>
        /// 连接管理
        /// </summary>
        private void ConnectManager()
        {
            if (!isAutoOpen.HasValue) isAutoOpen = true;
            if (isAutoOpen.Value) Connect();
            else if (!socket.Connected)
                Connect();
        }

        #region Read 读取
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="readLength">读取长度</param>
        /// <returns></returns>
        public Result<byte[]> Read(ushort address, byte stationNumber = 1, byte functionCode = 3, ushort readLength = 1)
        {
            ConnectManager();
            var readResult = new Result<byte[]>();
            try
            {
                //1 获取命令（组装报文）
                byte[] command = GetReadCommand(address, stationNumber, functionCode, readLength);
                //2 发送命令
                socket.Send(command);
                //3 获取响应报文
                var headBytes = SocketRead(socket, 8);
                int length = headBytes[4] * 256 + headBytes[5] - 2;
                var result = SocketRead(socket, length);

                byte[] resultBuffer = new byte[result.Length - 1];
                Array.Copy(result, 1, resultBuffer, 0, resultBuffer.Length);
                //4 获取响应报文数据（字节数组形式）                
                readResult.Value = resultBuffer.Reverse().ToArray();
            }
            catch (SocketException ex)
            {
                readResult.IsSucceed = false;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    readResult.Err = "连接超时";
                    readResult.ErrList.Add("连接超时");
                }
                else
                {
                    readResult.Err = ex.Message;
                    readResult.ErrList.Add(ex.Message);
                }
            }
            finally
            {
                if (isAutoOpen.Value) Dispose();
            }
            return readResult;
        }

        /// <summary>
        /// 读取Int16
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public Result<short> ReadInt16(ushort address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode);
            var result = new Result<short>()
            {
                IsSucceed = readResut.IsSucceed,
                Err = readResut.Err,
                ErrList = readResut.ErrList,
            };
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt16(readResut.Value, 0);
            return result;
        }

        /// <summary>
        /// 读取Int32
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public Result<int> ReadInt32(ushort address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 2);
            var result = new Result<int>()
            {
                IsSucceed = readResut.IsSucceed,
                Err = readResut.Err,
                ErrList = readResut.ErrList,
            };
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt32(readResut.Value, 0);
            return result;
        }

        /// <summary>
        /// 读取Int64
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public Result<long> ReadInt64(ushort address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 2);
            var result = new Result<long>()
            {
                IsSucceed = readResut.IsSucceed,
                Err = readResut.Err,
                ErrList = readResut.ErrList,
            };
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt64(readResut.Value, 0);
            return result;
        }

        /// <summary>
        /// 读取Float
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public Result<float> ReadFloat(ushort address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 2);
            var result = new Result<float>()
            {
                IsSucceed = readResut.IsSucceed,
                Err = readResut.Err,
                ErrList = readResut.ErrList,
            };
            if (result.IsSucceed)
                result.Value = BitConverter.ToSingle(readResut.Value, 0);
            return result;
        }

        /// <summary>
        /// 读取Double
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public Result<double> ReadDouble(ushort address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 2);
            var result = new Result<double>()
            {
                IsSucceed = readResut.IsSucceed,
                Err = readResut.Err,
                ErrList = readResut.ErrList,
            };
            if (result.IsSucceed)
                result.Value = BitConverter.ToDouble(readResut.Value, 0);
            return result;
        }

        /// <summary>
        /// 读取线圈
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public Result<bool> ReadCoil(ushort address, byte stationNumber = 1, byte functionCode = 1)
        {
            var readResut = Read(address, stationNumber, functionCode);
            var result = new Result<bool>()
            {
                IsSucceed = readResut.IsSucceed,
                Err = readResut.Err,
                ErrList = readResut.ErrList,
            };
            if (result.IsSucceed)
                result.Value = BitConverter.ToBoolean(readResut.Value, 0);
            return result;
        }
        #endregion

        #region Write 写入
        /// <summary>
        /// 线圈写入
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <param name="stationNumber"></param>
        /// <param name="functionCode"></param>
        public Result Write(ushort address, bool value, byte stationNumber = 1, byte functionCode = 5)
        {
            ConnectManager();
            var result = new Result();
            try
            {
                var command = GetWriteCoilCommand(address, value, stationNumber, functionCode);
                socket.Send(command);
            }
            catch (SocketException ex)
            {
                result.IsSucceed = false;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    result.Err = "连接超时";
                    result.ErrList.Add("连接超时");
                }
                else
                {
                    result.Err = ex.Message;
                    result.ErrList.Add(ex.Message);
                }
            }
            finally
            {
                if (isAutoOpen.Value) Dispose();
            }
            return result;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public Result Write(ushort address, short value, byte stationNumber = 1, byte functionCode = 16)
        {
            ConnectManager();
            var result = new Result();
            try
            {
                var values = BitConverter.GetBytes(value).Reverse().ToArray();
                var command = GetWriteCommand(address, values, stationNumber, functionCode);
                socket.Send(command);

                //获取响应报文
                var headBytes = SocketRead(socket, 8);
                int length = headBytes[4] * 256 + headBytes[5] - 2;
                SocketRead(socket, length);
            }
            catch (SocketException ex)
            {
                result.IsSucceed = false;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    result.Err = "连接超时";
                    result.ErrList.Add("连接超时");
                }
                else
                {
                    result.Err = ex.Message;
                    result.ErrList.Add(ex.Message);
                }
            }
            finally
            {
                if (isAutoOpen.Value) Dispose();
            }
            return result;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public Result Write(ushort address, int value, byte stationNumber = 1, byte functionCode = 16)
        {
            ConnectManager();
            var result = new Result();
            try
            {
                var values = BitConverter.GetBytes(value).Reverse().ToArray();
                var command = GetWriteCommand(address, values, stationNumber, functionCode);
                socket.Send(command);

                //获取响应报文
                var headBytes = SocketRead(socket, 8);
                int length = headBytes[4] * 256 + headBytes[5] - 2;
                SocketRead(socket, length);
            }
            catch (SocketException ex)
            {
                result.IsSucceed = false;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    result.Err = "连接超时";
                    result.ErrList.Add("连接超时");
                }
                else
                {
                    result.Err = ex.Message;
                    result.ErrList.Add(ex.Message);
                }
            }
            finally
            {
                if (isAutoOpen.Value) Dispose();
            }
            return result;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public Result Write(ushort address, long value, byte stationNumber = 1, byte functionCode = 16)
        {
            ConnectManager();
            var result = new Result();
            try
            {
                var values = BitConverter.GetBytes(value).Reverse().ToArray();
                var command = GetWriteCommand(address, values, stationNumber, functionCode);
                socket.Send(command);

                //获取响应报文
                var headBytes = SocketRead(socket, 8);
                int length = headBytes[4] * 256 + headBytes[5] - 2;
                SocketRead(socket, length);
            }
            catch (SocketException ex)
            {
                result.IsSucceed = false;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    result.Err = "连接超时";
                    result.ErrList.Add("连接超时");
                }
                else
                {
                    result.Err = ex.Message;
                    result.ErrList.Add(ex.Message);
                }
            }
            finally
            {
                if (isAutoOpen.Value) Dispose();
            }
            return result;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public Result Write(ushort address, float value, byte stationNumber = 1, byte functionCode = 16)
        {
            ConnectManager();
            var result = new Result();
            try
            {
                var values = BitConverter.GetBytes(value).Reverse().ToArray();
                var command = GetWriteCommand(address, values, stationNumber, functionCode);
                socket.Send(command);

                //获取响应报文
                var headBytes = SocketRead(socket, 8);
                int length = headBytes[4] * 256 + headBytes[5] - 2;
                SocketRead(socket, length);
            }
            catch (SocketException ex)
            {
                result.IsSucceed = false;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    result.Err = "连接超时";
                    result.ErrList.Add("连接超时");
                }
                else
                {
                    result.Err = ex.Message;
                    result.ErrList.Add(ex.Message);
                }
            }
            finally
            {
                if (isAutoOpen.Value) Dispose();
            }
            return result;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public Result Write(ushort address, double value, byte stationNumber = 1, byte functionCode = 16)
        {
            ConnectManager();
            var result = new Result();
            try
            {
                var values = BitConverter.GetBytes(value).Reverse().ToArray();
                var command = GetWriteCommand(address, values, stationNumber, functionCode);
                socket.Send(command);

                //获取响应报文
                var headBytes = SocketRead(socket, 8);
                int length = headBytes[4] * 256 + headBytes[5] - 2;
                SocketRead(socket, length);
            }
            catch (SocketException ex)
            {
                result.IsSucceed = false;
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    result.Err = "连接超时";
                    result.ErrList.Add("连接超时");
                }
                else
                {
                    result.Err = ex.Message;
                    result.ErrList.Add(ex.Message);
                }
            }
            finally
            {
                if (isAutoOpen.Value) Dispose();
            }
            return result;
        }
        #endregion

        #region 获取命令

        /// <summary>
        /// 获取读取命令
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public byte[] GetReadCommand(ushort address, byte stationNumber, byte functionCode, ushort length)
        {
            byte[] buffer = new byte[12];
            buffer[0] = 0x19;
            buffer[1] = 0xB2;//Client发出的检验信息
            buffer[2] = 0x00;
            buffer[3] = 0x00;//表示tcp/ip 的协议的modbus的协议
            buffer[4] = 0x00;
            buffer[5] = 0x06;//表示的是该字节以后的字节长度

            buffer[6] = stationNumber;  //站号
            buffer[7] = functionCode;   //功能码
            buffer[8] = BitConverter.GetBytes(address)[1];
            buffer[9] = BitConverter.GetBytes(address)[0];//寄存器地址
            buffer[10] = BitConverter.GetBytes(length)[1];
            buffer[11] = BitConverter.GetBytes(length)[0];//表示request 寄存器的长度(寄存器个数)
            return buffer;
        }

        /// <summary>
        /// 获取写入命令
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="values"></param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public byte[] GetWriteCommand(ushort address, byte[] values, byte stationNumber, byte functionCode)
        {
            byte[] buffer = new byte[13 + values.Length];
            buffer[0] = 0x19;
            buffer[1] = 0xB2;//检验信息，用来验证response是否串数据了           
            buffer[4] = BitConverter.GetBytes(7 + values.Length)[1];
            buffer[5] = BitConverter.GetBytes(7 + values.Length)[0];//表示的是header handle后面还有多长的字节

            buffer[6] = stationNumber; //站号
            buffer[7] = functionCode;  //功能码
            buffer[8] = BitConverter.GetBytes(address)[1];
            buffer[9] = BitConverter.GetBytes(address)[0];//寄存器地址
            buffer[10] = (byte)(values.Length / 2 / 256);
            buffer[11] = (byte)(values.Length / 2 % 256);//写寄存器数量(除2是两个字节一个寄存器，寄存器16位。除以256是byte最大存储255。)              
            buffer[12] = (byte)(values.Length);          //写字节的个数
            values.CopyTo(buffer, 13);                   //把目标值附加到数组后面
            return buffer;
        }

        /// <summary>
        /// 获取线圈写入命令
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value"></param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public byte[] GetWriteCoilCommand(ushort address, bool value, byte stationNumber, byte functionCode)
        {
            byte[] buffer = new byte[12];
            buffer[0] = 0x19;
            buffer[1] = 0xB2;//Client发出的检验信息     
            buffer[4] = 0x00;
            buffer[5] = 0x06;//表示的是该字节以后的字节长度

            buffer[6] = stationNumber;//站号
            buffer[7] = functionCode; //功能码
            buffer[8] = BitConverter.GetBytes(address)[1];
            buffer[9] = BitConverter.GetBytes(address)[0];//寄存器地址
            buffer[10] = (byte)(value ? 0xFF : 0x00);     //此处只可以是FF表示闭合00表示断开，其他数值非法
            buffer[11] = 0x00;
            return buffer;
        }

        #endregion      
    }
}