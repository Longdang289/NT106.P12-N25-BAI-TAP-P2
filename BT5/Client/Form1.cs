using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        int port = 8888;
        private TcpClient client;
        private NetworkStream stream;
        private string username;
        private MessageClient messageClient;

        public Form1()
        {
            InitializeComponent();
            EnterUsername();
            messageClient = new MessageClient(this); // Tạo đối tượng MessageClient
        }

        private void EnterUsername()
        {
            // Create a simple form to ask for username
            using (Form usernameForm = new Form())
            {
                Label label = new Label() { Text = "Enter your username:", Dock = DockStyle.Top };
                TextBox textBox = new TextBox() { Dock = DockStyle.Top };
                Button button = new Button() { Text = "Connect", Dock = DockStyle.Bottom };
                usernameForm.Controls.Add(label);
                usernameForm.Controls.Add(textBox);
                usernameForm.Controls.Add(button);
                button.Click += (sender, e) => { username = textBox.Text; usernameForm.Close(); };
                usernameForm.ShowDialog();
            }
            textBox1.Text = username;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Kết nối đến server
            messageClient.ConnectToServer();
            textBox3.Text = port.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Gửi tin nhắn từ richTextBox1
            messageClient.SendMessage(richTextBox1.Text);
        }

        public void Log(string message)
        {
            Invoke((MethodInvoker)delegate
            {
                richTextBox2.AppendText(message + Environment.NewLine);
            });
        }

        // Lớp MessageClient: Quản lý kết nối và tin nhắn
        public class MessageClient
        {
            private Form1 parentForm;
            private TcpClient client;
            private NetworkStream stream;

            public MessageClient(Form1 form)
            {
                this.parentForm = form;
            }

            public void ConnectToServer()
            {
                try
                {
                    if (client == null || !client.Connected)
                    {
                        client = new TcpClient();
                        client.Connect("127.0.0.1", parentForm.port); // Kết nối đến server
                        stream = client.GetStream();

                        // Gửi thông tin đăng ký: username:localPort
                        string registrationMessage = $"{parentForm.username}:{parentForm.port}";
                        byte[] data = Encoding.ASCII.GetBytes(registrationMessage);
                        stream.Write(data, 0, data.Length);

                        parentForm.Log($"Connected to server as {parentForm.username}");

                        // Bắt đầu nhận tin nhắn
                        BeginReceive();
                    }
                }
                catch (Exception ex)
                {
                    parentForm.Log("Error connecting to server: " + ex.Message);
                }
            }

            public void SendMessage(string message)
            {
                try
                {
                    if (client == null || !client.Connected)
                    {
                        parentForm.Log("Client is not connected. Cannot send message.");
                        return;
                    }

                    // Định dạng tin nhắn: "TARGET:message"
                    string targetClient = parentForm.textBox2.Text; // Có thể thêm textbox cho người dùng nhập tên client đích
                    string fullMessage = $"TARGET:{targetClient}:{message}"; // Định dạng tin nhắn

                    byte[] data = Encoding.ASCII.GetBytes(fullMessage);
                    stream.Write(data, 0, data.Length);

                    parentForm.Log($"{targetClient}: {message}");

                    // Hiển thị tin nhắn đã gửi
                    parentForm.richTextBox2.AppendText($"You: {message}" + Environment.NewLine);
                    parentForm.richTextBox1.Clear(); // Xóa nội dung tin nhắn đã gửi
                }
                catch (Exception ex)
                {
                    parentForm.Log("Error sending message: " + ex.Message);
                }
            }

            public void SendFile(string targetClient, byte[] fileBytes)
            {
                try
                {
                    if (client == null || !client.Connected)
                    {
                        parentForm.Log("Client is not connected. Cannot send file.");
                        return;
                    }

                    // Định dạng tin nhắn gửi file: "TARGET:targetClient"
                    string fileTransferHeader = $"TARGET:{targetClient}:FILE";
                    byte[] headerBytes = Encoding.ASCII.GetBytes(fileTransferHeader);
                    stream.Write(headerBytes, 0, headerBytes.Length);

                    // Gửi file dữ liệu
                    stream.Write(fileBytes, 0, fileBytes.Length);
                    parentForm.Log($"File sent to {targetClient}");
                }
                catch (Exception ex)
                {
                    parentForm.Log("Error sending file: " + ex.Message);
                }
            }


            private void BeginReceive()
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReceive), buffer);
                }
                catch (Exception ex)
                {
                    parentForm.Log("Error starting to receive: " + ex.Message);
                }
            }

            private void OnReceive(IAsyncResult ar)
            {
                try
                {
                    byte[] buffer = (byte[])ar.AsyncState;
                    int bytesRead = stream.EndRead(ar);

                    if (bytesRead > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        parentForm.Log($"Received: {message}");

                        // Kiểm tra xem tin nhắn có phải là yêu cầu kết nối đến client khác không
                        if (message.Contains(":"))
                        {
                            string[] parts = message.Split(':');
                            string senderClient = parts[0];
                            string receivedMessage = parts[1];

                            // Hiển thị tin nhắn nhận được lên richTextBox2
                            parentForm.richTextBox2.Invoke((MethodInvoker)delegate
                            {
                                parentForm.richTextBox2.AppendText($"{senderClient}: {receivedMessage}" + Environment.NewLine);
                            });
                        }
                        else
                        {
                            parentForm.Log("Error: " + message);
                        }
                        if (message.StartsWith("FILE:"))
                        {
                            string[] parts = message.Split(':');
                            string fileName = parts[2];

                            // Nhận file
                            byte[] fileBuffer = new byte[1024 * 1024]; // Giả sử file không quá 1MB
                            int fileBytesRead = stream.Read(fileBuffer, 0, fileBuffer.Length);

                            // Lưu file vào thư mục tạm
                            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
                            File.WriteAllBytes(tempFilePath, fileBuffer.Take(fileBytesRead).ToArray());

                            // Hiển thị file và thêm chức năng mở file khi click vào
                            parentForm.richTextBox2.Invoke((MethodInvoker)delegate
                            {
                                LinkLabel link = new LinkLabel { Text = fileName };
                                link.Click += (sender, e) => { Process.Start(tempFilePath); };
                                parentForm.richTextBox2.Controls.Add(link);
                            });

                            parentForm.Log($"File received: {fileName}");
                        }
                        else
                        {
                            // Xử lý tin nhắn văn bản như cũ
                            parentForm.Log($"Received: {message}");
                        }

                        // Lặp lại việc lắng nghe tin nhắn
                        BeginReceive();
                    }
                    else
                    {
                        parentForm.Log("Server has closed the connection.");
                        stream.Close();
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    parentForm.Log("Error receiving data: " + ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);
                byte[] fileBytes = File.ReadAllBytes(filePath);

                // Định dạng tin nhắn gửi file: "FILE:targetClient:fileName"
                string targetClient = textBox2.Text; // Lấy tên client đích từ textbox
                string fileMessage = $"FILE:{targetClient}:{fileName}";

                // Gửi thông báo với tên file trước
                messageClient.SendMessage(fileMessage);

                // Sau đó gửi nội dung file
                messageClient.SendFile(targetClient, fileBytes);
            }
        }
    }
}
