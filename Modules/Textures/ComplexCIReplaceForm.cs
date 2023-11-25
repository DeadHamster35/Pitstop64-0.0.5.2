using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pitstop64.Data;
using Cereal64.Microcodes.F3DEX.DataElements;
using System.IO;
using Cereal64.Common.DataElements.Encoding;
using Cereal64.Common.Rom;
using Cereal64.Common.Utils.Encoding;
using Cereal64.Common.DataElements;

namespace Pitstop64.Modules.Textures
{
    public partial class ComplexCIReplaceForm : Form
    {
        public List<MK64Image> Images { get; private set; }

        public enum SharedMode
        {
            Texture,
            Palette
        }

        private enum SelectedList
        {
            Before,
            After
        }

        private SharedMode Mode;
        private SelectedList Selection;

        public ComplexCIReplaceForm(List<MK64Image> images, SharedMode mode)
        {
            InitializeComponent();

            Images = images;
            Mode = mode;

            UpdateImages();
        }

        public void UpdateImages()
        {
            lbBefore.Items.Clear();
            lbAfter.Items.Clear();
            imagePreview.Image = null;
            SelectedImage = null;

            //Now populate
            foreach (MK64Image image in Images)
            {
                BitmapWrapper wrapper = new BitmapWrapper(image.Image, image.ImageName);
                lbBefore.Items.Add(wrapper);
                lbAfter.Items.Add(wrapper);
            }
        }


        private class BitmapWrapper
        {
            public Bitmap BMP;
            public string Name;

