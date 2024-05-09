using System;
using System.Drawing;
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
            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.Print();
        }

        private void PrintText(string text)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += (sender, e) =>
            {
                e.Graphics.DrawString(text, new Font("Arial", 12), Brushes.Black, new PointF(100, 100));
            };
            printDocument.Print();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                e.Graphics.DrawImage(image, e.MarginBounds);
            }
        }
    }
}
