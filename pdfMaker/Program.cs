using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Reflection.PortableExecutable;
using static System.Net.Mime.MediaTypeNames;

namespace pdfMaker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!Directory.Exists("pdf"))
                Directory.CreateDirectory("pdf");
            FromImageToPdf("C:\\Users\\lbigoni\\Downloads\\WhatsApp Unknown 2022-11-15 at 16.44.28", "pdf/elaborazione.pdf");
            AddImageWatermark("pdf/elaborazione.pdf", "image/watermark.png", "pdf/watermarkRisultato.pdf");
            Console.WriteLine("Fine procedura");
        }

        public static void FromImageToPdf(string imagesPath, string outputPdfFilePath)
        {
            if (Directory.Exists(outputPdfFilePath))
                Directory.Delete(outputPdfFilePath);
            Document doc = new Document();
            doc.SetPageSize(iTextSharp.text.PageSize.A4);
            using (var stream = new FileStream(outputPdfFilePath, FileMode.Create))
            using (PdfWriter writer = PdfWriter.GetInstance(doc, stream))
            {
                int[,] matrixpage = new int[(int)PageSize.A4.Width, (int)PageSize.A4.Height];//ci salviamo com'è venuta l'immgine è una copia della pagina65
                doc.Open();
                var listImages = Directory.GetFiles(imagesPath, "*.*", SearchOption.AllDirectories)
                          .Where(end => end.EndsWith(".jpg") || end.EndsWith(".png") || end.EndsWith(".jpeg") || end.EndsWith(".svg")).ToList();
                var x = 20;
                var y = 20;

                foreach (var file in listImages)
                {
                    doc.AddTitle("TITOLO");
                    var img = resizeImage(file);


                    var logo = iTextSharp.text.Image.GetInstance(img, ImageFormat.Png);
                    if (img.Width > x / 3*2)//centriamo l'immagine
                    {
                        x = ((int)PageSize.A4.Width/2) - img.Width/2;//melo centra nel caso si fosse più larghi di metà pagina
                    }
                    else { x = 20; }

                    //   logo.SetAbsolutePosition(20, (int)PageSize.A4.Height-20- img.Height);20 dal basso e venti dall'alto
                    logo.SetAbsolutePosition(x, (int)PageSize.A4.Height-20- img.Height);
                    doc.Add(logo);
                    doc.Add(iTextSharp.text.Phrase.GetInstance(file));
                    doc.NewPage();
                    x = 20;
                    y = 500;
                };

                doc.Close();
                writer.Close();
                stream.Close();
            }

        }
        public static System.Drawing.Image resizeImage(string stPhotoPath)
        {
            System.Drawing.Image imgPhoto = System.Drawing.Image.FromFile(stPhotoPath);


            double sourceWidth = imgPhoto.Width;
            double sourceHeight = imgPhoto.Height;
            while (sourceWidth > iTextSharp.text.PageSize.A4.Width /3*2 || sourceHeight > iTextSharp.text.PageSize.A4.Height /3*2)
            {
                sourceWidth = sourceWidth / 3*2;
                sourceHeight = sourceHeight / 3*2;
            }
            if (sourceHeight < 200)//se l'altezza è troppo bassa
            {
                sourceWidth = sourceWidth * 1.45;
                sourceHeight = sourceHeight * 1.45;
            }
            Bitmap bmPhoto = new Bitmap((int)sourceWidth, (int)sourceHeight);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                         imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Black);
            grPhoto.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto, 0, 0, (int)sourceWidth, (int)sourceHeight);
            /*   new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
               new System.Drawing.Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
               GraphicsUnit.Pixel);*/

            grPhoto.Dispose();
            imgPhoto.Dispose();
            return bmPhoto;
        }
        public static void AddImageWatermark(string sourceFilePath, string watermarkImagePath, string targetFilePath)
        {
            if (Directory.Exists(targetFilePath))
                Directory.Delete(targetFilePath);
            var pdfReader = new PdfReader(sourceFilePath);
            var pdfStamper = new PdfStamper(pdfReader, new FileStream(targetFilePath, FileMode.Create));
            var image = iTextSharp.text.Image.GetInstance(watermarkImagePath);
            image.SetAbsolutePosition(0, 0);

            for (var i = 0; i < pdfReader.NumberOfPages; i++)
            {
                var content = pdfStamper.GetUnderContent(i + 1);
                content.AddImage(image);
            }

            pdfStamper.Close();
            //  ProcessStartInfo startInfo = new ProcessStartInfo(targetFilePath);
            //     Process.Start(startInfo);
        }
    }
}