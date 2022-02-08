//Demonstrate Sockets
using System;
using System.Net.Sockets;
using System.IO;
public class Whois
{
    static void Main(string[] args)
    {
        int c;
        try
        {
            TcpClient client = new TcpClient();
            client.Connect("whois.net.dcs.hull.ac.uk", 43);
            StreamWriter writer = new StreamWriter(client.GetStream());
            StreamReader reader = new StreamReader(client.GetStream());

            if (args.Length == 1)
            {
                writer.WriteLine(args[0]);
                writer.Flush();
                Console.WriteLine(args[0] + " is " + reader.ReadToEnd());
            }
            else if(args.Length == 2)
            {
                writer.WriteLine(args[0] + " " + args[1]);
                writer.Flush();

                string error = reader.ReadToEnd();

                if(error == "OK\r\n")
                {
                    Console.WriteLine(args[0] + " location changed to be " + args[1]);
                }
                else
                {
                    Console.WriteLine("ERROR: Something went wrong");
                }
            }
            else
            {
                Console.WriteLine("Something went wrong");
            }
        }
        catch
        {
            Console.WriteLine("Something went wrong");
        }
    }
}
