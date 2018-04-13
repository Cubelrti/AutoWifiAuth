using AutoWifiAuth.Configs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace AutoWifiAuth.Helpers
{
    class CaptchaBreaker
    {
        public async Task<Bitmap> GetCaptcha(Internet i)
        {
            Stream stream = await i.GetAsyncStream(new Uri(Protocol.captcha));
            Bitmap map = new Bitmap(Image.FromStream(stream));
            return map;
        }

        public async static Task<string> GetValidateCode(Bitmap map)
        {
            const string dataUri = @"C:\Program Files (x86)\Tesseract-OCR\tessdata\";
            const string lang = "eng";
            const string defaultList = "0123456789";
            TesseractEngine ocr = new TesseractEngine(dataUri, lang);
            ocr.SetVariable("tessedit_char_whitelist", defaultList);
            return await Task.Run(() => {
                Page pg = ocr.Process(map, pageSegMode: ocr.DefaultPageSegMode);
                return pg.GetText().Trim().Replace(" ", "").ToLower();
            });
        }
    }
}
