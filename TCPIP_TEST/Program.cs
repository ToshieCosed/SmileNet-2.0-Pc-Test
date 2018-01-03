using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCPIP_TEST
{
    class Program
    {
        public static string s;
        //public static byte[] dataarray = new Byte[255];
        // public static byte[] messageback = new byte[255];


        static void Main(string[] args)
        
        {
            //byte[] ip = { 192, 168, 0, 179 };
            byte[] ip = { 192, 168, 0, 23 }; //Your Nintendo 3Ds's Local IP here
                //IN ORDER to work you MUST change this to your 3ds's local ip on YOUR router.
            IPAddress addr = new IPAddress(ip);
            IPEndPoint point = new IPEndPoint(addr, (4000));

            while (true)
            {

                byte[] blanksend = new byte[256];
                    //Kick off the send attempt which will connect attempt as well
                Send(blanksend, point);

            }
            
        }

        public static string Truncate(string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }

            return value;
        }

        public static void Send(byte[] rawData, IPEndPoint target)
        {
            // change what you pass to this constructor to your needs
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            try
            {
                s.SendBufferSize = 256;
                s.Connect(target);
               
                

                if (s.Connected) { Console.WriteLine("Connected succesfully"); }
                else
                {
                    Console.WriteLine("Could not connect to target address");
                }
                //s.Send(rawData);

                while (s.Connected)
                {
                        //Declare recieved variables for each packet to be blank for now 
                    int messagetype;
                    int messageformat;
                    int messageresult;

                    bool can_read = false;
                    bool can_write = false;
                    byte[] req = {255, 0}; //Form the req byte array
                    byte[] ret = new byte[256]; //Form the ret array
                    s.Send(req); //Request To Send Data

                   
                        s.Receive(ret);
                        if (ret.Length > 0)
                        {
                            // We got something back
                            //We are expecting back from SmileNet a response on the request, not a read from the SmileBasic Send Buffer.
                            messagetype = ret[0];
                            messageformat = ret[1];
                            messageresult = ret[2];

                            if (messagetype == 255)
                            {
                                if (messageformat == 1)
                                {
                                    if (messageresult == 1)
                                    {
                                        //We can send data.
                                        //Take user input, make the first byte zero, add it to the string then convert to byte array for utf8
                                        string read = "It worked";
                                        byte zerobyte = 0; //Create the zero byte for the first byte of the package
                                        string send_string = System.Text.Encoding.ASCII.GetString(new[] { zerobyte });
                                        send_string = send_string + read;
                                        byte[] send_array = System.Text.Encoding.UTF8.GetBytes(send_string);
                                        s.Send(send_array);


                                    }
                                }
                            }
                        }
                    

                    //Next we check if there's any data for us coming back from SmileNet
                        byte[] reqrecv = { 255, 0 };
                    Console.WriteLine("Requesting if SmileBasic has data");
                        s.Send(reqrecv); //Send the request to find out if there's data available.
                    
                        s.Receive(ret); //Call on receive to see if SmileNet can tell us if the state of SmileBasic has a read buffer ready.
                        Console.WriteLine("it did and the length was " + ret.Length);
                        if (ret.Length > 0)
                        {
                            messagetype = ret[0];
                            messageformat = ret[1];
                            messageresult = ret[2];
                            if (messagetype == 255)
                            {
                                if (messageformat == 3)
                                {
                                    if (messageresult == 1)
                                    {
                                        Console.WriteLine("Can_Read returned true so we can ask it for data now.");
                                        can_read = true; //Set the flag to read now        




                                    }
                                }
                            }
                        }
                    


                    if (can_read)
                    {
                        //Yes SmileBasic does have data for us
                        byte[] recieve_buffer = new byte[256]; //Create the blank recieve buffer

                        //Next create the request to get the buffer sent to us.
                        byte[] readreq = { 255, 4 }; //The messageformat of 4 means we want to ask for the buffer.
                        s.Send(readreq);
                        Console.WriteLine("Asking to have message buffer from SmileBasic sent to us");

                        //Now we do a recieve.
                        //We already know we're going to get the buffer back instantly :)
                       
                            s.Receive(recieve_buffer);
                            //Let's convert it to a string
                            Console.WriteLine("Data was actually recieved hooray");
                            string display_string = System.Text.Encoding.UTF8.GetString(recieve_buffer);
                            Console.WriteLine(display_string);

                        }
                    }



                




            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception of type " + ex.ToString() + " has occured");
            }
        }

        
    }
}