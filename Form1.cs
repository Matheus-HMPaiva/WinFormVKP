using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Windows.Forms;
using QRCoder;
namespace win_form
{
    public partial class Form1 : Form
    {
        private Button btnSelectImage;
        private Button btnPrintText;
        private TextBox txtTextToPrint;

        private TextBox txtQRCodeToPrint;

        private Button btnPrintQRCode;
    
        private string imagePath;

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
            this.Text = "Impressora VKP80III";
        }

        private void InitializeControls()
        {   
            // Caixa de texto para inserir texto a ser impresso
            txtTextToPrint = new TextBox();
            txtTextToPrint.Location = new Point(50, 50);
            txtTextToPrint.Size = new Size(150, 50);
            Controls.Add(txtTextToPrint);

            // Botão para imprimir texto
            btnPrintText = new Button();
            btnPrintText.Text = "Imprimir Texto";
            btnPrintText.Location = new Point(210, 50);
            btnPrintText.Size = new Size(150, 30);
            btnPrintText.Click += BtnPrintText_Click;
            Controls.Add(btnPrintText);

            // Caixa de texto para inserir texto a ser gerado o QRCode
            txtQRCodeToPrint = new TextBox();
            txtQRCodeToPrint.Location = new Point(50, 100);
            txtQRCodeToPrint.Size = new Size(150, 100);
            Controls.Add(txtQRCodeToPrint);

            // Botão para imprimir QRCode
            btnPrintQRCode = new Button();
            btnPrintQRCode.Text = "Gerar QRCode";
            btnPrintQRCode.Location = new Point(210, 100);
            btnPrintQRCode.Size = new Size(150, 30);
            btnPrintQRCode.Click += BtnPrintQRCode_Click;
            Controls.Add(btnPrintQRCode);


            // Botão para selecionar imagem
            btnSelectImage = new Button();
            btnSelectImage.Text = "Selecionar Imagem";
            btnSelectImage.Location = new Point(125, 150);
            btnSelectImage.Size = new Size(150, 30);
            btnSelectImage.Click += BtnSelectImage_Click;
            Controls.Add(btnSelectImage);

            // Definindo o tamanho padrão da página

           
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

            // Define a largura da área de impressão (sem margens)
            float printWidth = e.PageSettings.PrintableArea.Width;
  

            // Define o recuo desejado (espaço no início de cada linha)
            float indentation = 10; // Altere conforme necessário            
            
            // Inicializa a posição X e Y para o início do texto (sem margens)
            float x = indentation;
            float y = 0;


            // Calcula a altura do texto para ajustá-lo à página
            SizeF textSize = e.Graphics.MeasureString(text, font, (int)printWidth);

            // Calcula o número de linhas necessárias para o texto
            int numLines = (int)Math.Ceiling(textSize.Width / printWidth);

            // Divide o texto em palavras
            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Divide o texto em linhas
            //string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

           // Desenha cada linha do texto
                foreach (string word in words)
                {   
                    // Calcula o tamanho da palavra
                    SizeF wordSize = e.Graphics.MeasureString(word + " ", font);
                    // Calcula o tamanho da linha atual
                    //SizeF lineSize = e.Graphics.MeasureString(line, font);

                    // Verifica se a linha atual cabe na largura da página
                    if (x + wordSize.Width > printWidth - indentation)
                    {
                        // Se não couber, move para a próxima linha
                        x = indentation; // Volta para o início da linha
                       y += wordSize.Height; // Move para a próxima linha
                    }

                        // Desenha a linha atual
                        //e.Graphics.DrawString(line, font, Brushes.Black, x, y);
                        // Desenha a palavra
                        e.Graphics.DrawString(word + " ", font, Brushes.Black, x, y);

                        // Atualiza a posição X para o início da próxima linha
                        x += wordSize.Width;
                }
            };

                printDocument.Print();
        }


        private void BtnPrintQRCode_Click(object sender, EventArgs e)
        {
            string websiteUrl = txtQRCodeToPrint.Text;
            GenerateAndPrintQRCode(websiteUrl);            

        }

        private void GenerateAndPrintQRCode(string websiteUrl)      
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(websiteUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(10, Color.Black, Color.White, false); // Ajuste os parâmetros conforme necessário

            PrintDocument printDocument = new PrintDocument();

            // Define o tamanho padrão da página
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(80 * 100 / 25.4), (int)(120 * 100 / 25.4));

            printDocument.PrintPage += (s, ev) =>
            {
                // Definindo a área de impressão dentro da página (sem margens)
                Rectangle printArea = new Rectangle(
                0,
                0,
                printDocument.DefaultPageSettings.PaperSize.Width,
                printDocument.DefaultPageSettings.PaperSize.Height);

                // Calculando as dimensões do QRCode redimensionado para caber na área de impressão
                Size imageSize = GetQRCodeSizeToFit(qrCodeImage, printArea.Size);

                // Calculando a posição de desenho para centralizar o QRCode na área de impressão
                int x = (printArea.Width - imageSize.Width) / 2;
                int y = (printArea.Height - imageSize.Height) / 2;

                // Desenhando o QRCode redimensionado na área de impressão
                ev.Graphics.DrawImage(qrCodeImage, new Rectangle(x, y, imageSize.Width, imageSize.Height));
            };

            printDocument.Print();
        }


// Função para calcular o tamanho da QRCode redimensionada para caber na área de impressão
        private Size GetQRCodeSizeToFit(Image qrcode, Size fitSize)
        {
            float aspectRatio = (float)qrcode.Width / qrcode.Height;

            int newWidth = (int)Math.Min(qrcode.Width, fitSize.Width * 0.25); 
            int newHeight = (int)(newWidth / aspectRatio);

          

            return new Size(newWidth, newHeight);
        }

                    

        private void PrintDocument_PrintImagePage(object sender, PrintPageEventArgs e)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                // Escurece a imagem
                //AdjustContrast(image, 1); // Ajuste o valor de contraste conforme necessário

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
        private void AdjustImageProperties(Image image, float contrastValue, float brightnessValue, float saturationValue)
        {
            using (Graphics g = Graphics.FromImage(image))
            {
            // Cria um ajuste de cor para alterar o contraste, brilho e saturação
            ImageAttributes attributes = new ImageAttributes();

            // Matriz para ajuste de contraste, brilho e saturação
            float[][] matrix = {
            new float[] {contrastValue, 0, 0, 0, 0},    // Ajuste de contraste
            new float[] {0, contrastValue, 0, 0, 0},    // Ajuste de brilho
            new float[] {0, 0, contrastValue, 0, 0},    // Ajuste de saturação
            new float[] {0, 0, 0, 1, 0},                // Sem alteração no canal alfa
            new float[] {0, 0, 0, 0, 1}                 // Sem alteração na translação de cores
            };

                ColorMatrix colorMatrix = new ColorMatrix(matrix);
                attributes.SetColorMatrix(colorMatrix);

            // Aplica os ajustes na imagem
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
            0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
        }

    }
}

