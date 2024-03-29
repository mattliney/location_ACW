﻿//Demonstrate Sockets
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
    static bool mDebug = false;

    /// <summary>
    /// Parse the arguments from the command line. Takes in the array of arguments and outputs a boolean. Having fewer than 3 arguments outputs false.
    /// </summary>
    /// <param name="pArgs"></param>
    /// <param name="pValid"></param>
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
            else if(pArgs[i] == "-d")
            {
                mDebug = true;
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

        if(mDebug == true)
        {
            Console.WriteLine("Debug Mode:");
            Console.WriteLine("Current Address: " + mCurrentAddress + "\r\nCurrent Port: " + mCurrentPort + "\r\nCurrent Protocol: " + mCurrentProtocol + "\r\nName: " + mName + "\r\nLocation: " + mLocation);
        }

        pValid = true;
    }

    /// <summary>
    /// Reads the response from the server when the client sends a single argument. Will output an error if the name is not in the database or output their location if they are found in the database.
    /// </summary>
    /// <param name="pReader"></param>
    static void SingleArgResponse(StreamReader pReader)
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
                    string temp = pReader.ReadLine();
                    if(temp == null)
                    {
                        break;
                    }
                    location += temp + "\r\n";
                }
            }
            catch { }

            Console.Write(location);
        }
    }

    /// <summary>
    /// Reads the response from the server when the client sends two arguments. Will output an error if something goes wrong on the server's end. Will output their location if they have been successfully updated.
    /// </summary>
    /// <param name="pWriter"></param>
    /// <param name="pReader"></param>
    static void DoubleArgsResponse(StreamWriter pWriter, StreamReader pReader)
    {
        string response = pReader.ReadLine();
        if (mCurrentProtocol == "whois")
        {

            if (response == "OK")
            {
                Console.WriteLine(mName + " location changed to be " + mLocation);
            }

            else
            {
                Console.WriteLine("ERROR: Something went wrong");
            }

            return;
        }

        if (response.Contains("200"))
        {
            Console.WriteLine(mName + " location changed to be " + mLocation);
        }

        return;
    }

    /// <summary>
    /// Connects to the server and switches on the current protocol. Depending on the protocol and whether or not the location has been specified, the client will send the message in a different format.
    /// </summary>
    /// <param name="args"></param>
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

                        SingleArgResponse(reader);
                    }
                    else
                    {
                        writer.WriteLine(mName + " " + mLocation);
                        writer.Flush();

                        DoubleArgsResponse(writer, reader);
                    }
                }
                else if(mCurrentProtocol == "-h9")
                {
                    if(mLocation == null)
                    {
                        writer.WriteLine("GET /" + mName + "\r\n");
                        writer.Flush();
                        Console.Write(mName + " is ");

                        SingleArgResponse(reader);
                    }
                    else
                    {
                        writer.WriteLine("PUT /" + mName + "\r\n\r\n" + mLocation + "\r\n");
                        writer.Flush();

                        DoubleArgsResponse(writer, reader);
                    }
                }
                else if (mCurrentProtocol == "-h0")
                {
                    if (mLocation == null)
                    {
                        writer.WriteLine("GET /?" + mName + " HTTP/1.0\r\n\r\n");
                        writer.Flush();
                        Console.Write(mName + " is ");

                        SingleArgResponse(reader);
                    }
                    else
                    {
                        writer.WriteLine("POST /" + mName + " HTTP/1.0" + "\r\n" + "Content-Length: " + mLocation.Length + "\r\n\r\n" + mLocation);
                        writer.Flush();

                        DoubleArgsResponse(writer, reader);
                    }
                }
                else if (mCurrentProtocol == "-h1")
                {
                    if (mLocation == null)
                    {
                        writer.WriteLine("GET /?name=" + mName + " HTTP/1.1\r\n" + "Host: " + mCurrentAddress + "\r\n\r\n");
                        writer.Flush();
                        Console.Write(mName + " is ");

                        SingleArgResponse(reader);
                    }
                    else
                    {
                        string message = "name=" + mName + "&location=" + mLocation;
                        int length = message.Length;
                        writer.WriteLine("POST / HTTP/1.1" + "\r\nHost: " + mCurrentAddress + "\r\nContent-Length: " + length + "\r\n\r\nname=" + mName + "&location=" + mLocation);
                        writer.Flush();

                        DoubleArgsResponse(writer, reader);
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
