using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace multicast_test
{
    public class Program
    {
        public static int TTL = 128;
        private static readonly object LogLock = new object();
        private static string LogDirectory = "logs";
        private static string ProgramLogFile = "";
        private static string UserLogFile = "";

        public static void Main(string[] args)
        {
            try
            {
                InitializeLogging();
                LogProgram("Application started");
                
                // Display banner with colors
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("ICTFella.com - Multicast Testing Tool");
                Console.WriteLine("=====================================\n");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Interface list:\n");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"    0: {"0.0.0.0",-40} Any");
                Console.ResetColor();

                AddressDictionary.Add(0, IPAddress.Any);

                // enumerate available interfaces
                var i = 1;
                foreach (var iface in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.SupportsMulticast && n.OperationalStatus == OperationalStatus.Up))
                {
                    foreach (var ip in iface.GetIPProperties().UnicastAddresses)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"   {i,2}: ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"{ip.Address,-40} ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"{iface.Name} ({iface.Description})");
                        Console.ResetColor();
                        
                        AddressDictionary.Add(i, ip.Address);
                        i++;
                    }
                }

                // prompt user to select an interface
                var selection = -1;
                while (selection == -1)
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("\nSelect interface: ");
                        Console.ResetColor();
                        
                        var input = Console.ReadLine() ?? "";
                        LogUser($"Interface selection input: {input}");
                        if (int.TryParse(input, out selection))
                        {
                            // prevent user selecting a number beyond the range of display interfaces
                            if (selection > i - 1 || selection < 0) 
                            {
                                selection = -1;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Invalid selection. Please try again.");
                                Console.ResetColor();
                            }
                            else
                            {
                                // select binding address from the interface dictionary
                                _bindingAddress = AddressDictionary[selection];
                                LogUser($"Selected interface: {selection} - {_bindingAddress}");
                            }
                        }
                        else
                        {
                            selection = -1;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid input. Please enter a number.");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception ex)
                    {
                        selection = -1;
                        LogProgram($"Error in interface selection: {ex.Message}");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid input. Please enter a number.");
                        Console.ResetColor();
                    }
                }

                // prompt to select a multicast address
                Console.WriteLine();
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Enter multicast address (224.0.0.0 to 239.255.255.255) to use [default: ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write($"{MulticastAddress}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("]: ");
                    Console.ResetColor();
                    
                    string enteredMc = Console.ReadLine() ?? "";
                    LogUser($"Multicast address input: '{enteredMc}'");
                    
                    if(string.IsNullOrEmpty(enteredMc)) 
                    {
                        LogUser($"Using default multicast address: {MulticastAddress}");
                        break; // Use default multicast address
                    }
                    
                    if(IPAddress.TryParse(enteredMc, out IPAddress? multicastAddress) && multicastAddress != null)
                    {
                        if(IsMulticast(multicastAddress))
                        {
                            MulticastAddress = multicastAddress;
                            LogUser($"Set multicast address to: {MulticastAddress}");
                            break;
                        }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("A multicast IP addresses must be between 224.0.0.0 to 239.255.255.255.");
                        Console.ResetColor();
                        continue;
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Not a valid IP address");
                    Console.ResetColor();
                }

                // prompt to select a multicast port
                Console.WriteLine();
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Enter multicast port to use (between 1 and 65535) [default: {MulticastPort}]: ");
                    Console.ResetColor();
                    
                    string enteredPortString = Console.ReadLine() ?? "";
                    LogUser($"Port input: '{enteredPortString}'");
                    
                    if(string.IsNullOrEmpty(enteredPortString)) 
                    {
                        LogUser($"Using default port: {MulticastPort}");
                        break; // Use default port
                    }
                    
                    if(!int.TryParse(enteredPortString, out int enteredPort))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Not a valid number");
                        Console.ResetColor();
                        continue;
                    }
                    if(enteredPort < 1 || enteredPort > 65535)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Port must be between 1 and 65535");
                        Console.ResetColor();
                        continue;
                    }
                    MulticastPort = enteredPort;
                    LogUser($"Set port to: {MulticastPort}");
                    break;
                }

                // NEW: prompt to select TTL
                Console.WriteLine();
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Enter TTL (Time To Live) value (1-255) [default: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{TTL}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("]: ");
                    Console.ResetColor();
                    
                    string enteredTtlString = Console.ReadLine() ?? "";
                    LogUser($"TTL input: '{enteredTtlString}'");
                    
                    if(string.IsNullOrEmpty(enteredTtlString)) 
                    {
                        LogUser($"Using default TTL: {TTL}");
                        break; // Use default TTL
                    }
                    
                    if(!int.TryParse(enteredTtlString, out int enteredTtl))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Not a valid number");
                        Console.ResetColor();
                        continue;
                    }
                    if(enteredTtl < 1 || enteredTtl > 255)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("TTL must be between 1 and 255");
                        Console.ResetColor();
                        continue;
                    }
                    TTL = enteredTtl;
                    LogUser($"Set TTL to: {TTL}");
                    break;
                }

                // Display current configuration
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Configuration Summary:");
                Console.WriteLine("=====================");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Interface: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(_bindingAddress);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Multicast Address: ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(MulticastAddress);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Port: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(MulticastPort);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("TTL: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(TTL);
                Console.ResetColor();

                // reset selection variable
                selection = -1;

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Available Actions:");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("    1: Multicast sender (transmit data)");
                Console.WriteLine("    2: Multicast subscriber (listen socket, receive data)");
                Console.WriteLine("    9: Exit");
                Console.ResetColor();
                Console.WriteLine();

                // prompt to select an action
                while (selection == -1)
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("Select action: ");
                        Console.ResetColor();
                        
                        var actionInput = Console.ReadLine() ?? "";
                        LogUser($"Action selection: {actionInput}");
                        if (int.TryParse(actionInput, out selection))
                        {
                            switch (selection)
                            {
                                case 9:
                                {
                                    LogUser("User selected exit");
                                    LogProgram("Application exiting normally");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Thank you for using ICTFella.com Multicast Testing Tool!");
                                    Console.ResetColor();
                                    return;
                                }
                                case 1:
                                {
                                    LogUser("User selected sender mode");
                                    StartSender();
                                    break;
                                }
                                case 2:
                                {
                                    LogUser("User selected subscriber mode");
                                    StartListener();
                                    break;
                                }
                                default:
                                {
                                    selection = -1;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Invalid selection. Please choose 1, 2, or 9.");
                                    Console.ResetColor();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            selection = -1;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid input. Please enter a number.");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception e)
                    {
                        selection = -1;
                        LogProgram($"Error in action selection: {e.Message}");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid input. Please enter a number.");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                LogProgram($"Fatal error: {ex}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void StartSender()
        {
            try
            {
                LogProgram("Starting sender mode");
                using (var client = new UdpClient())
                {
                    // Bind to the selected local interface
                    client.Client.Bind(new IPEndPoint(_bindingAddress, 0));

                    // Set the TTL for multicast packets here (required for crossing VLANs/routers)
                    client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, TTL);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"\nBound UDP client to ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{_bindingAddress}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(". Sending data to multicast group address ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"{MulticastAddress}");
                    Console.ResetColor();
                    Console.WriteLine();

                    LogProgram($"Sender bound to {_bindingAddress}, targeting {MulticastAddress}:{MulticastPort}, TTL: {TTL}");

                    ulong n = 0;
                    while (true)
                    {
                        var timestamp = DateTime.Now;
                        var message = $"ICTFella.com Multicast Testing Tool @ {timestamp.ToLongTimeString()}";
                        SendMessage(client, message);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"Message {n,-5} sent to ");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write($"{MulticastAddress}");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(":");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"{MulticastPort}");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("  TTL: ");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"{TTL}");
                        Console.ResetColor();

                        LogUser($"Sent message {n} to {MulticastAddress}:{MulticastPort} at {timestamp}");
                        Thread.Sleep(1000);
                        n++;
                    }
                }
            }
            catch (Exception e)
            {
                LogProgram($"Error in sender mode: {e}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Sender error: {e.Message}");
                Console.ResetColor();
            }
        }

        private static void StartListener()
        {
            try
            {
                LogProgram("Starting listener mode");
                _udpClient = new UdpClient(MulticastPort);

                _udpClient.EnableBroadcast = true;
                _udpClient.JoinMulticastGroup(MulticastAddress, _bindingAddress);
                _udpClient.Client.SetSocketOption((_bindingAddress.AddressFamily == AddressFamily.InterNetwork) ? SocketOptionLevel.IP : SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, TTL);

                var receiveThread = new Thread(Receive) { IsBackground = true };
                receiveThread.Start();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"\nBound UDP listener on ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{_bindingAddress}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(". Joined multicast group ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($"{MulticastAddress}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(". Port ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{MulticastPort}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(". Waiting to receive data...\n");
                Console.ResetColor();

                LogProgram($"Listener bound to {_bindingAddress}:{MulticastPort}, joined group {MulticastAddress}, TTL: {TTL}");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Press any key to stop listening...");
                Console.ResetColor();
                Console.ReadKey();
                
                LogUser("User stopped listener");
                _udpClient?.Close();
            }
            catch (Exception e)
            {
                LogProgram($"Error in listener mode: {e}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Listener error: {e.Message}");
                Console.ResetColor();
            }
        }

        public static void Receive()
        {
            try
            {
                while (true)
                {
                    if (_udpClient == null) break;
                    
                    var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    var data = _udpClient.Receive(ref ipEndPoint);
                    var message = Encoding.Default.GetString(data);
                    var timestamp = DateTime.Now;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"Received ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{data.Length}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" bytes from ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{ipEndPoint}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(": \"");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{message}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\"");
                    Console.ResetColor();

                    LogUser($"Received {data.Length} bytes from {ipEndPoint} at {timestamp}: {message}");
                }
            }
            catch (Exception ex)
            {
                if (_udpClient != null) // Only log if client is still active
                {
                    LogProgram($"Error in receive loop: {ex.Message}");
                }
            }
        }

        public static void SendMessage(UdpClient client, string message)
        {
            var data = Encoding.Default.GetBytes(message);
            var ipEndPoint = new IPEndPoint(MulticastAddress, MulticastPort);
            client.Send(data, data.Length, ipEndPoint);
        }

        private static bool IsMulticast(IPAddress ipAddress)
        {
            byte addressFirstOctet = ipAddress.GetAddressBytes()[0];
            return addressFirstOctet >= 224 && addressFirstOctet <= 239;
        }

        private static void InitializeLogging()
        {
            try
            {
                // Create logs directory if it doesn't exist
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                ProgramLogFile = Path.Combine(LogDirectory, $"program_{timestamp}.log");
                UserLogFile = Path.Combine(LogDirectory, $"user_{timestamp}.log");

                // Initialize log files with headers
                File.WriteAllText(ProgramLogFile, $"ICTFella.com Multicast Testing Tool - Program Log\nStarted: {DateTime.Now}\n{new string('=', 50)}\n");
                File.WriteAllText(UserLogFile, $"ICTFella.com Multicast Testing Tool - User Log\nStarted: {DateTime.Now}\n{new string('=', 50)}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not initialize logging: {ex.Message}");
            }
        }

        private static void LogProgram(string message)
        {
            try
            {
                lock (LogLock)
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n";
                    File.AppendAllText(ProgramLogFile, logEntry);
                }
            }
            catch
            {
                // Silently ignore logging errors to prevent disrupting the main application
            }
        }

        private static void LogUser(string message)
        {
            try
            {
                lock (LogLock)
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n";
                    File.AppendAllText(UserLogFile, logEntry);
                }
            }
            catch
            {
                // Silently ignore logging errors to prevent disrupting the main application
            }
        }

        private static IPAddress _bindingAddress = IPAddress.Any;
        private static IPAddress MulticastAddress = IPAddress.Parse("239.0.1.2");
        private static int MulticastPort = 20480;
        private static readonly Dictionary<int, IPAddress> AddressDictionary = new Dictionary<int, IPAddress>();
        private static UdpClient? _udpClient;
    }
}