using System;
using GovorunPavel;

namespace ConsoleTest
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Playing");

      var govor = new SpeakerPavel();
      govor.Speak("1234567890");


      Console.ReadLine();
    }
  }
}
