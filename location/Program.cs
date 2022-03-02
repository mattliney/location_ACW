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
    static string mName = null;
    static string mLocation = null;

    static void ParseArgs(string[] pArgs, out bool pValid)
    {
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
                    return;
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
                if(mName == null)
                {
                    mName = pArgs[i];
                }
                else
                {
                    mLocation = pArgs[i];
                }
            }
        }

        pValid = true;
    }

    static void ReadAndWriteResponse(StreamReader pReader)
    {
        //WHOIS

        if(mCurrentProtocol == "whois")
        {
            string line = pReader.ReadToEnd();
            if(line == "ERROR: no entries found\r\n")
            {
                Console.WriteLine(line);
            }
            else
            {
                Console.Write(mName + " is ");
                Console.WriteLine(line);
            }
            return;
        }

        //HTML

        string location = "";
        string str = pReader.ReadLine();
        if(str.Contains("200")|| (str.Contains("301")))
        {
            while (str != "")
            {
                str = pReader.ReadLine();
            }

            try
            {
                while (true)
                {
                    location += pReader.ReadLine() + "\r\n";
                }
            }
            catch { }

            Console.Write(location);
        }
    }

    static void Main(string[] args)
    {
        bool valid;
        ParseArgs(args, out valid);

        if(valid)
        {
            try
            {
                TcpClient client = new TcpClient();

                client.Connect(mCurrentAddress, mCurrentPort);

                StreamWriter writer = new StreamWriter(client.GetStream());
                StreamReader reader = new StreamReader(client.GetStream());

                client.ReceiveTimeout = 1000;
                client.SendTimeout = 1000;

                if (mCurrentProtocol == "whois")
                {
                    if (mLocation == null)
                    {
                        writer.WriteLine(mName);
                        writer.Flush();

                        ReadAndWriteResponse(reader);
                    }
                    else
                    {
                        writer.WriteLine(mName + " " + mLocation);
                        writer.Flush();

                        string error = reader.ReadLine();

                        if (error == "OK")
                        {
                            Console.WriteLine(mName + " location changed to be " + mLocation);
                        }

                        else
                        {
                            Console.WriteLine("ERROR: Something went wrong");
                        }
                    }
                }
                else if(mCurrentProtocol == "-h9")
                {
                    if(mLocation == null)
                    {
                        writer.WriteLine("GET /" + mName + "\r\n");
                        writer.Flush();
                        Console.Write(mName + " is ");

                        ReadAndWriteResponse(reader);
                    }
                    else
                    {
                        writer.WriteLine("PUT /" + mName + "\r\n\r\n" + mLocation + "\r\n");
                        writer.Flush();

                        string response = reader.ReadLine();
                        if (response.Contains("200"))
                        {
                            Console.WriteLine(mName + " location changed to be " + mLocation);
                        }
                    }
                }
                else if (mCurrentProtocol == "-h0")
                {
                    if (mLocation == null)
                    {
                        writer.WriteLine("GET /?" + mName + " HTTP/1.0\r\n\r\n");
                        writer.Flush();
                        Console.Write(mName + " is ");

                        ReadAndWriteResponse(reader);
                    }
                    else
                    {
                        writer.WriteLine("POST /" + mName + " HTTP/1.0" + "\r\n" + "Content-Length: " + mLocation.Length + "\r\n\r\n" + mLocation);
                        writer.Flush();

                        string response = reader.ReadLine();
                        if (response.Contains("200"))
                        {
                            Console.WriteLine(mName + " location changed to be " + mLocation);
                        }
                    }
                }
                else if (mCurrentProtocol == "-h1")
                {
                    if (mLocation == null)
                    {
                        writer.WriteLine("GET /?name=" + mName + " HTTP/1.1\r\n" + "Host: " + mCurrentAddress + "\r\n\r\n");
                        writer.Flush();
                        Console.Write(mName + " is ");

                        ReadAndWriteResponse(reader);
                    }
                    else
                    {
                        string message = "name=" + mName + "&location=" + mLocation;
                        int length = message.Length;
                        writer.WriteLine("POST / HTTP/1.1" + "\r\nHost: " + mCurrentAddress + "\r\nContent-Length: " + length + "\r\n\r\nname=" + mName + "&location=" + mLocation);
                        writer.Flush();

                        string response = reader.ReadLine();
                        if (response.Contains("200"))
                        {
                            Console.WriteLine(mName + " location changed to be " + mLocation);
                        }
                    }
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        else
        {
            Console.WriteLine("Not enough arguments provided");
        }
    }
}
