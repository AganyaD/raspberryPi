using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ConsoleApp2
{
    class Program
    {
        


        static void Main(string[] args)
        {
            main mainProgram = new main();
            while (true)
            {
                try
                {
                    mainProgram.start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("Main program error - {0}", e.Message));
                }
            }
            
        }

        

        

    }
}
