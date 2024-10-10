using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Server
{
    public partial class Form1 : Form
    {
        int port = 8888;
        private TcpListener server;
        private Dictionary<string, ClientInfo> clientDict; // Dictionary lưu thông tin client theo username

        public Form1()
        {
            InitializeComponent();
            clientDict = new Dictionary<string, ClientInfo>(); // Khởi tạo Dictionary
            textBox1.Text = port.ToString(); // Hiển thị port trong TextBox
            button1.Click += button1_Click;
            // Thêm các cột hiển thị cho ListView
            listView1.Columns.Add("Username", 150);
            listView1.Columns.Add("IP Address", 150);
            listView1.Columns.Add("Port", 100);
            listView1.View = View.Details; // Hiển thị dưới dạng danh sách chi tiết
        }

        // Bắt đầu server khi nhấn nút
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                server.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
                Log("Server started...");
            }
            catch (Exception ex)
            {
                Log("Error starting server: " + ex.Message);
            }
        }

        // Xử lý khi client kết nối đến
        private void OnClientConnect(IAsyncResult ar)
        {
            try
            {
                TcpClient client = server.EndAcceptTcpClient(ar);
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();

                // Tiếp tục chờ client khác kết nối
                server.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
            }
            catch (Exception ex)
            {
                Log("Error accepting client: " + ex.Message);
            }
        }

        // Xử lý logic cho từng client
        // Xử lý khi nhận tin nhắn từ client
        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            string username = null;

            try
            {
                // Đăng ký username
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string registrationMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                string[] registrationData = registrationMessage.Split(':');
                username = registrationData[0];
                int clientPort = int.Parse(registrationData[1]);

                IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                var clientInfo = new ClientInfo(username, endPoint.Address, clientPort, client);
                lock (clientDict)
                {
                    clientDict[username] = clientInfo;
                }

                // Cập nhật giao diện và danh sách client
                this.Invoke((MethodInvoker)delegate
                {
                    ListViewItem item = new ListViewItem(username);
                    item.SubItems.Add(endPoint.Address.ToString());
                    item.SubItems.Add(clientPort.ToString());
                    listView1.Items.Add(item);
                });

                // Lắng nghe tin nhắn từ client
                while (true)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                        // Kiểm tra xem tin nhắn có phải gửi đến một client khác không
                        if (message.StartsWith("TARGET:"))
                        {
                            string targetUsername = message.Substring(7).Split(':')[0];
                            string userMessage = message.Substring(7).Split(':')[1];
                            SendPrivateMessage(targetUsername, userMessage);
                        }
                        if (message.StartsWith("FILE:"))
                        {
                            string targetUsername = message.Split(':')[1];
                            string fileName = message.Split(':')[2];

                            // Nhận file từ client
                            byte[] fileBuffer = new byte[1024 * 1024]; // Giả sử file không quá 1MB
                            int fileBytesRead = stream.Read(fileBuffer, 0, fileBuffer.Length);

                            // Gửi file đến client đích
                            SendPrivateFile(targetUsername, fileName, fileBuffer.Take(fileBytesRead).ToArray());
                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error handling client {client.Client.RemoteEndPoint}: " + ex.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(username))
                {
                    lock (clientDict)
                    {
                        clientDict.Remove(username);
                    }

                    this.Invoke((MethodInvoker)delegate
                    {
                        for (int i = 0; i < listView1.Items.Count; i++)
                        {
                            if (listView1.Items[i].SubItems[0].Text == username)
                            {
                                listView1.Items.RemoveAt(i);
                                break;
                            }
                        }
                    });

                    Log($"{username} disconnected.");
                }

                client.Close();
            }
        }


        // Xử lý yêu cầu tìm kiếm client khác
        private void HandleFindRequest(NetworkStream stream, string username, string targetUsername)
        {
            lock (clientDict)
            {
                if (clientDict.ContainsKey(targetUsername))
                {
                    ClientInfo targetClientInfo = clientDict[targetUsername];
                    string targetInfo = $"{targetClientInfo.IpAddress}:{targetClientInfo.Port}";

                    // Gửi thông tin của client mục tiêu cho client yêu cầu
                    byte[] targetData = Encoding.ASCII.GetBytes(targetInfo);
                    stream.Write(targetData, 0, targetData.Length);

                    Log($"{username} requested {targetUsername}: {targetInfo}");
                }
                else
                {
                    // Nếu không tìm thấy client
                    byte[] errorMessage = Encoding.ASCII.GetBytes("Client not found");
                    stream.Write(errorMessage, 0, errorMessage.Length);

                    Log($"{username} requested {targetUsername}: Client not found");
                }
            }
        }

        // Gửi tin nhắn riêng cho client
        private void SendPrivateMessage(string targetUsername, string message)
        {
            lock (clientDict)
            {
                if (clientDict.ContainsKey(targetUsername))
                {
                    ClientInfo targetClient = clientDict[targetUsername];
                    byte[] msgBytes = Encoding.ASCII.GetBytes(message);
                    targetClient.TcpClient.GetStream().Write(msgBytes, 0, msgBytes.Length);
                    Log($"Private message sent to {targetUsername}");
                }
                else
                {
                    Log($"Client {targetUsername} not found.");
                }
            }
        }

        private void SendPrivateFile(string targetUsername, string fileName, byte[] fileBytes)
        {
            lock (clientDict)
            {
                if (clientDict.ContainsKey(targetUsername))
                {
                    ClientInfo targetClient = clientDict[targetUsername];

                    // Gửi thông báo về file trước
                    string fileMessage = $"FILE:{targetUsername}:{fileName}";
                    byte[] fileMessageBytes = Encoding.ASCII.GetBytes(fileMessage);
                    targetClient.TcpClient.GetStream().Write(fileMessageBytes, 0, fileMessageBytes.Length);

                    // Gửi nội dung file
                    targetClient.TcpClient.GetStream().Write(fileBytes, 0, fileBytes.Length);

                    Log($"File {fileName} sent to {targetUsername}");
                }
                else
                {
                    Log($"Client {targetUsername} not found.");
                }
            }
        }

        // Ghi nhật ký vào RichTextBox
        private void Log(string message)
        {
            Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText(message + Environment.NewLine);
            });
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Xử lý khi lựa chọn client từ danh sách (nếu cần)
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Xử lý khi thay đổi giá trị trong TextBox
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Xử lý khi thay đổi nội dung nhật ký
        }
    }

    // Lớp lưu thông tin client
    public class ClientInfo
    {
        public string Username { get; set; }
        public IPAddress IpAddress { get; set; }
        public int Port { get; set; }
        public TcpClient TcpClient { get; set; }

        public ClientInfo(string username, IPAddress ipAddress, int port, TcpClient tcpClient)
        {
            Username = username;
            IpAddress = ipAddress;
            Port = port;
            TcpClient = tcpClient;
        }
    }
}
