namespace Bubbles
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.Canvas = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.ClearSceneButton = new System.Windows.Forms.Button();
            this.RTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.AddButton = new System.Windows.Forms.Button();
            this.ColorButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.OzTextBox = new System.Windows.Forms.TextBox();
            this.OyTextBox = new System.Windows.Forms.TextBox();
            this.OxTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.listBox1 = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.Canvas)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // Canvas
            // 
            this.Canvas.Location = new System.Drawing.Point(332, 5);
            this.Canvas.Name = "Canvas";
            this.Canvas.Size = new System.Drawing.Size(1200, 1200);
            this.Canvas.TabIndex = 0;
            this.Canvas.TabStop = false;
            this.Canvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseDown);
            this.Canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseMove);
            this.Canvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseUp);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.statusStrip1);
            this.groupBox1.Controls.Add(this.checkedListBox1);
            this.groupBox1.Controls.Add(this.trackBar1);
            this.groupBox1.Controls.Add(this.ClearSceneButton);
            this.groupBox1.Controls.Add(this.RTextBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.AddButton);
            this.groupBox1.Controls.Add(this.ColorButton);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.OzTextBox);
            this.groupBox1.Controls.Add(this.OyTextBox);
            this.groupBox1.Controls.Add(this.OxTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(314, 1184);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Меню:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(3, 1139);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(308, 42);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(237, 32);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.Font = new System.Drawing.Font("Arial", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(6, 553);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.ScrollAlwaysVisible = true;
            this.checkedListBox1.Size = new System.Drawing.Size(285, 270);
            this.checkedListBox1.TabIndex = 9;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(0, 484);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(308, 90);
            this.trackBar1.TabIndex = 2;
            this.trackBar1.Value = 1;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // ClearSceneButton
            // 
            this.ClearSceneButton.Font = new System.Drawing.Font("Arial", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ClearSceneButton.Location = new System.Drawing.Point(6, 1047);
            this.ClearSceneButton.Name = "ClearSceneButton";
            this.ClearSceneButton.Size = new System.Drawing.Size(302, 65);
            this.ClearSceneButton.TabIndex = 3;
            this.ClearSceneButton.Text = "Очистить сцену";
            this.ClearSceneButton.UseVisualStyleBackColor = true;
            this.ClearSceneButton.Click += new System.EventHandler(this.ClearSceneButton_Click);
            // 
            // RTextBox
            // 
            this.RTextBox.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.RTextBox.Location = new System.Drawing.Point(95, 251);
            this.RTextBox.Name = "RTextBox";
            this.RTextBox.Size = new System.Drawing.Size(213, 44);
            this.RTextBox.TabIndex = 8;
            this.RTextBox.Text = "1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(42, 254);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 36);
            this.label5.TabIndex = 7;
            this.label5.Text = "R:";
            // 
            // AddButton
            // 
            this.AddButton.Font = new System.Drawing.Font("Arial", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AddButton.Location = new System.Drawing.Point(6, 379);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(302, 84);
            this.AddButton.TabIndex = 2;
            this.AddButton.Text = "Добавить пузырь \r\nна сцену";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // ColorButton
            // 
            this.ColorButton.BackColor = System.Drawing.Color.Red;
            this.ColorButton.Location = new System.Drawing.Point(121, 314);
            this.ColorButton.Name = "ColorButton";
            this.ColorButton.Size = new System.Drawing.Size(187, 41);
            this.ColorButton.TabIndex = 2;
            this.ColorButton.UseVisualStyleBackColor = false;
            this.ColorButton.Click += new System.EventHandler(this.ColorButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(20, 318);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 36);
            this.label4.TabIndex = 2;
            this.label4.Text = "Цвет:";
            // 
            // OzTextBox
            // 
            this.OzTextBox.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.OzTextBox.Location = new System.Drawing.Point(95, 189);
            this.OzTextBox.Name = "OzTextBox";
            this.OzTextBox.Size = new System.Drawing.Size(213, 44);
            this.OzTextBox.TabIndex = 6;
            this.OzTextBox.Text = "3";
            // 
            // OyTextBox
            // 
            this.OyTextBox.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.OyTextBox.Location = new System.Drawing.Point(95, 119);
            this.OyTextBox.Name = "OyTextBox";
            this.OyTextBox.Size = new System.Drawing.Size(213, 44);
            this.OyTextBox.TabIndex = 5;
            this.OyTextBox.Text = "0";
            // 
            // OxTextBox
            // 
            this.OxTextBox.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.OxTextBox.Location = new System.Drawing.Point(95, 50);
            this.OxTextBox.Name = "OxTextBox";
            this.OxTextBox.Size = new System.Drawing.Size(213, 44);
            this.OxTextBox.TabIndex = 2;
            this.OxTextBox.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(20, 192);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 36);
            this.label3.TabIndex = 4;
            this.label3.Text = "OZ:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(21, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 36);
            this.label2.TabIndex = 3;
            this.label2.Text = "OY:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(21, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 36);
            this.label1.TabIndex = 2;
            this.label1.Text = "OX:";
            // 
            // listBox1
            // 
            this.listBox1.Font = new System.Drawing.Font("Arial", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 33;
            this.listBox1.Location = new System.Drawing.Point(1538, 5);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(425, 1159);
            this.listBox1.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1975, 1208);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Canvas);
            this.Name = "Form1";
            this.Text = "Bubbles";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Canvas)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Canvas;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ColorButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox OzTextBox;
        private System.Windows.Forms.TextBox OyTextBox;
        private System.Windows.Forms.TextBox OxTextBox;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.TextBox RTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button ClearSceneButton;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}

