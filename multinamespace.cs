using System;
using CustomNameSpace;

namespace Tutlane
{
    class Program
    {
        static void Main(string[] args)
        {
            Welcome w = new Welcome();
            w.GreetMessage();
            Console.WriteLine("Press Any Key to Exit..");
            Console.ReadLine();
        }
    }
}

namespace CustomNameSpace {
    class Welcome {
        public void GreetMessage() {
             Console.WriteLine("Welcome to Tutlane");
        }
    }
}
