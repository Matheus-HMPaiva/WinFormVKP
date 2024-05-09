using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace win_form
{
    public partial class Form1 : Form
    {
        private Button btnSelectImage;
        private Button btnPrintText;
        private TextBox txtTextToPrint;
        private string imagePath;

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Botão para selecionar imagem
            btnSelectImage = new Button();
            btnSelectImage.Text = "Selecionar Imagem";
            btnSelectImage.Location = new Point(50, 50);
            btnSelectImage.Size = new Size(150, 30);
            btnSelectImage.Click += BtnSelectImage_Click;
            Controls.Add(btnSelectImage);

            // Caixa de texto para inserir texto a ser impresso
            txtTextToPrint = new TextBox();
            txtTextToPrint.Location = new Point(50, 100);
            txtTextToPrint.Size = new Size(300, 100);
            Controls.Add(txtTextToPrint);

            // Botão para imprimir texto
            btnPrintText = new Button();
            btnPrintText.Text = "Imprimir Texto";
            btnPrintText.Location = new Point(50, 220);
            btnPrintText.Size = new Size(150, 30);
            btnPrintText.Click += BtnPrintText_Click;
            Controls.Add(btnPrintText);
        }

        private void BtnSelectImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de imagem|*.jpg;*.jpeg;*.png;*.gif|Todos os arquivos|*.*";
            openFileDialog.Title = "Selecione uma imagem para imprimir";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                imagePath = openFileDialog.FileName;
                PrintImage();
            }
        }

        private void BtnPrintText_Click(object sender, EventArgs e)
        {
            PrintText(txtTextToPrint.Text);
        }

        private void PrintImage()
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintImagePage;

            // Define o tamanho da página como 80mm por 120mm
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(80 * 100 / 25.4), (int)(120 * 100 / 25.4));

            printDocument.Print();
        }

        private void PrintText(string text)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += (sender, e) =>
            {
                // Define a fonte e a área retangular para o texto
                Font font = new Font("Arial", 12);
                RectangleF rect = new RectangleF(e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Width, e.MarginBounds.Height);

                // Calcula a altura do texto para ajustá-lo à página
                SizeF size = e.Graphics.MeasureString(text, font, (int)rect.Width);
                float lineHeight = size.Height / text.Split('\n').Length;
                float linesPerPage = rect.Height / lineHeight;

                // Divide o texto em linhas para caber na página
                int lineCount = 0;
                int charCount = 0;
                while (lineCount < linesPerPage && charCount < text.Length)
                {
                    int chars = (int)((text.Length - charCount) < rect.Width / font.Size ? (text.Length - charCount) : (rect.Width / font.Size));
                    e.Graphics.DrawString(text.Substring(charCount, chars), font, Brushes.Black, rect.Left, rect.Top + (lineHeight * lineCount));
                    charCount += chars;
                    lineCount++;
                }

                // Verifica se ainda há mais linhas para imprimir
                if (charCount < text.Length)
                {
                    e.HasMorePages = true;
                }
                else
                {
                    e.HasMorePages = false;
                }
            };
            printDocument.Print();
        }

        private void PrintDocument_PrintImagePage(object sender, PrintPageEventArgs e)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                // Escurece a imagem
                AdjustContrast(image, -30); // Ajuste o valor de contraste conforme necessário

                // Define a área de impressão dentro da página (com margens)
                Rectangle printArea = new Rectangle(
                    e.MarginBounds.Left,
                    e.MarginBounds.Top,
                    e.MarginBounds.Width,
                    e.MarginBounds.Height);

                // Calcula as dimensões da imagem redimensionada para caber na área de impressão
                Size imageSize = GetImageSizeToFit(image, printArea.Size);

                // Calcula a posição de desenho para centralizar a imagem na área de impressão
                int x = printArea.Left + (printArea.Width - imageSize.Width) / 2;
                int y = printArea.Top + (printArea.Height - imageSize.Height) / 2;

                // Desenha a imagem redimensionada na área de impressão
                e.Graphics.DrawImage(image, new Rectangle(x, y, imageSize.Width, imageSize.Height));
            }
        }

        // Função para calcular o tamanho da imagem redimensionada para caber na área de impressão
        private Size GetImageSizeToFit(Image image, Size fitSize)
        {
            float aspectRatio = (float)image.Width / image.Height;
            float fitWidth = fitSize.Width * 2;
            float fitHeight = fitSize.Width / aspectRatio *2;

            if (fitHeight > fitSize.Height)
            {
                fitHeight = fitSize.Height;
                fitWidth = fitSize.Height * aspectRatio;
            }

            return new Size((int)fitWidth, (int)fitHeight);
        }

        // Função para ajustar o contraste da imagem
        private void AdjustContrast(Image image, float value)
        {
            using (Graphics g = Graphics.FromImage(image))
            {
                // Cria um ajuste de cor para alterar o contraste
                ImageAttributes attributes = new ImageAttributes();
                float[][] matrix = {
                    new float[] {value, 0, 0, 0, 0},
                    new float[] {0, value, 0, 0, 0},
                    new float[] {0, 0, value, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                };
                ColorMatrix colorMatrix = new ColorMatrix(matrix);
                attributes.SetColorMatrix(colorMatrix);

                // Desenha a imagem com o ajuste de contraste
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                g.DrawImage(image, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel, attributes);
            }
        }
    }
}
