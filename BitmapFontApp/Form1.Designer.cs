namespace BitmapFontApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            txtInput = new TextBox();
            panelCanvas = new Panel();
            comboBox1 = new ComboBox();
            numericUpDown1 = new NumericUpDown();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            btnTextColor = new Button();
            btnBgColor = new Button();
            lblInput = new Label();
            lblFormat = new Label();
            lblScale = new Label();
            lblColors = new Label();
            chkTransparent = new CheckBox();
            cmbBmpDepth = new ComboBox();
            lblBmpDepth = new Label();
            tableLayoutPanelMain = new TableLayoutPanel();
            flowLayoutPanelControls = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            tableLayoutPanelMain.SuspendLayout();
            flowLayoutPanelControls.SuspendLayout();
            SuspendLayout();
            // 
            // txtInput
            // 
            txtInput.Dock = DockStyle.Fill;
            txtInput.Location = new Point(3, 23);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(878, 23);
            txtInput.TabIndex = 0;
            // 
            // panelCanvas
            // 
            panelCanvas.BackColor = Color.White;
            panelCanvas.Dock = DockStyle.Fill;
            panelCanvas.Location = new Point(3, 53);
            panelCanvas.Name = "panelCanvas";
            panelCanvas.Size = new Size(878, 402);
            panelCanvas.TabIndex = 1;
            panelCanvas.Paint += PanelCanvas_Paint;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(57, 3);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(70, 23);
            comboBox1.TabIndex = 1;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(406, 3);
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(50, 23);
            numericUpDown1.TabIndex = 3;
            numericUpDown1.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // button1
            // 
            button1.Location = new Point(3, 34);
            button1.Name = "button1";
            button1.Size = new Size(130, 25);
            button1.TabIndex = 7;
            button1.Text = "Seçili Karakteri Kaydet";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(139, 34);
            button2.Name = "button2";
            button2.Size = new Size(110, 25);
            button2.TabIndex = 8;
            button2.Text = "Tüm Fontu İndir";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(255, 34);
            button3.Name = "button3";
            button3.Size = new Size(130, 25);
            button3.TabIndex = 9;
            button3.Text = "Tüm Metni Dışa Aktar";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // btnTextColor
            // 
            btnTextColor.Location = new Point(524, 3);
            btnTextColor.Name = "btnTextColor";
            btnTextColor.Size = new Size(85, 25);
            btnTextColor.TabIndex = 5;
            btnTextColor.Text = "Yazı Rengi";
            btnTextColor.UseVisualStyleBackColor = true;
            btnTextColor.Click += BtnTextColor_Click;
            // 
            // btnBgColor
            // 
            btnBgColor.Location = new Point(615, 3);
            btnBgColor.Name = "btnBgColor";
            btnBgColor.Size = new Size(85, 25);
            btnBgColor.TabIndex = 6;
            btnBgColor.Text = "Arka Plan";
            btnBgColor.UseVisualStyleBackColor = true;
            btnBgColor.Click += BtnBgColor_Click;
            // 
            // lblInput
            // 
            lblInput.AutoSize = true;
            lblInput.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblInput.Location = new Point(3, 0);
            lblInput.Name = "lblInput";
            lblInput.Size = new Size(157, 15);
            lblInput.TabIndex = 0;
            lblInput.Text = "Ekranda Çizdirilecek Metin:";
            // 
            // lblFormat
            // 
            lblFormat.AutoSize = true;
            lblFormat.Location = new Point(3, 8);
            lblFormat.Margin = new Padding(3, 8, 3, 0);
            lblFormat.Name = "lblFormat";
            lblFormat.Size = new Size(48, 15);
            lblFormat.TabIndex = 0;
            lblFormat.Text = "Format:";
            // 
            // lblScale
            // 
            lblScale.AutoSize = true;
            lblScale.Location = new Point(360, 8);
            lblScale.Margin = new Padding(10, 8, 3, 0);
            lblScale.Name = "lblScale";
            lblScale.Size = new Size(40, 15);
            lblScale.TabIndex = 2;
            lblScale.Text = "Ölçek:";
            // 
            // lblColors
            // 
            lblColors.AutoSize = true;
            lblColors.Location = new Point(469, 8);
            lblColors.Margin = new Padding(10, 8, 3, 0);
            lblColors.Name = "lblColors";
            lblColors.Size = new Size(49, 15);
            lblColors.TabIndex = 4;
            lblColors.Text = "Renkler:";
            // 
            // chkTransparent
            // 
            chkTransparent.AutoSize = true;
            chkTransparent.Location = new Point(706, 6);
            chkTransparent.Margin = new Padding(3, 6, 3, 3);
            chkTransparent.Name = "chkTransparent";
            chkTransparent.Size = new Size(109, 19);
            chkTransparent.TabIndex = 12;
            chkTransparent.Text = "Şeffaf Arka Plan";
            chkTransparent.UseVisualStyleBackColor = true;
            chkTransparent.CheckedChanged += ChkTransparent_CheckedChanged;
            // 
            // cmbBmpDepth
            // 
            cmbBmpDepth.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBmpDepth.FormattingEnabled = true;
            cmbBmpDepth.Location = new Point(217, 3);
            cmbBmpDepth.Name = "cmbBmpDepth";
            cmbBmpDepth.Size = new Size(130, 23);
            cmbBmpDepth.TabIndex = 11;
            cmbBmpDepth.Visible = false;
            // 
            // lblBmpDepth
            // 
            lblBmpDepth.AutoSize = true;
            lblBmpDepth.Location = new Point(133, 8);
            lblBmpDepth.Margin = new Padding(3, 8, 3, 0);
            lblBmpDepth.Name = "lblBmpDepth";
            lblBmpDepth.Size = new Size(78, 15);
            lblBmpDepth.TabIndex = 10;
            lblBmpDepth.Text = "BMP Derinlik:";
            lblBmpDepth.Visible = false;
            // 
            // tableLayoutPanelMain
            // 
            tableLayoutPanelMain.ColumnCount = 1;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.Controls.Add(lblInput, 0, 0);
            tableLayoutPanelMain.Controls.Add(txtInput, 0, 1);
            tableLayoutPanelMain.Controls.Add(panelCanvas, 0, 2);
            tableLayoutPanelMain.Controls.Add(flowLayoutPanelControls, 0, 3);
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.Location = new Point(10, 10);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.RowCount = 4;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tableLayoutPanelMain.Size = new Size(884, 538);
            tableLayoutPanelMain.TabIndex = 0;
            // 
            // flowLayoutPanelControls
            // 
            flowLayoutPanelControls.Controls.Add(lblFormat);
            flowLayoutPanelControls.Controls.Add(comboBox1);
            flowLayoutPanelControls.Controls.Add(lblBmpDepth);
            flowLayoutPanelControls.Controls.Add(cmbBmpDepth);
            flowLayoutPanelControls.Controls.Add(lblScale);
            flowLayoutPanelControls.Controls.Add(numericUpDown1);
            flowLayoutPanelControls.Controls.Add(lblColors);
            flowLayoutPanelControls.Controls.Add(btnTextColor);
            flowLayoutPanelControls.Controls.Add(btnBgColor);
            flowLayoutPanelControls.Controls.Add(chkTransparent);
            flowLayoutPanelControls.Controls.Add(button1);
            flowLayoutPanelControls.Controls.Add(button2);
            flowLayoutPanelControls.Controls.Add(button3);
            flowLayoutPanelControls.Dock = DockStyle.Fill;
            flowLayoutPanelControls.Location = new Point(3, 461);
            flowLayoutPanelControls.Name = "flowLayoutPanelControls";
            flowLayoutPanelControls.Size = new Size(878, 74);
            flowLayoutPanelControls.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(904, 558);
            Controls.Add(tableLayoutPanelMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(700, 450);
            Name = "Form1";
            Padding = new Padding(10);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bitmap Font Studio";
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            tableLayoutPanelMain.ResumeLayout(false);
            tableLayoutPanelMain.PerformLayout();
            flowLayoutPanelControls.ResumeLayout(false);
            flowLayoutPanelControls.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtInput;
        private Panel panelCanvas;
        private ComboBox comboBox1;
        private NumericUpDown numericUpDown1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button btnTextColor;
        private Button btnBgColor;
        private Label lblInput;
        private Label lblFormat;
        private Label lblScale;
        private Label lblColors;
        private CheckBox chkTransparent;
        private ComboBox cmbBmpDepth;
        private Label lblBmpDepth;
        private TableLayoutPanel tableLayoutPanelMain;
        private FlowLayoutPanel flowLayoutPanelControls;
    }
}