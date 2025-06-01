# ICTFella.com - Multicast Testing Tool V3

An advanced command-line multicast network testing utility with enhanced features including colorized output, configurable TTL, comprehensive logging, and cross-platform support.

## 🚀 Features

### ✨ Enhanced User Experience
- **Colorized Console Output**: Easy-to-read color-coded interface for better visibility
- **Interactive Prompts**: User-friendly prompts for all configuration options
- **Configuration Summary**: Clear display of all selected settings before operation

### 🔧 Advanced Configuration
- **Configurable TTL**: Set custom Time-To-Live values (1-255) for multicast packets
- **Interface Selection**: Choose specific network interfaces for testing
- **Custom Multicast Address**: Configure any valid multicast address (224.0.0.0 to 239.255.255.255)
- **Custom Port Selection**: Choose any port between 1 and 65535

### 📊 Comprehensive Logging
- **Program Logs**: Detailed technical logs for debugging and troubleshooting
- **User Activity Logs**: Track user interactions, configurations, and test results
- **Timestamped Entries**: All log entries include precise timestamps
- **Automatic Log Organization**: Logs are stored in a dedicated `logs/` directory

### 🌐 Cross-Platform Support
- **Windows**: Native executable (.exe) with full functionality
- **Linux**: Native binary for Ubuntu and other Linux distributions
- **Single-File Deployment**: Self-contained executables require no additional dependencies

## 📥 Download

Pre-compiled binaries are available in the `Bin/` folder:

- **Windows**: `Multicast Testing Tool - ICTFella.com V3.exe`
- **Linux**: `multicast-testing-tool-ictfella-v3-linux`

## 🛠️ Building from Source

### Prerequisites
- .NET 8.0 SDK
- Git (for source control)

### Build Commands

#### Windows Build
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

#### Linux Build
```bash
dotnet publish MulticastTest-Linux.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

## 📋 Usage

### 1. Select Network Interface
The tool will display all available multicast-capable network interfaces. Choose the specific interface you want to test.

> **Note**: Don't use interface 0 (any) for sending - it won't work. Always select a specific interface.

### 2. Configure Settings
- **Multicast Address**: Default is `239.0.1.2`, or enter your custom address
- **Port**: Default is `20480`, or choose your preferred port
- **TTL**: Default is `128`, or set a custom value based on your network topology

### 3. Choose Operation Mode

#### Sender Mode (Option 1)
Transmits multicast packets with timestamped messages. You'll see colorized output showing:
- Message sequence numbers
- Target multicast address and port
- TTL value
- Transmission timestamps

#### Receiver Mode (Option 2)
Listens for multicast packets and displays:
- Source address and port
- Message content
- Packet size
- Reception timestamps

## 🎨 Color Scheme

The tool uses an intuitive color scheme for better readability:
- **🔵 Cyan**: Application banner and TTL values
- **🟡 Yellow**: Prompts and port numbers
- **🟢 Green**: Interface addresses and success messages
- **🟣 Magenta**: Multicast addresses
- **⚪ White**: General information and message counters
- **🔴 Red**: Error messages and warnings
- **🔘 Gray**: Secondary information

## 📁 Log Files

Logs are automatically created in the `logs/` directory:

- `program_YYYYMMDD_HHMMSS.log`: Technical program logs
- `user_YYYYMMDD_HHMMSS.log`: User activity and configuration logs

## 🔍 Troubleshooting

### Common Issues

1. **"Interface not found"**
   - Ensure the selected interface supports multicast
   - Check that the interface is up and operational

2. **"Permission denied" (Linux)**
   - Make the binary executable: `chmod +x multicast-testing-tool-ictfella-v3-linux`
   - May require sudo for certain network operations

3. **No packets received**
   - Verify firewall settings allow multicast traffic
   - Check that sender and receiver are using the same multicast address and port
   - Ensure TTL is sufficient for your network topology

### Network Requirements

- Multicast must be enabled on your network infrastructure
- Switches and routers must support and be configured for multicast forwarding
- IGMP (Internet Group Management Protocol) should be properly configured

## 🔧 Technical Details

- **Framework**: .NET 8.0
- **Language**: C#
- **Deployment**: Self-contained single-file executables
- **Logging**: Thread-safe file-based logging with millisecond precision
- **Network**: UDP-based multicast using System.Net.Sockets

## 📜 License

MIT License - see LICENSE file for details

## 🤝 Contributing

This project is based on the original work by [enclave-networks](https://github.com/enclave-networks/multicast-test). 

Contributions are welcome! Please feel free to submit issues and pull requests.

## 🔗 Links

- **Original Project**: [enclave-networks/multicast-test](https://github.com/enclave-networks/multicast-test)
- **ICTFella.com**: [Visit our website](https://ictfella.com)

## 📊 Version History

### V3.0.0 (Current)
- Added colorized console output
- Configurable TTL settings
- Comprehensive logging system
- Enhanced error handling
- Cross-platform single-file deployment
- Improved user interface

### Previous Versions
- Based on enclave-networks multicast testing tool
- Basic multicast send/receive functionality

---

**Made with ❤️ by ICTFella.com** 