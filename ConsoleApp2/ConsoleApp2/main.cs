using System;
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
            LedList.Add(new Gpio(21));
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
            Process python = new System.Diagnostics.Process();
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

            Read(python.StandardOutput);
            Read(python.StandardError);


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
            Console.WriteLine("---------------------------------");
            initGpios();
            printMessage("start python prosses");

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


            double tempBreakPress = 0;
            double tempGasPress = 0;
            bool pwm = false;
            int ontime = 0;
            

            int mode = 0;
            int duty = 10;
            bool demo = false;

            printMessage("Open python process");
            pythonProsses("sudo python /home/pi/Desktop/aganya/rpiApp/ConsoleApp2/ConsoleApp2/bin/Debug/pwm.py");


            while (true)
            {
                ReadSerial_main();

                
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

                if(gasPress == 0 && breakPress == 0 && yelow) 
                {
                    yelow = false;
                    setOutputYello();
                }
               


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

                    Thread.Sleep(100);

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



        void setOutputBreak()
        {
            int value = Convert.ToInt32(breakPress * 3.5);
            string[] levels = { "26", "19", "13", "6", "5", "21" };
            //red
            s1.SetState(Gpio.PinStat.Hi);
            s0.SetState(Gpio.PinStat.Low);

            foreach (string lev in levels)
            {
                if (value >= 0 && value <= 100)
                {
                    //level1
                    File.WriteAllText(lev, value.ToString());
                }
                else if (value >= 100)
                {
                    File.WriteAllText(lev, "100");
                }
                else if (value <= 0)
                {
                    File.WriteAllText(lev, "0");
                }
                value -= 100;
            }

        }


        void setOutputGas()
        {
            int value = Convert.ToInt32(gasPress * 3);
            string[] levels = {  "6", "5", "21" };
            //green
            s1.SetState(Gpio.PinStat.Low);
            s0.SetState(Gpio.PinStat.Hi);

            // off "26", "19", "13"
            File.WriteAllText("26", "0");
            File.WriteAllText("19", "0");
            File.WriteAllText("13", "0");

            foreach (string lev in levels)
            {
                if (value >= 0 && value <= 100)
                {
                    //level1
                    File.WriteAllText(lev, value.ToString());
                }
                else if (value >= 100)
                {
                    File.WriteAllText(lev, "100");
                }
                else if (value <= 0)
                {
                    File.WriteAllText(lev, "0");
                }
                value -= 100;
            }

        }

        void setOutputYello()
        {
            //int value = Convert.ToInt32(breakPress * 3);
            //string[] levels = { "6", "5", "21" };
            //yello
            s1.SetState(Gpio.PinStat.Low);
            s0.SetState(Gpio.PinStat.Low);

            // off "26", "19", "13"
            File.WriteAllText("26", "0");
            File.WriteAllText("19", "0");
            File.WriteAllText("13", "100");
            File.WriteAllText("6", "100");
            File.WriteAllText("5", "0");
            File.WriteAllText("21", "0");
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
