﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;

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
        double gasPress = 0.0;
        bool yelow = false;
        List<string> inFramList = new List<string>();

        CanLocationData CanMessageId_Break = new CanLocationData("0F1",1);
        CanLocationData CanMessageId_Gas = new CanLocationData("0C9", 4);
        class CanLocationData
        {
            public string MessageId { get; set; }
            public int dataByteNum  { get; set; }

            public CanLocationData(string MessageId, int dataByteNum)
            {
                this.MessageId = MessageId;
                this.dataByteNum = dataByteNum;
            }

            public string getMesID()
            {
                return MessageId;
            }

            public int getDataByteNum()
            {
                return dataByteNum;
            }
        }


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
            try
            {
                Console.WriteLine(mes);

                if (interface_port != null)
                {
                    if (interface_port.IsOpen)
                        interface_port.WriteLine(mes + "\r");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            
            
        }

        List<Gpio> LedList = new List<Gpio>();
        Gpio s0;
        Gpio s1;

        

        void initGpios()
        {

            ////////LedList.Add(new Gpio(26));  //0
            ////////LedList.Add(new Gpio(19));  //1
            ////////LedList.Add(new Gpio(13));  //2
            ////////LedList.Add(new Gpio(6));   //3
            ////////LedList.Add(new Gpio(5));   //4
            //////////LedList.Add(new Gpio(12));  //5
            //////////LedList.Add(new Gpio(16));
            //////////LedList.Add(new Gpio(20));
            ///
            levelsValue = new Dictionary<string, int>();
            printMessage("set all GPIOs as 0");
            levelsValue.Add("26", 0);
            levelsValue.Add("19", 0);
            levelsValue.Add("13", 0);
            levelsValue.Add("6", 0);
            levelsValue.Add("5", 0);
            levelsValue.Add("21", 0);

            //LedList.Add(new Gpio(21));
            printMessage("set mode concrol pins");
            s0 = new Gpio(23);
            s1 = new Gpio(24);
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

        Process python;

        public void pythonProsses(string command)
        {
            python = new System.Diagnostics.Process();
            //python.StartInfo.FileName = "/bin/bash";
            //python.StartInfo.Arguments = "-c \" " + command + " \"";
            //python.StartInfo.UseShellExecute = false;
            //python.StartInfo.RedirectStandardOutput = true;
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //startInfo.FileName = "cmd.exe";
            //startInfo.Arguments = "/C NET USE F: /delete";
            startInfo.FileName = "/bin/bash";
            startInfo.Arguments = "-c \" " + command + " \"";
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            python.StartInfo = startInfo;
            python.Start();

            //python.WaitForExit();
            //Read(python.StandardOutput);
            //Read(python.StandardError);


            //python.Start();
            
        }

        private static void Read(StreamReader reader)
        {
            new Thread(() =>
            {
                while (true)
                {
                    int current;
                    while ((current = reader.Read()) >= 0)
                        Console.Write((char)current);
                }
            }).Start();
        }
        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            printMessage(e.Data);
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
            initGpios();
            Console.WriteLine("---------------------------------");
            //printMessage("start python prosses");

            Console.WriteLine("---------------------------------");
            printMessage("init GPIOs");
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
            }
            catch (Exception e)
            {
                printMessage(e.Message);
                return;
            }
            



            double tempBreakPress = 0;
            double tempGasPress = 0;
            bool pwm = false;
            int ontime = 0;
            

            int mode = 0;
            int duty = 10;
            bool demo = false;
            bool exsit = false;

            //File.WriteAllText("exit", "1");
            //Thread.Sleep(100);
            //File.WriteAllText("exit", "0");
            //Thread.Sleep(100);

            //printMessage("Open python process");
            //pythonProsses("sudo python /home/pi/Desktop/aganya/rpiApp/ConsoleApp2/ConsoleApp2/bin/Debug/pwm.py");

            Thread.Sleep(100);


            ledTest();
            Thread.Sleep(1000);

            printMessage("Start!!!!!!!!!!!!!!!!");

            new Thread(() =>
            {
                printMessage("main port Read thread start");
                while (true)
                {
                    try
                    {
                        ReadSerial_main();
                    }
                    catch
                    { printMessage("fail read thread exsaption"); }
                }
            }).Start();

            Thread.Sleep(500);
            main_port.Write("C\r");
            Thread.Sleep(500);
            main_port.Write("C\r");
            Thread.Sleep(500);
            main_port.Write("S6\r");
            Thread.Sleep(500);
            main_port.Write("O\r");

            while (true)
            {
                if(exsit)
                {
                    printMessage("Close python Pross");
                    python.Kill();
                    printMessage("Close CAN");
                    main_port.Write("C\r");
                    printMessage("Exit program By By...");
                }

                
                if (tempBreakPress != breakPress)
                {

                    printMessage(string.Format("{0} tempPress != breakPress {1}",tempBreakPress,breakPress));
                    tempBreakPress = breakPress;
                    setOutputBreak();
                    yelow = true;
                }

                if(tempGasPress != gasPress)
                {
                    printMessage(string.Format("{0} tempPress != Gas Press {1}", tempGasPress, gasPress));
                    tempGasPress = gasPress;
                    setOutputGas();
                    yelow = true;
                }

                if(gasPress == 0 && breakPress == 0)// && yelow) 
                {
                    yelow = false;
                    setOutputYello();
                }

                //pythonProsses("sudo python /home/pi/Desktop/aganya/rpiApp/ConsoleApp2/ConsoleApp2/bin/Debug/pwm.py");

                if (interface_port.IsOpen && interface_port.BytesToRead>0)
                {
                    Console.WriteLine("rrrrrrrrrrrrrrrrrrrrr");
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

                       

                        case "M":
                            mode++;
                            printMessage(string.Format("mode change - {0}", mode));
                            
                            if (mode >3)
                            {
                                mode = 0;
                            }

                            if (mode == 0)
                            {
                                //off
                                s1.SetState(Gpio.PinStat.Low);
                                s0.SetState(Gpio.PinStat.Low);
                            }
                            else if (mode == 1)
                            {
                                //green
                                s1.SetState(Gpio.PinStat.Low);
                                s0.SetState(Gpio.PinStat.Hi);
                            }
                            else if (mode == 2)
                            {
                                //red
                                s1.SetState(Gpio.PinStat.Hi);
                                s0.SetState(Gpio.PinStat.Low);
                            }
                            else if (mode == 3)
                            {
                                //blue
                                s1.SetState(Gpio.PinStat.Hi);
                                s0.SetState(Gpio.PinStat.Hi);
                            }


                            break;

                        case "D":

                            if(duty == 10)
                            {
                                duty = 100;
                            }
                            else
                            {
                                duty = 10;
                            }
                                
                            printMessage(string.Format("duty = {0}", duty));
                            File.WriteAllText("13", duty.ToString());
                            File.WriteAllText("19", duty.ToString());
                            File.WriteAllText("21", duty.ToString());
                            File.WriteAllText("26", duty.ToString());
                            File.WriteAllText("5", duty.ToString());
                            File.WriteAllText("6", duty.ToString());
                            
                            break;
                        case "DEMO":
                            printMessage("Start demo");
                            demo = true;
                            mode = 0;
                            break;

                        case "EXIT":
                            
                            exsit = true;
                            break;

                        default:
                            Console.WriteLine("recive inretface:" + receiveBuffer_i);
                            break;

                    }

                    

                    
                }
                if (demo)
                {
                    if (mode == 0)
                    {
                        gasPress++;
                    }
                    else if (mode == 1)
                    {
                        breakPress++;
                    }
                    else if (mode == 2)
                    {
                        gasPress--;
                    }

                    Thread.Sleep(5);

                }
                if (breakPress == 200 && demo)
                {
                    printMessage("@@@@@@@@@ Demo is Done @@@@@@@@");
                    demo = false;
                    breakPress = 0;
                }
                if (gasPress == 200 && demo)
                {
           
                    mode = 2;
                }
                if (gasPress == 0 && mode >= 2 && demo)
                {

                    mode++;
                }
                if ( mode >= 50 && demo)
                {

                    mode=1;
                }
            }
            if (interface_port!= null)
                interface_port.Close();
            if (main_port != null)
                main_port.Close();

        }

        Dictionary<string, int> levelsValue = new Dictionary<string, int>();

        void SetPin(string pin,int value)
        {
            if (levelsValue[pin] != value)
                File.WriteAllText(pin, value.ToString());
            levelsValue[pin] = value;
        }

        int mode = 0;

        void ledTest()
        {
            string[] levels = { "26", "19", "13", "6", "5", "21" };
            s1.SetState(Gpio.PinStat.Low);
            s0.SetState(Gpio.PinStat.Low);

            foreach (string lev in levels)
            {
                SetPin(lev, 100);
            }
            Thread.Sleep(1000);
            s1.SetState(Gpio.PinStat.Low);
            s0.SetState(Gpio.PinStat.Hi);
            Thread.Sleep(1000);
            s1.SetState(Gpio.PinStat.Hi);
            s0.SetState(Gpio.PinStat.Hi);
            Thread.Sleep(1000);
            s1.SetState(Gpio.PinStat.Hi);
            s0.SetState(Gpio.PinStat.Low);
            Thread.Sleep(1000);

            foreach (string lev in levels)
            {
                SetPin(lev, 0);
            }

            s1.SetState(Gpio.PinStat.Low);
            s0.SetState(Gpio.PinStat.Low);

        }

        void setOutputBreak()
        {
            int value = Convert.ToInt32(breakPress * 3.5);
            string[] levels = { "26", "19", "13", "6", "5", "21" };
            //red
            if (mode != 1)
            {
                s1.SetState(Gpio.PinStat.Hi);
                s0.SetState(Gpio.PinStat.Low);
                mode = 1;
            }
            foreach (string lev in levels)
            {
                if (value >= 0 && value <= 100)
                {
                    //level1
                    SetPin(lev, value);

                    //if (levelsValue[lev] != value)
                    //    File.WriteAllText(lev, value.ToString());
                }
                else if (value >= 100)
                {
                    SetPin(lev, 100);
                    //if (levelsValue[lev] != 100)
                    //    File.WriteAllText(lev, "100");
                }
                else if (value <= 0)
                {
                    SetPin(lev, 0);
                    //File.WriteAllText(lev, "0");
                }
                value -= 100;

            }

        }


        void setOutputGas()
        {
            int value = Convert.ToInt32(gasPress * 3);
            string[] levels = {  "6", "5", "21" };
            //green
            if (mode != 2)
            {
                s1.SetState(Gpio.PinStat.Low);
                s0.SetState(Gpio.PinStat.Hi);
                mode = 2;
            }
            // off "26", "19", "13"
            SetPin("26", 0);
            SetPin("19", 0);
            SetPin("13", 0);
            //File.WriteAllText("26", "0");
            //File.WriteAllText("19", "0");
            //File.WriteAllText("13", "0");

            foreach (string lev in levels)
            {
                if (value >= 0 && value <= 100)
                {
                    //level1
                    SetPin(lev, value);
                    //File.WriteAllText(lev, value.ToString());
                }
                else if (value >= 100)
                {
                    SetPin(lev, 100);
                    //File.WriteAllText(lev, "100");
                }
                else if (value <= 0)
                {
                    SetPin(lev, 0);
                    //File.WriteAllText(lev, "0");
                }
                value -= 100;
            }

        }

        void setOutputYello()
        {
            //int value = Convert.ToInt32(breakPress * 3);
            //string[] levels = { "6", "5", "21" };
            //yello
            if (mode != 0)
            {
                s1.SetState(Gpio.PinStat.Low);
                s0.SetState(Gpio.PinStat.Low);
                mode = 0;
            }
            // off "26", "19", "13"
            SetPin("26", 0);
            SetPin("19", 0);
            SetPin("13", 100);
            SetPin("6", 100);
            SetPin("5", 0);
            SetPin("21", 0);
            
            //File.WriteAllText("26", "0");
            //File.WriteAllText("19", "0");
            //File.WriteAllText("13", "100");
            //File.WriteAllText("6", "100");
            //File.WriteAllText("5", "0");
            //File.WriteAllText("21", "0");
            //foreach (string lev in levels)
            //{
            //    if (value >= 0 && value <= 100)
            //    {
            //        //level1
            //        File.WriteAllText(lev, value.ToString());
            //    }
            //    else if (value >= 100)
            //    {
            //        File.WriteAllText(lev, "100");
            //    }
            //    else if (value <= 0)
            //    {
            //        File.WriteAllText(lev, "0");
            //    }
            //    value -= 100;
            //}

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
                try
                {
                    receiveBuffer += main_port.ReadExisting();
                }
                catch
                {
                    printMessage("Error reading data From Main port");
                }
                if (receiveBuffer != "")
                {
                    receiveBuffer = temp + receiveBuffer;

                    mesage = receiveBuffer;
                    temp = "";
                    if (receiveBuffer.Contains("\r"))
                    {
                        try
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
                            try
                            {
                                string mess = split[i];
                                //t7E8803410D0055555555

                                if (mess.Contains("t" + CanMessageId_Break.MessageId) && true)
                                {
                                    int t_loc = mess.IndexOf('t'); ;
                                    //01234 56 78 9  11 13 15 17 19 
                                    //t3E98 00 00 00 13 00 00 00 13
                                    //"t0F1 4 34 00 00 40 9633"

                                    int byteLoc = t_loc + 5 + (CanMessageId_Break.dataByteNum * 2);
                                    string data = mess.Substring(byteLoc, 2);
                                    double press = Convert.ToInt16(data, 16);

                                    string toText = press.ToString();

                                    breakPress = press;

                                    printMessage(string.Format("message : {0} ", mess));
                                    printMessage(string.Format("value hex : {0} ", data));
                                    printMessage(string.Format("value dec : {0} ", press));

                                }

                                if (mess.Contains("t" + CanMessageId_Gas.MessageId) && true)
                                {
                                    int t_loc = mess.IndexOf('t'); ;
                                    //01234 56 78 9  11 13 15 17 19 
                                    //t3E98 00 00 00 13 00 00 00 13

                                    //"t0F1 4 34 00 00 40 9633"

                                    int byteLoc = t_loc + 5 + (CanMessageId_Gas.dataByteNum * 2);
                                    string data = mess.Substring(byteLoc, 2);

                                    double press = Convert.ToInt16(data, 16);

                                    string toText = press.ToString();

                                    gasPress = press;

                                    printMessage(string.Format("message : {0} ", mess));
                                    printMessage(string.Format("value hex : {0} ", data));
                                    printMessage(string.Format("value dec : {0} ", press));

                                }
                            }
                            catch
                            {
                                printMessage("Error filtering messege-----------------------------------");
                            }
                        }

                        }
                        catch
                        {
                            printMessage("Error recive buffer-------------------------------------");
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
 
    }
}
