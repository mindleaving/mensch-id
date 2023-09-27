using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    /// <summary>
    /// German hospitals must publish quality reports, which are made public by G-BA (Gemeinsamer Bundesausschuss).
    /// </summary>
    public class GbaQualityReportParser
    {
        private const string ReportDirectory = @"F:\datasets\g-ba-qualitaetsberichte_2021";

        //[OneTimeSetUp]
        //public void XmlExceptionWorkaround()
        //{
        //    MethodInfo method = typeof(XmlSerializer).GetMethod("set_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        //    method.Invoke(null, new object[] { 1 });
        //}

        [Test]
        public void ExtractBirthClinics()
        {
            var filePaths = Directory.EnumerateFiles(ReportDirectory, "*-xml.xml");
            var reports = new List<GbaQualityReport>();
            foreach (var filePath in filePaths)
            {
                var report = Parse(filePath);
                reports.Add(report);
            }

            //Console.WriteLine("Geburtskliniken:");
            //Console.WriteLine("----------------------");
            var obstetricsHospitalsCount = 0;
            var birthRelatedOpsCodes = new Regex(@"5\-7[24]");
            foreach (var report in reports)
            {
                if(!HasObstetrics(report, out var obstetricsDepartment))
                    continue;
                var address = obstetricsDepartment.DepartmentAddress ?? report.LocationAddress ?? report.InstitutionAddress;
                if(address?.StreetName == null)
                    continue;
                //Console.WriteLine($"{report.Id}-{report.LocationId}: {report.InstitutionName} / {report.LocationName} / {obstetricsDepartment.Name} ({report.StreetName} {report.HouseNumber}, {report.PostalCode} {report.City})");
                var contactPerson = obstetricsDepartment.ContactPerson;
                var birthRelatedCases = obstetricsDepartment.Cases
                    .Where(x => birthRelatedOpsCodes.IsMatch(x.OpsCode))
                    .Sum(x => x.Count);
                var outputLine = StripNewLines($"{report.Id};"
                             + $"{report.LocationId};"
                             + $"{report.InstitutionName};"
                             + $"{report.LocationName};"
                             + $"{obstetricsDepartment.Name};"
                             + $"{birthRelatedCases};"
                             + $"{address.StreetName};"
                             + $"{address.HouseNumber};"
                             + $"{address.PostalCode};"
                             + $"{address.City};"
                             + $"{contactPerson?.Title};"
                             + $"{contactPerson?.FirstName};"
                             + $"{contactPerson?.LastName};"
                             + $"{contactPerson?.Role};"
                             + $"{contactPerson?.PhoneNumber};"
                             + $"{contactPerson?.Email}");
                if (outputLine.Count(c => c == ';') != 15)
                    throw new FormatException("Found unexpected semicolon");
                Console.WriteLine(outputLine);
                obstetricsHospitalsCount++;
            }
            //Console.WriteLine("----------------------");
            //Console.WriteLine($"Matching hospitals: {obstetricsHospitalsCount}");

            //Console.WriteLine();
            //Console.WriteLine("ABTEILUNGEN:");
            //Console.WriteLine("----------------------");
            //var abteilungsArten = reports
            //    .SelectMany(x => x.Departments)
            //    .SelectMany(x => x.Codes.Select(code => new
            //    {
            //        Name = x.Name,
            //        Code = code
            //    }))
            //    .GroupBy(x => x.Code);
            //foreach (var abteilungsArt in abteilungsArten.OrderBy(x => x.Key))
            //{
            //    Console.WriteLine($"{abteilungsArt.Key}: {string.Join(", ", abteilungsArt.Select(x => x.Name))}");
            //}
        }

        private static bool HasObstetrics(
            GbaQualityReport report,
            out GbaQualityReport.Department obstetricsDepartment)
        {
            obstetricsDepartment = report.Departments
                .Where(department => department.Name.ToLower().Contains("geburt"))
                //.Any(code => code == "2500");
                //.Any(code => code == "2400" || code == "2500");
                //.Any(code => code.StartsWith("24") || code.StartsWith("25"));
                .FirstOrDefault(department => department.Codes
                    .Any(code => code.StartsWith("24") || code.StartsWith("25"))
                );
            return obstetricsDepartment != null;
        }

        private GbaQualityReport Parse(
            string filePath)
        {
            //var serializer = new XmlSerializer(typeof(Qualitaetsbericht));
            //using var reader = XmlReader.Create(filePath);
            //var qualitaetsBericht = (Qualitaetsbericht)serializer.Deserialize(reader);
            //var krankenhaus = qualitaetsBericht.Krankenhaus.Single();
            //var standort = krankenhaus.Single();
            //var krankenhausKontaktDaten = standort.Krankenhauskontaktdaten.Single();
            //var krankenhausAdresse = krankenhausKontaktDaten.Kontakt_Adresse.First();
            //var standortKontaktdaten = standort.Standortkontaktdaten.Single();
            //var standortAdresse = standortKontaktdaten.Kontakt_Zugang.SingleOrDefault();
            //var abteilungen = qualitaetsBericht.Organisationseinheiten_Fachabteilungen.Single();
            //var departments = new List<GbaQualityReport.Department>();
            //foreach (var fachabteilung in abteilungen)
            //{
            //    var departmentType = fachabteilung.Fachabteilungsschluessel.FirstOrDefault()?.FA_Schluessel;
            //    var department = new GbaQualityReport.Department
            //    {
            //        Type = departmentType,
            //        Name = fachabteilung.Name
            //    };
            //    departments.Add(department);
            //}
            //return new GbaQualityReport
            //{
            //    Id = krankenhausKontaktDaten.IK,
            //    InstitutionName = krankenhausKontaktDaten.Name,
            //    StreetName = krankenhausAdresse.Adresse.First().Strasse,
            //    HouseNumber = krankenhausAdresse.Adresse.First().Hausnummer,
            //    PostalCode = krankenhausAdresse.Postleitzahl,
            //    City = krankenhausAdresse.Ort,
            //    Departments = departments
            //};

            var qualitaetsBericht = XElement.Load(filePath);
            var einleitung = qualitaetsBericht.Element("Einleitung");

            var krankenhaus = qualitaetsBericht.Element("Krankenhaus");
            var standort = krankenhaus.Element("Ein_Standort") ?? krankenhaus.Element("Mehrere_Standorte");
            var istMehrereStandorte = standort.Name == "Mehrere_Standorte";
            var krankenhausKontaktDaten = standort.Element("Krankenhauskontaktdaten");
            var krankenhausName = StripNewLines(krankenhausKontaktDaten.Element("Name").Value);
            var krankenhausId = krankenhausKontaktDaten.Element("IK").Value;
            var krankenhausKontaktAdresse = krankenhausKontaktDaten.Element("Kontakt_Adresse");
            var krankenhausZugangAdresse = krankenhausKontaktDaten.Element("Kontakt_Zugang");
            var anzahlBetten = int.Parse(qualitaetsBericht.Element("Anzahl_Betten").Value);
            var fallzahlen = qualitaetsBericht.Element("Fallzahlen");

            var standortKontaktDaten = standort.Element("Standortkontaktdaten");
            var standortZugangAdresse = standortKontaktDaten?.Element("Kontakt_Zugang");
            var standortId = krankenhausKontaktDaten.Element("Standortnummer")?.Value ?? standortKontaktDaten?.Element("Standortnummer").Value;
            var standortName = StripNewLines(standortKontaktDaten?.Element("Name")?.Value);

            var fachAbteilungen = qualitaetsBericht
                .Element("Organisationseinheiten_Fachabteilungen")
                .Elements("Organisationseinheit_Fachabteilung");
            var departments = new List<GbaQualityReport.Department>();
            foreach (var fachAbteilung in fachAbteilungen)
            {
                var name = fachAbteilung.Element("Name").Value;
                var schluessel = fachAbteilung.Elements("Fachabteilungsschluessel");
                var schluesselCodes = schluessel.Select(x => x.Element("FA_Schluessel")?.Value).Where(code => code != null).ToList();
                var aerztlicheLeitung = fachAbteilung.Element("Aerztliche_Leitung_OE");
                var arzt = aerztlicheLeitung?.Element("Chefarzt") ?? aerztlicheLeitung?.Element("Leitender_Belegarzt");
                var arztProfil = GetContact(arzt?.Element("Kontakt_Person_lang"));
                var abteilungsAdresse = GetAddress(arzt?.Element("Kontakt_Zugang"));
                var cases = GetCases(fachAbteilung);
                var department = new GbaQualityReport.Department
                {
                    Name = TakeUntilSemicolon(name),
                    Codes = schluesselCodes,
                    ContactPerson = arztProfil,
                    DepartmentAddress = abteilungsAdresse,
                    Cases = cases
                };
                departments.Add(department);
            }

            var krankenhausAddresse = krankenhausZugangAdresse ?? krankenhausKontaktAdresse;
            var standortAddress = standortZugangAdresse;
            return new GbaQualityReport
            {
                Id = krankenhausId,
                InstitutionName = TakeUntilSemicolon(krankenhausName),
                InstitutionAddress = GetAddress(krankenhausAddresse),
                LocationId = standortId,
                LocationName = TakeUntilSemicolon(standortName),
                LocationAddress = GetAddress(standortAddress),
                Departments = departments
            };
        }

        private List<GbaQualityReport.CaseStatistics> GetCases(
            XElement qualitaetsBericht)
        {
            var cases = new List<GbaQualityReport.CaseStatistics>();
            var stationaerFaelle = qualitaetsBericht
                .Element("Prozeduren")?
                .Element("Verpflichtende_Angabe")?
                .Elements("Prozedur") ?? new List<XElement>();
            foreach (var stationaererFall in stationaerFaelle)
            {
                var @case = new GbaQualityReport.CaseStatistics
                {
                    Type = GbaQualityReport.CaseStatistics.CaseType.Stationaer,
                    OpsCode = stationaererFall.Element("OPS_301").Value,
                    Count = int.Parse(stationaererFall.Element("Anzahl")?.Value ?? "0")
                };
                cases.Add(@case);
            }


            var ambulanteFaelle = qualitaetsBericht
                .Element("Ambulante_Operationen")?
                .Element("Verpflichtende_Angabe")?
                .Elements("Ambulante_Operation") ?? new List<XElement>();
            foreach (var ambulanterFall in ambulanteFaelle)
            {
                var @case = new GbaQualityReport.CaseStatistics
                {
                    Type = GbaQualityReport.CaseStatistics.CaseType.Ambulant,
                    OpsCode = ambulanterFall.Element("OPS_301").Value,
                    Count = int.Parse(ambulanterFall.Element("Anzahl")?.Value ?? "0")
                };
                cases.Add(@case);
            }
            return cases;
        }

        private static string TakeUntilSemicolon(
            string krankenhausName)
        {
            return krankenhausName != null ? new string(Regex.Replace(krankenhausName, @"&\w+;", "").TakeWhile(c => c != ';').ToArray()) : null;
        }

        private GbaQualityReport.Contact GetContact(
            XElement element)
        {
            if (element == null)
                return null;
            var person = element.Element("Person");
            var phoneNumber = element.Element("Telefon");
            return new GbaQualityReport.Contact
            {
                Title = StripNewLines(person?.Element("Titel")?.Value),
                FirstName = StripNewLines(person?.Element("Vorname")?.Value),
                LastName = StripNewLines(person?.Element("Nachname")?.Value),
                Role = StripNewLines(person?.Element("Funktion_Arbeitsschwerpunkt")?.Value?.Replace(";", ",")),
                PhoneNumber = SerializePhoneNumber(phoneNumber),
                Email = TakeUntilSemicolon(element.Element("Email")?.Value)
            };
        }

        private static string SerializePhoneNumber(
            XElement element)
        {
            if (element == null)
                return null;
            return $"{element.Element("Vorwahl")?.Value} {element.Element("Rufnummer")?.Value}-{element.Element("Durchwahl")?.Value}";
        }

        private static GbaQualityReport.Address GetAddress(
            XElement addressElement)
        {
            var streetName = addressElement?.Element("Adresse")?.Element("Strasse")?.Value
                             ?? addressElement?.Element("Strasse")?.Value;
            var houseNumber = addressElement?.Element("Adresse")?.Element("Hausnummer")?.Value
                              ?? addressElement?.Element("Hausnummer")?.Value;
            var postalCode = addressElement?.Element("Postleitzahl")?.Value;
            var city = addressElement?.Element("Ort")?.Value;
            if (streetName == null && houseNumber == null && postalCode == null && city == null)
                return null;
            return new GbaQualityReport.Address
            {
                StreetName = streetName,
                HouseNumber = houseNumber,
                PostalCode = postalCode,
                City = city,
            };
        }

        private static string StripNewLines(
            string str)
        {
            return str?.ReplaceLineEndings(" ");
        }
    }

    public class GbaQualityReport
    {
        public string Id { get; set; }
        public string InstitutionName { get; set; }
        public Address InstitutionAddress { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public Address LocationAddress { get; set; }

        public List<Department> Departments { get; set; }

        public class Contact
        {
            public string Title { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Role { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
        }
        public class Address
        {
            public string StreetName { get; set; }
            public string HouseNumber { get; set; }
            public string PostalCode { get; set; }
            public string City { get; set; }
        }
        public class Department
        {
            public string Name { get; set; }
            public Address DepartmentAddress { get; set; }
            public Contact ContactPerson { get; set; }
            public List<string> Codes { get; set; }
            public List<CaseStatistics> Cases { get; set; }
        }

        public class CaseStatistics
        {
            public CaseType Type { get; set; }
            public string OpsCode { get; set; }
            public int Count { get; set; }

            public enum CaseType
            {
                Stationaer,
                Ambulant
            }
        }
    }
}