            public BitmapWrapper(Bitmap bmp, string name)
            {
                BMP = bmp;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private BitmapWrapper SelectedImage = null;

        private void lbBefore_SelectedIndexChanged(object sender, EventArgs e)
        {
            Selection = SelectedList.Before;

            SelectedImage = ((BitmapWrapper)lbBefore.SelectedItem);
            if(SelectedImage != null)
                imagePreview.Image = SelectedImage.BMP;

            btnReplace.Enabled = false;
        }

        private void lbAfter_SelectedIndexChanged(object sender, EventArgs e)
        {
            Selection = SelectedList.After;

            SelectedImage = ((BitmapWrapper)lbAfter.SelectedItem);
            if (SelectedImage != null)
            {
                imagePreview.Image = SelectedImage.BMP;

                btnReplace.Enabled = true;
            }
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = (Bitmap)Bitmap.FromFile(openFileDialog.FileName);
                if (bmp == null)
                {
                    MessageBox.Show("Error: Couldn't load image file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (bmp.Height != SelectedImage.BMP.Height ||
                    bmp.Width != SelectedImage.BMP.Width)
                {
                    MessageBox.Show("Error: New image must be same size!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                BitmapWrapper wrapper = new BitmapWrapper(bmp, Path.GetFileNameWithoutExtension(openFileDialog.FileName));

                if (Selection == SelectedList.Before)
                {
                    lbBefore.Items[lbBefore.SelectedIndex] = wrapper;
                }
                else
                {
                    lbAfter.Items[lbAfter.SelectedIndex] = wrapper;
                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            bool hasChanges = false;

            for (int i = 0; i < lbBefore.Items.Count; i++)
            {
                if (lbBefore.Items[i] != lbAfter.Items[i])
                {
                    hasChanges = true;
                    break;
                }
            }

            if (!hasChanges) //Quit out if no changes
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                return;
            }

            //if the mode is texture, then do something new.
            //if the mode is palette, then generate the palette BEFORE making the image

            if (Mode == SharedMode.Texture)
            {
                //IDEA: AVERAGE ALL THE IMAGES TOGETHER, THEN MAKE A CI PALETTE/TEXTURE FROM THAT. THEN WE HAVE OUR TEXTURE.
                //       AFTER THAT, WE NEED TO ASSIGN PALETTES TO IT. I GUESS WE CAN AVERAGE ALL COLORS THAT HIT EACH POINT
                //       FOR EACH IMAGE

                //Slow process



                //Love you MIB but that makes no sense.
                //Can't get the proper palette of several images by averaging together the RGB values of their respective pixels.
                //I'm too lazy to figure this out, so we're just gonna hijack the function
                // and allow the end user to upload a palette manually.


                
                

                int width = Images[0].Image.Width;
                int height = Images[0].Height;
                int pixelCount = width * height;
                int imageCount = lbAfter.Items.Count;
                int colorCount = Images[0].ImageReference.BasePalettes[0].Colors.Length;

                /*
                int[] redCount = new int[pixelCount];
                int[] blueCount = new int[pixelCount];
                int[] greenCount = new int[pixelCount];
                int[] alphaCount = new int[pixelCount];

                    foreach (BitmapWrapper wrapper in lbAfter.Items)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            for (int i = 0; i < height; i++)
                            {
                                Color pixel = wrapper.BMP.GetPixel(i, j);
                                redCount[i + j * wrapper.BMP.Width] += pixel.R;
                                greenCount[i + j * wrapper.BMP.Width] += pixel.G;
                                blueCount[i + j * wrapper.BMP.Width] += pixel.B;
                                alphaCount[i + j * wrapper.BMP.Width] += pixel.A;
                            }
                        }
                    }

                    for (int j = 0; j < height; j++)
                    {
                        for (int i = 0; i < width; i++)
                        {
                            refImage.SetPixel(i, j, 
                                Color.FromArgb(alphaCount[i + j * width] / imageCount, 
                                redCount[i + j * width] / imageCount, 
                                greenCount[i + j * width] / imageCount, 
                                blueCount[i + j * width] / imageCount));
                        }
                    }
                */
                //Now do the set-texture palette search method
                Palette tempPalette = new Palette(-1, new byte[colorCount * 2]);


                MessageBox.Show("Please select the PNG to use as a shared Palette for " + Images[0].ImageName);

                OpenFileDialog FileOpen = new OpenFileDialog();
                FileOpen.Filter = "PNG files|*.png|All files|*.*";
                if (FileOpen.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(FileOpen.FileName))
                    {


                        Bitmap PaletteImage = new Bitmap(FileOpen.FileName);

                        //Now that we have the colors added up, we calculate the new colors and that's our new palette!
                        Color[][] colors = new Color[PaletteImage.Height][];
                        for (int k = 0; k < PaletteImage.Height; k++)
                        {
                            colors[k] = new Color[colorCount];
                            for (int i = 0; i < PaletteImage.Width; i++)
                            {
                                colors[k][i] = PaletteImage.GetPixel(i, k);
                            }
                        }
                        
                        //Now take this and do some stupid shit

                        List<byte> RawData = new List<byte>();
                        for (int k = 0; k < imageCount; k++)
                        {
                            byte[] newPaletteData = TextureConversion.PaletteToBinary(colors[k]);
                            Bitmap refImage = ((BitmapWrapper)lbAfter.Items[k]).BMP;
                            Images[k].ImageReference.BasePalettes[0].RawData = newPaletteData;
                            Palette DummyPal = Images[k].ImageReference.BasePalettes[0];


                            byte[] finalTextureData = TextureConversion.ImageToBinary(Images[k].Format, Images[k].PixelSize, refImage, ref DummyPal, false);
                            Images[k].ImageReference.Texture.RawData = finalTextureData;

                            if (Images[k].PaletteEncoding[0] == MK64Image.MK64ImageEncoding.MIO0)
                            {
                                N64DataElement element;
                                if (!RomProject.Instance.Files[0].HasElementExactlyAt(Images[k].PaletteOffset[0], out element))
                                    throw new Exception();
                                MIO0Block block = (MIO0Block)element;
                                byte[] oldMIO0Data = block.DecodedData;

                                Array.Copy(newPaletteData, 0, oldMIO0Data, Images[k].PaletteBlockOffset[0], newPaletteData.Length);

                                byte[] compressedNewMIO0 = MIO0.Encode(oldMIO0Data);
                                block.RawData = compressedNewMIO0;
                            }

                                Images[k].ImageReference.UpdateImage();

                            if (Images[k].TextureEncoding == MK64Image.MK64ImageEncoding.MIO0)
                            {

                                N64DataElement element;
                                if (!RomProject.Instance.Files[0].HasElementExactlyAt(Images[0].TextureOffset, out element))
                                    throw new Exception();
                                MIO0Block block = (MIO0Block)element;

                                byte[] MIO0Data = block.DecodedData;
                                Array.Copy(finalTextureData, 0, MIO0Data, Images[k].TextureBlockOffset, finalTextureData.Length);

                                byte[] compressedNewMIO0 = MIO0.Encode(MIO0Data);
                                block.RawData = compressedNewMIO0;
                            }
                        }

                        MessageBox.Show("Finished!");
                            
                    }
                    
                }
                

            }
            else
            {
                PaletteMedianCutAnalyzer paletteMaker = new PaletteMedianCutAnalyzer();
                foreach (BitmapWrapper wrapper in lbAfter.Items)
                {
                    for (int i = 0; i < wrapper.BMP.Width; i++)
                    {
                        for (int j = 0; j < wrapper.BMP.Height; j++)
                        {
                            paletteMaker.AddColor(wrapper.BMP.GetPixel(i, j));
                        }
                    }
                }

                Color[] colors = paletteMaker.GetPalette(Images[0].ImageReference.BasePalettes[0].Colors.Length);
                byte[] paletteData = TextureConversion.PaletteToBinary(colors);
                Palette palette = new Palette(-1, paletteData);
                byte[] newPaletteData = palette.RawData;
                Images[0].ImageReference.BasePalettes[0].RawData = newPaletteData;

                if (Images[0].PaletteEncoding[0] == MK64Image.MK64ImageEncoding.MIO0)
                {
                    N64DataElement element;
                    if (!RomProject.Instance.Files[0].HasElementExactlyAt(Images[0].PaletteOffset[0], out element))
                        throw new Exception();
                    MIO0Block block = (MIO0Block)element;
                    byte[] oldMIO0Data = block.DecodedData;

                    Array.Copy(newPaletteData, 0, oldMIO0Data, Images[0].PaletteBlockOffset[0], newPaletteData.Length);

                    byte[] compressedNewMIO0 = MIO0.Encode(oldMIO0Data);
                    
                    block.RawData = compressedNewMIO0;
                }

                //Now generate the images
                for (int i = 0; i < Images.Count; i++)
                {
                    MK64Image image = Images[i];
                    Bitmap bmp = ((BitmapWrapper)lbAfter.Items[i]).BMP;
                    byte[] newData = TextureConversion.ImageToBinary(image.Format, image.PixelSize, bmp, ref palette, false);

                    if (newData == null || newData.Length == 0)
                    {
                        MessageBox.Show("Error: Couldn't convert image file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    image.ImageReference.Texture.RawData = newData;
                    image.ImageReference.UpdateImage();

                    if (image.TextureEncoding == MK64Image.MK64ImageEncoding.MIO0)
                    {
                        N64DataElement element;
                        if (!RomProject.Instance.Files[0].HasElementExactlyAt(image.TextureOffset, out element))
                            throw new Exception();
                        MIO0Block block = (MIO0Block)element;

                        byte[] MIO0Data = block.DecodedData;

                        Array.Copy(newData, 0, MIO0Data, image.TextureBlockOffset, newData.Length);

                        byte[] compressedNewMIO0 = MIO0.Encode(MIO0Data);

                        block.RawData = compressedNewMIO0;
                    }
                }
                
            }
        }

        private void ComplexCIReplaceForm_Load(object sender, EventArgs e)
        {

        }
    }
}
