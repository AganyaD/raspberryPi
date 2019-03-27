using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApp2
{
    class Gpio
    {
        public enum GpioPin
        {
            GPIO17 = 17,
            GPIO18 = 18,
            GPIO19 = 19,
            GPIO27 = 27,
            GPIO22 = 22,
            GPIO23 = 23,
            GPIO24 = 24,
            GPIO25 = 25,
            GPIO5 = 5,
            GPIO6 = 6,
            GPIO13 = 13,
            GPIO12 = 12,
            GPIO26 = 26,
            GPIO16 = 16,
            GPIO20 = 20,
            GPIO21 = 21

        }

        public enum PinStat
        {
            Hi =1,
            Low =0
        }

        int pinNumber = 0;
        public Gpio(int GpioNum)
        {
            pinNumber = GpioNum;
        }

        public void SetPin()
        {
            // about to open pin 26 - test if it's already open
            string dirName = string.Format("/sys/class/gpio/gpio{0}", pinNumber);
            if (!Directory.Exists(dirName))
            {
                Console.WriteLine("...about to open pin " + pinNumber);
                File.WriteAllText("/sys/class/gpio/export", pinNumber.ToString());
            }
            else
            {
                Console.WriteLine("...pin is already open");
                //throw new Exception("pin is already open");
            }

            Console.WriteLine("...specifying direction of Pin " + pinNumber + " as OUT");
            File.WriteAllText("/sys/class/gpio/gpio26/direction", "out");
        }
        public void SetState(PinStat stat)
        {
            int st = 0;
            if (stat == PinStat.Hi)
                st = 1;
            string tosend = string.Format("...setting output level to {0}", st);
            Console.WriteLine(tosend);
            tosend = string.Format("/sys/class/gpio/gpio{0}/value", pinNumber);
            File.WriteAllText(tosend, st.ToString());
        }
    }
}

