using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace ConsoleApp2
{
    class apppp
    {
        List<string> comPortList;
        string receiveBuffer = "";
        string mesage = "";
        bool sendSpeedQury = false;
        double lastSpeed = 0;
        double lastSpeedTime = 0;
        double acc = 0.0;
        List<string> inFramList = new List<string>();


        SerialPort port;

        public class CanMessageData
        {
            byte[] Data = new byte[8];

            public CanMessageData(byte[] data)
            {
                Data = new byte[8];

                for (int i = 0; i < 8; i++)
                {
                    Data[i] = 0;
                }

                for (int i = 0; i < data.Length; i++)
                {
                    Data[i] = data[i];
                }
            }

            public byte GetByte(int index)
            {
                return Data[index];
            }
        }

        public class CanMessage
        {
            int mesgId;
            public CanMessageData date;

            public CanMessage(int MessageId, CanMessageData Date)
            {
                mesgId = MessageId;
                date = Date;
            }

            public int GetID()
            {
                return mesgId;
            }

        }

        public void stam2()
        {

            Console.WriteLine("program start");

            printPorts();
            Console.WriteLine("set serial port");

            Console.WriteLine("Enter port mane");
            string portName = Console.ReadLine();

            port = new SerialPort(portName, 115200);
            try
            {
                port.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);

                Console.WriteLine("press any key to exsit....");
                Console.ReadKey();
                return;
            }

            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;

                ReadSerial();

            }).Start();

            while (true)
            {
                Console.WriteLine("Enter command");
                string command = Console.ReadLine();


                switch (command)
                {

                    case "q":
                        break;
                }

                if (command == "q")
                {
                    Console.WriteLine("Exit program");
                    break;
                }

            }

            port.Close();
        }

        public void printPorts()
        {
            Console.WriteLine("current port");

            comPortList = new List<string>();
            foreach (string portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                comPortList.Add(portName);
                Console.WriteLine(portName);
            }
        }


        void ReadSerial()
        {
            string temp = "";

            while (port.IsOpen)
            {
                receiveBuffer += port.ReadExisting();

                if (receiveBuffer != "")
                {
                    receiveBuffer = temp + receiveBuffer;

                    mesage = receiveBuffer;
                    temp = "";
                    if (receiveBuffer.Contains("\r") && false)
                    {
                        string[] split = receiveBuffer.Split('\r');
                        char[] chararry = receiveBuffer.ToArray();

                        if (chararry[chararry.Count() - 1] != '\r')
                        {
                            temp = split[split.Count() - 1];
                        }

                        int limit = split.Count();

                        if (temp != "")
                        {
                            limit--;
                            receiveBuffer = receiveBuffer.Substring(0, receiveBuffer.Length - temp.Length);
                        }


                        for (int i = 0; i < limit; i++)
                        {
                            string mess = split[i];
                            //t7E8803410D0055555555
                            if ((mess.Contains("t7E") || mess.Contains("t3E9")) && false)
                            {
                                inFramList.Add(mess);
                                //t7E8803410D0055555555
                                string t = mess.Substring(9, 2);

                                {
                                    double speedDataKmPerH = 0;
                                    if (mess.Contains("t3E9"))
                                    {
                                        double speedDatamilsPerH = Convert.ToInt16(mess.Substring(5, 4), 16);
                                        speedDataKmPerH = Convert.ToInt16((double)((double)speedDatamilsPerH / 100) / 0.62137);
                                    }
                                    else if (mess.Substring(9, 2) == "0D" && mess.Contains("t7E"))
                                    {
                                        speedDataKmPerH = Convert.ToInt16(mess.Substring(11, 2), 16);
                                    }

                                    t = mess.Substring(mess.Length - 4, 4);
                                    double speedDataTitmeS = (double)Convert.ToInt16(mess.Substring(mess.Length - 4, 4), 16) / 1000;
                                    string toText = speedDataKmPerH.ToString();

                                    double temp_speedDataTitmeS = speedDataTitmeS;

                                    if (speedDataTitmeS < lastSpeedTime)
                                    {
                                        temp_speedDataTitmeS += 60;
                                    }

                                    acc = (double)(speedDataKmPerH - lastSpeed) / (double)(temp_speedDataTitmeS - lastSpeedTime);
                                    //acc = (double)acc;

                                    new System.Threading.Thread(() =>
                                    {


                                    }).Start();

                                    lastSpeedTime = speedDataTitmeS;
                                    lastSpeed = speedDataKmPerH;
                                }

                                //this.Invoke(new EventHandler(DisplayText));

                            }

                            if (mess.Contains("t0F1") && true)
                            {
                                //01234 56 78 9  11 13 15 17 19 
                                //t3E98 00 00 00 13 00 00 00 13

                                //"t0F1 4 34 00 00 40 9633"
                                string t;

                                {
                                    double press = Convert.ToInt16(mess.Substring(7, 2), 16);
                                    //double speedDataKmPerH = (double)((double)speedDatamilsPerH / 100) / 0.62137;
                                    t = mess.Substring(mess.Length - 4, 4);
                                    double pressDataTitmeS = Convert.ToUInt16(t, 16);
                                    string toText = press.ToString();

                                    double temp_speedDataTitmeS = pressDataTitmeS;

                                    if (pressDataTitmeS < lastSpeedTime)
                                    {
                                        temp_speedDataTitmeS += 60;
                                    }

                                    acc = press; // (double)(speedDataKmPerH - lastSpeed) / (double)(temp_speedDataTitmeS - lastSpeedTime);
                                    //acc = (double)acc;

                                    new System.Threading.Thread(() =>
                                    {
                                        System.Threading.Thread.CurrentThread.IsBackground = true;

                                        //label3.Invoke(new Action(() => label3.Text = toText));

                                        //label4.Invoke(new Action(() => label4.Text = acc.ToString("F3")));

                                        //this.Invoke(new EventHandler(DisplayPics));

                                    }).Start();


                                    lastSpeedTime = pressDataTitmeS;
                                    //lastSpeed = speedDataKmPerH;
                                }

                                //this.Invoke(new EventHandler(DisplayText));

                            }

                            // t 7E8 8 03 41 0D 00 55 55 55 55
                            //"t19D8C0003FFD000BD9FFCBA3"
                            if (mess.Contains("t") && false)
                            {
                                try
                                {
                                    int mesid = Convert.ToInt16(mess.Substring(1, 3), 16);

                                    int lng = Convert.ToInt16(mess.Substring(4, 1), 16);

                                    byte[] data = new byte[lng];

                                    for (int indx = 0; indx < lng; indx++)
                                    {
                                        data[indx] = Convert.ToByte(mess.Substring(5 + indx + (indx * 2), 2), 16);
                                    }

                                    CanMessageData candata;
                                    candata = new CanMessageData(data);
                                    CanMessage message = new CanMessage(mesid, candata);

                                    //datagridUpdataInfo(message);
                                }
                                catch
                                {

                                }
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("recive:" +receiveBuffer);
                        port.Write(receiveBuffer);
                    }

                    receiveBuffer = "";


                }

            }

        }


    }
}
