using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;

namespace Tiff_Splitter
{
    public partial class frmTiffSplitter : Form
    {
        private string srcFileName;
        private string dstFilePath;
        bool success = false;

        public frmTiffSplitter()
        {
            InitializeComponent();
        }

        private void SplitTiffImages(string sourceFilePath, string destFilePath)
        {
            Image image = Image.FromFile(sourceFilePath);
            string filename = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
            int frames = 0;
            Guid[] guid = image.FrameDimensionsList;
            FrameDimension fd = new FrameDimension(guid[0]);
            frames = image.GetFrameCount(fd);

            for (int i = 0; i < frames; i++)
            {
                image.SelectActiveFrame(fd, i);
                image.Save(destFilePath + @"\"+filename+"_" + i.ToString() + ".tif", System.Drawing.Imaging.ImageFormat.Tiff);
            }
        }

        private void Start(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(srcFileName) || string.IsNullOrEmpty(dstFilePath))
            {
                MessageBox.Show("Please select Source and Destination");
                return;
            }
            Button btnTarget = (Button)sender;
            bgdWorker.RunWorkerAsync(new string[3] {srcFileName,dstFilePath,btnTarget.Tag.ToString()});
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void btnSrc_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Source Tiff File";
            ofd.Filter = @"Tiff(*.tif,*.tiff)|*.tif;*.tiff";
            try
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    srcFileName = ofd.FileName;
                    txtSrc.Text = srcFileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDst_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(srcFileName))
                fbd.SelectedPath = System.IO.Path.GetDirectoryName(srcFileName);
            try
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    dstFilePath = fbd.SelectedPath;
                    txtDst.Text = dstFilePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bgdWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string source = (e.Argument as string[])[0];
                string destinaton = (e.Argument as string[])[1];
                string worktype = (e.Argument as string[])[2];

                switch (worktype)
                {
                    case "Tif":
                        SplitTiffImages(source, destinaton);
                        success = true;
                        break;
                    case "Pdf":
                        PdfDocument doc = new PdfDocument();
                        Image image = Image.FromFile(source);
                        string filename = System.IO.Path.GetFileNameWithoutExtension(source);
                        int frames = 0;
                        Guid[] guid = image.FrameDimensionsList;
                        FrameDimension fd = new FrameDimension(guid[0]);
                        frames = image.GetFrameCount(fd);
                        for (int i=0;i<frames;i++)
                        {
                            doc.AddPage();
                            XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[i]);
                            image.SelectActiveFrame(fd, i);
                            XImage img = XImage.FromGdiPlusImage(image);
                            doc.Pages[i].Width = img.Width;
                            doc.Pages[i].Height = img.Height;
                            xgr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
                        }
                        doc.Save(destinaton + "\\" + filename +".pdf");
                        doc.Close();
                        success = true;
                        break;
                }
                /*
                 * PdfPage[] pp = new PdfPage[frames];
                        for (int i = 0; i < frames; i++)
                        {
                            pp[i] = new PdfPage();
                            doc.AddPage(pp[i]);
                        }
                        Stream stream = ToStream(image,ImageFormat.Tiff);
                        Parallel.For(0, frames, i =>
                            {
                                Image imageTemp = Image.FromStream(stream);
                                XGraphics xgr = XGraphics.FromPdfPage(pp[i]);
                                imageTemp.SelectActiveFrame(fd, i);
                                XImage img = XImage.FromGdiPlusImage(imageTemp);
                                xgr.DrawImage(img,new Rectangle(0,0,img.Width,img.Height));
                                imageTemp.Dispose();
                            }
                        );
                 * 
                 * 
                 *
                 * 
                 * 
                 * 
                 */


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bgdWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
            toolStripProgressBar1.Value = 0;

            if (success)
                MessageBox.Show("The converion successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static Stream ToStream(Image image, ImageFormat formaw)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, formaw);
            stream.Position = 0;
            return stream;
        }
    }
}
