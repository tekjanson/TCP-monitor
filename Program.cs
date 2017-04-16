using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

/*H**********************************************************************
* FILENAME : telnet sensor monitoring            DESIGNER : JAMIE LEVINE
*
* DESCRIPTION : This program is made to monitor a host at a port for sensor data,
*               this program is not ment to be used for profit. To use this project
*               the project can be run and it will prompt you for all changes to adapt 
*               to a new sensor type
*       
*       
*       
*        
*
* AUTHOR :    JAMIE LEVINE        START DATE :    4/15/17
*
* CHANGES :
* 
* VERSION 1.00.00 
*
*H*/

namespace tcpconnect
{
    internal class Program
    {
        private static bool logging = false;
        private static bool defaults = false;
        private static string host = "tools-cluster-01.tulipintra.net";
        private static int port = 3333;
        private static readonly object locker = new object();
        private static readonly string log_name = "Sensor Log.txt";
        private static char start_char = '[';
        private static char sep_char = ',';
        private static char end_char = ']';
        private static double low_range = 0;
        private static double high_range = 65535;

        private static int bad_counter = 0;

        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to telnet thing, lazy name I know but go with it");
            messagebox("The defualts for the systems are would you like to change them? \r"
                + "data logging: off \r"
                + "host : " + host + "\r"
                + "port : " + port + "\r"
                + "start character : \"" + start_char + "\"\r"
                + "seporating character : \"" + sep_char + "\"\r"
                + "end character : \"" + end_char + "\"\r"
                + "making : " + start_char + "xxxx" + sep_char + "xxxx" + end_char + " \r"
                + "sensor_low range : \"" + low_range + "\"\r"
                + "sensor_low range : \"" + high_range + "\"\r"
                , "defualt change", 0);
            if(defaults)
            { 
            messagebox("Would you like to enable logging of your data?", "logging box", 1);
            messagebox("would you like to change the host and port from " + host + ":" + port , "host/port", 2);
            messagebox("would you like to change the message structure?   \"[xxxx,xxxx]\" ?", "message structure", 3);
            messagebox("would you like to change the sensor range? " + low_range + " - " + high_range + " ?", "message structure", 4);

            }
            Connect1(host, port);
        }

