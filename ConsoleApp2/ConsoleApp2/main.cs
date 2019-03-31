using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

using System.IO.Ports;

namespace ConsoleApp2
{
    class main
    {
        List<string> comPortList;
        string receiveBuffer = "";
        string receiveBuffer_i = "";
        string mesage = "";
        bool sendSpeedQury = false;
        double lastSpeed = 0;
        double lastSpeedTime = 0;
        double acc = 0.0;
        List<string> inFramList = new List<string>();


        SerialPort interface_port;
        SerialPort main_port;
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

        void printMessage(string mes)
        {
            Console.WriteLine(mes);
            if (interface_port!= null)
            {
                if (interface_port.IsOpen)
                    interface_port.WriteLine(mes+"\r\n");
            }
            
            
        }

        List<Gpio> LedList = new List<Gpio>();
        void initGpios()
        {

            LedList.Add(new Gpio(26));
            LedList.Add(new Gpio(19));
            LedList.Add(new Gpio(13));
            LedList.Add(new Gpio(6));
            LedList.Add(new Gpio(5));
            LedList.Add(new Gpio(12));
            LedList.Add(new Gpio(16));
            LedList.Add(new Gpio(20));
            LedList.Add(new Gpio(21));
        }

        public void ExecuteCommand(string command)
        {
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \" " + command + " \"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                Console.WriteLine(proc.StandardOutput.ReadLine());
            }
        }

        public void start()
        {
             
            printMessage("program start");
            printMessage("Execute Command");
            ExecuteCommand("sudo chmod -R 666 /dev/ttyS0");
            printMessage("init GPIOs");

            initGpios();

            printPorts();
            printMessage("set serial port");

            printMessage("interface port mane /dev/ttyS0");
            string portName = "";
            portName = "/dev/ttyS0";  
            printMessage("Interface open port in " + portName);
            interface_port = new SerialPort(portName, 115200);
            try
            {
                interface_port.Open();
                interface_port.Write("port opend"); 
                new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.CurrentThread.IsBackground = true;
                    ReadSerial_interface();
                }).Start();
            }
            catch(Exception e)
            {
                printMessage(e.Message);
                return;
            }

            portName = "empty staff";
            printMessage("Main port mane /dev/ttyUSB0");
            portName = "/dev/ttyUSB0";

            printMessage("open port in " + portName);
            main_port = new SerialPort(portName, 115200);
            try
            {
                main_port.Open();
                new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.CurrentThread.IsBackground = true;
                    ReadSerial_main();
                }).Start();
            }
            catch (Exception e)
            {
                printMessage(e.Message);
                return;
            }


           
            //pin26.SetPin();

            //Console.WriteLine("Commands\n"+
            //                  "setPin26\n"+
            //                  "setPin26-1\n"+
            //                  "setPin26-0\n"+
            //                  "q"
            //    );


            while (true)
            {
                for (int i=0;i<LedList.Count;i++)
                {
                    LedList[i].SetState(Gpio.PinStat.Hi);
                }
                
                Console.WriteLine("set pin 26 hi");
                interface_port.WriteLine("set pin 26 hi");
                Thread.Sleep(500);
                for (int i = 0; i < LedList.Count; i++)
                {
                    LedList[i].SetState(Gpio.PinStat.Low);
                }
                
                interface_port.WriteLine("set pin 26 low");
                Console.WriteLine("set pin 26 low");
                Thread.Sleep(500);

                //Console.WriteLine("Enter command");
                //string command = Console.ReadLine();


                //switch (command)
                //{

                //    case "setPin26":
                //        pin26.SetPin();
                //        break;
                //    case "setPin26-1":
                //        pin26.SetState(Gpio.PinStat.Hi);
                //        break;
                //    case "setPin2-0":
                //        pin26.SetState(Gpio.PinStat.Low);
                //        break;
                //    case "q":
                //        break;
                //}

                //if (command == "q")
                //{
                //    Console.WriteLine("Exit program");
                //    break;
                //}

            }
            if (interface_port!= null)
                interface_port.Close();
            if (main_port != null)
                main_port.Close();

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


        void ReadSerial_main()
        {
            string temp = "";

            while (main_port.IsOpen)
            {
                receiveBuffer += main_port.ReadExisting();

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
                    //else
                    //{
                    //    Console.WriteLine("recive:" +receiveBuffer);
                    //    port.Write(receiveBuffer);
                    //}

                    receiveBuffer = "";


                }

            }

        }


        void ReadSerial_interface()
        {
            string temp = "";

            while (interface_port.IsOpen)
            {
                receiveBuffer_i = interface_port.ReadExisting();

                if (receiveBuffer_i != "")
                {
                    //    receiveBuffer_i = temp + receiveBuffer_i;
                    //    temp = "";
                    //    if (receiveBuffer_i.Contains(";"))
                    //    {
                    string command = receiveBuffer_i;
                       
                        switch(command.ToUpper())
                        {
                            case "S":
                                printMessage("set CAN to 500Kbit");
                                main_port.Write("S6\r");
                                break;

                            case "O":
                                printMessage("Open Can port");
                                main_port.Write("O\r");
                                break;

                            case "C":
                                printMessage("Close Can port");
                                main_port.Write("C\r");
                                break;
                        default:
                            Console.WriteLine("recive inretface:" + receiveBuffer_i);
                            break;

                        }
                    //}
                    
                //interface_port.Write(receiveBuffer);

                //receiveBuffer = "";


            }

        }

        }


    }
}
