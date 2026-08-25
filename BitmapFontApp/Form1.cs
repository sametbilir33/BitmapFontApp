using System.Drawing.Imaging;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace BitmapFontApp
{
    public partial class Form1 : Form
    {
        private int pixelSize = 2;
        private Color textColor = Color.Black;
        private Color backgroundColor = Color.White;
        private bool transparentBackground = false;

        public Form1()
        {
            InitializeComponent();
            SetupEvents();
            UpdateColorButtonStyles();
        }

        private void SetupEvents()
        {
            txtInput.TextChanged += (s, e) => panelCanvas.Invalidate();

            if (comboBox1.Items.Count == 0)
            {
                comboBox1.Items.AddRange(new string[]
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
            "XBM",
            "WEBP"
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
                cmbBmpDepth.Items.AddRange(new object[]
                {
            "1-bit (Siyah/Beyaz)",
            "4-bit (16 Renk)",
            "8-bit (256 Renk)",
            "24-bit (True Color)",
            "32-bit (Alpha)"
                });
            }

            cmbBmpDepth.SelectedIndex = 3;

            UpdateBmpDepthVisibility();
        }

        private void UpdateBmpDepthVisibility()
        {
            string ext =
                comboBox1.SelectedItem?
                .ToString()?
                .ToLowerInvariant() ?? "";

            bool isBmp = ext == "bmp";

            lblBmpDepth.Visible = isBmp;
            cmbBmpDepth.Visible = isBmp;
        }

        private void UpdateColorButtonStyles()
        {
            btnTextColor.BackColor = textColor;
            btnTextColor.ForeColor = GetContrastingColor(textColor);

            btnBgColor.BackColor = backgroundColor;
            btnBgColor.ForeColor = GetContrastingColor(backgroundColor);

            panelCanvas.BackColor = transparentBackground
                ? Color.White
                : backgroundColor;

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

        private void BtnTextColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = textColor;

                if (cd.ShowDialog() == DialogResult.OK)
                {
                    textColor = cd.Color;
                    UpdateColorButtonStyles();
                }
            }
        }

        private void BtnBgColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = backgroundColor;

                if (cd.ShowDialog() == DialogResult.OK)
                {
                    backgroundColor = cd.Color;
                    UpdateColorButtonStyles();
                }
            }
        }

        private void ChkTransparent_CheckedChanged(object sender, EventArgs e)
        {
            transparentBackground = chkTransparent.Checked;
            UpdateColorButtonStyles();
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateBmpDepthVisibility();

            panelCanvas.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string input = txtInput.Text;

            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show(
                    "Lütfen kaydetmek için kutuya en az bir karakter yazýn!",
                    "Uyarý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            char c = input[0];

            if (!PixelFont.pixelFont.ContainsKey(c))
            {
                MessageBox.Show(
                    $"'{c}' karakteri font verisinde bulunamadý!",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            string ext = comboBox1.SelectedItem?.ToString()?.ToLowerInvariant() ?? "png";

            if (ext == "webp")
            {
                MessageBox.Show(
                    "System.Drawing doðrudan WebP çýktýsý oluþturamaz. Lütfen PNG, BMP, GIF, TIFF veya baþka bir desteklenen format seçin.",
                    "Desteklenmeyen Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string safeFileName = GetSafeFileName(c);

            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = $"{safeFileName}.{ext}",
                Filter = $"{ext.ToUpperInvariant()} Dosyasý|*.{ext}"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            int scale = (int)numericUpDown1.Value;
            byte[,] matrix = PixelFont.pixelFont[c];

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

        private void button2_Click(object sender, EventArgs e)
        {
            if (PixelFont.pixelFont.Count == 0)
            {
                MessageBox.Show(
                    "Kaydedilecek font verisi yok!",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            string ext = comboBox1.SelectedItem?.ToString()?.ToLowerInvariant() ?? "png";

            if (ext == "webp")
            {
                MessageBox.Show(
                    "System.Drawing doðrudan WebP çýktýsý oluþturamaz. Lütfen baþka bir format seçin.",
                    "Desteklenmeyen Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = $"PixelFont_All_{ext.ToUpperInvariant()}.zip",
                Filter = "ZIP Arþivi|*.zip"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            int scale = (int)numericUpDown1.Value;

            using (FileStream zipToOpen = new FileStream(
                sfd.FileName,
                FileMode.Create,
                FileAccess.Write))
            {
                using (ZipArchive archive = new ZipArchive(
                    zipToOpen,
                    ZipArchiveMode.Create))
                {
                    foreach (KeyValuePair<char, byte[,]> kvp in PixelFont.pixelFont)
                    {
                        char c = kvp.Key;
                        byte[,] matrix = kvp.Value;

                        string fileNameInZip =
                            $"{GetSafeFileName(c)}.{ext}";

                        ZipArchiveEntry entry =
                            archive.CreateEntry(fileNameInZip);

                        using (Stream entryStream = entry.Open())
                        {
                            SaveMatrixToStream(
                                matrix,
                                ext,
                                scale,
                                entryStream);
                        }
                    }
                }
            }

            MessageBox.Show(
                $"Tüm fontlar baþarýyla paketlendi:\n{sfd.FileName}",
                "Baþarýlý",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string textToDraw = txtInput.Text;

            if (string.IsNullOrEmpty(textToDraw))
            {
                MessageBox.Show(
                    "Tuvalde kaydedilecek bir metin yok!",
                    "Uyarý",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string ext = comboBox1.SelectedItem?.ToString()?.ToLowerInvariant() ?? "png";

            if (ext == "webp")
            {
                MessageBox.Show(
                    "System.Drawing doðrudan WebP çýktýsý oluþturamaz. Lütfen baþka bir format seçin.",
                    "Desteklenmeyen Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = $"Canvas_Render.{ext}",
                Filter = $"{ext.ToUpperInvariant()} Dosyasý|*.{ext}"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            int scale = (int)numericUpDown1.Value;

            if (ext == "svg")
            {
                string svgContent =
                    GenerateCanvasSVG(textToDraw, scale);

                File.WriteAllText(
                    sfd.FileName,
                    svgContent,
                    new UTF8Encoding(false));
            }
            else if (ext == "eps")
            {
                string epsContent =
                    GenerateCanvasEPS(textToDraw, scale);

                File.WriteAllText(
                    sfd.FileName,
                    epsContent,
                    Encoding.ASCII);
            }
            else if (ext == "xbm")
            {
                string xbmContent =
                    GenerateCanvasXBM(textToDraw, scale);

                File.WriteAllText(
                    sfd.FileName,
                    xbmContent,
                    Encoding.ASCII);
            }
            else
            {
                using (Bitmap bmp = RenderCanvasBitmap(textToDraw, scale))
                {
                    SaveBitmapToFile(
                        bmp,
                        ext,
                        sfd.FileName);
                }
            }

            MessageBox.Show(
                $"Tuval görüntüsü baþarýyla kaydedildi:\n{sfd.FileName}",
                "Baþarýlý",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ExportCharacterToFile(
            byte[,] matrix,
            string ext,
            int scale,
            string filePath)
        {
            using (FileStream fs = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write))
            {
                SaveMatrixToStream(
                    matrix,
                    ext,
                    scale,
                    fs);
            }
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
                    {
                        string svgContent =
                            GenerateSVG(matrix, scale);

                        byte[] bytes =
                            Encoding.UTF8.GetBytes(svgContent);

                        targetStream.Write(
                            bytes,
                            0,
                            bytes.Length);

                        break;
                    }

                case "eps":
                    {
                        string epsContent =
                            GenerateEPS(matrix, scale);

                        byte[] bytes =
                            Encoding.ASCII.GetBytes(epsContent);

                        targetStream.Write(
                            bytes,
                            0,
                            bytes.Length);

                        break;
                    }

                case "xbm":
                    {
                        string xbmContent =
                            GenerateXBM(matrix, scale);

                        byte[] bytes =
                            Encoding.ASCII.GetBytes(xbmContent);

                        targetStream.Write(
                            bytes,
                            0,
                            bytes.Length);

                        break;
                    }

                default:
                    {
                        using (Bitmap bmp =
                            RenderCharacterBitmap(matrix, scale))
                        {
                            SaveBitmapToStream(
                                bmp,
                                ext,
                                targetStream);
                        }

                        break;
                    }
            }
        }

        private void SaveBitmapToFile(
            Bitmap bmp,
            string ext,
            string filePath)
        {
            using (FileStream fs = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write))
            {
                SaveBitmapToStream(
                    bmp,
                    ext,
                    fs);
            }
        }

        private void SaveBitmapToStream(
            Bitmap bmp,
            string ext,
            Stream targetStream)
        {
            if (ext == "ico")
            {
                IntPtr hIcon = bmp.GetHicon();

                try
                {
                    using (Icon icon = Icon.FromHandle(hIcon))
                    {
                        icon.Save(targetStream);
                    }
                }
                finally
                {
                    DestroyIcon(hIcon);
                }

                return;
            }

            if (ext == "bmp")
            {
                SaveBmpWithSelectedDepth(
                    bmp,
                    targetStream);

                return;
            }

            ImageFormat imgFormat =
                GetImageFormat(ext);

            bmp.Save(
                targetStream,
                imgFormat);
        }

        private void SaveBmpWithSelectedDepth(
    Bitmap source,
    Stream targetStream)
        {
            int depth = GetSelectedBmpDepth();

            using (Bitmap bmp =
                ConvertBitmapToBmpDepth(source, depth))
            {
                bmp.Save(
                    targetStream,
                    ImageFormat.Bmp);
            }
        }

        private int GetSelectedBmpDepth()
        {
            switch (cmbBmpDepth.SelectedIndex)
            {
                case 0:
                    return 1;

                case 1:
                    return 4;

                case 2:
                    return 8;

                case 3:
                    return 24;

                case 4:
                    return 32;

                default:
                    return 24;
            }
        }

        private Bitmap ConvertBitmapToBmpDepth(
    Bitmap source,
    int depth)
        {
            PixelFormat targetPixelFormat;

            switch (depth)
            {
                case 1:
                    targetPixelFormat =
                        PixelFormat.Format1bppIndexed;
                    break;

                case 4:
                    targetPixelFormat =
                        PixelFormat.Format4bppIndexed;
                    break;

                case 8:
                    targetPixelFormat =
                        PixelFormat.Format8bppIndexed;
                    break;

                case 24:
                    targetPixelFormat =
                        PixelFormat.Format24bppRgb;
                    break;

                case 32:
                    targetPixelFormat =
                        PixelFormat.Format32bppArgb;
                    break;

                default:
                    targetPixelFormat =
                        PixelFormat.Format24bppRgb;
                    break;
            }

            Bitmap result =
                new Bitmap(
                    source.Width,
                    source.Height,
                    targetPixelFormat);

            if (depth == 1 ||
                depth == 4 ||
                depth == 8)
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

            int colorCount =
                depth == 1 ? 2 :
                depth == 4 ? 16 :
                256;

            Color bg =
                transparentBackground
                    ? Color.White
                    : backgroundColor;

            Color fg =
                textColor;

            // Ýlk iki palette rengi gerçek renkler
            palette.Entries[0] = bg;
            palette.Entries[1] = fg;

            // Geri kalan renkleri doldur.
            for (int i = 2; i < colorCount; i++)
            {
                int value =
                    (int)(
                        255.0 *
                        (i - 2) /
                        Math.Max(
                            1,
                            colorCount - 3));

                palette.Entries[i] =
                    Color.FromArgb(
                        255,
                        value,
                        value,
                        value);
            }

            bmp.Palette = palette;
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
                    ImageLockMode.WriteOnly,
                    destination.PixelFormat);

            try
            {
                int sourceStride =
                    sourceData.Stride;

                int destinationStride =
                    destinationData.Stride;

                int sourceBytesPerPixel = 4;

                for (int y = 0; y < source.Height; y++)
                {
                    IntPtr sourceRow =
                        IntPtr.Add(
                            sourceData.Scan0,
                            y * sourceStride);

                    IntPtr destinationRow =
                        IntPtr.Add(
                            destinationData.Scan0,
                            y * destinationStride);

                    for (int x = 0; x < source.Width; x++)
                    {
                        byte* sourcePixel =
                            (byte*)sourceRow +
                            (x * sourceBytesPerPixel);

                        byte blue =
                            sourcePixel[0];

                        byte green =
                            sourcePixel[1];

                        byte red =
                            sourcePixel[2];

                        byte alpha =
                            sourcePixel[3];

                        Color color =
                            Color.FromArgb(
                                alpha,
                                red,
                                green,
                                blue);

                        if (depth == 32)
                        {
                            byte* destinationPixel =
                                (byte*)destinationRow +
                                (x * 4);

                            destinationPixel[0] =
                                blue;

                            destinationPixel[1] =
                                green;

                            destinationPixel[2] =
                                red;

                            destinationPixel[3] =
                                alpha;
                        }
                        else if (depth == 24)
                        {
                            byte* destinationPixel =
                                (byte*)destinationRow +
                                (x * 3);

                            destinationPixel[0] =
                                blue;

                            destinationPixel[1] =
                                green;

                            destinationPixel[2] =
                                red;
                        }
                        else if (depth == 8)
                        {
                            byte* destinationPixel =
                                (byte*)destinationRow +
                                x;

                            destinationPixel[0] =
                                GetPaletteIndex(
                                    color,
                                    8);
                        }
                        else if (depth == 4)
                        {
                            byte* destinationPixel =
                                (byte*)destinationRow +
                                (x / 2);

                            byte index =
                                GetPaletteIndex(
                                    color,
                                    4);

                            if ((x & 1) == 0)
                            {
                                destinationPixel[0] =
                                    (byte)(
                                        (index << 4) |
                                        (destinationPixel[0] & 0x0F));
                            }
                            else
                            {
                                destinationPixel[0] =
                                    (byte)(
                                        (destinationPixel[0] & 0xF0) |
                                        (index & 0x0F));
                            }
                        }
                        else if (depth == 1)
                        {
                            byte* destinationPixel =
                                (byte*)destinationRow +
                                (x / 8);

                            byte index =
                                GetPaletteIndex(
                                    color,
                                    1);

                            int bit =
                                7 - (x % 8);

                            if (index == 1)
                            {
                                destinationPixel[0] |=
                                    (byte)(1 << bit);
                            }
                            else
                            {
                                destinationPixel[0] &=
                                    (byte)~(1 << bit);
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
            Color background =
                transparentBackground
                    ? Color.White
                    : backgroundColor;

            Color text =
                textColor;

            if (depth == 1)
            {
                double textDistance =
                    ColorDistance(
                        color,
                        text);

                double backgroundDistance =
                    ColorDistance(
                        color,
                        background);

                return
                    textDistance <
                    backgroundDistance
                        ? (byte)1
                        : (byte)0;
            }

            // Fontumuz temel olarak iki renkli olduðu için
            // text veya background'a en yakýn rengi seçiyoruz.
            double textDist =
                ColorDistance(
                    color,
                    text);

            double bgDist =
                ColorDistance(
                    color,
                    background);

            if (textDist < bgDist)
                return 1;

            return 0;
        }

        private double ColorDistance(
    Color a,
    Color b)
        {
            double r =
                a.R - b.R;

            double g =
                a.G - b.G;

            double bl =
                a.B - b.B;

            double alpha =
                a.A - b.A;

            return
                (r * r) +
                (g * g) +
                (bl * bl) +
                (alpha * alpha);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private Bitmap RenderCharacterBitmap(
            byte[,] matrix,
            int scale)
        {
            int width = 8 * scale;
            int height = 16 * scale;

            Bitmap bmp = new Bitmap(
                width,
                height,
                PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(
                    transparentBackground
                        ? Color.Transparent
                        : backgroundColor);

                using (Brush brush =
                    new SolidBrush(textColor))
                {
                    for (int r = 0; r < 16; r++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (matrix[r, col] == 1)
                            {
                                g.FillRectangle(
                                    brush,
                                    col * scale,
                                    r * scale,
                                    scale,
                                    scale);
                            }
                        }
                    }
                }
            }

            return bmp;
        }

        private string GenerateSVG(
            byte[,] matrix,
            int scale)
        {
            int width = 8 * scale;
            int height = 16 * scale;

            string hexText =
                ColorToHex(textColor);

            string hexBg =
                ColorToHex(backgroundColor);

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" " +
                $"width=\"{width}\" height=\"{height}\" " +
                $"viewBox=\"0 0 {width} {height}\">");

            if (!transparentBackground)
            {
                sb.AppendLine(
                    $"  <rect width=\"100%\" height=\"100%\" fill=\"{hexBg}\" />");
            }

            for (int r = 0; r < 16; r++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (matrix[r, col] == 1)
                    {
                        int x = col * scale;
                        int y = r * scale;

                        sb.AppendLine(
                            $"  <rect x=\"{x}\" y=\"{y}\" " +
                            $"width=\"{scale}\" height=\"{scale}\" " +
                            $"fill=\"{hexText}\" />");
                    }
                }
            }

            sb.AppendLine("</svg>");

            return sb.ToString();
        }

        private string GenerateEPS(
            byte[,] matrix,
            int scale)
        {
            int width = 8 * scale;
            int height = 16 * scale;

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                "%!PS-Adobe-3.0 EPSF-3.0");

            sb.AppendLine(
                $"%%BoundingBox: 0 0 {width} {height}");

            sb.AppendLine("%%EndComments");

            if (!transparentBackground)
            {
                sb.AppendLine(
                    $"{backgroundColor.R / 255.0} " +
                    $"{backgroundColor.G / 255.0} " +
                    $"{backgroundColor.B / 255.0} setrgbcolor");

                sb.AppendLine(
                    $"0 0 {width} {height} rectfill");
            }

            sb.AppendLine(
                $"{textColor.R / 255.0} " +
                $"{textColor.G / 255.0} " +
                $"{textColor.B / 255.0} setrgbcolor");

            for (int r = 0; r < 16; r++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (matrix[r, col] == 1)
                    {
                        int x = col * scale;
                        int y =
                            height -
                            ((r + 1) * scale);

                        sb.AppendLine(
                            $"{x} {y} {scale} {scale} rectfill");
                    }
                }
            }

            return sb.ToString();
        }

        private string GenerateXBM(
            byte[,] matrix,
            int scale)
        {
            int width = 8 * scale;
            int height = 16 * scale;

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                $"#define char_width {width}");

            sb.AppendLine(
                $"#define char_height {height}");

            sb.AppendLine(
                "static unsigned char char_bits[] = {");

            List<string> bytesHex =
                new List<string>();

            for (int r = 0; r < height; r++)
            {
                int origR = r / scale;

                for (int cGroup = 0;
                     cGroup < width;
                     cGroup += 8)
                {
                    byte b = 0;

                    for (int bit = 0; bit < 8; bit++)
                    {
                        int currentX =
                            cGroup + bit;

                        if (currentX >= width)
                            continue;

                        int origCol =
                            currentX / scale;

                        if (origCol < 8 &&
                            matrix[origR, origCol] == 1)
                        {
                            b |=
                                (byte)(1 << bit);
                        }
                    }

                    bytesHex.Add(
                        $"0x{b:X2}");
                }
            }

            sb.AppendLine(
                "  " + string.Join(
                    ", ",
                    bytesHex));

            sb.AppendLine("};");

            return sb.ToString();
        }

        private Bitmap RenderCanvasBitmap(
            string text,
            int scale)
        {
            int charWidth = 8 * scale;
            int charHeight = 16 * scale;
            int spacing = 2 * scale;
            int padding = 10;

            int totalWidth =
                Math.Max(
                    1,
                    (text.Length *
                        (charWidth + spacing)) +
                    (padding * 2));

            int totalHeight =
                charHeight +
                (padding * 2);

            Bitmap bmp =
                new Bitmap(
                    totalWidth,
                    totalHeight,
                    PixelFormat.Format32bppArgb);

            using (Graphics g =
                Graphics.FromImage(bmp))
            {
                g.Clear(
                    transparentBackground
                        ? Color.Transparent
                        : backgroundColor);

                int currentX = padding;
                int currentY = padding;

                using (Brush pixelBrush =
                    new SolidBrush(textColor))
                {
                    foreach (char c in text)
                    {
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
                            for (int r = 0; r < 16; r++)
                            {
                                for (int col = 0; col < 8; col++)
                                {
                                    if (matrix[r, col] == 1)
                                    {
                                        g.FillRectangle(
                                            pixelBrush,
                                            currentX +
                                                (col * scale),
                                            currentY +
                                                (r * scale),
                                            scale,
                                            scale);
                                    }
                                }
                            }
                        }

                        currentX +=
                            charWidth + spacing;
                    }
                }
            }

            return bmp;
        }

        private string GenerateCanvasSVG(
            string text,
            int scale)
        {
            int charWidth = 8 * scale;
            int charHeight = 16 * scale;
            int spacing = 2 * scale;
            int padding = 10;

            int totalWidth =
                Math.Max(
                    1,
                    (text.Length *
                        (charWidth + spacing)) +
                    (padding * 2));

            int totalHeight =
                charHeight +
                (padding * 2);

            string hexText =
                ColorToHex(textColor);

            string hexBg =
                ColorToHex(backgroundColor);

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" " +
                $"width=\"{totalWidth}\" " +
                $"height=\"{totalHeight}\" " +
                $"viewBox=\"0 0 {totalWidth} {totalHeight}\">");

            if (!transparentBackground)
            {
                sb.AppendLine(
                    $"  <rect width=\"100%\" height=\"100%\" fill=\"{hexBg}\" />");
            }

            int currentX = padding;
            int currentY = padding;

            foreach (char c in text)
            {
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
                    for (int r = 0; r < 16; r++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (matrix[r, col] == 1)
                            {
                                int x =
                                    currentX +
                                    (col * scale);

                                int y =
                                    currentY +
                                    (r * scale);

                                sb.AppendLine(
                                    $"  <rect x=\"{x}\" y=\"{y}\" " +
                                    $"width=\"{scale}\" " +
                                    $"height=\"{scale}\" " +
                                    $"fill=\"{hexText}\" />");
                            }
                        }
                    }
                }

                currentX +=
                    charWidth + spacing;
            }

            sb.AppendLine("</svg>");

            return sb.ToString();
        }

        private string GenerateCanvasEPS(
            string text,
            int scale)
        {
            int charWidth = 8 * scale;
            int charHeight = 16 * scale;
            int spacing = 2 * scale;
            int padding = 10;

            int totalWidth =
                Math.Max(
                    1,
                    (text.Length *
                        (charWidth + spacing)) +
                    (padding * 2));

            int totalHeight =
                charHeight +
                (padding * 2);

            StringBuilder sb =
                new StringBuilder();

            sb.AppendLine(
                "%!PS-Adobe-3.0 EPSF-3.0");

            sb.AppendLine(
                $"%%BoundingBox: 0 0 {totalWidth} {totalHeight}");

            sb.AppendLine("%%EndComments");

            if (!transparentBackground)
            {
                sb.AppendLine(
                    $"{backgroundColor.R / 255.0} " +
                    $"{backgroundColor.G / 255.0} " +
                    $"{backgroundColor.B / 255.0} setrgbcolor");

                sb.AppendLine(
                    $"0 0 {totalWidth} {totalHeight} rectfill");
            }

            sb.AppendLine(
                $"{textColor.R / 255.0} " +
                $"{textColor.G / 255.0} " +
                $"{textColor.B / 255.0} setrgbcolor");

            int currentX = padding;
            int currentY = padding;

            foreach (char c in text)
            {
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
                    for (int r = 0; r < 16; r++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (matrix[r, col] == 1)
                            {
                                int x =
                                    currentX +
                                    (col * scale);

                                int y =
                                    totalHeight -
                                    (currentY +
                                     ((r + 1) * scale));

                                sb.AppendLine(
                                    $"{x} {y} " +
                                    $"{scale} {scale} rectfill");
                            }
                        }
                    }
                }

                currentX +=
                    charWidth + spacing;
            }

            return sb.ToString();
        }

        private string GenerateCanvasXBM(
            string text,
            int scale)
        {
            using (Bitmap bmp =
                RenderCanvasBitmap(text, scale))
            {
                StringBuilder sb =
                    new StringBuilder();

                sb.AppendLine(
                    $"#define canvas_width {bmp.Width}");

                sb.AppendLine(
                    $"#define canvas_height {bmp.Height}");

                sb.AppendLine(
                    "static unsigned char canvas_bits[] = {");

                List<string> bytesHex =
                    new List<string>();

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int xGroup = 0;
                         xGroup < bmp.Width;
                         xGroup += 8)
                    {
                        byte b = 0;

                        for (int bit = 0; bit < 8; bit++)
                        {
                            int px =
                                xGroup + bit;

                            if (px >= bmp.Width)
                                continue;

                            Color pixel =
                                bmp.GetPixel(px, y);

                            bool isPixelSet;

                            if (transparentBackground)
                            {
                                isPixelSet =
                                    pixel.A > 0 &&
                                    pixel.R == textColor.R &&
                                    pixel.G == textColor.G &&
                                    pixel.B == textColor.B;
                            }
                            else
                            {
                                isPixelSet =
                                    pixel.ToArgb() !=
                                    backgroundColor.ToArgb();
                            }

                            if (isPixelSet)
                            {
                                b |=
                                    (byte)(1 << bit);
                            }
                        }

                        bytesHex.Add(
                            $"0x{b:X2}");
                    }
                }

                sb.AppendLine(
                    "  " + string.Join(
                        ", ",
                        bytesHex));

                sb.AppendLine("};");

                return sb.ToString();
            }
        }

        private void PanelCanvas_Paint(
            object sender,
            PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            string textToDraw = txtInput.Text;

            int startX = 15;
            int startY = 45;

            int currentX = startX;
            int currentY = startY;

            int charWidth =
                8 * pixelSize;

            int charHeight =
                16 * pixelSize;

            int spacing =
                2 * pixelSize;

            using (Brush pixelBrush =
                new SolidBrush(textColor))
            {
                foreach (char c in textToDraw)
                {
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
                        for (int r = 0; r < 16; r++)
                        {
                            for (int col = 0; col < 8; col++)
                            {
                                if (matrix[r, col] == 1)
                                {
                                    g.FillRectangle(
                                        pixelBrush,
                                        currentX +
                                            (col * pixelSize),
                                        currentY +
                                            (r * pixelSize),
                                        pixelSize,
                                        pixelSize);
                                }
                            }
                        }
                    }

                    currentX +=
                        charWidth + spacing;

                    if (currentX >
                        panelCanvas.Width - charWidth)
                    {
                        currentX = startX;
                        currentY +=
                            charHeight + 5;
                    }
                }
            }
        }

        private ImageFormat GetImageFormat(
            string ext)
        {
            switch (ext.ToLowerInvariant())
            {
                case "jpg":
                case "jpeg":
                    return ImageFormat.Jpeg;

                case "bmp":
                    return ImageFormat.Bmp;

                case "gif":
                    return ImageFormat.Gif;

                case "tiff":
                    return ImageFormat.Tiff;

                case "ico":
                    return ImageFormat.Icon;

                case "png":
                default:
                    return ImageFormat.Png;
            }
        }

        private string ColorToHex(Color c)
        {
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        private string GetSafeFileName(char c)
        {
            switch (c)
            {
                case '/':
                    return "slash";

                case '\\':
                    return "backslash";

                case ':':
                    return "colon";

                case '*':
                    return "asterisk";

                case '?':
                    return "question";

                case '"':
                    return "quote";

                case '<':
                    return "less";

                case '>':
                    return "greater";

                case '|':
                    return "pipe";

                case ' ':
                    return "space";

                case '.':
                    return "dot";

                default:
                    return char.IsLetterOrDigit(c)
                        ? c.ToString()
                        : $"symbol_{(int)c}";
            }
        }
    }
}