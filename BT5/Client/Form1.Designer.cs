namespace Client;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        button1 = new Button();
        button2 = new Button();
        button3 = new Button();
        richTextBox1 = new RichTextBox();
        richTextBox2 = new RichTextBox();
        textBox1 = new TextBox();
        textBox2 = new TextBox();
        textBox3 = new TextBox();
        label1 = new Label();
        label2 = new Label();
        label3 = new Label();
        SuspendLayout();
        // 
        // button1
        // 
        button1.Location = new Point(694, 363);
        button1.Name = "button1";
        button1.Size = new Size(94, 29);
        button1.TabIndex = 0;
        button1.Text = "File";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // button2
        // 
        button2.Location = new Point(694, 328);
        button2.Name = "button2";
        button2.Size = new Size(94, 29);
        button2.TabIndex = 1;
        button2.Text = "Send";
        button2.UseVisualStyleBackColor = true;
        // 
        // button3
        // 
        button3.Location = new Point(694, 3);
        button3.Name = "button3";
        button3.Size = new Size(94, 29);
        button3.TabIndex = 2;
        button3.Text = "Connect";
        button3.UseVisualStyleBackColor = true;
        button3.Click += button3_Click;
        // 
        // richTextBox1
        // 
        richTextBox1.Location = new Point(3, 328);
        richTextBox1.Name = "richTextBox1";
        richTextBox1.Size = new Size(685, 120);
        richTextBox1.TabIndex = 3;
        richTextBox1.Text = "";
        // 
        // richTextBox2
        // 
        richTextBox2.Location = new Point(3, 38);
        richTextBox2.Name = "richTextBox2";
        richTextBox2.Size = new Size(785, 284);
        richTextBox2.TabIndex = 4;
        richTextBox2.Text = "";
        // 
        // textBox1
        // 
        textBox1.Location = new Point(82, 5);
        textBox1.Name = "textBox1";
        textBox1.Size = new Size(125, 27);
        textBox1.TabIndex = 5;
        // 
        // textBox2
        // 
        textBox2.Location = new Point(314, 5);
        textBox2.Name = "textBox2";
        textBox2.Size = new Size(125, 27);
        textBox2.TabIndex = 6;
        // 
        // textBox3
        // 
        textBox3.Location = new Point(539, 5);
        textBox3.Name = "textBox3";
        textBox3.Size = new Size(125, 27);
        textBox3.TabIndex = 7;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(30, 9);
        label1.Name = "label1";
        label1.Size = new Size(46, 20);
        label1.TabIndex = 8;
        label1.Text = "From:";
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(495, 9);
        label2.Name = "label2";
        label2.Size = new Size(38, 20);
        label2.TabIndex = 9;
        label2.Text = "Port:";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(280, 9);
        label3.Name = "label3";
        label3.Size = new Size(28, 20);
        label3.TabIndex = 10;
        label3.Text = "To:";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(label3);
        Controls.Add(label2);
        Controls.Add(label1);
        Controls.Add(textBox3);
        Controls.Add(textBox2);
        Controls.Add(textBox1);
        Controls.Add(richTextBox2);
        Controls.Add(richTextBox1);
        Controls.Add(button3);
        Controls.Add(button2);
        Controls.Add(button1);
        Name = "Form1";
        Text = "r";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button button1;
    private Button button2;
    private Button button3;
    private RichTextBox richTextBox1;
    private RichTextBox richTextBox2;
    private TextBox textBox1;
    private TextBox textBox2;
    private TextBox textBox3;
    private Label label1;
    private Label label2;
    private Label label3;
}
