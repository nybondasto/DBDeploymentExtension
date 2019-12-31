using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace t1 
{
    public class ModelChecksumWriter
    {
        private readonly string _checksumProvider;
        private readonly string _path;

        public ModelChecksumWriter(string path,
            string checksumProvider = "System.Security.Cryptography.SHA256CryptoServiceProvider")
        {
            _path = path;
            _checksumProvider = checksumProvider;
        }

        public string FixChecksum(string filename = "model.xml")
        {
            string csum = "";

            using (var dac = new DacHacXml(_path))
            {
                var originXml = dac.GetXml("Origin.xml");
                var sourceXml = dac.GetStream(filename);

                //-- Poistetaan model.xml:sta Model-segmentin alla oleva SqlDatabaseOptions-elementti
                //-- jotta deployment ei aja Visual Studion asetuksia olemassaolevan kannan asetusten päälle
                XDocument xDoc = XDocument.Load(sourceXml, LoadOptions.None);
                var root = xDoc.Root;

                var Model = root.Elements().Select(x => x.Elements()).Last();
                Model.First().Remove();
                string newXml = xDoc.ToString();

                //-- Viedään jäljelläoleva xml stringiin ja sieltä streamiksi
                byte[] byteArray = Encoding.ASCII.GetBytes(newXml);
                MemoryStream streamSourceXml = new MemoryStream(byteArray);
                //-- Elementin poisto valmis, checksum voidaan laskea

                var calculatedChecksum =
                    BitConverter.ToString(
                        (HashAlgorithm.Create(_checksumProvider)
                            .ComputeHash(streamSourceXml))).Replace("-", "");
                ;

                //-- Kirjoitetaan takaisin muokattu xml
                dac.SetXml(filename, newXml);

                var reader = XmlReader.Create(new StringReader(originXml));
                reader.MoveToContent();


                while (reader.Read())
                {
                    if (reader.Name == "Checksum" && reader.GetAttribute("Uri") == string.Format("/{0}", filename))
                    {
                        var oldChecksum = reader.ReadInnerXml();

                        if (oldChecksum == calculatedChecksum)
                        {
                            csum = "(OLD) " + oldChecksum;
                            break;
                        }

                        originXml = originXml.Replace(oldChecksum, calculatedChecksum);

                        dac.SetXml("Origin.xml", originXml);

                        csum = "(NEW) " + calculatedChecksum;
                        break;
                    }
                }
            }
            return csum;
        }
    }
}
