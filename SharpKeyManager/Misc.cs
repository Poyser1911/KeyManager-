
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SharpKeyManager
{
    public static class Print
    {
        public static void Richtextbox(RichTextBox box, string text)
        {
            SolidColorBrush solidColorBrush = Brushes.White;
            box.Document.Blocks.Clear();
            for (int index = 0; index < text.Length; ++index)
            {
                if ((int)text[index] == 94)
                {
                    switch (text[index + 1])
                    {
                        case '0':
                            solidColorBrush = Brushes.Black;
                            break;
                        case '1':
                            solidColorBrush = Brushes.Red;
                            break;
                        case '2':
                            solidColorBrush = Brushes.Green;
                            break;
                        case '3':
                            solidColorBrush = Brushes.Yellow;
                            break;
                        case '4':
                            solidColorBrush = Brushes.Blue;
                            break;
                        case '5':
                            solidColorBrush = Brushes.Aqua;
                            break;
                        case '6':
                            solidColorBrush = Brushes.Purple;
                            break;
                        case '7':
                            solidColorBrush = Brushes.White;
                            break;
                        case '8':
                            solidColorBrush = Brushes.Gray;
                            break;
                        case '9':
                            solidColorBrush = Brushes.Brown;
                            break;
                    }
                    index += 2;
                    TextRange textRange = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                    textRange.Text = text[index].ToString();
                    textRange.ApplyPropertyValue(TextElement.ForegroundProperty, (object)solidColorBrush);
                    textRange.ApplyPropertyValue(TextElement.FontWeightProperty, (object)FontWeights.Regular);
                }
                else
                {
                    TextRange textRange = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                    textRange.Text = text[index].ToString();
                    textRange.ApplyPropertyValue(TextElement.ForegroundProperty, (object)solidColorBrush);
                    textRange.ApplyPropertyValue(TextElement.FontWeightProperty, (object)FontWeights.Regular);
                }
            }
        }
    }
    public class Location
    {
        public string ip { get; set; }
        public string countrycode { get; set; }
        public string countryname { get; set; }
        public string region { get; set; }
    }
   static class GeoIP
    {
        public static Location GetCurrentLocation()
        {
            DataSet dataSet = new DataSet();
            try
            {
                int num = (int)dataSet.ReadXml("http://l-gc.pw/geoip/");
                DataRow dataRow = dataSet.Tables[0].Rows[0];
                return new Location()
                {
                    ip = dataRow["IP"].ToString(),
                    countrycode = dataRow["CountryCode"].ToString(),
                    countryname = dataRow["CountryName"].ToString(),
                    region = dataRow["RegionName"].ToString()
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new Location();
            }
        }
    }
}
