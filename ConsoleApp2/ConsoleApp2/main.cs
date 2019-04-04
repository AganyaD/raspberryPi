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
        bool debug = false;

        bool Zero_Flg = true;

        List<string> comPortList;
        string receiveBuffer = "";
        string receiveBuffer_i = "";
        string mesage = "";
        bool sendSpeedQury = false;
        double lastSpeed = 0;
        double lastSpeedTime = 0;
        double acc = 0.0;
        double breakPress = 0.0;
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
                    interface_port.WriteLine(mes+"\r");
            }
            
            
        }

        List<Gpio> LedList = new List<Gpio>();
        void initGpios()
        {

            LedList.Add(new Gpio(26));  //0
            LedList.Add(new Gpio(19));  //1
            LedList.Add(new Gpio(13));  //2
            LedList.Add(new Gpio(6));   //3
            LedList.Add(new Gpio(5));   //4
            //LedList.Add(new Gpio(12));  //5
            //LedList.Add(new Gpio(16));
            //LedList.Add(new Gpio(20));
            LedList.Add(new Gpio(21));

            if (debug)
            {
                foreach (var y in LedList)
                {
                    y.debug = true;
                }
            }
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

        public string ExecuteCommand_stringOutpot(string command)
        {
            string outpot = string.Empty;
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \" " + command + " \"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                //Console.WriteLine(proc.StandardOutput.ReadLine());
                outpot += proc.StandardOutput.ReadLine();
            }

            return outpot;
        }

        public void start()
        {
            
             
            if (debug)
                printMessage("program start - debug Mode");
            else
                printMessage("program start");

            printMessage("Execute Command");
            if(!debug)
            {
                //check HW revision
                //cat / etc / debian_version
                string o = ExecuteCommand_stringOutpot("cat /proc/cpuinfo");
                string[] split1 = o.Split(':');
                
                for (int i=0; i<split1.Count();i++)
                {
                   if (split1[i].Contains("Revision"))
                    {
                        if (!split1[i+1].Contains("900093"))
                        {
                            Zero_Flg = false;
                            Console.WriteLine("Rpi 3 ------------------------");
                        }
                        break;
                    }
                    //.WriteLine(split1[i]);
                }

                if (Zero_Flg)
                    ExecuteCommand("sudo chmod -R 666 /dev/ttyAMA0");
                else
                    ExecuteCommand("sudo chmod -R 666 /dev/ttyS0");
            }


            Console.WriteLine("---------------------------------");
            printMessage("init GPIOs");
            Console.WriteLine("---------------------------------");
            initGpios();
            Console.WriteLine("---------------------------------");
            printPorts();
            Console.WriteLine("---------------------------------");
            printMessage("set serial port");
            string portName = "";
            if (comPortList.IndexOf("/dev/ttyUSB1")<0)
            {
                if (Zero_Flg)
                {
                    printMessage("interface port mane /dev/ttyAMA0");
                    portName = "/dev/ttyAMA0";
                }
                else
                {
                    printMessage("interface port mane /dev/ttyS0");
                    portName = "/dev/ttyS0";
                }
            }
            else
            {
                printMessage("interface port mane /dev/ttyUSB1");
                portName = "/dev/ttyUSB1";
            }

            if (debug)
                portName = "COM27";


            printMessage("Interface open port in " + portName);
            interface_port = new SerialPort(portName, 115200);
            try
            {
                interface_port.Open();
                interface_port.Write("port opend");

                //this.interface_port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.ReadSerial_interface);
                //new System.Threading.Thread(() =>
                //{
                //    System.Threading.Thread.CurrentThread.IsBackground = true;
                //    ReadSerial_interface();
                //}).Start();
            }
            catch(Exception e)
            {
                printMessage(e.Message);
                return;
            }

            portName = "empty staff";
            printMessage("Main port mane /dev/ttyUSB0");
            portName = "/dev/ttyUSB0";

            if (debug)
                portName = "COM26";

            printMessage("open port in " + portName);
            main_port = new SerialPort(portName, 115200);
            try
            {
                main_port.Open();
                //new System.Threading.Thread(() =>
                //{
                //    System.Threading.Thread.CurrentThread.IsBackground = true;
                //    ReadSerial_main();
                //}).Start();
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

            printMessage("Start!!!!!!!!!!!!!!!!");


            Thread.Sleep(500);
            main_port.Write("C\r");
            Thread.Sleep(500);
            main_port.Write("C\r");
            Thread.Sleep(500);
            main_port.Write("S6\r");
            Thread.Sleep(500);
            main_port.Write("O\r");


            double tempPress = 0;
            while (true)
            {
                ReadSerial_main();

                if (tempPress != breakPress)
                {

                    printMessage(string.Format("{0} tempPress != breakPress {1}",tempPress,breakPress));
                    tempPress = breakPress;
                    setOutput();
                }


                if (interface_port.IsOpen && interface_port.BytesToRead>0)
                {
                    receiveBuffer_i = interface_port.ReadExisting();
                    Console.WriteLine("Debug recive inretface:" + receiveBuffer_i);
                }
                
                if (receiveBuffer_i != "")
                {

                    string command = receiveBuffer_i;
                    receiveBuffer_i = "";
                    switch (command.ToUpper())
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

                        case "0":
                            breakPress = 5;
                            break;

                        case "1":
                            breakPress = 30;
                            break;

                        case "2":;
                            breakPress = 60;
                            break;

                        case "3":
                            breakPress = 90;
                            break;

                        case "4":
                            breakPress = 120;
                            break;

                        case "5":
                            breakPress = 145;
                            break;

                        case "6":
                            breakPress = 155;
                            break;
                        default:
                            Console.WriteLine("recive inretface:" + receiveBuffer_i);
                            break;

                    }

                    
                }

            }
            if (interface_port!= null)
                interface_port.Close();
            if (main_port != null)
                main_port.Close();

        }

        void setOutput()
        {
            if(breakPress<10)
            {
                foreach(var led in LedList)
                {
                    led.SetState(Gpio.PinStat.Low);
                }

            }
            else
                if (breakPress < 40)
            {
                LedList[0].SetState(Gpio.PinStat.Hi);
                for (int i = 1; i< LedList.Count;i++)
                {
                    LedList[i].SetState(Gpio.PinStat.Low);
                }
            }
            else
                if (breakPress < 70)
            {
                LedList[0].SetState(Gpio.PinStat.Hi);
                LedList[1].SetState(Gpio.PinStat.Hi);
                for (int i = 2; i < LedList.Count; i++)
                {
                    LedList[i].SetState(Gpio.PinStat.Low);
                }
            }
            else
                if (breakPress < 100)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        LedList[i].SetState(Gpio.PinStat.Hi);
                    }

                    for (int i = 3; i < LedList.Count; i++)
                    {
                        LedList[i].SetState(Gpio.PinStat.Low);
                    }
                }
            else
                if (breakPress < 130)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        LedList[i].SetState(Gpio.PinStat.Hi);
                    }

                    for (int i = 4; i < LedList.Count; i++)
                    {
                        LedList[i].SetState(Gpio.PinStat.Low);
                    }
                }
            else
                if (breakPress < 150)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        LedList[i].SetState(Gpio.PinStat.Hi);
                    }

                    for (int i = 5; i < LedList.Count; i++)
                    {
                        LedList[i].SetState(Gpio.PinStat.Low);
                    }
                }
            else
            {
                for (int i = 0; i < LedList.Count; i++)
                {
                    LedList[i].SetState(Gpio.PinStat.Hi);
                }
            }
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
        string temp = "";

        void ReadSerial_main()
        {
            

            if (main_port.IsOpen && main_port.BytesToRead>0)
            {
                receiveBuffer += main_port.ReadExisting();

                if (receiveBuffer != "")
                {
                    receiveBuffer = temp + receiveBuffer;

                    mesage = receiveBuffer;
                    temp = "";
                    if (receiveBuffer.Contains("\r"))
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
                                int t_loc = mess.IndexOf('t'); ;
                                
                                //01234 56 78 9  11 13 15 17 19 
                                //t3E98 00 00 00 13 00 00 00 13

                                //"t0F1 4 34 00 00 40 9633"
                                string t;

                                {
                                    string data = mess.Substring(t_loc + 7, 2);
                                    double press = Convert.ToInt16(data, 16);
                                    //double speedDataKmPerH = (double)((double)speedDatamilsPerH / 100) / 0.62137;
                                    //t = mess.Substring(mess.Length - 4, 4);
                                    //double pressDataTitmeS = Convert.ToUInt16(t, 16);
                                    string toText = press.ToString();

                                    //double temp_speedDataTitmeS = pressDataTitmeS;

                                    //if (pressDataTitmeS < lastSpeedTime)
                                    //{
                                    //    temp_speedDataTitmeS += 60;
                                    //}

                                    breakPress = press; // (double)(speedDataKmPerH - lastSpeed) / (double)(temp_speedDataTitmeS - lastSpeedTime);
                                    //acc = (double)acc;

                                    //lastSpeedTime = pressDataTitmeS;
                                    //lastSpeed = speedDataKmPerH;

                                    printMessage(string.Format("message : {0} ",mess));
                                    printMessage(string.Format("value hex : {0} ",data));
                                    printMessage(string.Format("value dec : {0} ", press));
                                }

                                //this.Invoke(new EventHandler(DisplayText));

                            }

                            // t 7E8 8 03 41 0D 00 55 55 55 55
                            //"t19D8C0003FFD000BD9FFCBA3"
                            //if (mess.Contains("t") && false)
                            //{
                            //    try
                            //    {
                            //        int mesid = Convert.ToInt16(mess.Substring(1, 3), 16);

                            //        int lng = Convert.ToInt16(mess.Substring(4, 1), 16);

                            //        byte[] data = new byte[lng];

                            //        for (int indx = 0; indx < lng; indx++)
                            //        {
                            //            data[indx] = Convert.ToByte(mess.Substring(5 + indx + (indx * 2), 2), 16);
                            //        }

                            //        CanMessageData candata;
                            //        candata = new CanMessageData(data);
                            //        CanMessage message = new CanMessage(mesid, candata);

                            //        //datagridUpdataInfo(message);
                            //    }
                            //    catch
                            //    {

                            //    }
                            //}
                        }

                    }
                    else
                    {
                        temp = receiveBuffer;
                    }

                    receiveBuffer = "";


                }

            }

        }


        ////void ReadSerial_interface(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        //{
        //    printMessage("reciveDate Function");
        //    receiveBuffer_i = interface_port.ReadExisting();
        //    Console.WriteLine("Debug recive inretface:" + receiveBuffer_i);
        //    //if (receiveBuffer_i != "")
        //    //{

        //    //    string command = receiveBuffer_i;
                       
        //    //        switch(command.ToUpper())
        //    //        {
        //    //            case "S":
        //    //                printMessage("set CAN to 500Kbit");
        //    //                main_port.Write("S6\r");
        //    //                break;

        //    //            case "O":
        //    //                printMessage("Open Can port");
        //    //                main_port.Write("O\r");
        //    //                break;

        //    //            case "C":
        //    //                printMessage("Close Can port");
        //    //                main_port.Write("C\r");
        //    //                break;
        //    //        default:
        //    //            Console.WriteLine("recive inretface:" + receiveBuffer_i);
        //    //            break;

        //    //        }

        //    //receiveBuffer = "";

        //    //}   

        //}


    }
}
