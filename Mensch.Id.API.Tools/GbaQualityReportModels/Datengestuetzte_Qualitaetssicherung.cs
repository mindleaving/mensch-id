using System.Collections.Generic;

namespace Mensch.Id.API.Tools.GbaQualityReportModels
{
    public class Datengestuetzte_Qualitaetssicherung
    {
        public int Berichtsjahr { get; set; }
        public int IK { get; set; }
        public int Standortnummer { get; set; }
        public string Land { get; set; }
        public List<Leistungsbereich_DeQS> Dokumentationsraten { get; set; }
    }

    public class Einzige_Auswertungseinheit
    {
        
    }

    public class Leistungsbereich_DeQS
    {
        public string Kuerzel { get; set; }
        public string Bezeichnung { get; set; }
        public int Fallzahl { get; set; }
        public int Dokumentationsrate { get; set; }
        public int Anzahl_Datensaetze_Standort { get; set; }
        public string Kommentar_Krankenhaus { get; set; }
    }
}
