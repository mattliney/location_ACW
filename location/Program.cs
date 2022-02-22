//Demonstrate Sockets
using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
public class Whois
{
    static string[] mProtocols = new string[4]
    {
        "whois", "-h0", "-h1", "h9"
    };
    static int mCurrentPort = 43;
    static string mCurrentProtocol = mProtocols[0];
    static string mCurrentAddress = "whois.net.dcs.hull.ac.uk";

    static List<string> ParseArgs(string[] pArgs, out bool pValid)
    {
        List<string> output = new List<string>();

        for(int i = 0; i < pArgs.Length; i++)
        {
            foreach (string p in mProtocols) //find protocol
            {
                if (pArgs[i] == p)
                {
                    mCurrentProtocol = p;
                }
            }

            if(pArgs[i] == "-h") //find address
            {
                if(pArgs.Length < 3)
                {
                    pValid = false;
                    return output;
                }
                else
                {
                    mCurrentAddress = pArgs[i + 1];
                    i++;
                }
            }
            else if(pArgs[i] == "-p")
            {
                mCurrentPort = int.Parse(pArgs[i + 1]);
                i++;
            }
            else
            {
                output.Add(pArgs[i]);
            }
        }

        pValid = true;
        return output;
    }
    
    static void Main(string[] args)
    {
        bool valid;
        List<string> arguments = ParseArgs(args, out valid);

        try
        {
            TcpClient client = new TcpClient();

            client.Connect(mCurrentAddress, mCurrentPort);

            StreamWriter writer = new StreamWriter(client.GetStream());
            StreamReader reader = new StreamReader(client.GetStream());

            if(mCurrentProtocol == "whois")
            {
                if (arguments.Count == 1)
                {
                    writer.WriteLine(arguments[0]);
                    writer.Flush();
                    Console.WriteLine(arguments[0] + " is " + reader.ReadToEnd());
                }
                else if (args.Length == 2)
                {
                    writer.WriteLine(args[0] + " " + args[1]);
                    writer.Flush();

                    string error = reader.ReadToEnd();

                    if (error == "OK\r\n")
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

        }
        catch
        {
            Console.WriteLine("Something went wrong");
        }
    }
}