        public static void messagebox(string message, string title, int picker)
        {
            switch (MessageBox.Show(message, title, MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    string check = "N";
                    switch (picker)
                    {
                        case 0:
                            defaults = true;
                            return;
                        case 1:
                            Console.WriteLine("We will log!!!");
                            logging = true;
                            return;
                        case 2:

                            while (check != "Y")
                                update_host(ref check);
                            check = "N";
                            while (check != "Y")
                                update_Port(ref check);
                            return;
                        case 3:
                            check = "N";
                            while (check != "Y")
                            { 
                            while (check != "Y")
                                update_start(ref check);
                            check = "N";
                            while (check != "Y")
                                update_end(ref check);
                            check = "N";
                            while (check != "Y")
                                update_sep(ref check);

                            check = "N";
                            structure_verify(ref check);
                            }
                            return;
                        case 4:
                            check = "N";
                            while (check != "Y")
                            {
                                while (check != "Y")
                                    update_low(ref check);
                                check = "N";
                                while (check != "Y")
                                    update_high(ref check);
                              
                                check = "N";
                                range_verify(ref check);
                            }
                            return;
                        default:
                            return;
                    }
                case DialogResult.No:
                    switch (picker)
                    {
                        case 0:
                            defaults = false;
                            break;
                        case 1:
                            Console.WriteLine("We will not log :(");
                            logging = false;
                            break;
                        case 2:
                            Console.WriteLine("We connect to  " + host);
                            Console.WriteLine("At Port  " + (object)port);
                            break;
                    }
                    break;
            }
        }
        public static void update_start(ref string check)
        {
            Console.WriteLine("please enter the new start char now:");
            try { 
            start_char = Convert.ToChar(Console.ReadLine());
                   // break;
            }
            catch
            {
                Console.WriteLine("that was not an acceptable input reverting to defualt make sure entry is only one character");
                start_char = '\0';
            }
            if (start_char == '\0')
                start_char = '[';
            Console.WriteLine("Is this correct Y/N   " + start_char);
            check = Console.ReadLine();
        }
        public static void update_end(ref string check)
        {
            Console.WriteLine("please enter the new ending char now:");
            try
            {
                end_char = Convert.ToChar(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("that was not an acceptable input reverting to defualt make sure entry is only one character");
                end_char = '\0';
            }
            if (end_char == '\0')
                end_char = ']';
            Console.WriteLine("Is this correct Y/N   " + end_char);
            check = Console.ReadLine();
        }
        public static void update_sep(ref string check)
        {
            Console.WriteLine("please enter the new seporating char now:");
            try
            {
                sep_char = Convert.ToChar(Console.ReadLine());
                
            }
            catch
            {
                Console.WriteLine("that was not an acceptable input reverting to defualt make sure entry is only one character");
                sep_char = '\0';
            }
            if (sep_char == '\0')
                sep_char = ',';
            Console.WriteLine("Is this correct Y/N   " + sep_char);
            check = Console.ReadLine();
        }
        public static void update_low(ref string check)
        {
            Console.WriteLine("please enter the new low value now:");
            try
            {
                low_range = Convert.ToDouble(Console.ReadLine());
             
            }
            catch
            {
                Console.WriteLine("that was not an acceptable input reverting to defualt make sure entry is a number");
                low_range = 0;
            }
            if (low_range == 0)
                low_range = 0;
            Console.WriteLine("Is this correct Y/N   " + low_range);
            check = Console.ReadLine();
        }
        public static void update_high(ref string check)
        {
            Console.WriteLine("please enter the new high value now:");
            try
            {
                high_range = Convert.ToDouble(Console.ReadLine());
                
            }
            catch
            {
                Console.WriteLine("that was not an acceptable input reverting to defualt make sure entry is a number");
                high_range = 0;
            }
            if (high_range == 0)
                high_range = 0;
            Console.WriteLine("Is this correct Y/N   " + high_range);
            check = Console.ReadLine();
        }
        public static void structure_verify(ref string check)
        {
            Console.WriteLine(start_char+"xxxx"+sep_char+"xxxx"+end_char);
            Console.WriteLine("Is this the correct message sturcture Y/N");
            check = Console.ReadLine();
        }
        public static void range_verify(ref string check)
        {
            Console.WriteLine(low_range + "-" + high_range);
            Console.WriteLine("Is this the correct range Y/N");
            check = Console.ReadLine();
        }

        public static void update_host(ref string check)
        {
            Console.WriteLine("please enter the new host now:");
            host = Console.ReadLine();
            if (host == "")
                host = "tools-cluster-01.tulipintra.net";
            Console.WriteLine("Is this correct Y/N   " + host);
            check = Console.ReadLine();
        }

        public static void update_Port(ref string check)
        {
            Console.WriteLine("please enter the new port now:");
            port = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Is this correct Y/N   " + host);
            check = Console.ReadLine();
        }

        public static void Connect1(string host, int port)
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(host);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(hostAddresses[0], port);
            byte[] numArray = new byte[50];
            int i = 0;
            int j = 0;
            while (true)
            {
                if (socket.Available > 0)
                {
                    i = 0;
                    while (socket.Available > 0)
                    {
                        socket.Receive(numArray);
                        string message = Encoding.ASCII.GetString(numArray);
                        int start = message.IndexOf(start_char);
                        int end = message.IndexOf(end_char);
                        int sepor = message.IndexOf(sep_char);
                        DateTime C_time;
                        if (start != -1 && start < sepor && sepor < end)
                        {
                            try
                            {
                                string[] strArray = new Regex("[\n\r]").Replace(message.Substring(start, end + 1 - start).Substring(1).Replace("[", "").Replace("]", ""), "").Split(',');
                                if (strArray[0] != "" && strArray[1] != "")
                                {
                                    C_time = DateTime.Now;
                                    C_time = C_time.ToUniversalTime();
                                    double C_epoc = C_time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).TotalMilliseconds * 1000.0;
                                    double sensor_epoc = Convert.ToDouble(strArray[0]) / 1000000.0;
                                    C_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                                    DateTime dateTime2 = C_time.AddMilliseconds(C_epoc / 1000.0);
                                    DateTime dateTime3;
                                    try
                                    {
                                        C_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                                        dateTime3 = C_time.AddMilliseconds(sensor_epoc);
                                    }
                                    catch
                                    {
                                        break;
                                    }
                                    double temp_val = 0;
                                    try { temp_val = Convert.ToDouble(strArray[1]); }
                                    catch { break; }
                                    message = start_char + strArray[0] + sep_char + strArray[1] + end_char;
                                    if ((dateTime2.AddSeconds(15.0) > dateTime3 && dateTime2.AddSeconds(-15.0) < dateTime3) && (low_range<temp_val && temp_val<high_range))
                                    {
                                        Console.WriteLine(message);
                                        ++j;
                                        if (logging && j > 100)
                                        {
                                            j = 0;
                                            WriteToLog(message);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (logging)
                                {
                                    Console.WriteLine("{0} Exception caught.", ex);
                                    C_time = DateTime.Now;
                                    string C_time_string = C_time.ToString("yyyy-MM-dd HH:mm:ss");
                                    string error_message = " an error with " + ex + " occured at : " + C_time_string;
                                    error_log(error_message);
                                }
                            }
                        }
                        else {
                            bad_counter++;
                            if (bad_counter > 5000)
                            {
                                Console.WriteLine("Hey I think you have set up the program incorrectly would you like to try again? again Y/N");
                                if (Console.ReadLine() == "Y")
                                {
                                  
                                    System.Diagnostics.Process.Start(Application.ExecutablePath);
                                    System.Environment.Exit(-1);
                                }
                            }
                        }
                    }

                }
                else
                {
                    if ((!socket.Connected || i > 10) && socket.Available == 0)
                    {
                        socket.Close();
                        socket.Dispose();
                        Thread.Sleep(500);
                        Connect1(host, port);
                    }
                    Thread.Sleep(10);
                    ++i;
                }
            }
        }

        public static void WriteToLog(string message)
        {
            if (!System.IO.File.Exists(log_name))
            {
                using (StreamWriter text = System.IO.File.CreateText(log_name))
                    text.Close();
            }
            lock (locker)
            {
                FileInfo fileInfo = new FileInfo(log_name);
                if (fileInfo.Length > 20000000)
                    fileInfo.Delete();
                StreamWriter streamWriter = System.IO.File.AppendText(log_name);
                streamWriter.WriteLine(message);
                streamWriter.Close();
            }
        }

        public static void error_log(string message)
        {
            if (!System.IO.File.Exists("error_log.txt"))
            {
                using (StreamWriter text = System.IO.File.CreateText("error_log.txt"))
                    text.Close();
            }
            lock (locker)
            {
                FileInfo fileInfo = new FileInfo("error_log.txt");
                if (fileInfo.Length > 20000000)
                    fileInfo.Delete();
                StreamWriter streamWriter = System.IO.File.AppendText("error_log.txt");
                streamWriter.WriteLine(message);
                streamWriter.Close();
            }
        }
    }
}
