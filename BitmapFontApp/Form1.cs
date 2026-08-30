using System.Drawing.Imaging;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace BitmapFontApp
{
    public partial class Form1 : Form
    {
        private int pixelSize = 4;

        private Color textColor = Color.Black;
        private Color backgroundColor = Color.White;

        private bool transparentBackground = false;

        private const int FontWidth = 8;
        private const int PixelFontHeight = 16;

        private const int CanvasPadding = 10;
        private const int CharacterSpacing = 2;

        public Form1()
        {
            InitializeComponent();

            SetupEvents();

            pixelSize = (int)numericUpDown1.Value;

            UpdateFormatUI();
            UpdateColorButtonStyles();
        }

        #region Setup

        private void SetupEvents()
        {
            txtInput.TextChanged += (s, e) =>
            {
                UpdateCanvasSize();
                panelCanvas.Invalidate();
            };

            numericUpDown1.ValueChanged += (s, e) =>
            {
                pixelSize = (int)numericUpDown1.Value;

                UpdateCanvasSize();
                panelCanvas.Invalidate();
            };

            if (comboBox1.Items.Count == 0)
            {
                comboBox1.Items.AddRange(
                    new object[]
                    {
                        "PNG",
                        "JPG",
                        "JPEG",
                        "BMP",
                        "ICO",
                        "GIF",
                        "TIFF",
                        "SVG",
                        "EPS",
                        "XBM"
                    });
            }

            if (comboBox1.SelectedIndex == -1)
                comboBox1.SelectedIndex = 0;

            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = 32;

            if (numericUpDown1.Value < 1)
                numericUpDown1.Value = 4;

            if (cmbBmpDepth.Items.Count == 0)
            {
                cmbBmpDepth.Items.AddRange(
                    new object[]
                    {
                        "1-bit (Siyah/Beyaz)",
                        "4-bit (16 Renk)",
                        "8-bit (256 Renk)",
                        "24-bit (True Color)",
                        "32-bit (Alpha)"
                    });
            }

            cmbBmpDepth.SelectedIndexChanged +=
                (s, e) =>
                {
                    UpdateFormatUI();
                    panelCanvas.Invalidate();
                };
        }

        #endregion

        #region Format / UI

        private string GetSelectedExtension()
        {
            return comboBox1.SelectedItem?
                       .ToString()?
                       .ToLowerInvariant()
                   ?? "png";
        }

        private int GetSelectedBmpDepth()
        {
            return cmbBmpDepth.SelectedIndex switch
            {
                0 => 1,
                1 => 4,
                2 => 8,
                3 => 24,
                4 => 32,
                _ => 24
            };
        }

        private bool FormatSupportsTransparency()
        {
            string ext = GetSelectedExtension();

            switch (ext)
            {
                case "png":
                case "ico":
                case "tiff":
                case "svg":
                    return true;

                case "gif":
                    return true;

                case "bmp":
                    return GetSelectedBmpDepth() == 32;

                case "eps":
                    // EPS'te arka plan objesi çizilmeyerek
                    // transparent görünüm elde edilir.
                    return true;

                default:
                    return false;
            }
        }

        private bool FormatSupportsColors()
        {
            string ext = GetSelectedExtension();

            // XBM gerçek anlamda monochrome bitmap'tir.
            return ext != "xbm";
        }

        private void UpdateFormatUI()
        {
            string ext = GetSelectedExtension();

            bool isBmp = ext == "bmp";

            lblBmpDepth.Visible = isBmp;
            cmbBmpDepth.Visible = isBmp;

            bool transparencySupported =
                FormatSupportsTransparency();

            chkTransparent.Enabled =
                transparencySupported;

            if (!transparencySupported)
            {
                if (chkTransparent.Checked)
                    chkTransparent.Checked = false;

                transparentBackground = false;
            }

            bool colorsSupported =
                FormatSupportsColors();

            btnTextColor.Enabled = colorsSupported;
            btnBgColor.Enabled = colorsSupported;

            if (!colorsSupported)
            {
                btnTextColor.Text = "Yazý Rengi";
                btnBgColor.Text = "Arka Plan";
            }

            // ICO için aþýrý büyük boyutlarý engelle.
            if (ext == "ico")
            {
                numericUpDown1.Maximum = 8;

                if (numericUpDown1.Value > 8)
                    numericUpDown1.Value = 8;
            }
            else
            {
                numericUpDown1.Maximum = 32;
            }

            UpdateColorButtonStyles();
            UpdateCanvasSize();
            panelCanvas.Invalidate();
        }

        private void UpdateColorButtonStyles()
        {
            btnTextColor.BackColor = textColor;
            btnTextColor.ForeColor =
                GetContrastingColor(textColor);

            btnBgColor.BackColor = backgroundColor;
            btnBgColor.ForeColor =
                GetContrastingColor(backgroundColor);

            if (transparentBackground)
            {
                panelCanvas.BackColor = Color.White;
            }
            else
            {
                panelCanvas.BackColor = backgroundColor;
            }

            panelCanvas.Invalidate();
        }

        private Color GetContrastingColor(Color color)
        {
            double luminance =
                (0.299 * color.R +
                 0.587 * color.G +
                 0.114 * color.B) / 255.0;

            return luminance > 0.5
                ? Color.Black
                : Color.White;
        }

        #endregion

        #region Color Events

        private void BtnTextColor_Click(
            object sender,
            EventArgs e)
        {
            if (!FormatSupportsColors())
                return;

            using ColorDialog cd = new ColorDialog();

            cd.Color = textColor;
            cd.FullOpen = true;

            if (cd.ShowDialog() == DialogResult.OK)
            {
                textColor = cd.Color;
                UpdateColorButtonStyles();
            }
        }

        private void BtnBgColor_Click(
            object sender,
            EventArgs e)
        {
            if (!FormatSupportsColors())
                return;

            using ColorDialog cd = new ColorDialog();

            cd.Color = backgroundColor;
            cd.FullOpen = true;

            if (cd.ShowDialog() == DialogResult.OK)
            {
                backgroundColor = cd.Color;
                UpdateColorButtonStyles();
            }
        }

        private void ChkTransparent_CheckedChanged(
            object sender,
            EventArgs e)
        {
            if (!FormatSupportsTransparency())
            {
                chkTransparent.Checked = false;
                transparentBackground = false;
            }
            else
            {
                transparentBackground =
                    chkTransparent.Checked;
            }

            UpdateColorButtonStyles();
        }

        private void ComboBox1_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            UpdateFormatUI();
        }

        #endregion

        #region Canvas

        private int CharacterWidth =>
            FontWidth * pixelSize;

        private int CharacterHeight =>
            FontHeight * pixelSize;

        private int CharacterAdvance =>
            CharacterWidth +
            CharacterSpacing * pixelSize;

        private void UpdateCanvasSize()
        {
            if (panelCanvas == null)
                return;

            string text = txtInput?.Text ?? "";

            if (text.Length == 0)
            {
                panelCanvas.AutoScrollMinSize =
                    new Size(
                        panelCanvas.ClientSize.Width,
                        panelCanvas.ClientSize.Height);

                return;
            }

            Size size =
                CalculateCanvasSize(text);

            panelCanvas.AutoScrollMinSize =
                new Size(
                    Math.Max(
                        size.Width,
                        panelCanvas.ClientSize.Width),
                    Math.Max(
                        size.Height,
                        panelCanvas.ClientSize.Height));
        }

        private Size CalculateCanvasSize(
            string text)
        {
            int width =
                CanvasPadding * 2;

            int maxWidth = 0;
            int currentWidth = 0;

            int lineCount = 1;

            foreach (char c in text)
            {
                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    maxWidth =
                        Math.Max(
                            maxWidth,
                            currentWidth);

                    currentWidth = 0;
                    lineCount++;

                    continue;
                }

                currentWidth +=
                    CharacterAdvance;
            }

            maxWidth =
                Math.Max(
                    maxWidth,
                    currentWidth);

            width += maxWidth;

            int height =
                CanvasPadding * 2 +
                (lineCount * CharacterHeight) +
                ((lineCount - 1) *
                 5 * pixelSize);

            return new Size(
                Math.Max(1, width),
                Math.Max(1, height));
        }

        private void PanelCanvas_Paint(
            object sender,
            PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(
                transparentBackground
                    ? Color.White
                    : backgroundColor);

            DrawTextToGraphics(
                g,
                txtInput.Text,
                pixelSize,
                CanvasPadding,
                CanvasPadding,
                false);
        }

        private void DrawTextToGraphics(
            Graphics g,
            string text,
            int scale,
            int startX,
            int startY,
            bool drawMissingCharacter)
        {
            int currentX = startX;
            int currentY = startY;

            int charWidth =
                FontWidth * scale;

            int charHeight =
                FontHeight * scale;

            int spacing =
                CharacterSpacing * scale;

            using Brush pixelBrush =
                new SolidBrush(textColor);

            foreach (char c in text)
            {
                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    currentX = startX;
                    currentY +=
                        charHeight +
                        (5 * scale);

                    continue;
                }

                if (c == ' ')
                {
                    currentX +=
                        charWidth + spacing;

                    continue;
                }

                byte[,] matrix =
                    PixelFont.GetCharacter(c);

                if (matrix == null)
                {
                    if (drawMissingCharacter)
                    {
                        DrawMissingCharacter(
                            g,
                            currentX,
                            currentY,
                            scale,
                            pixelBrush);
                    }
                }
                else
                {
                    DrawMatrix(
                        g,
                        matrix,
                        currentX,
                        currentY,
                        scale,
                        pixelBrush);
                }

                currentX +=
                    charWidth + spacing;
            }
        }

        private void DrawMatrix(
            Graphics g,
            byte[,] matrix,
            int x,
            int y,
            int scale,
            Brush brush)
        {
            for (int row = 0; row < FontHeight; row++)
            {
                for (int col = 0; col < FontWidth; col++)
                {
                    if (matrix[row, col] != 1)
                        continue;

                    g.FillRectangle(
                        brush,
                        x + col * scale,
                        y + row * scale,
                        scale,
                        scale);
                }
            }
        }

        private void DrawMissingCharacter(
            Graphics g,
            int x,
            int y,
            int scale,
            Brush brush)
        {
            int s = scale;

            g.FillRectangle(
                brush,
                x,
                y,
                s,
                s);

            g.FillRectangle(
                brush,
                x + (6 * s),
                y,
                s,
                s);

            g.FillRectangle(
                brush,
                x,
                y + (15 * s),
                s,
                s);

            g.FillRectangle(
                brush,
                x + (6 * s),
                y + (15 * s),
                s,
                s);
        }

        #endregion

        #region Save Selected Character

        private void button1_Click(
            object sender,
            EventArgs e)
        {
            string input = txtInput.Text;

            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show(
                    "Lütfen kaydetmek için en az bir karakter yazýn.",
                    "Uyarý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            char c = input[0];

            if (!PixelFont.pixelFont.ContainsKey(c))
            {
                MessageBox.Show(
                    $"'{c}' karakteri font verisinde bulunamadý.",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            string ext =
                GetSelectedExtension();

            if (!IsExportFormatSupported(ext))
                return;

            string safeFileName =
                GetSafeFileName(c);

            SaveFileDialog sfd =
                new SaveFileDialog
                {
                    FileName =
                        $"{safeFileName}.{ext}",

                    Filter =
                        $"{ext.ToUpperInvariant()} Dosyasý|*.{ext}",

                    AddExtension = true,
                    OverwritePrompt = true
                };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                int scale =
                    (int)numericUpDown1.Value;

                byte[,] matrix =
                    PixelFont.pixelFont[c];

                ExportCharacterToFile(
                    matrix,
                    ext,
                    scale,
                    sfd.FileName);

                MessageBox.Show(
                    $"Karakter baþarýyla kaydedildi:\n{sfd.FileName}",
                    "Baþarýlý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Dosya kaydedilirken hata oluþtu:\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Save All Font

        private void button2_Click(
            object sender,
            EventArgs e)
        {
            if (PixelFont.pixelFont.Count == 0)
            {
                MessageBox.Show(
                    "Kaydedilecek font verisi yok.",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            string ext =
                GetSelectedExtension();

            if (!IsExportFormatSupported(ext))
                return;

            SaveFileDialog sfd =
                new SaveFileDialog
                {
                    FileName =
                        $"PixelFont_All_{ext.ToUpperInvariant()}.zip",

                    Filter =
                        "ZIP Arþivi|*.zip",

                    AddExtension = true,
                    OverwritePrompt = true
                };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                int scale =
                    (int)numericUpDown1.Value;

                using FileStream zipStream =
                    new FileStream(
                        sfd.FileName,
                        FileMode.Create,
                        FileAccess.Write);

                using ZipArchive archive =
                    new ZipArchive(
                        zipStream,
                        ZipArchiveMode.Create);

                foreach (var kvp in PixelFont.pixelFont)
                {
                    char c = kvp.Key;

                    byte[,] matrix =
                        kvp.Value;

                    string fileName =
                        $"{GetSafeFileName(c)}.{ext}";

                    ZipArchiveEntry entry =
                        archive.CreateEntry(
                            fileName,
                            CompressionLevel.Optimal);

                    using Stream entryStream =
                        entry.Open();

                    SaveMatrixToStream(
                        matrix,
                        ext,
                        scale,
                        entryStream);
                }

                MessageBox.Show(
                    $"Tüm fontlar baþarýyla paketlendi:\n{sfd.FileName}",
                    "Baþarýlý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"ZIP oluþturulurken hata oluþtu:\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Export Canvas

        private void button3_Click(
            object sender,
            EventArgs e)
        {
            string text =
                txtInput.Text;

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show(
                    "Tuvalde kaydedilecek bir metin yok.",
                    "Uyarý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string ext =
                GetSelectedExtension();

            if (!IsExportFormatSupported(ext))
                return;

            SaveFileDialog sfd =
                new SaveFileDialog
                {
                    FileName =
                        $"Canvas_Render.{ext}",

                    Filter =
                        $"{ext.ToUpperInvariant()} Dosyasý|*.{ext}",

                    AddExtension = true,
                    OverwritePrompt = true
                };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                int scale =
                    (int)numericUpDown1.Value;

                if (ext == "svg")
                {
                    File.WriteAllText(
                        sfd.FileName,
                        GenerateCanvasSVG(
                            text,
                            scale),
                        new UTF8Encoding(false));
                }
                else if (ext == "eps")
                {
                    File.WriteAllText(
                        sfd.FileName,
                        GenerateCanvasEPS(
                            text,
                            scale),
                        Encoding.ASCII);
                }
                else if (ext == "xbm")
                {
                    File.WriteAllText(
                        sfd.FileName,
                        GenerateCanvasXBM(
                            text,
                            scale),
                        Encoding.ASCII);
                }
                else
                {
                    using Bitmap bmp =
                        RenderCanvasBitmap(
                            text,
                            scale);

                    SaveBitmapToFile(
                        bmp,
                        ext,
                        sfd.FileName);
                }

                MessageBox.Show(
                    $"Tuval görüntüsü baþarýyla kaydedildi:\n{sfd.FileName}",
                    "Baþarýlý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Dosya dýþa aktarýlýrken hata oluþtu:\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Export Validation

        private bool IsExportFormatSupported(
            string ext)
        {
            switch (ext)
            {
                case "png":
                case "jpg":
                case "jpeg":
                case "bmp":
                case "ico":
                case "gif":
                case "tiff":
                case "svg":
                case "eps":
                case "xbm":
                    return true;

                default:
                    MessageBox.Show(
                        $"'{ext}' formatý desteklenmiyor.",
                        "Desteklenmeyen Format",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return false;
            }
        }

        #endregion

        #region Character Rendering

        private void ExportCharacterToFile(
            byte[,] matrix,
            string ext,
            int scale,
            string filePath)
        {
            using FileStream fs =
                new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write);

            SaveMatrixToStream(
                matrix,
                ext,
                scale,
                fs);
        }

        private void SaveMatrixToStream(
            byte[,] matrix,
            string ext,
            int scale,
            Stream targetStream)
        {
            switch (ext)
            {
                case "svg":
                    WriteString(
                        targetStream,
                        GenerateSVG(
                            matrix,
                            scale),
                        Encoding.UTF8);
                    break;

                case "eps":
                    WriteString(
                        targetStream,
                        GenerateEPS(
                            matrix,
                            scale),
                        Encoding.ASCII);
                    break;

                case "xbm":
                    WriteString(
                        targetStream,
                        GenerateXBM(
                            matrix,
                            scale),
                        Encoding.ASCII);
                    break;

                default:
                    using (Bitmap bmp =
                        RenderCharacterBitmap(
                            matrix,
                            scale))
                    {
                        SaveBitmapToStream(
                            bmp,
                            ext,
                            targetStream);
                    }

                    break;
            }
        }

        private void WriteString(
            Stream stream,
            string text,
            Encoding encoding)
        {
            byte[] data =
                encoding.GetBytes(text);

            stream.Write(
                data,
                0,
                data.Length);
        }

        private Bitmap RenderCharacterBitmap(
            byte[,] matrix,
            int scale)
        {
            int width =
                FontWidth * scale;

            int height =
                FontHeight * scale;

            Bitmap bmp =
                new Bitmap(
                    width,
                    height,
                    PixelFormat.Format32bppArgb);

            using Graphics g =
                Graphics.FromImage(bmp);

            g.Clear(
                transparentBackground
                    ? Color.Transparent
                    : backgroundColor);

            using Brush brush =
                new SolidBrush(textColor);

            DrawMatrix(
                g,
                matrix,
                0,
                0,
                scale,
                brush);

            return bmp;
        }

        #endregion

        #region Canvas Bitmap

        private Bitmap RenderCanvasBitmap(
            string text,
            int scale)
        {
            Size canvasSize =
                CalculateCanvasSize(text);

            Bitmap bmp =
                new Bitmap(
                    canvasSize.Width,
                    canvasSize.Height,
                    PixelFormat.Format32bppArgb);

            using Graphics g =
                Graphics.FromImage(bmp);

            g.Clear(
                transparentBackground
                    ? Color.Transparent
                    : backgroundColor);

            DrawTextToGraphics(
                g,
                text,
                scale,
                CanvasPadding,
                CanvasPadding,
                true);

            return bmp;
        }

        #endregion

        #region Bitmap Saving

        private void SaveBitmapToFile(
            Bitmap bmp,
            string ext,
            string filePath)
        {
            using FileStream fs =
                new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write);

            SaveBitmapToStream(
                bmp,
                ext,
                fs);
        }

        private void SaveBitmapToStream(
            Bitmap bmp,
            string ext,
            Stream targetStream)
        {
            switch (ext)
            {
                case "ico":
                    SaveIco(
                        bmp,
                        targetStream);
                    return;

                case "bmp":
                    SaveBmpWithSelectedDepth(
                        bmp,
                        targetStream);
                    return;

                case "gif":
                    SaveGif(
                        bmp,
                        targetStream);
                    return;
            }

            ImageFormat format =
                GetImageFormat(ext);

            using Bitmap output =
                PrepareBitmapForFormat(
                    bmp,
                    ext);

            output.Save(
                targetStream,
                format);
        }

        private Bitmap PrepareBitmapForFormat(
            Bitmap source,
            string ext)
        {
            if (ext == "jpg" ||
                ext == "jpeg")
            {
                Bitmap result =
                    new Bitmap(
                        source.Width,
                        source.Height,
                        PixelFormat.Format24bppRgb);

                using Graphics g =
                    Graphics.FromImage(result);

                g.Clear(backgroundColor);

                using Bitmap flattened =
                    FlattenTransparentBitmap(
                        source,
                        backgroundColor);

                g.DrawImageUnscaled(
                    flattened,
                    0,
                    0);

                return result;
            }

            return new Bitmap(
                source);
        }

        private Bitmap FlattenTransparentBitmap(
            Bitmap source,
            Color background)
        {
            Bitmap result =
                new Bitmap(
                    source.Width,
                    source.Height,
                    PixelFormat.Format32bppArgb);

            using Graphics g =
                Graphics.FromImage(result);

            g.Clear(background);

            g.DrawImageUnscaled(
                source,
                0,
                0);

            return result;
        }

        private ImageFormat GetImageFormat(
            string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                "jpg" => ImageFormat.Jpeg,
                "jpeg" => ImageFormat.Jpeg,
                "bmp" => ImageFormat.Bmp,
                "gif" => ImageFormat.Gif,
                "tiff" => ImageFormat.Tiff,
                "ico" => ImageFormat.Icon,
                _ => ImageFormat.Png
            };
        }

        #endregion

        #region BMP

        private void SaveBmpWithSelectedDepth(
            Bitmap source,
            Stream targetStream)
        {
            int depth =
                GetSelectedBmpDepth();

            using Bitmap bmp =
                ConvertBitmapToBmpDepth(
                    source,
                    depth);

            bmp.Save(
                targetStream,
                ImageFormat.Bmp);
        }

        private Bitmap ConvertBitmapToBmpDepth(
            Bitmap source,
            int depth)
        {
            PixelFormat targetFormat =
                depth switch
                {
                    1 =>
                        PixelFormat.Format1bppIndexed,

                    4 =>
                        PixelFormat.Format4bppIndexed,

                    8 =>
                        PixelFormat.Format8bppIndexed,

                    24 =>
                        PixelFormat.Format24bppRgb,

                    32 =>
                        PixelFormat.Format32bppArgb,

                    _ =>
                        PixelFormat.Format24bppRgb
                };

            Bitmap result =
                new Bitmap(
                    source.Width,
                    source.Height,
                    targetFormat);

            if (depth <= 8)
            {
                SetIndexedPalette(
                    result,
                    depth);
            }

            CopyPixelsToBitmap(
                source,
                result,
                depth);

            return result;
        }

        private void SetIndexedPalette(
            Bitmap bmp,
            int depth)
        {
            ColorPalette palette =
                bmp.Palette;

            int count =
                depth == 1
                    ? 2
                    : depth == 4
                        ? 16
                        : 256;

            palette.Entries[0] =
                backgroundColor;

            palette.Entries[1] =
                textColor;

            // Palette'in kalan bölümlerini doldur.
            // Font iki renkli olduðu için gerçek pikseller
            // yalnýzca 0 ve 1 indekslerini kullanýr.
            for (int i = 2; i < count; i++)
            {
                int value =
                    (int)(
                        255.0 *
                        (i - 2) /
                        Math.Max(
                            1,
                            count - 3));

                palette.Entries[i] =
                    Color.FromArgb(
                        255,
                        value,
                        value,
                        value);
            }

            bmp.Palette =
                palette;
        }

        private unsafe void CopyPixelsToBitmap(
            Bitmap source,
            Bitmap destination,
            int depth)
        {
            Rectangle rect =
                new Rectangle(
                    0,
                    0,
                    source.Width,
                    source.Height);

            BitmapData sourceData =
                source.LockBits(
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

            BitmapData destinationData =
                destination.LockBits(
                    rect,
                    ImageLockMode.ReadWrite,
                    destination.PixelFormat);

            try
            {
                for (int y = 0;
                     y < source.Height;
                     y++)
                {
                    byte* sourceRow =
                        (byte*)sourceData.Scan0 +
                        y * sourceData.Stride;

                    byte* destinationRow =
                        (byte*)destinationData.Scan0 +
                        y * destinationData.Stride;

                    for (int x = 0;
                         x < source.Width;
                         x++)
                    {
                        byte* src =
                            sourceRow +
                            x * 4;

                        Color color =
                            Color.FromArgb(
                                src[3],
                                src[2],
                                src[1],
                                src[0]);

                        switch (depth)
                        {
                            case 32:
                                {
                                    byte* dst =
                                        destinationRow +
                                        x * 4;

                                    dst[0] = src[0];
                                    dst[1] = src[1];
                                    dst[2] = src[2];
                                    dst[3] = src[3];

                                    break;
                                }

                            case 24:
                                {
                                    byte* dst =
                                        destinationRow +
                                        x * 3;

                                    dst[0] = src[0];
                                    dst[1] = src[1];
                                    dst[2] = src[2];

                                    break;
                                }

                            case 8:
                                {
                                    byte* dst =
                                        destinationRow + x;

                                    dst[0] =
                                        GetPaletteIndex(
                                            color,
                                            8);

                                    break;
                                }

                            case 4:
                                {
                                    byte* dst =
                                        destinationRow +
                                        (x / 2);

                                    byte index =
                                        GetPaletteIndex(
                                            color,
                                            4);

                                    if ((x & 1) == 0)
                                    {
                                        dst[0] =
                                            (byte)(
                                                (index << 4) |
                                                (dst[0] & 0x0F));
                                    }
                                    else
                                    {
                                        dst[0] =
                                            (byte)(
                                                (dst[0] & 0xF0) |
                                                (index & 0x0F));
                                    }

                                    break;
                                }

                            case 1:
                                {
                                    byte* dst =
                                        destinationRow +
                                        (x / 8);

                                    byte index =
                                        GetPaletteIndex(
                                            color,
                                            1);

                                    int bit =
                                        7 - (x % 8);

                                    if (index == 1)
                                    {
                                        dst[0] |=
                                            (byte)(1 << bit);
                                    }
                                    else
                                    {
                                        dst[0] &=
                                            (byte)~(1 << bit);
                                    }

                                    break;
                                }
                        }
                    }
                }
            }
            finally
            {
                source.UnlockBits(sourceData);
                destination.UnlockBits(destinationData);
            }
        }

        private byte GetPaletteIndex(
            Color color,
            int depth)
        {
            Color bg =
                backgroundColor;

            Color fg =
                textColor;

            double fgDistance =
                ColorDistance(
                    color,
                    fg);

            double bgDistance =
                ColorDistance(
                    color,
                    bg);

            return fgDistance < bgDistance
                ? (byte)1
                : (byte)0;
        }

        private double ColorDistance(
            Color a,
            Color b)
        {
            double r = a.R - b.R;
            double g = a.G - b.G;
            double bl = a.B - b.B;
            double alpha = a.A - b.A;

            return
                r * r +
                g * g +
                bl * bl +
                alpha * alpha;
        }

        #endregion

        #region GIF

        private void SaveGif(
            Bitmap source,
            Stream targetStream)
        {
            if (!transparentBackground)
            {
                using Bitmap bmp =
                    new Bitmap(
                        source.Width,
                        source.Height,
                        PixelFormat.Format24bppRgb);

                using Graphics g =
                    Graphics.FromImage(bmp);

                g.Clear(backgroundColor);
                g.DrawImageUnscaled(source, 0, 0);

                bmp.Save(
                    targetStream,
                    ImageFormat.Gif);

                return;
            }

            using Bitmap indexed =
                CreateTransparentGifBitmap(source);

            indexed.Save(
                targetStream,
                ImageFormat.Gif);
        }

        private unsafe Bitmap CreateTransparentGifBitmap(
            Bitmap source)
        {
            Bitmap result =
                new Bitmap(
                    source.Width,
                    source.Height,
                    PixelFormat.Format8bppIndexed);

            ColorPalette palette =
                result.Palette;

            palette.Entries[0] =
                Color.FromArgb(
                    0,
                    255,
                    255,
                    255);

            palette.Entries[1] =
                Color.FromArgb(
                    255,
                    textColor.R,
                    textColor.G,
                    textColor.B);

            for (int i = 2;
                 i < palette.Entries.Length;
                 i++)
            {
                palette.Entries[i] =
                    Color.FromArgb(
                        255,
                        i,
                        i,
                        i);
            }

            result.Palette =
                palette;

            Rectangle rect =
                new Rectangle(
                    0,
                    0,
                    result.Width,
                    result.Height);

            BitmapData sourceData =
                source.LockBits(
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

            BitmapData destinationData =
                result.LockBits(
                    rect,
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format8bppIndexed);

            try
            {
                for (int y = 0;
                     y < result.Height;
                     y++)
                {
                    byte* src = (byte*)sourceData.Scan0 + y * sourceData.Stride;

                    byte* dst = (byte*)destinationData.Scan0 + y * destinationData.Stride;

                    for (int x = 0;
                         x < result.Width;
                         x++)
                    {
                        byte alpha =
                            src[x * 4 + 3];

                        dst[x] =
                            alpha == 0
                                ? (byte)0
                                : (byte)1;
                    }
                }
            }
            finally
            {
                source.UnlockBits(sourceData);
                result.UnlockBits(destinationData);
            }

            return result;
        }

        #endregion

        #region ICO

        private void SaveIco(
            Bitmap source,
            Stream targetStream)
        {
            int maxSize =
                Math.Min(
                    Math.Min(
                        source.Width,
                        source.Height),
                    256);

            int size =
                Math.Max(
                    1,
                    Math.Min(
                        256,
                        maxSize));

            using Bitmap iconBitmap =
                new Bitmap(
                    size,
                    size,
                    PixelFormat.Format32bppArgb);

            using Graphics g =
                Graphics.FromImage(iconBitmap);

            g.Clear(Color.Transparent);

            g.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            g.PixelOffsetMode =
                System.Drawing.Drawing2D.PixelOffsetMode.Half;

            g.DrawImage(
                source,
                new Rectangle(
                    0,
                    0,
                    size,
                    size));

            IntPtr hIcon =
                iconBitmap.GetHicon();

            try
            {
                using Icon icon =
                    Icon.FromHandle(hIcon);

                icon.Save(targetStream);
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }

        [DllImport(
            "user32.dll",
            CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(
            IntPtr handle);

        #endregion

        #region SVG Character

        private string GenerateSVG(
            byte[,] matrix,
            int scale)
        {
            int width =
                FontWidth * scale;

            int height =
                FontHeight * scale;

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" " +
                $"width=\"{width}\" " +
                $"height=\"{height}\" " +
                $"viewBox=\"0 0 {width} {height}\">");

            if (!transparentBackground)
            {
                sb.AppendLine(
                    $"  <rect width=\"100%\" " +
                    $"height=\"100%\" " +
                    $"fill=\"{ColorToHex(backgroundColor)}\" />");
            }

            for (int row = 0;
                 row < FontHeight;
                 row++)
            {
                for (int col = 0;
                     col < FontWidth;
                     col++)
                {
                    if (matrix[row, col] != 1)
                        continue;

                    int x = col * scale;
                    int y = row * scale;

                    sb.AppendLine(
                        $"  <rect x=\"{x}\" y=\"{y}\" " +
                        $"width=\"{scale}\" " +
                        $"height=\"{scale}\" " +
                        $"fill=\"{ColorToHex(textColor)}\" />");
                }
            }

            sb.AppendLine("</svg>");

            return sb.ToString();
        }

        #endregion

        #region SVG Canvas

        private string GenerateCanvasSVG(
            string text,
            int scale)
        {
            Size size =
                CalculateCanvasSize(text);

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" " +
                $"width=\"{size.Width}\" " +
                $"height=\"{size.Height}\" " +
                $"viewBox=\"0 0 {size.Width} {size.Height}\">");

            if (!transparentBackground)
            {
                sb.AppendLine(
                    $"  <rect width=\"100%\" " +
                    $"height=\"100%\" " +
                    $"fill=\"{ColorToHex(backgroundColor)}\" />");
            }

            int currentX =
                CanvasPadding;

            int currentY =
                CanvasPadding;

            int charWidth =
                FontWidth * scale;

            int charHeight =
                FontHeight * scale;

            int spacing =
                CharacterSpacing * scale;

            string textHex =
                ColorToHex(textColor);

            foreach (char c in text)
            {
                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    currentX =
                        CanvasPadding;

                    currentY +=
                        charHeight +
                        5 * scale;

                    continue;
                }

                if (c == ' ')
                {
                    currentX +=
                        charWidth + spacing;

                    continue;
                }

                byte[,] matrix =
                    PixelFont.GetCharacter(c);

                if (matrix != null)
                {
                    for (int row = 0;
                         row < FontHeight;
                         row++)
                    {
                        for (int col = 0;
                             col < FontWidth;
                             col++)
                        {
                            if (matrix[row, col] != 1)
                                continue;

                            int x =
                                currentX +
                                col * scale;

                            int y =
                                currentY +
                                row * scale;

                            sb.AppendLine(
                                $"  <rect x=\"{x}\" y=\"{y}\" " +
                                $"width=\"{scale}\" " +
                                $"height=\"{scale}\" " +
                                $"fill=\"{textHex}\" />");
                        }
                    }
                }

                currentX +=
                    charWidth + spacing;
            }

            sb.AppendLine("</svg>");

            return sb.ToString();
        }

        #endregion

        #region EPS

        private string GenerateEPS(
            byte[,] matrix,
            int scale)
        {
            int width =
                FontWidth * scale;

            int height =
                FontHeight * scale;

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                "%!PS-Adobe-3.0 EPSF-3.0");

            sb.AppendLine(
                $"%%BoundingBox: 0 0 {width} {height}");

            sb.AppendLine(
                "%%LanguageLevel: 2");

            sb.AppendLine(
                "%%EndComments");

            if (!transparentBackground)
            {
                AppendEPSColor(
                    sb,
                    backgroundColor);

                sb.AppendLine(
                    $"0 0 {width} {height} rectfill");
            }

            AppendEPSColor(
                sb,
                textColor);

            for (int row = 0;
                 row < FontHeight;
                 row++)
            {
                for (int col = 0;
                     col < FontWidth;
                     col++)
                {
                    if (matrix[row, col] != 1)
                        continue;

                    int x =
                        col * scale;

                    int y =
                        height -
                        ((row + 1) * scale);

                    sb.AppendLine(
                        $"{x} {y} {scale} {scale} rectfill");
                }
            }

            sb.AppendLine("showpage");
            sb.AppendLine("%%EOF");

            return sb.ToString();
        }

        private void AppendEPSColor(
            StringBuilder sb,
            Color color)
        {
            sb.AppendLine(
                $"{color.R / 255.0:0.######} " +
                $"{color.G / 255.0:0.######} " +
                $"{color.B / 255.0:0.######} " +
                "setrgbcolor");
        }

        #endregion

        #region EPS Canvas

        private string GenerateCanvasEPS(
            string text,
            int scale)
        {
            Size size =
                CalculateCanvasSize(text);

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                "%!PS-Adobe-3.0 EPSF-3.0");

            sb.AppendLine(
                $"%%BoundingBox: 0 0 " +
                $"{size.Width} {size.Height}");

            sb.AppendLine(
                "%%LanguageLevel: 2");

            sb.AppendLine(
                "%%EndComments");

            if (!transparentBackground)
            {
                AppendEPSColor(
                    sb,
                    backgroundColor);

                sb.AppendLine(
                    $"0 0 {size.Width} {size.Height} rectfill");
            }

            AppendEPSColor(
                sb,
                textColor);

            int currentX =
                CanvasPadding;

            int currentY =
                CanvasPadding;

            int charWidth =
                FontWidth * scale;

            int charHeight =
                FontHeight * scale;

            int spacing =
                CharacterSpacing * scale;

            foreach (char c in text)
            {
                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    currentX =
                        CanvasPadding;

                    currentY +=
                        charHeight +
                        5 * scale;

                    continue;
                }

                if (c == ' ')
                {
                    currentX +=
                        charWidth + spacing;

                    continue;
                }

                byte[,] matrix =
                    PixelFont.GetCharacter(c);

                if (matrix != null)
                {
                    for (int row = 0;
                         row < FontHeight;
                         row++)
                    {
                        for (int col = 0;
                             col < FontWidth;
                             col++)
                        {
                            if (matrix[row, col] != 1)
                                continue;

                            int x =
                                currentX +
                                col * scale;

                            int y =
                                size.Height -
                                currentY -
                                ((row + 1) * scale);

                            sb.AppendLine(
                                $"{x} {y} " +
                                $"{scale} {scale} rectfill");
                        }
                    }
                }

                currentX +=
                    charWidth + spacing;
            }

            sb.AppendLine("showpage");
            sb.AppendLine("%%EOF");

            return sb.ToString();
        }

        #endregion

        #region XBM

        private string GenerateXBM(
            byte[,] matrix,
            int scale)
        {
            int width =
                FontWidth * scale;

            int height =
                FontHeight * scale;

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                $"#define char_width {width}");

            sb.AppendLine(
                $"#define char_height {height}");

            sb.AppendLine(
                "static unsigned char char_bits[] = {");

            List<string> bytes =
                new List<string>();

            for (int y = 0;
                 y < height;
                 y++)
            {
                int originalRow =
                    y / scale;

                for (int xGroup = 0;
                     xGroup < width;
                     xGroup += 8)
                {
                    byte value = 0;

                    for (int bit = 0;
                         bit < 8;
                         bit++)
                    {
                        int x =
                            xGroup + bit;

                        if (x >= width)
                            continue;

                        int originalCol =
                            x / scale;

                        if (matrix[
                                originalRow,
                                originalCol] == 1)
                        {
                            value |=
                                (byte)(1 << bit);
                        }
                    }

                    bytes.Add(
                        $"0x{value:X2}");
                }
            }

            sb.AppendLine(
                "  " +
                string.Join(
                    ", ",
                    bytes));

            sb.AppendLine("};");

            return sb.ToString();
        }

        private string GenerateCanvasXBM(
            string text,
            int scale)
        {
            Size size =
                CalculateCanvasSize(text);

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                $"#define canvas_width {size.Width}");

            sb.AppendLine(
                $"#define canvas_height {size.Height}");

            sb.AppendLine(
                "static unsigned char canvas_bits[] = {");

            List<string> bytes =
                new List<string>();

            bool[,] pixels =
                BuildCanvasPixelMask(
                    text,
                    scale,
                    size);

            for (int y = 0;
                 y < size.Height;
                 y++)
            {
                for (int xGroup = 0;
                     xGroup < size.Width;
                     xGroup += 8)
                {
                    byte value = 0;

                    for (int bit = 0;
                         bit < 8;
                         bit++)
                    {
                        int x =
                            xGroup + bit;

                        if (x >= size.Width)
                            continue;

                        if (pixels[y, x])
                        {
                            value |=
                                (byte)(1 << bit);
                        }
                    }

                    bytes.Add(
                        $"0x{value:X2}");
                }
            }

            sb.AppendLine(
                "  " +
                string.Join(
                    ", ",
                    bytes));

            sb.AppendLine("};");

            return sb.ToString();
        }

        private bool[,] BuildCanvasPixelMask(
            string text,
            int scale,
            Size size)
        {
            bool[,] pixels =
                new bool[
                    size.Height,
                    size.Width];

            int currentX =
                CanvasPadding;

            int currentY =
                CanvasPadding;

            int charWidth =
                FontWidth * scale;

            int charHeight =
                FontHeight * scale;

            int spacing =
                CharacterSpacing * scale;

            foreach (char c in text)
            {
                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    currentX =
                        CanvasPadding;

                    currentY +=
                        charHeight +
                        5 * scale;

                    continue;
                }

                if (c == ' ')
                {
                    currentX +=
                        charWidth + spacing;

                    continue;
                }

                byte[,] matrix =
                    PixelFont.GetCharacter(c);

                if (matrix != null)
                {
                    for (int row = 0;
                         row < FontHeight;
                         row++)
                    {
                        for (int col = 0;
                             col < FontWidth;
                             col++)
                        {
                            if (matrix[row, col] != 1)
                                continue;

                            for (int py = 0;
                                 py < scale;
                                 py++)
                            {
                                for (int px = 0;
                                     px < scale;
                                     px++)
                                {
                                    int x =
                                        currentX +
                                        col * scale +
                                        px;

                                    int y =
                                        currentY +
                                        row * scale +
                                        py;

                                    if (x >= 0 &&
                                        x < size.Width &&
                                        y >= 0 &&
                                        y < size.Height)
                                    {
                                        pixels[y, x] =
                                            true;
                                    }
                                }
                            }
                        }
                    }
                }

                currentX +=
                    charWidth + spacing;
            }

            return pixels;
        }

        #endregion

        #region Helpers

        private string ColorToHex(Color color)
        {
            return
                $"#{color.R:X2}" +
                $"{color.G:X2}" +
                $"{color.B:X2}";
        }

        private string GetSafeFileName(char c)
        {
            return c switch
            {
                '/' => "slash",
                '\\' => "backslash",
                ':' => "colon",
                '*' => "asterisk",
                '?' => "question",
                '"' => "quote",
                '<' => "less",
                '>' => "greater",
                '|' => "pipe",
                ' ' => "space",
                '.' => "dot",

                _ => char.IsLetterOrDigit(c)
                    ? c.ToString()
                    : $"symbol_{(int)c}"
            };
        }

        #endregion
    }
}