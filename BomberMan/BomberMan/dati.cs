using System.Xml.Serialization;

namespace BomberMan
{
    public class dati
    {
        [XmlElement("Giocatore")]
        public string portaLocale { get; set; }
        public string portaRemota { get; set; }
        public string indirizzoIP { get; set; }
        public string username { get; set; }
    }
}
