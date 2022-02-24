//Demonstrate Sockets
using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
public class Whois
{
    static string[] mProtocols = new string[4]
    {
        "whois", "-h0", "-h1", "-h9"
    };
    static int mCurrentPort = 43;
    static string mCurrentProtocol = mProtocols[0];
    static string mCurrentAddress = "whois.net.dcs.hull.ac.uk";

    static List<string> ParseArgs(string[] pArgs, out bool pValid)
    {
        List<string> output = new List<string>();
        bool protocolFound = false;

        for(int i = 0; i < pArgs.Length; i++)
        {
            protocolFound = false;
            foreach (string p in mProtocols) //find protocol
            {
                if (pArgs[i] == p)
                {
                    mCurrentProtocol = p;
                    protocolFound = true;
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
            else if(pArgs[i] == "-p") // find port
            {
                mCurrentPort = int.Parse(pArgs[i + 1]);
                i++;
            }
            else if(!protocolFound) // add argument to list
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

        if(valid)
        {
            try
            {
                TcpClient client = new TcpClient();

                client.Connect(mCurrentAddress, mCurrentPort);

                StreamWriter writer = new StreamWriter(client.GetStream());
                StreamReader reader = new StreamReader(client.GetStream());

                if (mCurrentProtocol == "whois")
                {
                    if (arguments.Count == 1)
                    {
                        writer.WriteLine(arguments[0]);
                        writer.Flush();
                        Console.WriteLine(arguments[0] + " is " + reader.ReadLine());
                    }
                    else if (arguments.Count == 2)
                    {
                        writer.WriteLine(arguments[0] + " " + arguments[1]);
                        writer.Flush();

                        string error = reader.ReadLine();

                        if (error == "OK")
                        {
                            Console.WriteLine(arguments[0] + " location changed to be " + arguments[1]);
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
                else if(mCurrentProtocol == "-h9")
                {
                    writer.WriteLine("GET /" + arguments[0] + "\r\n");
                    writer.Flush();
                }

            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        Console.ReadLine();
    }
}
