using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace zakupki.gov.ru
{
    [Serializable]
    public class Settings
    {
        public string FirstPartReferencePath { get; set; }
        public string SecondPartReferencePath { get; set; }
        public string Examination66Path { get; set; }
        public string Examination31Path { get; set; }
        public string ExaminationITNPath { get; set; }
        public string FirstPDFFilePath { get; set; }
        public string SecondPDFFilePath { get; set; }
        public string PrivisionFilePath { get; set; }
        public string ReportTablePath { get; set; }
        public string FolderForResults { get; set; }
        public string License { get; set; }

        [NonSerialized]
        private readonly string fileName = "Settings.dat";
        public void Save()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream("Settings.dat", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, this);
            }
        }
        public Settings Load()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            if (!File.Exists(fileName))
            {
                Save();
            }
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                return (Settings)formatter.Deserialize(fs);
            }
        }
    }
}
